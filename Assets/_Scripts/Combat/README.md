# Sekiro-Style Combat

## ⚡ ทางลัด — Auto-Wire ครั้งเดียว

ถ้าอยากข้าม manual setup ทั้งหมด:

1. เปิดโปรเจกต์ใน **Unity 6000.0.25f1**
2. เมนู: **Tools → Sekiro Combat → Auto-Wire Everything**
3. รอ ~3 วินาที — Editor script จะ:
   - เพิ่ม component ที่ขาดบน Player.prefab (PostureManager, ParryController, MikiriDetector, DodgeController, ComboCounter, MikiriArrowIndicator, AudioSource, CombatSfx, PlayerActionTracker, PlayerPatternProfile)
   - เพิ่ม PostureManager + DeathblowMarker + AttackTelegraph บน enemy prefabs ทุกตัว
   - เพิ่ม UtilityBrain + BossPhaseManager บน Boss prefab
   - สร้าง 4 AttackData SO assets (Slash / Sweep / Thrust / Crash) ใน `Assets/Data/Combat/`
   - Wire 11 UnityEvents ที่ commonly needed
4. **ขั้นตอน manual ที่เหลือ** (ไม่ auto ได้เพราะเป็น Input System UI):
   - เปิด `Assets/_Scripts/Entity/Player/New Controls.inputactions`
   - Add Action ชื่อ `Parry` (Button) + binding `<Mouse>/rightButton`
   - Drag InputActionReference → Player.prefab → PlayerInput.parry
5. Build & Play

ขั้นตอน manual ทั้งหมดด้านล่างยังคงใช้ได้ถ้าอยากปรับเองทีละ component

---

# Phase 1 MVP

ระบบ posture + parry + deathblow สำหรับ 2D top-down. โค้ดเสร็จแล้ว — เหลือ wiring ใน Unity Editor

## ไฟล์ที่เพิ่ม

| ไฟล์ | หน้าที่ |
|------|---------|
| `IParriable.cs` | Interface — เป้าหมายที่ parry ได้ implement ตัวนี้ |
| `PostureManager.cs` | เก็บ posture pool + auto-regen + stagger state |
| `ParryController.cs` | จับจังหวะ parry window (default 0.18s) |
| `HitStop.cs` | Singleton helper — `HitStop.Trigger(0.08f)` หยุดเวลาให้รู้สึก hit |
| `DeathblowMarker.cs` | Optional: indicator (เครื่องหมาย ! แดง) ตอน enemy โดน stagger |

## ไฟล์ที่แก้

| ไฟล์ | สิ่งที่เปลี่ยน |
|------|---------------|
| `Entity/Agent.cs` | `OnAttackAnimation()` วิ่งผ่าน `ResolveHit()` ใหม่ — pipeline parry → deathblow → clean hit |
| `Entity/Player/PlayerInput.cs` | implement `IParriable`, bind input "parry" |

---

## 🛠️ Editor Setup (ทำใน Unity ทีละขั้น)

### 1. เพิ่ม Parry input action

เปิด `Assets/Settings/PlayerInput.inputactions` (หรือชื่อที่ใช้อยู่) → ใน Action Map "Player":

- กด `+` เพิ่ม Action ชื่อ **`Parry`** (Action Type: Button)
- เพิ่ม Binding: `<Mouse>/rightButton` (หรือ `<Keyboard>/q` ตามที่ Ant ชอบ)
- Save asset

### 2. ตั้งค่า Player Prefab

เปิด `Assets/Prefabs/Agents/Player.prefab`:

1. **Add Component → `Posture Manager`**
   - Max Posture: `100`
   - Regen Per Sec: `25`
   - Regen Delay After Hit: `1.0`
   - Stagger Duration: `2.0`
   - Posture Bar: drag UI Image (clone Health Bar pattern)

2. **Add Component → `Parry Controller`**
   - Parry Window: `0.18` (Sekiro = ~0.3, ที่นี่หินกว่าเพื่อความท้าทาย)
   - Cooldown After Fail: `0.35`
   - Parry Stance VFX: optional GameObject (ดาบยกขึ้น)
   - Parry Spark VFX: optional prefab (ประกายไฟตอนปะทะ)

