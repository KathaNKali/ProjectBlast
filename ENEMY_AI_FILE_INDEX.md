# Enemy AI Documentation Index

All documentation for setting up enemy movement using TDE's AIBrain system.

---

## 📚 Documentation Files

### **1. QUICK_START_ENEMY_MOVEMENT.md** ⭐ START HERE
**Purpose:** Get enemies moving in 15 minutes
**Target Audience:** Developers who want fast implementation
**Content:**
- Step-by-step checklist
- Minimal explanation
- Quick verification tests
- Troubleshooting section

**Use this if:** You want to get it working ASAP

---

### **2. ENEMY_AI_SETUP.md** 📖 DETAILED GUIDE
**Purpose:** Comprehensive understanding of the system
**Target Audience:** Developers who want to learn how it works
**Content:**
- Full explanation of TDE AI components
- Detailed component configuration
- Inspector setup instructions
- Runtime flow explanation
- Future enhancement roadmap

**Use this if:** You want to understand the architecture deeply

---

### **3. ENEMY_AI_VISUAL_GUIDE.md** 🎨 VISUAL REFERENCE
**Purpose:** Quick visual reference for component hierarchy
**Target Audience:** Visual learners, quick lookup
**Content:**
- Component hierarchy diagrams
- Inspector view layouts (text-based)
- Scene layout visualization
- Configuration cheat sheet
- Common mistakes list

**Use this if:** You prefer visual diagrams and quick reference

---

### **4. ENEMY_AI_IMPLEMENTATION_SUMMARY.md** 📝 OVERVIEW
**Purpose:** High-level summary of what was implemented
**Target Audience:** Project managers, leads, returning developers
**Content:**
- What was built
- Files modified
- How it works (runtime flow)
- TDE components used
- Future roadmap
- Testing scenarios

**Use this if:** You need an overview or are catching up on the project

---

## 🚀 Recommended Reading Order

### **For First-Time Implementation:**
1. Read: `ENEMY_AI_IMPLEMENTATION_SUMMARY.md` (5 min)
   - Get high-level understanding
2. Follow: `QUICK_START_ENEMY_MOVEMENT.md` (15 min)
   - Implement step-by-step
3. Reference: `ENEMY_AI_VISUAL_GUIDE.md` (as needed)
   - Check component hierarchy while configuring
4. Deep Dive: `ENEMY_AI_SETUP.md` (optional, 20 min)
   - Understand why things work this way

**Total Time:** ~20 minutes to working implementation

---

### **For Troubleshooting:**
1. Check: `QUICK_START_ENEMY_MOVEMENT.md` → Troubleshooting section
2. Verify: `ENEMY_AI_VISUAL_GUIDE.md` → Verification Checklist
3. Understand: `ENEMY_AI_SETUP.md` → How It Works section

---

### **For Extending the System:**
1. Read: `ENEMY_AI_SETUP.md` → Future Enhancements
2. Reference: `ENEMY_AI_IMPLEMENTATION_SUMMARY.md` → Future Enhancements
3. Study: TDE documentation → AI Actions & Transitions

---

## 🎯 Quick Reference

### **Enemy Prefab Setup Checklist:**
```
Root GameObject:
  ✓ Character (AI, Type3D)
  ✓ TopDownController3D (Speed: 3.0)
  ✓ CharacterMovement
  ✓ Health (100 HP)
  ✓ Capsule Collider
  ✓ Rigidbody (Gravity OFF, Rotation Frozen)
  ✓ Layer: Enemy, Tag: Enemy

AIBrain Child:
  ✓ AI Brain component
  ✓ State "MoveToBase"
  ✓ Action: AIActionMoveTowardsTarget3D
  ✓ Current State: MoveToBase
```

### **Scene Setup Checklist:**
```
✓ PlayerBase at (0, 0, -8)
✓ PlayerBase tag: "PlayerBase"
✓ PlayerBase has Health component
✓ Spawner at (0, 0, +20) or has Spawn Center there
✓ Spawner has Enemy_00 prefab assigned
✓ Spawner Debug Mode enabled
```

---

## 📋 File Locations

**Documentation:**
```
/QUICK_START_ENEMY_MOVEMENT.md
/ENEMY_AI_SETUP.md
/ENEMY_AI_VISUAL_GUIDE.md
/ENEMY_AI_IMPLEMENTATION_SUMMARY.md
/ENEMY_AI_FILE_INDEX.md (this file)
```

**Code:**
```
/Assets/ProjectBlast/Scripts/Enemy/SimpleEnemySpawner.cs
```

**Assets:**
```
/Assets/ProjectBlast/Prefab/Enemy_00.prefab (needs configuration)
/Assets/ProjectBlast/Scenes/GameScene.unity (needs PlayerBase)
```

---

## 🐛 Common Issues & Solutions

### **Issue: Enemy doesn't move**
**Quick Fix:**
1. Check: `QUICK_START_ENEMY_MOVEMENT.md` → Troubleshooting
2. Verify: AIBrain.Target is set (not null)
3. Check: CharacterMovement ability exists

### **Issue: Console shows "Could not find PlayerBase"**
**Quick Fix:**
1. Create PlayerBase at (0, 0, -8)
2. Set Tag to "PlayerBase"
3. Restart Play mode

