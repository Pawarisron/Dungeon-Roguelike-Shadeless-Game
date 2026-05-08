#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// One-click wiring for the Sekiro combat stack. Adds the new components to the
// Player + enemy prefabs, sets the SerializeField references on PlayerInput,
// and creates a default set of AttackData ScriptableObjects.
//
// Usage in Unity Editor:
//   Tools → Sekiro Combat → Auto-Wire Everything
//
// Re-running is safe: components and assets are only created if missing.
public static class SekiroAutoWire
{
    private const string PLAYER_PATH = "Assets/Prefabs/Agents/Player.prefab";
    private const string ATTACK_DATA_FOLDER = "Assets/Data/Combat";
    private const string AI_ACTIONS_FOLDER = "Assets/Data/AI";
    private const string INPUT_ACTIONS_PATH = "Assets/_Scripts/Entity/Player/New Controls.inputactions";
    private const string ACTION_MAP_NAME = "PlayerInput";
    private const string PARRY_ACTION_NAME = "Parry";
    private const string PARRY_BINDING_PATH = "<Mouse>/rightButton";

    // Tier-0 grunts: simple AI, normal attacks only.
    private static readonly string[] GruntPaths = {
        "Assets/Prefabs/Agents/Orc.prefab",
        "Assets/Prefabs/Agents/NPC-Solider.prefab",
    };
    // Tier-1 elites: smarter AI, can throw a Sweep.
    private static readonly string[] ElitePaths = {
        "Assets/Prefabs/Agents/GiantOrc.prefab",
        "Assets/Prefabs/Agents/Orc Behaviro Testing.prefab",
    };
    private const string BOSS_PATH = "Assets/Prefabs/Agents/GiantOrc Boss.prefab";

    private enum EnemyTier { Grunt, Elite, Boss }

