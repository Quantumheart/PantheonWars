# Passive Favor Integration Plan

## Overview

This document outlines how to integrate passive favor mechanics into PantheonWars' favor and prestige systems, creating a more engaging and rewarding progression system.

## Key Design Decision: Passive Favor Rate

**Chosen Rate: 2.0 favor per in-game hour**

This rate was chosen after analyzing progression times (see `favor_progression_calculations.md`):

- **Passive-only players (optimized):** ~57 days continuous play to reach Avatar
- **Casual PvP (1 kill/hour):** ~6 days of play to reach Avatar
- **Moderate PvP (2-3 kills/hour):** ~3-4 days of play to reach Avatar
- **Per tick value:** 0.04 favor every second

**Why 2.0 instead of 0.5?**
The initial proposal of 0.5 favor/hour would require 228 days of continuous passive play to reach Avatar. The 4x increase (2.0/hour) makes passive progression viable for non-PvP players while keeping PvP combat as the primary and most efficient favor source. PvP kills still provide 10-20 favor instantly, making them 5-10x more efficient than passive gain.

## Current System Analysis

### Current Favor System
- **Gain Method:** PvP kills only (10 base favor per kill)
- **Modifiers:** Deity relationship multipliers (0.5x - 2.0x)
- **Death Penalty:** Fixed 5 favor loss
- **Progression:** Devotion ranks based on lifetime favor (500/2000/5000/10000)
- **Usage:** Spent on abilities

**Files:**
- `/home/quantumheart/RiderProjects/PantheonWars/PantheonWars/Systems/FavorSystem.cs`

### Current Prestige System
- **Gain Method:** Not yet implemented (placeholder)
- **Progression:** Religion ranks (500/2000/5000/10000)
- **Purpose:** Unlocks religion-wide blessings
- **Scope:** Shared across all religion members

**Files:**
- `/home/quantumheart/RiderProjects/PantheonWars/PantheonWars/Systems/ReligionPrestigeManager.cs`

## Integration Opportunities

### 1. Passive Favor Generation (High Priority)

**Concept:** Players passively gain favor over time, similar to xskills survival experience.

**Implementation:**

```csharp
// Add to FavorSystem.cs
public class FavorSystem
{
    private const float BASE_FAVOR_PER_HOUR = 2.0f; // 4x higher than initial proposal for reasonable progression

    public void Initialize()
    {
        // ... existing code ...

        // Register game tick for passive favor
        _sapi.Event.RegisterGameTickListener(OnGameTick, 1000); // Once per second
    }

    private void OnGameTick(float dt)
    {
        // Award passive favor to all online players with deities
        foreach (var player in _sapi.World.AllOnlinePlayers)
        {
            if (player is IServerPlayer serverPlayer)
            {
                AwardPassiveFavor(serverPlayer, dt);
            }
        }
    }

    private void AwardPassiveFavor(IServerPlayer player, float dt)
    {
        var playerData = _playerDataManager.GetOrCreatePlayerData(player);

        if (!playerData.HasDeity()) return;

        // Calculate in-game hours elapsed this tick
        float inGameHoursElapsed = dt / _sapi.World.Calendar.HoursPerDay;
        // With 1 second ticks: inGameHoursElapsed = 0.02 in-game hours

        // Calculate base favor for this tick
        float baseFavor = BASE_FAVOR_PER_HOUR * inGameHoursElapsed;
        // baseFavor = 2.0 × 0.02 = 0.04 favor per tick

        // Apply multipliers
        float finalFavor = baseFavor * CalculatePassiveFavorMultiplier(player, playerData);

        // Award favor (accumulate fractional amounts)
        if (finalFavor >= 0.01f) // Only award when we have at least 0.01 favor
        {
            _playerDataManager.AddFavor(player.PlayerUID, finalFavor, "Passive devotion");
        }
    }

    private float CalculatePassiveFavorMultiplier(IServerPlayer player, PlayerData playerData)
    {
        float multiplier = 1.0f;

        // Devotion rank bonuses (like xskills' specialization)
        multiplier *= playerData.DevotionRank switch
        {
            DevotionRank.Initiate => 1.0f,
            DevotionRank.Disciple => 1.1f,
            DevotionRank.Zealot => 1.2f,
            DevotionRank.Champion => 1.3f,
            DevotionRank.Avatar => 1.5f,
            _ => 1.0f
        };

        // Religion prestige bonuses
        if (!string.IsNullOrEmpty(playerData.ReligionUID))
        {
            var religion = _religionManager.GetReligion(playerData.ReligionUID);
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
        }

        // Deity-specific bonuses (could vary by deity)
        // e.g., Khoras might favor combat more, Lysa might favor passive gain

        // Activity bonuses (like xskills' Well Rested)
        // - Recently prayed at shrine: 2x for 10 minutes
        // - In deity's sacred territory: 1.5x
        // - During deity's favored time of day: 1.2x

        return multiplier;
    }
}
```

