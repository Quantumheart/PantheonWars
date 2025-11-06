# Perk Stat Application Implementation

**Date**: 2025-10-24
**Status**: ✅ IMPLEMENTED - Ready for Testing

## Overview

Implemented the critical `ApplyPerksToPlayer()` functionality in `PerkEffectSystem.cs` using the XSkills pattern. Perks can now actually affect player stats in-game.

---

## What Was Implemented

### 1. Core Stat Application (`ApplyPerksToPlayer`)

**Location**: `PantheonWars/Systems/PerkEffectSystem.cs:165-229`

**Key Features:**
- Uses Vintage Story's `entity.Stats.Set()` API (XSkills pattern)
- Applies combined player + religion perk modifiers
- Tracks applied modifiers for cleanup
- Proper error handling with logging
- Follows "deferred update" pattern (`false` parameter)

**Implementation Pattern:**
```csharp
agent.Stats.Set(statName, modifierId, value, false);
// statName: VS stat name (e.g., "walkspeed")
// modifierId: Namespaced ID (e.g., "perk-playerUID")
// value: Modifier value (e.g., 0.15 for +15%)
// false: Deferred update (per XSkills best practice)
```

### 2. Modifier Cleanup (`RemovePerksFromPlayer`)

**Location**: `PantheonWars/Systems/PerkEffectSystem.cs:234-269`

**Key Features:**
- Removes old modifiers before applying new ones
- Prevents stat modifier accumulation
- Tracks which stats were modified per player
- Clean removal using `agent.Stats.Remove()`

### 3. Stat Name Mapping

**Location**: `PantheonWars/Systems/PerkEffectSystem.cs:275-303`

Maps custom perk stat names to Vintage Story's actual stat names:

| Perk Stat Name | VS Stat Name | Effect |
|----------------|--------------|--------|
| `melee_damage` | `meleeWeaponsDamage` | Increases melee damage |
| `ranged_damage` | `rangedWeaponsDamage` | Increases ranged damage |
| `attack_speed` | `meleeWeaponsSpeed` | Increases attack speed |
| `armor` | `meleeWeaponArmor` | Increases armor |
| `max_health` | `maxhealthExtraPoints` | Adds max health points |
| `walk_speed` | `walkspeed` | Increases movement speed |
| `health_regen` | `healingeffectivness` | Improves healing |

**Legacy Format Support:** Also supports camelCase names like `meleeDamageMultiplier` for backward compatibility with existing perk definitions.

### 4. Applied Modifier Tracking

**New Field**: `Dictionary<string, HashSet<string>> _appliedModifiers`

Tracks which stats have modifiers applied per player, enabling:
- Proper cleanup on perk changes
- Refresh without duplication
- Debugging visibility

---

## Code Changes

### Files Modified

1. **PantheonWars/Systems/PerkEffectSystem.cs**
   - Added `using Vintagestory.API.Common` and `Vintagestory.API.Common.Entities`
   - Added `_appliedModifiers` tracking dictionary
   - Implemented `ApplyPerksToPlayer()` (65 lines)
   - Implemented `RemovePerksFromPlayer()` (36 lines)
   - Implemented `MapToVintageStoryStat()` (29 lines)

### Build Status

✅ **Build Successful**
- 0 Errors
- 23 Warnings (nullable reference types - non-critical)

---

## How It Works

### Player Join Flow

```
1. Player joins server
   └─ OnPlayerJoin event triggered
      └─ RefreshPlayerPerks(playerUID) called
         └─ ApplyPerksToPlayer(player) called
            ├─ GetCombinedStatModifiers() - Calculate all modifiers
            ├─ RemovePerksFromPlayer() - Clear old modifiers
            ├─ Loop through modifiers:
            │  ├─ MapToVintageStoryStat() - Get VS stat name
            │  ├─ agent.Stats.Set(stat, id, value, false)
            │  └─ Track applied stats
            └─ Log results
```

### Perk Unlock Flow

```
1. Player unlocks perk via /perks unlock
   └─ PerkCommands adds to UnlockedPerks
      └─ RefreshPlayerPerks() called
         └─ ApplyPerksToPlayer() called
            └─ New modifiers applied
```

### Religion Join/Leave Flow

```
1. Player joins/leaves religion
   └─ PlayerReligionDataManager updates ReligionUID
      └─ RefreshPlayerPerks() called
         └─ ApplyPerksToPlayer() called
            └─ Religion perks added/removed
```

---

## Testing Instructions

### 1. Basic Stat Application Test

```
1. Start server with mod loaded
2. Join as player
3. Create a religion: /religion create TestReligion Khoras public
4. Use creative mode to test quickly (if available)
5. Check server logs for:
   - "Applied X perk modifiers to player [name]"
   - Individual stat applications: "Applied walkspeed: perk-[uid] = 0.150"
```

### 2. Verify Stat Effects In-Game

**Test Walk Speed Perk:**
```
1. Get baseline: Walk and observe speed
2. Unlock a walk speed perk (if any exist)
3. Walk again and observe increased speed
4. Check: /perks active (should show walk_speed modifier)
```

