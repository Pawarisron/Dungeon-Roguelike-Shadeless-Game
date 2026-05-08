#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

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

    private static readonly string[] EnemyPaths = {
        "Assets/Prefabs/Agents/Orc.prefab",
        "Assets/Prefabs/Agents/GiantOrc.prefab",
        "Assets/Prefabs/Agents/Orc Behaviro Testing.prefab",
        "Assets/Prefabs/Agents/NPC-Solider.prefab",
    };
    private const string BOSS_PATH = "Assets/Prefabs/Agents/GiantOrc Boss.prefab";

    [MenuItem("Tools/Sekiro Combat/Auto-Wire Everything")]
    public static void WireEverything()
    {
        int log = 0;
        log += CreateDefaultAttackData();
        log += WirePlayer();
        log += WireAllEnemies();
        log += WireBoss();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Sekiro Auto-Wire — Done",
            $"{log} change(s) applied. See Console for details.\n\n" +
            "MANUAL STEPS REMAINING:\n" +
            "1. Add 'Parry' Action to:\n" +
            "   Assets/_Scripts/Entity/Player/New Controls.inputactions\n" +
            "   • Action type: Button\n" +
            "   • Binding: <Mouse>/rightButton\n\n" +
            "2. Drag the Parry InputActionReference into:\n" +
            "   Player.prefab → PlayerInput.parry\n\n" +
            "3. (Optional) Wire UnityEvents per Combat/README.md\n\n" +
            "4. Open DunLevel1 scene → Press Play → enjoy.",
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
        foreach (var p in EnemyPaths) total += WireEnemy(p, isBoss: false);
        return total;
    }

    [MenuItem("Tools/Sekiro Combat/Auto-Wire Boss")]
    public static int WireBoss() => WireEnemy(BOSS_PATH, isBoss: true);

    private static int WireEnemy(string path, bool isBoss)
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

            // Larger enemies / boss have higher posture pools
            if (posture != null)
            {
                var so = new SerializedObject(posture);
                var max = so.FindProperty("maxPosture");
                if (max != null) { max.floatValue = isBoss ? 140f : 60f; changes++; }
                so.ApplyModifiedProperties();
            }

            if (isBoss)
            {
                EnsureComponent<UtilityBrain>(instance, ref changes);
                EnsureComponent<BossPhaseManager>(instance, ref changes);
            }

            // Wire AIEnemy.OnBeingAttacked → UtilityBrain.NotifyTookHit (if brain present)
            var brain = instance.GetComponent<UtilityBrain>();
            var aiEnemy = instance.GetComponent<AIEnemy>();
            if (brain != null && aiEnemy != null)
            {
                AddPersistentListener(aiEnemy, "OnBeingAttacked", brain, "NotifyTookHit");
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Debug.Log($"<color=#7ee37e>[SekiroAutoWire] ✓ {Path.GetFileNameWithoutExtension(path)} wired</color> ({changes} change(s), boss={isBoss})");
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