3. **PlayerInput** component (เดิม) → fields ใหม่:
   - Parry Controller: drag self (Player Prefab)
   - Posture Manager: drag self (Player Prefab)
   - Parry: drag InputActionReference จาก step 1
   - Guarded Posture Damage: `12`
   - Guarded Damage Multiplier: `0.4`

4. **Agent** component (เดิม) → fields ใหม่:
   - Posture Damage On Hit: `18`
   - Parry Retaliation Posture: `35`
   - Deathblow Damage: `9999`
   - Hit Stop On Hit: `0.04`
   - Hit Stop On Parry: `0.10`
   - Hit Stop On Deathblow: `0.20`

### 3. ตั้งค่า Enemy Prefab(s)

เปิด `Assets/Prefabs/Agents/[Enemy].prefab` (ทุกตัวที่อยาก stagger ได้):

1. **Add Component → `Posture Manager`**
   - Max Posture: `60` (น้อยกว่า player → stagger ได้ง่ายกว่า)
   - Stagger Duration: `2.5`
   - Posture Bar: optional (สร้าง bar เล็กๆ บนหัว)

2. **Add Component → `Deathblow Marker`** (optional)
   - Indicator: drag GameObject ที่จะโชว์ตอน stagger (เช่น sprite สัญลักษณ์ "!")
   - ใน PostureManager event:
     - `On Posture Broken` → `DeathblowMarker.Show`
     - `On Stagger End` → `DeathblowMarker.Hide`

3. **Agent** fields ใหม่: ตั้งค่าเหมือน Player แต่ให้ enemy น้อยกว่า
   - Posture Damage On Hit: `10`
   - Parry Retaliation Posture: `0` (enemy parry ไม่ได้ใน Phase 1)

### 4. UI — Posture Bar

ใน Canvas ของ HUD:
- Duplicate "Health Bar" → rename "Posture Bar"
- เปลี่ยนสี (แนะนำ ส้ม → เหลือง → แดง gradient)
- ตำแหน่ง: ใต้ Health Bar
- Drag ลง `Posture Manager.Posture Bar` ของ Player

---

## 🎮 Gameplay Flow

```
Enemy โจมตี ── Agent.OnAttackAnimation
                │
                ▼
          ResolveHit(player collider)
                │
        ┌───────┴───────┐
        │               │
   IParriable?       fall through
        │
   Player กด parry ใน 0.18s ก่อนหน้า?
        │
   ┌────┴────┐
   YES        NO ── Player กด parry แต่พ้น window? (cooldown)
   │              │
   ▼              ▼
PERFECT         GUARD          OPEN HIT
posture ←       HP × 0.4       full HP
retaliate       posture +12    posture +18
HitStop 0.10s   HitStop 0.04s  HitStop 0.04s
```

เมื่อ posture เต็ม → IsStaggered = true (2 วินาที) → Player swing ใส่ = **DEATHBLOW** (9999 damage = ตายทันที + slow-mo)

---

## ✅ Quick Test Checklist

- [ ] กด attack ใส่ enemy → bar posture ของ enemy ขึ้น
- [ ] ตี enemy ติดต่อกันจนเต็ม → enemy หยุดนิ่ง 2 วิ + ! โผล่
- [ ] Swing ใส่ enemy ตอน stagger → ตายทันที + slow-mo
- [ ] กด parry ก่อน enemy ตี = enemy posture ขึ้น แทนที่ HP ผู้เล่นจะลด
- [ ] กด parry สาย = HP ลด 40% + posture player ขึ้น
- [ ] ไม่กดเลย = HP ลด 100% (เหมือนเดิม)

---

## ✅ Phase 2 (เพิ่ม)

- **AttackDataSO** — ScriptableObject กำหนด attack type/damage/telegraph
- **AttackTelegraph** — sprite flash + perilous indicator ก่อนปล่อย attack
- **MikiriDetector** — roll ใส่ thrust attack = counter (posture damage 60 + cancel attack)
- **DodgeController** — i-frames ตอน roll = ผ่าน sweep/crash ได้
- **AIEnemy refactor** — recursive coroutine → while loop (ฟิกซ์ memory bug + ใส่ telegraph flow)

### 4 Attack Types (AttackDataSO.AttackType)