**Benefits:**
- Rewards active playtime even without PvP
- Creates incentive to stay online and engaged
- Provides steady progression for peaceful players
- Multipliers create interesting optimization choices

**Balance Considerations:**
- 2.0 favor/hour base = ~48 favor per day (24 hour cycle)
- With 2.0x average multiplier = ~96 favor per day
- Still encourages PvP (10-20 favor per kill) as primary source
- Provides fallback for low-population servers
- Passive only to Avatar: ~57 days continuous play (optimized)
- With casual PvP (1 kill/hr): ~6 days of play
- See `favor_progression_calculations.md` for detailed breakdown

### 2. Prestige Contribution from Active Players

**Concept:** Active, online players passively contribute prestige to their religion.

**Implementation:**

```csharp
// Add to ReligionPrestigeManager.cs
public class ReligionPrestigeManager
{
    private const float BASE_PRESTIGE_PER_HOUR = 0.1f; // Lower than favor

    public void Initialize()
    {
        // ... existing code ...

        // Register game tick for passive prestige
        _sapi.Event.RegisterGameTickListener(OnGameTick, 1000);
    }

    private void OnGameTick(float dt)
    {
        // Track prestige contributions from online players
        var religionContributions = new Dictionary<string, float>();

        foreach (var player in _sapi.World.AllOnlinePlayers)
        {
            if (player is IServerPlayer serverPlayer)
            {
                var contribution = CalculatePrestigeContribution(serverPlayer, dt);
                if (contribution > 0)
                {
                    var playerData = _playerDataManager.GetOrCreatePlayerData(serverPlayer);
                    if (!string.IsNullOrEmpty(playerData.ReligionUID))
                    {
                        if (!religionContributions.ContainsKey(playerData.ReligionUID))
                            religionContributions[playerData.ReligionUID] = 0;

                        religionContributions[playerData.ReligionUID] += contribution;
                    }
                }
            }
        }

        // Award prestige to religions
        foreach (var kvp in religionContributions)
        {
            int prestigeToAward = (int)Math.Ceiling(kvp.Value);
            if (prestigeToAward > 0)
            {
                AddPrestige(kvp.Key, prestigeToAward, "Member activity");
            }
        }
    }

    private float CalculatePrestigeContribution(IServerPlayer player, float dt)
    {
        var playerData = _playerDataManager.GetOrCreatePlayerData(player);

        if (string.IsNullOrEmpty(playerData.ReligionUID)) return 0;

        // Base prestige per hour of activity
        float hoursElapsed = dt / _sapi.World.Calendar.HoursPerDay;
        float basePrestige = BASE_PRESTIGE_PER_HOUR * hoursElapsed;

        // Higher devotion rank = more prestige contribution
        float multiplier = playerData.DevotionRank switch
        {
            DevotionRank.Initiate => 1.0f,
            DevotionRank.Disciple => 1.5f,
            DevotionRank.Zealot => 2.0f,
            DevotionRank.Champion => 3.0f,
            DevotionRank.Avatar => 5.0f,
            _ => 1.0f
        };

        return basePrestige * multiplier;
    }
}
```

**Benefits:**
- Religion prestige grows with active member count
- High-devotion players contribute more (rewards dedication)
- Creates incentive for religions to recruit and retain members
- Automatically balances large vs small religions

**Balance Considerations:**
- 0.1 prestige/hour base = ~2.4 prestige per day per member
- Religion with 10 active members = ~24 prestige/day
- Large, active religions grow faster
- Could add diminishing returns for very large religions

### 3. Activity-Based Multipliers (Medium Priority)

**Concept:** Create temporary XP multipliers like xskills' "Well Rested" effect.

