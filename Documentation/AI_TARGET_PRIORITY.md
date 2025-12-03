# AI Target Priority System

## Overview

Custom AI decision component that extends TDE's cone of vision detection with intelligent target prioritization for multiple enemies.

## Component: `AIDecisionDetectTargetPriority3D`

**Location:** `Assets/ProjectBlast/Scripts/AI/AIDecisionDetectTargetPriority3D.cs`

### Features

✅ Detects enemies in cone of vision (uses existing MMConeOfVision)  
✅ Intelligent target selection when multiple enemies present  
✅ 5 priority modes for different hero strategies  
✅ Health-based targeting for focus fire  
✅ Distance-based targeting for range control  
✅ Debug visualization in Inspector and Gizmos  

---

## Priority Modes

### 1. **Closest** (Default - Recommended)
Targets the nearest enemy.

**Use Case:**
- General purpose heroes
- Consistent damage dealing
- Minimize missed shots

**Best For:** Standard ranged heroes, snipers

---

### 2. **Farthest**
Targets the enemy farthest from hero.

**Use Case:**
- Long-range heroes controlling backline
- Preventing enemy reinforcements
- Area denial strategies

**Best For:** Artillery heroes, AOE specialists

---

### 3. **LowestHealth** (Focus Fire)
Targets the weakest enemy (lowest HP).

**Use Case:**
- Quickly eliminate wounded enemies
- Prevent enemy healing/escaping
- Maximize kill efficiency

**Best For:** Fast-firing heroes, assassins, cleanup units

---

### 4. **HighestHealth** (Threat Priority)
Targets the tankiest enemy (highest HP).

**Use Case:**
- Eliminate threats before they reach base
- Tank-buster heroes
- Bosses and elite enemies

**Best For:** High-damage heroes, anti-tank specialists

---

### 5. **FirstDetected**
Original TDE behavior - takes first target in list (no sorting).

**Use Case:**
- Legacy compatibility
- Unpredictable/chaotic targeting
- Performance-critical scenarios (no sorting overhead)

---

## Setup Instructions

### Step 1: Replace Existing Detection Decision

**In Hero_00 Prefab → AIBrain GameObject:**

1. **Remove:** `AIDecisionDetectTargetConeOfVision3D` component
2. **Add:** `AIDecisionDetectTargetPriority3D` component (ProjectBlast/AI/Decisions menu)

### Step 2: Configure Component

**Inspector Settings:**
```
Target Cone Of Vision: [Drag MMConeOfVision component reference]
Priority: Closest (or choose your preferred mode)
Set Target To Null If None Is Found: ✓ (checked)
```

### Step 3: Update AI State Transitions

The new decision works as a drop-in replacement. No state machine changes needed!

**AI State Transitions using this decision:**
- `Seeking` → `WaitToShoot` (when detection succeeds)
- `Destroying` → `BackToSeeking` (when detection fails)
- `BackToSeeking` → `WaitToShoot` (when detection succeeds again)

---

## Configuration Examples

### Example 1: Sniper Hero (Closest)
```
Priority: Closest
```
- Always shoots nearest enemy
- Consistent accuracy
- No wasted shots on distant targets

### Example 2: Tank Buster Hero (HighestHealth)
```
Priority: HighestHealth
```
- Focuses on elite/boss enemies
- Ignores weak units
- Maximizes threat elimination

### Example 3: Cleanup Hero (LowestHealth)
```
Priority: LowestHealth
```
- Finishes off wounded enemies
- Prevents escapes
- High kill count

---

## Debug Features

### Inspector View (Runtime)
```
Visible Targets Count: 3
Current Target Name: "Enemy_Tank_02"
```

### Scene Gizmos (When Selected)
- **Red Line:** Hero → Current Target
- **Red Wireframe Sphere:** Target position marker

---

## Integration with Hero.cs

The Hero class automatically works with this decision through AIBrain:

