# Phase 3: Activity Bonuses Implementation Plan

## Overview

Phase 3 implements temporary multiplier bonuses that reward active, strategic gameplay. These bonuses stack with the base passive favor multipliers from Phase 1.

## Architecture

### Approach: Entity Stats System

Vintage Story's entity stats system is perfect for tracking temporary multipliers:
- Supports time-based expiration
- Allows multiple named multipliers to stack
- Automatically manages cleanup
- Already integrated into the game engine

### Multiplier Application

```csharp
// Setting a temporary multiplier
player.Entity.Stats.Set(
    "passiveFavorMultiplier",     // Stat code
    "prayer_bonus",                // Unique identifier
    2.0f,                          // Multiplier value
    expiryTime                     // When it expires (TotalHours)
);

// Reading all multipliers
float totalMultiplier = player.Entity.Stats.GetBlended("passiveFavorMultiplier");
```

## Implementation Components

### 1. Activity Bonus System (New File)

**File:** `PantheonWars/Systems/ActivityBonusSystem.cs`

**Purpose:** Centralized system for managing all activity bonuses

**Responsibilities:**
- Register and initialize all bonus types
- Update time-based bonuses (time-of-day)
- Provide public API for triggering bonuses
- Query combined multiplier for favor calculation

**Key Methods:**
```csharp
public class ActivityBonusSystem
{
    // Initialization
    public void Initialize();

    // Bonus application
    public void ApplyPrayerBonus(IServerPlayer player);
    public void ApplyCombatStreakBonus(IServerPlayer player, int streakCount);
    public void UpdateSacredTerritoryBonus(IServerPlayer player);
    public void UpdateTimeOfDayBonus(IServerPlayer player);

    // Multiplier querying
    public float GetActivityMultiplier(IServerPlayer player);

    // Tick updates
    private void OnGameTick(float dt);
}
```

### 2. Prayer Bonus

**Trigger:** Player prays at a shrine (requires shrine system or command)

**Effect:** 2x passive favor for 10 minutes

**Implementation:**
```csharp
public void ApplyPrayerBonus(IServerPlayer player)
{
    var playerData = _playerDataManager.GetOrCreatePlayerData(player);
    if (!playerData.HasDeity()) return;

    // Calculate expiry time (10 minutes from now)
    double expiryTime = _sapi.World.Calendar.TotalHours + (10.0 / 60.0);

    // Set multiplier
    player.Entity.Stats.Set(
        "passiveFavorMultiplier",
        "prayer_bonus",
        2.0f,
        expiryTime
    );

    player.SendMessage(
        GlobalConstants.GeneralChatGroup,
        "Your devotion is rewarded! 2x favor gain for 10 minutes.",
        EnumChatType.Notification
    );
}
```

**Integration Points:**
- Add to shrine interaction (future shrine system)
- OR add temporary command: `/deity pray`

### 3. Sacred Territory Bonus

**Trigger:** Player enters/exits defined sacred areas

**Effect:** 1.5x passive favor while in territory

**Implementation:**
```csharp
public void UpdateSacredTerritoryBonus(IServerPlayer player)
{
    var playerData = _playerDataManager.GetOrCreatePlayerData(player);
    if (!playerData.HasDeity()) return;

    bool inSacredTerritory = CheckIfInSacredTerritory(player);

    if (inSacredTerritory)
    {
        // Apply permanent multiplier (no expiry)
        player.Entity.Stats.Set(
            "passiveFavorMultiplier",
            "sacred_territory",
            1.5f
        );
    }
    else
    {
        // Remove bonus when leaving
        player.Entity.Stats.Remove("passiveFavorMultiplier", "sacred_territory");
    }
}

private bool CheckIfInSacredTerritory(IServerPlayer player)
{
    // Option 1: Check distance to shrine blocks
    // Option 2: Use land claims system
    // Option 3: Define areas in config (x/z radius around specific coords)

    // Simple implementation: Check for shrine blocks nearby
    var pos = player.Entity.ServerPos.AsBlockPos;
    var nearbyBlocks = _sapi.World.BlockAccessor.SearchBlocks(
        pos.AddCopy(-10, -5, -10),
        pos.AddCopy(10, 5, 10),
        (block, blockPos) => block.Code?.Path?.Contains("shrine") ?? false
    );

    return nearbyBlocks.Length > 0;
}
```