**Implementation Ideas:**

#### Prayer Bonus
```csharp
// After praying at a shrine
public void ApplyPrayerBonus(IServerPlayer player)
{
    // 2x favor gain for 10 minutes (similar to Well Rested)
    var entity = player.Entity;
    entity.Stats.Set("passiveFavorMultiplier", "prayer_bonus", 2.0f,
                     player.Entity.World.Calendar.TotalHours + 10.0 / 24.0);

    player.SendMessage(GlobalConstants.GeneralChatGroup,
        "Your devotion is rewarded! 2x favor gain for 10 minutes.",
        EnumChatType.Notification);
}
```

#### Combat Victory Streak
```csharp
// After killing multiple enemies in succession
public void ApplyCombatStreakBonus(IServerPlayer player, int streak)
{
    float multiplier = 1.0f + (streak * 0.1f); // +10% per kill in streak
    float duration = 5.0f; // 5 minutes

    player.Entity.Stats.Set("passiveFavorMultiplier", "combat_streak",
                           multiplier,
                           player.Entity.World.Calendar.TotalHours + duration / 24.0);
}
```

#### Sacred Territory
```csharp
// While in deity's sacred territory/shrine area
public void UpdateTerritoryBonus(IServerPlayer player)
{
    bool inSacredTerritory = CheckIfInSacredTerritory(player);

    if (inSacredTerritory)
    {
        // Permanent 1.5x while in territory (no expiry)
        player.Entity.Stats.Set("passiveFavorMultiplier", "sacred_territory", 1.5f);
    }
    else
    {
        // Remove bonus when leaving
        player.Entity.Stats.Remove("passiveFavorMultiplier", "sacred_territory");
    }
}
```

#### Time of Day
```csharp
// Deity-specific time bonuses
public void UpdateTimeOfDayBonus(IServerPlayer player, PlayerData playerData)
{
    var currentHour = _sapi.World.Calendar.HourOfDay;
    float multiplier = 1.0f;

    // Example: Khoras favors midday (combat), Lysa favors dawn/dusk (hunting)
    switch (playerData.DeityType)
    {
        case DeityType.Khoras:
            if (currentHour >= 10 && currentHour <= 14)
                multiplier = 1.3f; // Midday bonus
            break;
        case DeityType.Lysa:
            if (currentHour <= 6 || currentHour >= 18)
                multiplier = 1.3f; // Dawn/dusk bonus
            break;
    }

    if (multiplier > 1.0f)
    {
        player.Entity.Stats.Set("passiveFavorMultiplier", "time_of_day", multiplier);
    }
    else
    {
        player.Entity.Stats.Remove("passiveFavorMultiplier", "time_of_day");
    }
}
```

**Benefits:**
- Creates engaging moment-to-moment gameplay
- Rewards strategic timing and positioning
- Provides goals beyond just "kill enemies"
- Adds depth to the devotion system

### 4. Dynamic Death Penalty (Low Priority)

**Concept:** Scale death penalty based on current favor, like xskills (percentage-based with cap).

**Implementation:**

```csharp
// Modify ProcessDeathPenalty in FavorSystem.cs
internal void ProcessDeathPenalty(IServerPlayer player)
{
    var playerData = _playerDataManager.GetOrCreatePlayerData(player);

    if (!playerData.HasDeity()) return;

    // Calculate penalty: 50% of current favor, capped at 10
    int penalty = Math.Min(
        (int)(playerData.DivineFavor * 0.5f),
        10
    );

    if (penalty > 0)
    {
        _playerDataManager.RemoveFavor(player.PlayerUID, penalty, "Death penalty");

        player.SendMessage(
            GlobalConstants.GeneralChatGroup,
            $"[Divine Favor] You lost {penalty} favor upon death ({(int)(penalty / (float)playerData.DivineFavor * 100)}% of current favor).",
            EnumChatType.Notification
        );
    }
}
```

**Benefits:**
- More forgiving for low-favor players
- Higher stakes for favor-rich players
- Natural risk/reward balance

### 5. Experience Formula for Devotion Ranks (Low Priority)

**Concept:** Use xskills' quadratic formula for smoother devotion progression.

**Current System:**
- Initiate: 0
- Disciple: 500
- Zealot: 2000
- Champion: 5000
- Avatar: 10000

