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

## 🚧 ที่ยังไม่มี (Phase 2/3)

- Perilous attacks (สีแดง telegraph) — ต้องเพิ่ม `AttackDataSO` + แก้ `AIEnemy.ChaseAndAttack` ให้ broadcast attack type
- Mikiri counter (roll into thrust)
- Sweep dodge requirement
- Camera shake (Cinemachine Impulse Source)
- Sound effects (parry "chink!", guard "thud", deathblow whoosh)
- Aggressive momentum reward (combo bonus)

---

## 🐛 Known Caveats

- **`HitStop` ใช้ `Time.timeScale`** — ระวังถ้ามี physics simulation ที่ไม่ใช่ FixedUpdate-friendly. ส่วนใหญ่ Unity 2D OK
- **Parry window 0.18s** เริ่มต้น — ปรับให้กว้างขึ้น (0.25–0.30) ถ้ารู้สึกหินไป
- **Enemy ยัง parry ไม่ได้** — ถ้าจะทำ "boss สามารถ parry ได้" ให้ implement `IParriable` บน AIEnemy
- **Deathblow ใช้ damage 9999** ไม่ใช่ insta-kill animation จริง — Phase 3 ค่อยทำ animation