**Integration Points:**
- Requires shrine block definitions
- Runs every tick (or every 5 seconds for optimization)
- Could integrate with land claims for "temple" areas

### 4. Time of Day Bonus

**Trigger:** Automatic based on in-game time

**Effect:** 1.3x during deity's favored hours

**Implementation:**
```csharp
public void UpdateTimeOfDayBonus(IServerPlayer player)
{
    var playerData = _playerDataManager.GetOrCreatePlayerData(player);
    if (!playerData.HasDeity()) return;

    var currentHour = _sapi.World.Calendar.HourOfDay;
    bool isFavoredTime = IsFavoredTimeForDeity(playerData.DeityType, currentHour);

    if (isFavoredTime)
    {
        player.Entity.Stats.Set(
            "passiveFavorMultiplier",
            "time_of_day",
            1.3f
        );
    }
    else
    {
        player.Entity.Stats.Remove("passiveFavorMultiplier", "time_of_day");
    }
}

private bool IsFavoredTimeForDeity(DeityType deity, int hour)
{
    return deity switch
    {
        DeityType.Khoras => hour >= 10 && hour <= 14,  // Midday (combat)
        DeityType.Lysa => hour <= 6 || hour >= 18,      // Dawn/dusk (hunting)
        DeityType.Thalos => hour >= 8 && hour <= 16,    // Daylight (crafting)
        DeityType.Noxara => hour >= 20 || hour <= 4,    // Night (shadows)
        DeityType.Verdara => hour >= 6 && hour <= 10,   // Morning (nature)
        _ => false
    };
}
```

**Integration Points:**
- Runs every game tick (or every 30 seconds)
- Could show notification when entering/exiting favored time
- Deity-specific time windows can be configured

### 5. Combat Streak Bonus

**Trigger:** Multiple PvP kills in succession

**Effect:** +10% per kill, lasts 5 minutes per kill

**Implementation:**
```csharp
// Add to FavorSystem.cs or ActivityBonusSystem.cs
private readonly Dictionary<string, CombatStreakData> _combatStreaks = new();

private class CombatStreakData
{
    public int StreakCount { get; set; }
    public double LastKillTime { get; set; }
}

public void OnPvPKill(IServerPlayer attacker)
{
    var playerUID = attacker.PlayerUID;
    var currentTime = _sapi.World.Calendar.TotalHours;

    // Get or create streak data
    if (!_combatStreaks.TryGetValue(playerUID, out var streak))
    {
        streak = new CombatStreakData();
        _combatStreaks[playerUID] = streak;
    }

    // Check if streak is still active (last kill within 10 minutes)
    if (currentTime - streak.LastKillTime < (10.0 / 60.0))
    {
        streak.StreakCount++;
    }
    else
    {
        streak.StreakCount = 1; // Reset streak
    }

    streak.LastKillTime = currentTime;

    // Apply bonus (5 minute duration)
    float multiplier = 1.0f + (streak.StreakCount * 0.1f); // +10% per kill
    double expiryTime = currentTime + (5.0 / 60.0);

    attacker.Entity.Stats.Set(
        "passiveFavorMultiplier",
        "combat_streak",
        multiplier,
        expiryTime
    );

    if (streak.StreakCount > 1)
    {
        attacker.SendMessage(
            GlobalConstants.GeneralChatGroup,
            $"Combat streak x{streak.StreakCount}! +{multiplier * 100}% favor gain for 5 minutes.",
            EnumChatType.Notification
        );
    }
}
```