| Type | Parry | Mikiri | Dodge | สีtelegraph |
|------|:-----:|:------:|:-----:|------------|
| `Normal` | ✅ | — | — | ขาว |
| `Perilous_Sweep` | ❌ | ❌ | ✅ ต้อง roll | แดง |
| `Perilous_Thrust` | ❌ | ✅ roll เข้า | — | แดง + ลูกศร |
| `Perilous_Crash` | ❌ | ❌ | ✅ ต้อง roll | แดง + crash |

## 🛠️ Editor Setup — Phase 2

### A. สร้าง AttackData assets

ใน `Assets/Data/Combat/` (สร้างโฟลเดอร์ใหม่):
- คลิกขวา → Create → Combat → Attack Data
- สร้างอย่างน้อย 4 อัน:
  - `Atk_Slash` — Normal, hpDamage 20, postureDmg 18, telegraph 0.35s
  - `Atk_Sweep` — Perilous_Sweep, hpDamage 30, postureDmg 25, telegraph 0.55s
  - `Atk_Thrust` — Perilous_Thrust, hpDamage 25, postureDmg 30, telegraph 0.50s
  - `Atk_Crash` — Perilous_Crash, hpDamage 40, postureDmg 35, telegraph 0.60s

### B. Player Prefab — เพิ่ม 2 component

1. **Add Component → `Mikiri Detector`**
   - Mikiri Posture Damage: `60`

2. **Add Component → `Dodge Controller`**
   - Dodge Duration: `0.30`
   - Cooldown: `0.15`

3. **PlayerInput** field ใหม่:
   - Mikiri Detector: drag self
   - Dodge Controller: drag self

### C. Enemy Prefab — เพิ่ม telegraph + attack pool

1. **Add Component → `Attack Telegraph`**
   - Sprite Renderer: drag enemy SpriteRenderer
   - Perilous Indicator: drag GameObject ที่เป็นสัญลักษณ์ "危" หรือ "!" (สร้างเป็น child ของ enemy)
   - Pulses: `3`

2. **AIEnemy** field ใหม่:
   - Attack Pool: drag AttackData assets ที่อยากให้ enemy นี้ใช้ (เช่น `Atk_Slash` 70%, `Atk_Sweep` 30% — random pick)
   - Telegraph: drag self

> **Tips**: Mini-boss / boss ควรมี attack pool ครบ 4 ชนิด. Goblin ปกติแค่ Normal 1 อันก็พอ

---

## 🧪 Phase 2 Test Checklist (เพิ่มจากเดิม)

- [ ] Enemy ติด Sweep attack → sprite flash แดงก่อนตี → ผู้เล่น **ห้ามกดอะไร = ตายเต็ม**
- [ ] Sweep มา → กด **roll ในจังหวะ telegraph** = ไม่โดนเลย (i-frame)
- [ ] Sweep มา → กด **parry** = ไม่ป้องกัน (โดนเต็ม) — เพราะ unparriable
- [ ] Thrust telegraph (สีแดง + ลูกศร) → กด **roll เข้าหา** = enemy stagger ทันที (Mikiri)
- [ ] Thrust ผ่าน Mikiri = ผู้เล่นไม่เสีย HP (cancel attack)
- [ ] Normal attack มา → parry ปกติ ใช้งานได้เหมือน Phase 1
- [ ] AIEnemy ตี → memory profiler ไม่มี IEnumerator allocations ทุกวินาที (recursive bug fix)

---

## ✅ Phase 3 (เพิ่ม — game feel polish)

- **CombatSfx** — AudioSource wrapper, fire SFX ตาม UnityEvent (parry/guard/dodge/mikiri/clean hit/deathblow/posture break/perilous hum)
- **CameraShakeOnHit** — wraps Cinemachine 2.10 `CinemachineImpulseSource`, มี Light/Medium/Heavy methods
- **MikiriArrowIndicator** — spawn arrow prefab เหนือหัว enemy ตอนกำลัง thrust
- **ComboCounter** — track player streak (decay 2.5 วิ), multiplier scaling 1x → 2.5x
- **AIEnemy.PickStrategy** — Random (default) / Sequential (boss combo cycling)
- **Agent UnityEvents** — `OnHitLanded` / `OnAttackParried` / `OnDeathblowDealt`
- **MikiriDetector.OnIncomingThrust** — UnityEvent broadcast attacker (สำหรับ arrow indicator)

