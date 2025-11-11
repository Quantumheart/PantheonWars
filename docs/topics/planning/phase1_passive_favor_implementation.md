# Phase 1: Passive Favor Implementation

## Implementation Date
November 11, 2025

## Overview
Phase 1 implements the core passive favor generation system as outlined in `passive_favor_integration_plan.md`. This allows players to passively gain favor over time while online, with multipliers based on devotion rank and religion prestige rank.

## Changes Made

### 1. PlayerDeityData.cs
**File:** `/home/user/PantheonWars/PantheonWars/Data/PlayerDeityData.cs`

#### Added Fields:
- `AccumulatedFractionalFavor` (ProtoMember 10) - Tracks fractional favor that hasn't yet reached 1.0

#### Added Methods:
- `AddFractionalFavor(float amount)` - Accumulates fractional favor and awards integer favor when >= 1.0

**Why Fractional Accumulation?**
With a rate of 0.04 favor per second, fractional amounts need to be tracked to prevent loss. The system accumulates these fractional amounts and awards integer favor when the total reaches 1.0, preserving the remainder for future accumulation.

### 2. PlayerDataManager.cs
**File:** `/home/user/PantheonWars/PantheonWars/Systems/PlayerDataManager.cs`

#### Added Methods:
- `AddFractionalFavor(string playerUID, float amount, string reason)` - Public interface for adding fractional favor

### 3. FavorSystem.cs
**File:** `/home/user/PantheonWars/PantheonWars/Systems/FavorSystem.cs`

#### Added Constants:
- `BASE_FAVOR_PER_HOUR = 2.0f` - Base passive favor generation rate
- `PASSIVE_TICK_INTERVAL_MS = 1000` - Tick interval (1 second)

#### Added Fields:
- `_religionManager` - Reference to ReligionManager for prestige multipliers

#### Modified Constructor:
- Now accepts `ReligionManager` parameter

#### Modified Initialize():
- Registers game tick listener for passive favor generation

#### Added Methods:
- `OnGameTick(float dt)` - Tick handler that awards passive favor to all online players
- `AwardPassiveFavor(IServerPlayer player, float dt)` - Calculates and awards passive favor
- `CalculatePassiveFavorMultiplier(IServerPlayer player, PlayerDeityData playerData)` - Calculates total multiplier

### 4. PantheonWarsSystem.cs
**File:** `/home/user/PantheonWars/PantheonWars/PantheonWarsSystem.cs`

#### Modified Initialization Order:
- ReligionManager now initializes before FavorSystem (needed for passive favor multipliers)
- FavorSystem constructor now passes ReligionManager parameter

## Passive Favor Mechanics

### Base Rate
- **2.0 favor per in-game hour**
- **0.04 favor per second** (with 1-second ticks)

### Calculation Formula
```
Favor Per Tick = BASE_FAVOR_PER_HOUR × (dt / HoursPerDay) × Multipliers
```

Where:
- `dt` = real-time seconds elapsed (typically 1.0)
- `HoursPerDay` = in-game hours per real-time day (Vintage Story default: ~20)
- `Multipliers` = Devotion Rank Multiplier × Religion Prestige Multiplier

### Devotion Rank Multipliers
| Rank      | Multiplier | Effective Rate (favor/hour) |
|-----------|------------|-----------------------------|
| Initiate  | 1.0x       | 2.0                         |
| Disciple  | 1.1x       | 2.2                         |
| Zealot    | 1.2x       | 2.4                         |
| Champion  | 1.3x       | 2.6                         |
| Avatar    | 1.5x       | 3.0                         |

### Religion Prestige Multipliers
| Rank        | Multiplier | Combined with Avatar (favor/hour) |
|-------------|------------|------------------------------------|
| Fledgling   | 1.0x       | 3.0                                |
| Established | 1.1x       | 3.3                                |
| Renowned    | 1.2x       | 3.6                                |
| Legendary   | 1.3x       | 3.9                                |
| Mythic      | 1.5x       | 4.5                                |

### Maximum Passive Rate
An Avatar in a Mythic religion: **4.5 favor per in-game hour** (1.5 × 1.5 × 2.0)

## Expected Progression Times

### Passive-Only Progression (Base Rate: 2.0 favor/hour)
| Rank      | Total Favor Required | Hours Required | Days @ 24hr/day |
|-----------|---------------------|----------------|-----------------|
| Initiate  | 0                   | 0              | 0               |
| Disciple  | 500                 | 250            | 10.4            |
| Zealot    | 2000                | 1000           | 41.7            |
| Champion  | 5000                | 2500           | 104.2           |
| Avatar    | 10000               | 5000           | 208.3           |

**Note:** These times assume no multipliers. With rank multipliers, actual progression is faster.

### Optimized Passive Progression (Avatar + Mythic)
With maximum multipliers (4.5 favor/hour):
- **Avatar Rank**: ~2,222 hours (~92.6 days continuous play)