**Integration Points:**
- Hook into `FavorSystem.ProcessPvPKill()`
- Track streak count per player
- Reset after 10 minutes of inactivity

## Integration with Phase 1

### Update FavorSystem.cs

Modify `CalculatePassiveFavorMultiplier()` to include activity bonuses:

```csharp
private float CalculatePassiveFavorMultiplier(IServerPlayer player, PlayerDeityData playerData)
{
    float multiplier = 1.0f;

    // Devotion rank bonuses (Phase 1)
    multiplier *= playerData.DevotionRank switch
    {
        DevotionRank.Initiate => 1.0f,
        DevotionRank.Disciple => 1.1f,
        DevotionRank.Zealot => 1.2f,
        DevotionRank.Champion => 1.3f,
        DevotionRank.Avatar => 1.5f,
        _ => 1.0f
    };

    // Religion prestige bonuses (Phase 1)
    var religion = _religionManager.GetPlayerReligion(player.PlayerUID);
    if (religion != null)
    {
        multiplier *= religion.PrestigeRank switch
        {
            PrestigeRank.Fledgling => 1.0f,
            PrestigeRank.Established => 1.1f,
            PrestigeRank.Renowned => 1.2f,
            PrestigeRank.Legendary => 1.3f,
            PrestigeRank.Mythic => 1.5f,
            _ => 1.0f
        };
    }

    // Activity bonuses (Phase 3) - NEW
    if (_activityBonusSystem != null)
    {
        multiplier *= _activityBonusSystem.GetActivityMultiplier(player);
    }

    return multiplier;
}
```

### Update PantheonWarsSystem.cs

Initialize ActivityBonusSystem:

```csharp
// Initialize activity bonus system (Phase 3)
_activityBonusSystem = new ActivityBonusSystem(api, _playerDataManager, _favorSystem);
_activityBonusSystem.Initialize();
```

## Files to Create/Modify

### New Files:
1. **`PantheonWars/Systems/ActivityBonusSystem.cs`** - Main activity bonus system
2. **`PantheonWars/Models/CombatStreakData.cs`** - Combat streak tracking (optional, can be inner class)

### Modified Files:
1. **`PantheonWars/Systems/FavorSystem.cs`** - Add combat streak integration
2. **`PantheonWars/PantheonWarsSystem.cs`** - Initialize ActivityBonusSystem
3. **`PantheonWars/Commands/DeityCommands.cs`** - Add `/deity pray` command (temporary)

## Configuration (Future)

```json
{
  "activityBonuses": {
    "prayer": {
      "multiplier": 2.0,
      "durationMinutes": 10
    },
    "sacredTerritory": {
      "multiplier": 1.5,
      "radiusBlocks": 50
    },
    "timeOfDay": {
      "multiplier": 1.3,
      "deityTimeWindows": {
        "Khoras": { "startHour": 10, "endHour": 14 },
        "Lysa": { "startHour": 18, "endHour": 6 },
        "Thalos": { "startHour": 8, "endHour": 16 },
        "Noxara": { "startHour": 20, "endHour": 4 },
        "Verdara": { "startHour": 6, "endHour": 10 }
      }
    },
    "combatStreak": {
      "multiplierPerKill": 0.1,
      "durationMinutes": 5,
      "streakTimeoutMinutes": 10
    }
  }
}
```

## Implementation Task Breakdown

### Task 1: Create ActivityBonusSystem (Core)
1. Create `ActivityBonusSystem.cs` file
2. Implement initialization and tick registration
3. Add `GetActivityMultiplier()` method
4. Add references to PlayerDataManager

### Task 2: Implement Prayer Bonus
1. Add `ApplyPrayerBonus()` method
2. Create temporary `/deity pray` command
3. Test bonus application and expiry
4. Add notifications

### Task 3: Implement Time of Day Bonus
1. Add `UpdateTimeOfDayBonus()` method
2. Define deity time windows
3. Add tick update (every 30 seconds)
4. Test transitions between time periods