**Problems:**
- Large jumps between ranks
- Linear progression feels grindy at high levels

**Proposed Formula:**
```
XP Required = a × (level-1)² + b × (level-1) + c

For devotion ranks:
Level 2 (Disciple): 500
Level 3 (Zealot): 1500
Level 4 (Champion): 3000
Level 5 (Avatar): 5000

Solving for a, b, c:
a = 125
b = 250
c = 125

Formula: 125(L-1)² + 250(L-1) + 125
```

**Implementation:**
```csharp
public class PlayerDataManager
{
    private int CalculateFavorRequirement(int devotionLevel)
    {
        if (devotionLevel <= 1) return 0;

        int x = devotionLevel - 1;
        return 125 * x * x + 250 * x + 125;
    }

    public DevotionRank CalculateDevotionRank(int totalFavor)
    {
        if (totalFavor >= CalculateFavorRequirement(5)) return DevotionRank.Avatar;
        if (totalFavor >= CalculateFavorRequirement(4)) return DevotionRank.Champion;
        if (totalFavor >= CalculateFavorRequirement(3)) return DevotionRank.Zealot;
        if (totalFavor >= CalculateFavorRequirement(2)) return DevotionRank.Disciple;
        return DevotionRank.Initiate;
    }
}
```

## Implementation Phases

### Phase 1: Core Passive Systems (Week 1)
1. Implement passive favor generation tick system
2. Add basic multiplier calculation
3. Test balance with small group
4. Document new favor rates

### Phase 2: Multiplier Systems (Week 2)
1. Implement devotion rank multipliers
2. Add religion prestige multipliers
3. Create stat-based multiplier system
4. Test multiplier stacking

### Phase 3: Activity Bonuses (Week 3)
1. Implement prayer bonus system
2. Add sacred territory detection and bonus
3. Create time-of-day bonuses
4. Add combat streak tracking

### Phase 4: Prestige Contributions (Week 4)
1. Implement passive prestige generation
2. Add devotion rank contribution scaling
3. Balance religion growth rates
4. Test with multiple religions

### Phase 5: Refinement (Week 5)
1. Implement dynamic death penalty
2. Add experience formula for devotion ranks
3. Balance all systems together
4. Performance optimization

## Configuration Recommendations

### Config File Structure
```json
{
  "favorSystem": {
    "passiveFavorPerHour": 2.0,
    "pvpBaseFavor": 10,
    "deathPenaltyPercent": 50,
    "deathPenaltyMax": 10,
    "devotionRankMultipliers": {
      "initiate": 1.0,
      "disciple": 1.1,
      "zealot": 1.2,
      "champion": 1.3,
      "avatar": 1.5
    }
  },
  "prestigeSystem": {
    "passivePrestigePerHour": 0.1,
    "prestigeRankMultipliers": {
      "fledgling": 1.0,
      "established": 1.1,
      "renowned": 1.2,
      "legendary": 1.3,
      "mythic": 1.5
    }
  },
  "activityBonuses": {
    "prayerMultiplier": 2.0,
    "prayerDurationMinutes": 10,
    "sacredTerritoryMultiplier": 1.5,
    "timeOfDayMultiplier": 1.3,
    "combatStreakMultiplierPerKill": 0.1,
    "combatStreakDurationMinutes": 5
  }
}
```

## Performance Considerations

### Tick Frequency
- **Once per second** is reasonable for favor calculation
- Could optimize to once per 5 seconds if needed
- Cache calculations between ticks

### Multiplier Calculations
- Cache stat-based multipliers
- Only recalculate when stats change
- Use efficient lookup tables for rank bonuses

### Network Synchronization
- Send favor updates in batches
- Only sync significant changes (>0.1 favor)
- Use delta updates for efficiency

### Save Frequency
- Auto-save on world save events (existing)
- Optional: periodic auto-save every 5 minutes
- Always save on player disconnect

## Testing Strategy

### Unit Tests
- Test multiplier calculations
- Verify progression formulas
- Check favor gain rates

### Balance Testing
- Track favor gain rates over 1-hour play sessions
- Compare passive vs PvP favor income
- Monitor time-to-rank progression
- Test with different player counts

### Performance Testing
- Monitor tick performance with 50+ players
- Check memory usage over time
- Profile multiplier calculations
- Test save/load times

## Migration Strategy