### With Casual PvP (1 kill/hour @ 10 favor)
Combined passive (2.0/hr) + PvP (10/hr) = 12 favor/hour:
- **Avatar Rank**: ~833 hours (~34.7 days)

### With Moderate PvP (2-3 kills/hour @ 10 favor each)
Combined passive (2.0/hr) + PvP (20-30/hr) = 22-32 favor/hour:
- **Avatar Rank**: ~312-454 hours (~13-19 days)

## System Behavior

### Tick Processing
1. Every 1 second (real-time), the system:
   - Iterates through all online players
   - Checks if player has a deity
   - Calculates in-game hours elapsed since last tick
   - Calculates base favor for this tick
   - Applies devotion rank multiplier
   - Applies religion prestige multiplier (if in a religion)
   - Adds fractional favor to player's accumulator
   - Awards integer favor when accumulator >= 1.0

### Fractional Favor Example
```
Second 1: +0.04 favor → Accumulator: 0.04
Second 2: +0.04 favor → Accumulator: 0.08
...
Second 25: +0.04 favor → Accumulator: 1.00 → Awards 1 favor, Accumulator: 0.00
```

### Requirements for Passive Favor
- Player must be online
- Player must have pledged to a deity
- Player receives favor every tick (1 second) regardless of activity

### Performance Considerations
- Tick rate: 1 second (not performance-intensive)
- Only processes online players
- Simple multiplication operations
- Fractional accumulation prevents data loss
- Auto-saves occur on world save events (existing functionality)

## Testing Requirements

### Unit Tests Needed
- [ ] Verify base favor calculation (2.0 favor/hour → 0.04 favor/second)
- [ ] Test fractional accumulation (ensure no favor loss)
- [ ] Verify devotion rank multipliers (1.0x to 1.5x)
- [ ] Verify religion prestige multipliers (1.0x to 1.5x)
- [ ] Test multiplier stacking (devotion × prestige)

### Integration Tests Needed
- [ ] Player with deity gains passive favor every second
- [ ] Player without deity does not gain passive favor
- [ ] Offline players do not gain passive favor
- [ ] Favor accumulates to devotion rank thresholds correctly
- [ ] Multiple players can receive passive favor simultaneously

### Manual Testing Steps
1. Join server with a character pledged to a deity
2. Stand idle for 5 minutes
3. Check `/favor` command - should show ~12 favor gained (5 min × 2 favor/hr ÷ 60 = 0.167 favor/min × 5 = 0.83, rounds to ~1-2 favor with accumulation)
4. Join a religion and verify prestige multiplier applies
5. Gain devotion ranks and verify multiplier increases

### Expected Test Results
- **5 minutes idle**: ~0.83 favor (Initiate, no religion)
- **10 minutes idle**: ~1.67 favor (Initiate, no religion)
- **1 hour idle**: ~2.0 favor (Initiate, no religion)
- **1 hour idle**: ~4.5 favor (Avatar, Mythic religion)

## Future Enhancements (Phase 3)

The following activity bonuses are planned for Phase 3 but not yet implemented:

- **Prayer Bonus**: 2x favor for 10 minutes after praying at shrine
- **Sacred Territory**: 1.5x favor while in deity's sacred territory
- **Time of Day**: 1.3x favor during deity's favored time
- **Combat Streak**: +10% per kill in succession (5 minute duration)

See `passive_favor_integration_plan.md` for complete details.

## Compatibility Notes

### Save Data Migration
- Added `AccumulatedFractionalFavor` field to PlayerDeityData (ProtoMember 10)
- Existing player data will automatically initialize this field to 0.0
- No manual migration required

### Backward Compatibility
- All existing favor mechanics remain unchanged
- PvP kills still award 10 base favor
- Death penalty still -5 favor
- Existing devotion rank thresholds unchanged

## Configuration (Future)

Currently hardcoded. Future configuration file structure:
```json
{
  "favorSystem": {
    "passiveFavorPerHour": 2.0,
    "devotionRankMultipliers": {
      "initiate": 1.0,
      "disciple": 1.1,
      "zealot": 1.2,
      "champion": 1.3,
      "avatar": 1.5
    }
  }
}
```

## Summary

Phase 1 successfully implements:
- ✅ Passive favor generation (2.0 favor/hour base)
- ✅ Fractional favor accumulation
- ✅ Devotion rank multipliers (1.0x - 1.5x)
- ✅ Religion prestige multipliers (1.0x - 1.5x)
- ✅ Real-time tick system (1 second interval)
- ✅ Proper save/load support

**Status**: Implementation complete, ready for testing

**Next Steps**:
1. Build and deploy to test server
2. Manual testing with players
3. Balance adjustments if needed
4. Proceed to Phase 2 (Multiplier Systems - already included)
5. Proceed to Phase 3 (Activity Bonuses)
6. Proceed to Phase 4 (Prestige Contributions)