### **Issue: Enemy moves wrong direction**
**Quick Fix:**
1. Check Spawn Center is at Z = +20 (positive)
2. Check PlayerBase is at Z = -8 (negative)
3. Enemy should move from higher Z to lower Z

---

## 🔗 Related Documentation

**Game Architecture:**
- `/GAME_ARCHITECTURE.md` - Overall game structure
- `/GAME_DEVELOPMENT_PLAN.md` - Project roadmap
- `/GAME_LAYOUT_DESIGN.md` - Visual layout and zones

**TDE Integration:**
- `/TDE_INTEGRATION_GUIDE.md` - How we use TDE systems
- `/Documentation/HERO_AIBRAIN_INTEGRATION.md` - Hero AI (similar pattern)

---

## 📊 Documentation Stats

| File | Purpose | Lines | Audience | Time to Read |
|------|---------|-------|----------|--------------|
| QUICK_START_ENEMY_MOVEMENT.md | Fast implementation | ~200 | Implementers | 5 min read + 15 min do |
| ENEMY_AI_SETUP.md | Deep understanding | ~300 | Learners | 20 min |
| ENEMY_AI_VISUAL_GUIDE.md | Visual reference | ~250 | Visual learners | 10 min |
| ENEMY_AI_IMPLEMENTATION_SUMMARY.md | Project overview | ~250 | Leads/PMs | 10 min |
| ENEMY_AI_FILE_INDEX.md | Navigation | ~150 | Everyone | 5 min |

**Total Documentation:** ~1,150 lines across 5 files

---

## ✅ What's Next After Enemy Movement Works?

### **Phase 4A: Enemy Attack (Immediate)**
1. Create `AIActionDamageBase.cs` script
2. Add "AttackBase" state to enemy
3. Add transition from MoveToBase → AttackBase when distance < 1.5m
4. Test enemy damaging base

**Estimated Time:** 30 minutes
**Documentation:** Will create when implementing

### **Phase 4B: Enemy Variety (Short-term)**
1. Duplicate Enemy_00 → Enemy_Fast (speed 6, HP 50)
2. Duplicate Enemy_00 → Enemy_Tank (speed 2, HP 300)
3. Update spawner to spawn random enemy types

**Estimated Time:** 20 minutes

### **Phase 4C: Wave System (Medium-term)**
1. Create WaveManager.cs
2. Define wave data structures
3. Coordinate spawning across waves
4. Add stage progression

**Estimated Time:** 2-3 hours

---

## 🎓 Learning Path

**Beginner (Just Get It Working):**
1. Read: ENEMY_AI_IMPLEMENTATION_SUMMARY.md
2. Do: QUICK_START_ENEMY_MOVEMENT.md
3. Test: Verify enemies move
4. Done! ✅

**Intermediate (Understand the System):**
1. Read: ENEMY_AI_IMPLEMENTATION_SUMMARY.md
2. Read: ENEMY_AI_SETUP.md (skim technical details)
3. Do: QUICK_START_ENEMY_MOVEMENT.md
4. Reference: ENEMY_AI_VISUAL_GUIDE.md while configuring
5. Extend: Add your own AI state

**Advanced (Master TDE AI):**
1. Read: All 4 docs completely
2. Study: TDE source code (AIBrain, AIAction base classes)
3. Implement: Custom AIAction and AIDecision scripts
4. Extend: Complex state machines with multiple transitions

---

## 💡 Pro Tips

1. **Always enable Debug Mode** on spawner during development
   - See exactly what's happening in console
   - Easy to spot configuration issues

2. **Test with 1 enemy first**
   - Set Spawn Count = 1
   - Verify movement works
   - Then increase to 3, 5, 10

3. **Use Scene View during Play Mode**
   - Select spawned enemy
   - Watch AIBrain → Target field
   - See Transform position update in real-time

4. **Keep Enemy_00 as base template**
   - Duplicate it for variants (Fast, Tank, etc.)
   - Don't modify original, always duplicate

5. **Name your states clearly**
   - "MoveToBase" not "State1"
   - "AttackBase" not "Combat"
   - Makes debugging much easier

---

## 📞 Support / Questions

**If you're stuck:**
1. Check Troubleshooting sections in docs
2. Verify all checklist items
3. Enable Debug Mode and read console output
4. Compare your setup to Visual Guide diagrams

**Common Questions:**

**Q: Do enemies need WeaponAttachment like heroes?**
A: No, basic movement doesn't need weapons. Add later for ranged enemies.

**Q: Should enemies use AIDecisionDetectTarget?**
A: Not needed for basic movement. Target is set by spawner. Useful later for combat AI.

**Q: Can I use NavMeshAgent instead of AIActionMoveTowardsTarget3D?**
A: Yes, but start simple. Add NavMesh later if you need obstacle avoidance.

**Q: Why use AIBrain instead of custom movement script?**
A: Extensibility. Easy to add states (attack, patrol, flee) via Inspector without code changes.

---

**Documentation Version:** 1.0
**Last Updated:** December 30, 2025
**Status:** Complete, ready for implementation
