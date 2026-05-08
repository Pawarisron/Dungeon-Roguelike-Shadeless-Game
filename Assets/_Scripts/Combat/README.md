# Sekiro-Style Combat — Phase 1 MVP

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

## 🚧 ที่ยังไม่มี (Phase 3)

- Camera shake (Cinemachine Impulse Source — package ติดตั้งอยู่แล้ว)
- Sound effects (parry "chink!", guard "thud", deathblow whoosh, telegraph hum)
- Mikiri arrow indicator (Sekiro มีลูกศรเหนือหัว)
- VFX particle prefabs (parry spark, sweep wind, deathblow burst)
- Aggressive momentum reward (combo counter)
- Boss-specific attack patterns (3-hit string, delayed strike)

---

## 🐛 Known Caveats

- **`HitStop` ใช้ `Time.timeScale`** — ระวังถ้ามี physics simulation ที่ไม่ใช่ FixedUpdate-friendly. ส่วนใหญ่ Unity 2D OK
- **Parry window 0.18s** เริ่มต้น — ปรับให้กว้างขึ้น (0.25–0.30) ถ้ารู้สึกหินไป
- **Enemy ยัง parry ไม่ได้** — ถ้าจะทำ "boss สามารถ parry ได้" ให้ implement `IParriable` บน AIEnemy
- **Deathblow ใช้ damage 9999** ไม่ใช่ insta-kill animation จริง — Phase 3 ค่อยทำ animation