### For Existing Players
1. Announce new passive system in advance
2. Ensure no favor is lost during migration
3. Recalculate devotion ranks with new formula
4. Grandfather in existing progression

### For Existing Religions
1. Preserve all existing prestige
2. Recalculate ranks with new thresholds if needed
3. Maintain all unlocked blessings
4. No penalty for the change

## Future Expansion Ideas

### Advanced Multipliers
- **Blessing-based multipliers:** Unlock blessings that increase favor gain
- **Group bonuses:** Gain more favor when playing with religion members
- **Event multipliers:** 2x favor during server events
- **Daily bonuses:** First hour of each day gives bonus

### Prestige Enhancements
- **Prestige expenditure:** Spend prestige to unlock powerful religion blessings
- **Prestige decay:** Inactive religions slowly lose prestige
- **Prestige competition:** Rankings for most prestigious religions
- **Prestige rewards:** Server-wide bonuses for top religions

### Analytics & Feedback
- Track average favor gain rates
- Monitor devotion rank distribution
- Display religion prestige rankings
- Show personal favor gain breakdown

## Comparison: PantheonWars vs XSkills

| Feature | XSkills Survival | PantheonWars Favor | Notes |
|---------|------------------|-------------------|-------|
| **Base passive rate** | 0.042 XP/hour | 2.0 favor/hour | ~48x faster (combat-focused) |
| **Per tick value** | 0.00084 XP | 0.04 favor | Awarded every second |
| **Update frequency** | 1 second | 1 second | Same |
| **Progression formula** | Quadratic | Linear (current) | Could adopt quadratic |
| **Death penalty** | 50% (max 10 XP) | 50% (max 10 favor) | Same approach |
| **Multiplier sources** | 5+ types | 4+ types | Similar depth |
| **Well Rested equivalent** | 6-12x for 8-10min | 2x prayer bonus | Similar concept |
| **Level thresholds** | 10/22/... | 500/2000/5000/10000 | Different scale |
| **Time to max (passive)** | ~240 hours (level 2) | ~57 days (optimized) | Both supplement main activities |

## Files to Modify

### Core Implementation
- `PantheonWars/Systems/FavorSystem.cs` - Add passive favor generation
- `PantheonWars/Systems/ReligionPrestigeManager.cs` - Add passive prestige contribution
- `PantheonWars/Systems/PlayerDataManager.cs` - Update devotion rank calculations

### New Files Needed
- `PantheonWars/Systems/FavorMultiplierSystem.cs` - Centralize multiplier logic
- `PantheonWars/Systems/ActivityBonusSystem.cs` - Track and apply activity bonuses
- `PantheonWars/Config/FavorConfig.cs` - Configuration data structure

### Data Models
- `PantheonWars/Models/PlayerData.cs` - Add active multiplier tracking
- `PantheonWars/Models/ReligionData.cs` - Add prestige contribution tracking

## Summary

Integrating xskills-style passive experience into PantheonWars creates:

1. **Steady Progression:** Players progress even without constant PvP
2. **Engagement Incentives:** Multipliers reward active, strategic play
3. **Religion Growth:** Passive prestige ties religion success to member activity
4. **Depth:** Multiple optimization paths for favor gain
5. **Balance:** Passive gain supplements but doesn't replace PvP

**Core Design Values:**
- **Base passive rate:** 2.0 favor per in-game hour (0.04 favor per second tick)
- **PvP remains primary:** 10-20 favor per kill vs ~0.04 per second passive
- **Progression times:** 6 days with casual PvP, 57 days passive-only (optimized)
- **Multiplier stacking:** Devotion rank × Religion prestige × Activity bonuses
- **Death penalty:** 50% of current favor (max 10), same as xskills

**Recommended Implementation Priority:**
1. Phase 1: Passive favor generation (core feature) - Week 1
2. Phase 2: Multiplier systems (engagement) - Week 2
3. Phase 4: Prestige contributions (religion mechanics) - Week 4
4. Phase 3: Activity bonuses (polish) - Week 3
5. Phase 5: Formula refinements (optimization) - Week 5

This approach maintains PvP as the primary favor source (5-10x more efficient) while creating a richer, more rewarding devotion system that works for all playstyles. Passive progression provides a meaningful fallback for low-population servers and peaceful players without undermining the combat-focused design.