**Test Health Perk:**
```
1. Note current max health (F3 debug info or /health command)
2. Unlock a max health perk
3. Check max health again (should be higher)
```

**Test Damage Perk:**
```
1. Spawn a mob
2. Attack and note damage dealt
3. Unlock a melee damage perk
4. Attack same mob type - should deal more damage
```

### 3. Perk Refresh Test

```
1. Unlock a perk
2. Leave religion: /religion leave
3. Join a different religion: /religion join OtherReligion
4. Check logs - should see:
   - "Removed X old perk modifiers"
   - "Applied Y perk modifiers"
5. Stats should reflect only new religion's perks
```

### 4. Debug Commands

```bash
# Check active perks
/perks active

# View player's perk tree
/perks tree player

# View religion's perk tree
/perks tree religion

# Check religion info
/religion info
```

---

## Known Limitations & Next Steps

### ⚠️ Current Limitations

1. **Stat Name Uncertainty**
   - Some VS stat names are assumptions based on XSkills
   - May need adjustment based on actual in-game testing
   - Try-catch blocks will log warnings for invalid stats

2. **Special Effects Not Implemented**
   - Perks with special effects (lifesteal, poison_dot, critical_strike) are defined but **not functional**
   - Only stat modifiers work currently
   - Special effects need separate handler system

3. **No Buff System Yet**
   - Temporary buffs from abilities not implemented
   - Would require BuffBehavior system (see BUFF_IMPLEMENTATION_GUIDE.md)

### 🔄 Recommended Next Steps

1. **In-Game Testing** (High Priority)
   - Test each stat modifier type
   - Verify actual stat names work
   - Adjust mapper if needed

2. **Implement Special Effect Handlers** (Phase 3.4 Task 4.3)
   - Create handler system for complex effects
   - Implement lifesteal (Khoras)
   - Implement poison DoT (Morthen)
   - Implement critical strikes (Lysa)

3. **Balance Testing** (Phase 3.4 Task 4.4)
   - Once effects work, test perk values
   - Ensure no deity is overpowered
   - Adjust modifier percentages

4. **Complete Remaining Deity Perks** (Phase 3.4)
   - Aethra (Light) - 20 perks
   - Umbros (Shadows) - 20 perks
   - Tharos (Storms) - 20 perks
   - Gaia (Earth) - 20 perks
   - Vex (Madness) - 20 perks

---

## Troubleshooting

### Issue: Stats not applying

**Check:**
1. Server logs for warnings about stat names
2. Player actually has unlocked perks: `/perks player`
3. Player is in a religion: `/religion info`
4. Logs show "Applied X perk modifiers"

**Solution:** If stat name warnings appear, update `MapToVintageStoryStat()` with correct VS stat names.

### Issue: Stats apply but no visible effect

**Check:**
1. Modifier values are reasonable (0.15 = +15%)
2. VS stat actually affects what you expect
3. Game systems use the stat (some may be unused)

**Solution:** Test with larger values (e.g., 1.0 = +100%) to see if effect is more obvious.

### Issue: Stats accumulate/don't refresh

**Check:**
1. `RemovePerksFromPlayer()` is being called
2. Logs show "Removed X old perk modifiers"
3. `_appliedModifiers` tracking is working

**Solution:** Check if modifier ID is consistent and removal is working.

---

## Technical Details

### XSkills Pattern Compliance

✅ Uses `entity.Stats.Set(statName, modifierId, value, false)`
✅ Uses `entity.Stats.Remove(statName, modifierId)`
✅ Namespaced modifier IDs (`"perk-{playerUID}"`)
✅ Deferred updates (`false` parameter)
✅ Server-side only (via `IServerPlayer` parameter)
✅ Proper cleanup before reapplication

### Vintage Story Stats API

The implementation uses VS's stat blending system:
- Multiple modifiers can affect the same stat
- Each has a unique ID for tracking
- VS automatically blends all modifiers
- `GetBlended()` returns final value
- Modifiers are additive by default

### Performance Considerations

**Caching:**
- Modifier calculations cached per player/religion
- Cache cleared on perk unlock/religion change
- Reduces repeated dictionary lookups

**Update Frequency:**
- Only on perk change events (not per-tick)
- Efficient for large player counts
- Stats automatically update via VS system

---

## References

- **XSkills Analysis**: `docs/XSKILLS_ANALYSIS.md`
- **Buff Implementation Guide**: `docs/BUFF_IMPLEMENTATION_GUIDE.md`
- **XSkills Summary**: `docs/XSKILLS_EXPLORATION_SUMMARY.md`
- **Phase 3 Task Breakdown**: `docs/phase3_task_breakdown.md`

---

## Conclusion

The perk stat application system is now **functional and ready for testing**. The 60 existing perks (Khoras, Lysa, Morthen) should now actually affect player stats in-game.

**Critical Gap Resolved**: ✅ `ApplyPerksToPlayer()` is no longer a placeholder

**Next Priority**: In-game testing to verify stat names and effects, then implementing special effect handlers for complex perk mechanics.