    [MenuItem("Tools/Sekiro Combat/Auto-Wire Everything")]
    public static void WireEverything()
    {
        int log = 0;
        log += AddParryInputAction();
        log += CreateDefaultAttackData();
        log += CreateDefaultEnemyActions();
        log += WirePlayer();
        log += WireParryReferenceToPlayer();
        log += WireAllEnemies();
        log += WireBoss();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Sekiro Auto-Wire — Done",
            $"{log} change(s) applied. See Console for details.\n\n" +
            "Parry input is now bound to RIGHT MOUSE BUTTON.\n\n" +
            "OPTIONAL:\n" +
            "• Wire AttackData → enemy.attackPool to enable Perilous attacks\n" +
            "• Wire any extra UnityEvents per Combat/README.md\n" +
            "• Add a Posture UI Image and drag it into PostureManager.postureBar\n\n" +
            "Open DunLevel1 scene → Press Play → enjoy.",
            "OK");
    }

    [MenuItem("Tools/Sekiro Combat/Auto-Wire Player")]
    public static int WirePlayer()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PATH);
        if (prefab == null) { Debug.LogError($"[SekiroAutoWire] Player prefab not found at {PLAYER_PATH}"); return 0; }

        int changes = 0;
        var instance = PrefabUtility.LoadPrefabContents(PLAYER_PATH);
        try
        {
            // 1. Combat components
            var posture       = EnsureComponent<PostureManager>(instance, ref changes);
            var parryCtrl     = EnsureComponent<ParryController>(instance, ref changes);
            var mikiri        = EnsureComponent<MikiriDetector>(instance, ref changes);
            var dodge         = EnsureComponent<DodgeController>(instance, ref changes);
            var combo         = EnsureComponent<ComboCounter>(instance, ref changes);
            var arrow         = EnsureComponent<MikiriArrowIndicator>(instance, ref changes);
            var tracker       = EnsureComponent<PlayerActionTracker>(instance, ref changes);
            EnsureComponent<PlayerPatternProfile>(instance, ref changes);

            var audio = instance.GetComponent<AudioSource>();
            if (audio == null) { audio = instance.AddComponent<AudioSource>(); audio.playOnAwake = false; changes++; }
            EnsureComponent<CombatSfx>(instance, ref changes);

            // 2. Wire SerializeField refs on PlayerInput
            var playerInput = instance.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                var so = new SerializedObject(playerInput);
                if (TrySetObjectField(so, "parryController",  parryCtrl)) changes++;
                if (TrySetObjectField(so, "postureManager",   posture))   changes++;
                if (TrySetObjectField(so, "mikiriDetector",   mikiri))    changes++;
                if (TrySetObjectField(so, "dodgeController",  dodge))     changes++;
                so.ApplyModifiedProperties();

                // Wire UnityEvents → PlayerActionTracker
                AddPersistentListener(playerInput, "OnAttack", tracker, "OnAttack");
                AddPersistentListener(playerInput, "OnRoll", tracker, "OnRoll");
                AddPersistentListener(playerInput, "OnParryPressed", tracker, "OnParryPressed");
                AddPersistentListener(playerInput, "OnParrySuccess", tracker, "OnParrySuccess");
                AddPersistentListener(playerInput, "OnBeingAttacked", tracker, "OnBeingAttacked");
                AddPersistentListener(playerInput, "OnBeingAttacked", combo,   "ResetCombo");
            }

            // 3. Wire Agent.OnHitLanded → ComboCounter; OnAttackWhiffed → Tracker
            var agent = instance.GetComponent<Agent>();
            if (agent != null)
            {
                AddPersistentListener(agent, "OnHitLanded", combo, "RegisterHit");
                AddPersistentListener(agent, "OnAttackWhiffed", tracker, "OnAttackWhiff");
            }

            // 4. Wire MikiriDetector arrow
            AddPersistentListenerWithArg(mikiri, "OnIncomingThrust", arrow, "ShowFor");
            AddPersistentListener(mikiri, "OnMikiriSuccess", arrow, "Hide");

            PrefabUtility.SaveAsPrefabAsset(instance, PLAYER_PATH);
            Debug.Log($"<color=#7ee37e>[SekiroAutoWire] ✓ Player wired</color> ({changes} change(s))");
        }
        finally { PrefabUtility.UnloadPrefabContents(instance); }
        return changes;
    }

    [MenuItem("Tools/Sekiro Combat/Auto-Wire All Enemies")]
    public static int WireAllEnemies()
    {
        int total = 0;
        foreach (var p in GruntPaths) total += WireEnemy(p, EnemyTier.Grunt);
        foreach (var p in ElitePaths) total += WireEnemy(p, EnemyTier.Elite);
        return total;
    }

    [MenuItem("Tools/Sekiro Combat/Auto-Wire Boss")]
    public static int WireBoss() => WireEnemy(BOSS_PATH, EnemyTier.Boss);

    private static int WireEnemy(string path, EnemyTier tier)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogWarning($"[SekiroAutoWire] skipped (not found): {path}"); return 0; }

        int changes = 0;
        var instance = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var posture = EnsureComponent<PostureManager>(instance, ref changes);
            EnsureComponent<DeathblowMarker>(instance, ref changes);
            EnsureComponent<AttackTelegraph>(instance, ref changes);

            // Posture pool scales with tier.
            if (posture != null)
            {
                var so = new SerializedObject(posture);
                var max = so.FindProperty("maxPosture");
                if (max != null)
                {
                    float target = tier switch { EnemyTier.Grunt => 50f, EnemyTier.Elite => 80f, EnemyTier.Boss => 140f, _ => 60f };
                    if (!Mathf.Approximately(max.floatValue, target)) { max.floatValue = target; changes++; }
                }
                so.ApplyModifiedProperties();
            }

            // Every enemy now gets a UtilityBrain (grunts use a smaller action pool).
            var brain = EnsureComponent<UtilityBrain>(instance, ref changes);

            if (tier == EnemyTier.Boss)
            {
                EnsureComponent<BossPhaseManager>(instance, ref changes);
            }

            // Auto-fill brain.actions if empty.
            AssignBrainActions(brain, tier, ref changes);

            // Auto-fill AIEnemy.attackPool if empty.
            var aiEnemy = instance.GetComponent<AIEnemy>();
            AssignAttackPool(aiEnemy, tier, ref changes);

            // Wire AIEnemy.OnBeingAttacked → UtilityBrain.NotifyTookHit
            if (brain != null && aiEnemy != null)
            {
                AddPersistentListener(aiEnemy, "OnBeingAttacked", brain, "NotifyTookHit");
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Debug.Log($"<color=#7ee37e>[SekiroAutoWire] ✓ {Path.GetFileNameWithoutExtension(path)} wired</color> ({changes} change(s), tier={tier})");
        }
        finally { PrefabUtility.UnloadPrefabContents(instance); }
        return changes;
    }

    private static void AssignBrainActions(UtilityBrain brain, EnemyTier tier, ref int changes)
    {
        if (brain == null) return;
        var so = new SerializedObject(brain);
        var actionsProp = so.FindProperty("actions");
        if (actionsProp == null) return;
        if (actionsProp.arraySize > 0) return;  // designer already filled it — leave alone

        var (approach, retreat, atkNormal, atkPerilous, feint, hold) = LoadEnemyActions();
        var picks = new List<EnemyAction>();
        switch (tier)
        {
            case EnemyTier.Grunt:
                if (approach   != null) picks.Add(approach);
                if (atkNormal  != null) picks.Add(atkNormal);
                if (retreat    != null) picks.Add(retreat);
                if (hold       != null) picks.Add(hold);
                break;
            case EnemyTier.Elite:
                if (approach   != null) picks.Add(approach);
                if (atkNormal  != null) picks.Add(atkNormal);
                if (atkPerilous!= null) picks.Add(atkPerilous);
                if (retreat    != null) picks.Add(retreat);
                if (hold       != null) picks.Add(hold);
                break;
            case EnemyTier.Boss:
                if (approach   != null) picks.Add(approach);
                if (atkNormal  != null) picks.Add(atkNormal);
                if (atkPerilous!= null) picks.Add(atkPerilous);
                if (feint      != null) picks.Add(feint);
                if (retreat    != null) picks.Add(retreat);
                if (hold       != null) picks.Add(hold);
                break;
        }

        actionsProp.arraySize = picks.Count;
        for (int i = 0; i < picks.Count; i++)
            actionsProp.GetArrayElementAtIndex(i).objectReferenceValue = picks[i];
        so.ApplyModifiedProperties();
        changes++;
    }

    private static void AssignAttackPool(AIEnemy enemy, EnemyTier tier, ref int changes)
    {
        if (enemy == null) return;
        var so = new SerializedObject(enemy);
        var pool = so.FindProperty("attackPool");
        if (pool == null) return;
        if (pool.arraySize > 0) return;

        var slash  = AssetDatabase.LoadAssetAtPath<AttackDataSO>($"{ATTACK_DATA_FOLDER}/Atk_Slash.asset");
        var sweep  = AssetDatabase.LoadAssetAtPath<AttackDataSO>($"{ATTACK_DATA_FOLDER}/Atk_Sweep.asset");
        var thrust = AssetDatabase.LoadAssetAtPath<AttackDataSO>($"{ATTACK_DATA_FOLDER}/Atk_Thrust.asset");
        var crash  = AssetDatabase.LoadAssetAtPath<AttackDataSO>($"{ATTACK_DATA_FOLDER}/Atk_Crash.asset");

        var picks = new List<AttackDataSO>();
        switch (tier)
        {
            case EnemyTier.Grunt:
                if (slash != null) picks.Add(slash);
                break;
            case EnemyTier.Elite:
                if (slash != null) picks.Add(slash);
                if (sweep != null) picks.Add(sweep);
                break;
            case EnemyTier.Boss:
                if (slash != null) picks.Add(slash);
                if (sweep != null) picks.Add(sweep);
                if (thrust != null) picks.Add(thrust);
                if (crash != null) picks.Add(crash);
                break;
        }

        pool.arraySize = picks.Count;
        for (int i = 0; i < picks.Count; i++)
            pool.GetArrayElementAtIndex(i).objectReferenceValue = picks[i];
        so.ApplyModifiedProperties();
        changes++;
    }

    private static (EnemyAction approach, EnemyAction retreat, EnemyAction atkNormal,
                    EnemyAction atkPerilous, EnemyAction feint, EnemyAction hold) LoadEnemyActions()
    {
        EnemyAction Load(string fileName) =>
            AssetDatabase.LoadAssetAtPath<EnemyAction>($"{AI_ACTIONS_FOLDER}/{fileName}.asset");
        return (Load("Action_Approach"), Load("Action_Retreat"), Load("Action_AttackNormal"),
                Load("Action_AttackPerilous"), Load("Action_Feint"), Load("Action_HoldGround"));
    }

    [MenuItem("Tools/Sekiro Combat/Create Default Enemy Actions")]
    public static int CreateDefaultEnemyActions()
    {
        if (!AssetDatabase.IsValidFolder(AI_ACTIONS_FOLDER))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
            AssetDatabase.CreateFolder("Assets/Data", "AI");
        }

        int changes = 0;
        changes += CreateActionAsset<EnemyAction_Approach>("Action_Approach");
        changes += CreateActionAsset<EnemyAction_Retreat>("Action_Retreat");
        changes += CreateActionAsset<EnemyAction_AttackNormal>("Action_AttackNormal");
        changes += CreateActionAsset<EnemyAction_AttackPerilous>("Action_AttackPerilous");
        changes += CreateActionAsset<EnemyAction_Feint>("Action_Feint");
        changes += CreateActionAsset<EnemyAction_HoldGround>("Action_HoldGround");
        return changes;
    }

    private static int CreateActionAsset<T>(string fileName) where T : EnemyAction
    {
        var path = $"{AI_ACTIONS_FOLDER}/{fileName}.asset";
        if (AssetDatabase.LoadAssetAtPath<T>(path) != null) return 0;
        var so = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"<color=#7ee37e>[SekiroAutoWire] ✓ Created</color> {path}");
        return 1;
    }

    [MenuItem("Tools/Sekiro Combat/Add Parry Input Action")]
    public static int AddParryInputAction()
    {
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(INPUT_ACTIONS_PATH);
        if (asset == null)
        {
            Debug.LogError($"[SekiroAutoWire] InputActionAsset not found at {INPUT_ACTIONS_PATH}");
            return 0;
        }

        var map = asset.FindActionMap(ACTION_MAP_NAME);
        if (map == null)
        {
            Debug.LogError($"[SekiroAutoWire] Action map '{ACTION_MAP_NAME}' not found in {INPUT_ACTIONS_PATH}");
            return 0;
        }

        if (map.FindAction(PARRY_ACTION_NAME) != null)
        {
            Debug.Log("[SekiroAutoWire] Parry action already exists — skipping.");
            return 0;
        }

        var action = map.AddAction(PARRY_ACTION_NAME, InputActionType.Button);
        action.AddBinding(PARRY_BINDING_PATH);

        // Persist via JSON round-trip — works across all InputSystem versions.
        File.WriteAllText(INPUT_ACTIONS_PATH, asset.ToJson());
        AssetDatabase.ImportAsset(INPUT_ACTIONS_PATH);

        Debug.Log($"<color=#7ee37e>[SekiroAutoWire] ✓ Added Parry action</color> bound to {PARRY_BINDING_PATH}");
        return 1;
    }

    [MenuItem("Tools/Sekiro Combat/Wire Parry Reference to Player")]
    public static int WireParryReferenceToPlayer()
    {
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(INPUT_ACTIONS_PATH);
        if (asset == null) return 0;
        var map = asset.FindActionMap(ACTION_MAP_NAME);
        var action = map != null ? map.FindAction(PARRY_ACTION_NAME) : null;
        if (action == null)
        {
            Debug.LogError("[SekiroAutoWire] Parry action missing — run 'Add Parry Input Action' first.");
            return 0;
        }

        // Look for an existing InputActionReference sub-asset that points at this action.
        InputActionReference parryRef = null;
        foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(INPUT_ACTIONS_PATH))
        {
            if (sub is InputActionReference iar && iar.action != null && iar.action.id == action.id)
            {
                parryRef = iar;
                break;
            }
        }

        // Create one if Unity hasn't auto-generated it yet.
        if (parryRef == null)
        {
            parryRef = ScriptableObject.CreateInstance<InputActionReference>();
            parryRef.name = $"{ACTION_MAP_NAME}/{PARRY_ACTION_NAME}";
            var refSo = new SerializedObject(parryRef);
            var assetProp = refSo.FindProperty("m_Asset");
            var idProp = refSo.FindProperty("m_ActionId");
            if (assetProp != null) assetProp.objectReferenceValue = asset;
            if (idProp != null) idProp.stringValue = $"{{{action.id}}}";
            refSo.ApplyModifiedProperties();
            AssetDatabase.AddObjectToAsset(parryRef, asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(INPUT_ACTIONS_PATH);
        }

        // Drop the reference onto PlayerInput.parry on the prefab.
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PATH);
        if (playerPrefab == null) return 0;
        int changes = 0;
        var instance = PrefabUtility.LoadPrefabContents(PLAYER_PATH);
        try
        {
            var pi = instance.GetComponent<PlayerInput>();
            if (pi == null) return 0;
            var so = new SerializedObject(pi);
            var prop = so.FindProperty("parry");
            if (prop != null && prop.objectReferenceValue != parryRef)
            {
                prop.objectReferenceValue = parryRef;
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(instance, PLAYER_PATH);
                changes++;
                Debug.Log("<color=#7ee37e>[SekiroAutoWire] ✓ Parry reference linked to PlayerInput.parry</color>");
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(instance); }
        return changes;
    }

    [MenuItem("Tools/Sekiro Combat/Create Default AttackData Assets")]
    public static int CreateDefaultAttackData()
    {
        if (!AssetDatabase.IsValidFolder(ATTACK_DATA_FOLDER))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
            AssetDatabase.CreateFolder("Assets/Data", "Combat");
        }

        int changes = 0;
        changes += CreateAttack("Atk_Slash",  AttackDataSO.AttackType.Normal,           20, 18f, 0.35f);
        changes += CreateAttack("Atk_Sweep",  AttackDataSO.AttackType.Perilous_Sweep,   30, 25f, 0.55f);
        changes += CreateAttack("Atk_Thrust", AttackDataSO.AttackType.Perilous_Thrust,  25, 30f, 0.50f);
        changes += CreateAttack("Atk_Crash",  AttackDataSO.AttackType.Perilous_Crash,   40, 35f, 0.60f);
        return changes;
    }

    private static int CreateAttack(string fileName, AttackDataSO.AttackType type, int hp, float posture, float telegraph)
    {
        var path = $"{ATTACK_DATA_FOLDER}/{fileName}.asset";
        if (AssetDatabase.LoadAssetAtPath<AttackDataSO>(path) != null) return 0;
        var so = ScriptableObject.CreateInstance<AttackDataSO>();
        so.attackName = fileName;
        so.type = type;
        so.hpDamage = hp;
        so.postureDamage = posture;
        so.telegraphTime = telegraph;
        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"<color=#7ee37e>[SekiroAutoWire] ✓ Created</color> {path}");
        return 1;
    }

    // ===== helpers =====

    private static T EnsureComponent<T>(GameObject go, ref int changes) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) { c = go.AddComponent<T>(); changes++; }
        return c;
    }

    private static bool TrySetObjectField(SerializedObject so, string fieldName, Object value)
    {
        var prop = so.FindProperty(fieldName);
        if (prop == null) return false;
        if (prop.objectReferenceValue == value) return false;
        prop.objectReferenceValue = value;
        return true;
    }

    // No-arg UnityEvent listener.
    private static void AddPersistentListener(Object source, string eventFieldName, Object target, string methodName)
    {
        var so = new SerializedObject(source);
        var ev = so.FindProperty(eventFieldName);
        if (ev == null) { return; }
        // Need actual UnityEvent instance to use UnityEventTools.
        var fi = source.GetType().GetField(eventFieldName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fi == null) return;
        var unityEvent = fi.GetValue(source) as UnityEvent;
        if (unityEvent == null) return;

        // Avoid duplicate listener
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            if (unityEvent.GetPersistentTarget(i) == target && unityEvent.GetPersistentMethodName(i) == methodName) return;

        var action = System.Delegate.CreateDelegate(typeof(UnityAction), target, methodName, false, false) as UnityAction;
        if (action == null) return;
        UnityEventTools.AddPersistentListener(unityEvent, action);
    }

    // UnityEvent<T> listener (e.g. MikiriDetector.OnIncomingThrust : UnityEvent<MonoBehaviour>)
    private static void AddPersistentListenerWithArg<T>(Object source, string eventFieldName, Object target, string methodName)
        where T : Object
    {
        var fi = source.GetType().GetField(eventFieldName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fi == null) return;
        var unityEvent = fi.GetValue(source) as UnityEvent<T>;
        if (unityEvent == null) return;
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            if (unityEvent.GetPersistentTarget(i) == target && unityEvent.GetPersistentMethodName(i) == methodName) return;
        var action = System.Delegate.CreateDelegate(typeof(UnityAction<T>), target, methodName, false, false) as UnityAction<T>;
        if (action == null) return;
        UnityEventTools.AddPersistentListener(unityEvent, action);
    }

    // Variant for non-generic call sites — used for OnIncomingThrust which is UnityEvent<MonoBehaviour>.
    private static void AddPersistentListenerWithArg(Object source, string eventFieldName, Object target, string methodName)
    {
        AddPersistentListenerWithArg<MonoBehaviour>(source, eventFieldName, target, methodName);
    }
}
#endif