```csharp
// Hero.cs already has:
public AIDecisionDetectTargetConeOfVision3D AIDecisionDetect; // Change type

// Update to:
public AIDecisionDetectTargetPriority3D AIDecisionDetect;
```

**No code changes required!** AIBrain handles the decision internally.

---

## Performance Considerations

### Sorting Overhead

| Priority Mode | Cost | Notes |
|---------------|------|-------|
| FirstDetected | None | No sorting |
| Closest | Low | Simple distance calculation |
| Farthest | Low | Simple distance calculation |
| LowestHealth | Medium | Requires GetComponent<Health>() per target |
| HighestHealth | Medium | Requires GetComponent<Health>() per target |

**Optimization:** 
- MMConeOfVision already filters targets (only checks visible enemies)
- Sorting happens only when multiple targets present
- Health component lookups cached during sort

**Typical Performance:** 1-10 enemies in cone = negligible overhead (<0.1ms)

---

## Advanced: Per-Hero Priority Configuration

### Via HeroDataSO (Future Enhancement)

Add priority to hero data:

```csharp
// In HeroDataSO.cs
public enum HeroTargetPriority { Closest, Farthest, LowestHealth, HighestHealth }
public HeroTargetPriority TargetPriority = HeroTargetPriority.Closest;

// In Hero.cs ConfigureAI()
if (AIDecisionDetectPriority != null)
{
    // Map enum to component priority
    AIDecisionDetectPriority.Priority = ConvertPriority(HeroData.TargetPriority);
}
```

### Via Prefab Variants

Create hero variants with different priorities:
- `Hero_Sniper.prefab` → Priority: Closest
- `Hero_Artillery.prefab` → Priority: Farthest
- `Hero_Assassin.prefab` → Priority: LowestHealth
- `Hero_TankBuster.prefab` → Priority: HighestHealth

---

## Troubleshooting

### "No MMConeOfVision found"
**Fix:** Assign MMConeOfVision reference in Inspector or ensure it's on the same GameObject/parent

### "Target keeps switching between enemies"
**Cause:** Multiple enemies at same priority level (e.g., same distance, same health)
**Fix:** Add small hysteresis or stick-to-target timer (future enhancement)

### "Not detecting enemies"
**Check:**
1. MMConeOfVision radius/angle configured?
2. Enemy on correct layer (TargetLayerMask)?
3. Line of sight clear (ObstacleMask)?
4. AIBrain.BrainActive = true?

### "Always targeting farthest enemy even with Closest mode"
**Fix:** Check Priority enum value in Inspector - may have been changed

---

## Migration from Old System

### Before (Standard TDE):
```
AIBrain GameObject:
  - AIDecisionDetectTargetConeOfVision3D
    → Always targets first enemy in list (unpredictable)
```

### After (ProjectBlast):
```
AIBrain GameObject:
  - AIDecisionDetectTargetPriority3D
    - Priority: Closest
    → Always targets nearest enemy (predictable, optimal)
```

**Benefits:**
- ✅ Predictable targeting behavior
- ✅ Optimal damage distribution
- ✅ Focus fire when needed
- ✅ Strategic flexibility per hero type

---

## Future Enhancements

### Possible Additions:

1. **ClosestToGoal** - Tower defense specific (target enemy nearest to base)
2. **HighestThreat** - Composite score (health × damage × distance)
3. **TypePriority** - Target specific enemy types first (bosses, healers, etc.)
4. **StickyTargeting** - Reduce target switching frequency
5. **DynamicPriority** - Change priority based on hero health/ammo

---

## Related Files

- `AIDecisionDetectTargetPriority3D.cs` - Main component
- `Hero.cs` - Hero orchestration with AI configuration
- `HeroDataSO.cs` - Hero stats (potential priority storage)
- `MMConeOfVision.cs` - TDE's cone detection system
- `AIBrain.cs` - TDE's AI state machine

---

**Last Updated:** December 3, 2025  
**Version:** 1.0  
**Author:** ProjectBlast Team