### 🛠️ Editor Setup — Phase 3

#### A. เตรียม assets (Designer / Ant)

**Audio (8 clips, ใส่ใน `Assets/Audio/Combat/`):**
| ไฟล์ | ใช้ที่ไหน | hint |
|------|-----------|------|
| `parry_spark.wav` | perfect parry | metallic high "chink!" |
| `guard_thud.wav` | partial guard | low body thud |
| `dodge_swoosh.wav` | i-frame roll | wind whoosh |
| `mikiri_shing.wav` | counter success | sword shing + impact |
| `clean_hit.wav` | normal hit | flesh impact |
| `deathblow_impact.wav` | finisher | heavy crunch |
| `posture_break.wav` | stagger trigger | glass shatter |
| `perilous_hum.wav` | telegraph wind-up | ominous bass hum |

**VFX prefabs (ใน `Assets/Prefabs/VFX/`):**
- `MikiriArrow.prefab` — sprite arrow shape pointing down (จะ spawn เหนือหัว enemy)
- (Phase 1 ของเดิมก็ใช้: `ParrySpark.prefab`, `ParryStance.prefab` — ถ้ายังไม่ทำ)

#### B. Player Prefab

1. **Add Component → `Audio Source`** (ปิด Play On Awake)
2. **Add Component → `Combat Sfx`** — drag clip ทุกตัวลงใน Inspector
3. **Add Component → `Mikiri Arrow Indicator`** — drag MikiriArrow.prefab
4. **Add Component → `Combo Counter`** — link UI Text (optional)
5. **Add Component → `Cinemachine Impulse Source`** (จาก Cinemachine package)
6. **Add Component → `Camera Shake On Hit`**

#### C. Wire UnityEvents (ใน Inspector)

**Player.Agent:**
- `OnHitLanded` →
  - `ComboCounter.RegisterHit`
  - `CameraShakeOnHit.ShakeLight`
  - `CombatSfx.PlayCleanHit`
- `OnAttackParried` (รวมถึงตอน Player ตีโดน enemy parry — ใน Phase 1 enemy ยัง parry ไม่ได้, leave for boss)
- `OnDeathblowDealt` →
  - `CameraShakeOnHit.ShakeHeavy`
  - `CombatSfx.PlayDeathblow`

**Player.PlayerInput:**
- `OnParrySuccess` → `CombatSfx.PlayParry` + `CameraShakeOnHit.ShakeMedium`
- `OnDodgeSuccess` → `CombatSfx.PlayDodge`
- `OnMikiriSuccess` → `CombatSfx.PlayMikiri` + `CameraShakeOnHit.ShakeMedium`
- `OnBeingAttacked` → `ComboCounter.ResetCombo` + `CombatSfx.PlayGuard` (ถ้าใช้)

**Player.PostureManager:**
- `OnPostureBroken` → `CombatSfx.PlayPostureBreak`

**Player.MikiriDetector:**
- `OnIncomingThrust` → `MikiriArrowIndicator.ShowFor` (drag attacker param ใน UnityEvent slot)
- `OnMikiriSuccess` → `MikiriArrowIndicator.Hide`

**Enemy.AIEnemy** (Boss only):
- เปลี่ยน `Pick Strategy` เป็น **Sequential** + ใส่ attackPool ตามลำดับที่ต้องการ (เช่น Slash → Slash → Sweep → Thrust)

#### D. ComboCounter UI (optional)

ใน Canvas → สร้าง Text 2 อัน:
- "Combo Text" — drag ลง `Combo Text` ใน ComboCounter
- "Multiplier Text" — drag ลง `Multiplier Text`

---

## 🧪 Phase 3 Test Checklist