### Task 4: Implement Sacred Territory Bonus
1. Add `CheckIfInSacredTerritory()` method
2. Add `UpdateSacredTerritoryBonus()` method
3. Define territory detection logic
4. Add tick update (every 5 seconds)
5. Test entering/exiting territory

### Task 5: Implement Combat Streak Bonus
1. Add streak tracking dictionary
2. Hook into `FavorSystem.ProcessPvPKill()`
3. Implement streak reset logic
4. Add streak notifications
5. Test multi-kill scenarios

### Task 6: Integration
1. Update `FavorSystem.CalculatePassiveFavorMultiplier()`
2. Initialize ActivityBonusSystem in PantheonWarsSystem
3. Test all bonuses stacking correctly
4. Verify total multiplier calculations

### Task 7: Testing & Documentation
1. Manual testing of each bonus type
2. Test bonus stacking (prayer + time + territory + streak)
3. Verify maximum multiplier scenarios
4. Document expected rates with bonuses

## Maximum Multiplier Examples

With all Phase 3 bonuses active:

**Base Multipliers (Phase 1):**
- Devotion: 1.5x (Avatar)
- Prestige: 1.5x (Mythic)
- Base Product: 2.25x

**Activity Multipliers (Phase 3):**
- Prayer: 2.0x
- Territory: 1.5x
- Time of Day: 1.3x
- Combat Streak (3 kills): 1.3x
- Activity Product: 5.07x

**Total Multiplier:** 2.25 × 5.07 = **11.41x**

**Maximum Rate:** 2.0 favor/hour × 11.41 = **22.82 favor/hour**

This is intentionally very high for "perfect play" scenarios but requires:
- Prayer buff active
- Inside sacred territory
- Correct time of day
- Active PvP streak
- Avatar rank + Mythic religion

## Performance Considerations

### Tick Frequency Optimization:
- **Prayer/Combat Streak**: Event-driven (no tick overhead)
- **Time of Day**: Check every 30 seconds (low overhead)
- **Sacred Territory**: Check every 5 seconds (medium overhead)
- **Total overhead**: Minimal, ~2-3 checks per player every 5 seconds

### Optimization Options:
1. Cache territory checks (only update on movement threshold)
2. Batch time-of-day updates (all players at once)
3. Use area triggers for sacred territory instead of distance checks

## Testing Strategy

### Unit Tests:
- [x] Prayer bonus applies correctly
- [x] Prayer bonus expires after 10 minutes
- [x] Time of day bonus activates/deactivates correctly
- [x] Sacred territory detection works
- [x] Combat streak increments correctly
- [x] Combat streak resets after timeout
- [x] All bonuses stack multiplicatively

### Integration Tests:
- [x] Activity multiplier integrates with Phase 1 multipliers
- [x] Multiple players can have different bonuses simultaneously
- [x] Bonuses persist through area transitions
- [x] Bonuses clear properly on logout

### Manual Testing:
1. Use `/deity pray` command, wait 10 minutes, verify expiry
2. Stand near shrine, verify territory bonus, walk away, verify removal
3. Wait for deity's favored time, verify bonus appears
4. Kill 3 players in succession, verify streak bonus
5. Combine all bonuses, verify total multiplier

## Next Steps After Phase 3

**Phase 4: Prestige Contributions**
- Passive prestige generation from online players
- Religion-wide benefits

**Phase 5: Refinements**
- Dynamic death penalty
- Experience formula adjustments
- Configuration system
- Balance tuning

## Summary

Phase 3 adds engaging moment-to-moment gameplay through temporary multiplier bonuses:
- **Prayer**: Reward shrine interaction
- **Territory**: Encourage temple building/visiting
- **Time of Day**: Add strategic timing element
- **Combat Streak**: Reward PvP skill

All bonuses stack multiplicatively with Phase 1 multipliers, creating interesting optimization opportunities without being required for progression.