- [ ] Parry สำเร็จ → ได้ยินเสียง "chink!" + camera shake medium
- [ ] Mikiri counter → ได้ยิน "shing!" + camera shake medium + arrow หาย
- [ ] Deathblow → camera shake หนัก + เสียง "crunch" + slow-mo (Phase 1)
- [ ] ตี normal hit ติดกัน 5 ครั้ง → combo text "x5" + multiplier "1.50x"
- [ ] โดน enemy ตี → combo reset เป็น 0
- [ ] หยุดตี 2.5 วิ → combo decay เอง
- [ ] Thrust telegraph → arrow โผล่เหนือหัว enemy
- [ ] Boss enemy (Sequential) → ตี Slash → Slash → Sweep → Thrust ตามลำดับ ทุก loop เหมือนกัน
- [ ] Goblin (Random) → ตี attack สุ่มต่างกันแต่ละรอบ

---

## ✅ Phase 4 (เพิ่ม — Boss-grade AI Brain)

แทนที่ AI loop เดิมที่ "เข้าใส่ → ตี → รอ → ซ้ำ" ด้วย **Utility AI** ที่ตัดสินใจจริง: อ่านสถานะผู้เล่น, จัดการระยะ, punish whiff, feint parry-trigger-happy player, multi-enemy awareness, boss phase transitions

### Architecture (Combat/AI/)

```
PlayerActionTracker     ← record player attack/roll/parry/whiff timestamps + counts
PlayerPatternProfile    ← derives ParryFrequency / RollFrequency from tracker
CombatContext           ← struct snapshot (16 fields)
EnemyAction (abstract)  ← Score(ctx) + Execute(brain, ctx)
Actions/
  ├── Approach          ← walk toward target (score scales with distance)
  ├── Retreat           ← walk away (score: own posture danger + recent damage)
  ├── AttackNormal      ← parryable strike (score: in-range + whiff bonus)
  ├── AttackPerilous    ← Sweep/Thrust/Crash (score: target posture + roll-spam)
  ├── Feint             ← partial telegraph then cancel (score: target parry rate)
  └── HoldGround        ← stand & regen posture (score: own posture critical)
UtilityBrain            ← tick loop: snapshot → score → execute → cooldown → repeat
BossPhaseSO             ← per-phase config (HP threshold + actions/pool/aggression)
BossPhaseManager        ← swap phase when HP crosses thresholds
```

### Decision Flow (every tick ~0.08s)

```
Snapshot CombatContext from world
  │
  ├── Self: hp%, posture%, staggered, time since hit, allies near
  └── Target: hp%, posture%, staggered, last action timing, parry/roll rates, boss phase
  ↓
Score every action via Score(ctx) × weightMultiplier
  ↓
Filter: skip if cooldown active, skip if score < minScoreToConsider
  ↓
Pick highest score → Execute coroutine (yields seconds)
  ↓
Set cooldown end-time + brief postActionDelay → loop
```

### 🛠️ Editor Setup — Phase 4

#### A. Player Prefab — เพิ่ม 2 component

1. **Add Component → `Player Action Tracker`** (no fields needed)
2. **Add Component → `Player Pattern Profile`** (auto-fetches Tracker)

#### B. Wire UnityEvents (PlayerInput → Tracker)

ใน Player Prefab → PlayerInput component → ใน Inspector ทำ wiring:
- `OnAttack` → `PlayerActionTracker.OnAttack`
- `OnRoll` → `PlayerActionTracker.OnRoll`
- `OnParryPressed` → `PlayerActionTracker.OnParryPressed`
- `OnParrySuccess` → `PlayerActionTracker.OnParrySuccess`
- `OnBeingAttacked` → `PlayerActionTracker.OnBeingAttacked`

ใน Player Prefab → **Agent** component:
- `OnAttackWhiffed` → `PlayerActionTracker.OnAttackWhiff`

#### C. สร้าง Action assets

ใน `Assets/Data/AI/` (สร้างโฟลเดอร์ใหม่):
- คลิกขวา → Create → Combat/AI/Action: Approach → ตั้งชื่อ `Action_Approach_Default`
- ทำซ้ำสำหรับทุก action: Retreat / AttackNormal / AttackPerilous / Feint / HoldGround
- ปรับค่า weight, cooldown, thresholds ตามชอบ

#### D. Enemy Prefab — เพิ่ม UtilityBrain

1. **Add Component → `Utility Brain`**
   - Actions: drag SO assets ที่ enemy นี้รู้จัก (เช่น Goblin = Approach + AttackNormal; Boss = ทุกอย่าง)
   - Decision Tick Interval: `0.08`
   - Post Action Delay: `0.2`
   - Allies Scan Radius: `6`
   - Allies Layer: `Enemies` layer

2. **AIEnemy** field ใหม่:
   - Utility Brain: drag self (จะ auto-fetch ถ้าไม่ใส่)

3. **PostureManager** event ใหม่ (ถ้าอยากให้ brain ฉลาดเรื่อง hit reactions):
   - `OnPostureBroken` → `UtilityBrain.NotifyTookHit`

#### E. Boss Prefab — เพิ่ม BossPhaseManager

1. สร้าง BossPhase SO assets ใน `Assets/Data/AI/Bosses/[BossName]/`:
   - `Phase1_Normal.asset` — hpThreshold 1.0, actions = standard pool, aggressionMultiplier 1.0
   - `Phase2_Aggressive.asset` — hpThreshold 0.5, actions += Feint + AttackPerilous, aggressionMultiplier 0.6
   - `Phase3_Desperate.asset` — hpThreshold 0.25, actions = perilous-heavy, telegraphSpeedMultiplier 0.7

2. **Add Component → `Boss Phase Manager`**
   - Phases: drag 3 SO assets ตามลำดับ (Phase1 → Phase2 → Phase3)
   - Audio Source: optional (เล่นเสียงตอนเปลี่ยน phase)

---

## 🧪 Phase 4 Test Checklist

### Single Goblin (basic AI)
- [ ] Goblin ไกลจาก player → score Approach สูง → เดินเข้า
- [ ] อยู่ในระยะ → score AttackNormal สูง → ตี
- [ ] Player attack แล้วพลาด (whiff) → goblin ตีกลับทันที (whiff punish)
- [ ] โดน player ตี posture > 60% → goblin เลือก Retreat / HoldGround แทน push

### Boss (with full pool + phases)
- [ ] HP > 50% → ใช้ Approach + AttackNormal + Retreat (มาตรฐาน)
- [ ] HP < 50% → unlocked Feint + AttackPerilous (Sweep/Thrust)
- [ ] HP < 25% → telegraph เร็วขึ้น (telegraphSpeedMultiplier 0.7), อะกรอกขึ้น
- [ ] Player parry บ่อย (rate > 0.5) → boss เริ่ม Feint หลอกบ่อย
- [ ] Player roll บ่อย → boss เลือก Sweep / Thrust แทน Slash บ่อย
- [ ] Boss posture > 70% → boss เลือก HoldGround เพื่อ regen

### Multi-enemy (3 goblins)
- [ ] 3 goblins → ตัวที่ใกล้สุดตี, ตัวที่อยู่ไกลกว่า score Retreat สูง (alliesNearby > 0 → ไม่ต้องรุม)

---

## 🎯 Done — full Sekiro stack + boss-grade AI

| Layer | P1 | P2 | P3 | P4 |
|-------|:---:|:---:|:---:|:---:|
| Posture / Parry / Deathblow / Hit-stop | ✅ | | | |
| Perilous attacks / Mikiri / Dodge i-frames / Telegraph | | ✅ | | |
| Camera shake / SFX / Combo / Mikiri arrow / Boss patterns | | | ✅ | |
| **Player tracking + pattern profile** | | | | ✅ |
| **Utility AI brain (6 scored actions)** | | | | ✅ |
| **Boss phase transitions (HP tiers)** | | | | ✅ |
| **Whiff punish + Feint + Multi-enemy awareness** | | | | ✅ |

---

## 🐛 Known Caveats

- **`HitStop` ใช้ `Time.timeScale`** — ระวังถ้ามี physics simulation ที่ไม่ใช่ FixedUpdate-friendly. ส่วนใหญ่ Unity 2D OK
- **Parry window 0.18s** เริ่มต้น — ปรับให้กว้างขึ้น (0.25–0.30) ถ้ารู้สึกหินไป
- **Enemy ยัง parry ไม่ได้** — ถ้าจะทำ "boss สามารถ parry ได้" ให้ implement `IParriable` บน AIEnemy
- **Deathblow ใช้ damage 9999** ไม่ใช่ insta-kill animation จริง — Phase 3 ค่อยทำ animation
