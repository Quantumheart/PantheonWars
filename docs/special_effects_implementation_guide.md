# Special Effects Implementation Guide

**Document Version:** 1.0
**Last Updated:** October 30, 2025
**Phase:** 3.4 - Deity Blessings (Special Effect Handlers)

## Overview

This guide outlines the implementation plan for special effect handlers that power the 80 blessings across all 8 deities. Special effects provide gameplay mechanics beyond simple stat modifiers, including lifesteal, critical strikes, damage over time, auras, and group abilities.

**Current Status:** Architecture design phase
**Estimated Total Time:** 8-14 hours
**Dependencies:** BlessingEffectSystem, BlessingDefinitions (both complete)

---

## Task Breakdown

### Phase 1: Core Architecture (1-2 hours)

#### Task 1.1: Design SpecialEffectHandler Base Architecture
**Priority:** Critical
**Estimated Time:** 30-45 minutes

Create the foundation for all effect handlers:

- **Abstract base class or interface** defining common lifecycle methods:
  - `Apply(IServerPlayer player, Blessing blessing)` - Initialize effect when blessing is unlocked
  - `Remove(IServerPlayer player)` - Clean up effect when blessing is removed
  - `Update(float deltaTime)` - Optional periodic updates for DoTs/auras
  - `OnCombatEvent(...)` - Handle combat-related triggers

- **Common properties:**
  - Effect identifier (string)
  - Active state tracking
  - Player/entity references
  - Configuration parameters (percentages, durations, cooldowns)

**Deliverable:** `ISpecialEffectHandler` interface and `SpecialEffectHandlerBase` abstract class

---

#### Task 1.2: Create SpecialEffectManager
**Priority:** Critical
**Estimated Time:** 45-60 minutes

Build the central coordinator for all effect handlers:

- **Registration system:**
  - Map effect IDs (from `SpecialEffects.cs` constants) to handler instances
  - Support multiple handlers per player
  - Track which effects are active per player

- **Core methods:**
  - `RegisterHandler(string effectId, ISpecialEffectHandler handler)`
  - `ApplyEffect(IServerPlayer player, string effectId, Blessing blessing)`
  - `RemoveEffect(IServerPlayer player, string effectId)`
  - `RemoveAllEffects(IServerPlayer player)` - Cleanup on logout/blessing removal
  - `UpdateEffects(float deltaTime)` - Tick all active effects

- **Event coordination:**
  - Subscribe to game events (damage, death, tick)
  - Route events to appropriate handlers
  - Handle edge cases (player disconnect, world reload)

**Deliverable:** `SpecialEffectManager.cs` system class

---

### Phase 2: Combat Damage Effects (2-3 hours)

#### Task 2.1: Implement Lifesteal Handlers
**Priority:** High
**Estimated Time:** 30-45 minutes

**Effect IDs:** `lifesteal_3`, `lifesteal_10`, `lifesteal_15`, `lifesteal_20`
**Used by:** 12 blessings across Khoras, Morthen, Aethra, and others

**Implementation:**
- Hook into `OnEntityReceiveDamage` event
- When player deals damage, heal player for X% of damage dealt
- Cap healing to prevent exploits with high-damage hits
- Display heal number particle effect (visual feedback)

**Configuration:**
```csharp
lifesteal_3: 0.03f (3%)
lifesteal_10: 0.10f (10%)
lifesteal_15: 0.15f (15%)
lifesteal_20: 0.20f (20%)
```

---

#### Task 2.2: Implement Damage Reduction Handler
**Priority:** High
**Estimated Time:** 20-30 minutes

**Effect ID:** `damage_reduction_10`
**Used by:** 8 blessings across Khoras, Morthen, Aethra, Gaia

**Implementation:**
- Hook into `OnEntityReceiveDamage` event (before damage applied)
- Reduce incoming damage by 10%
- Apply multiplicatively if multiple sources exist
- Log reduction for debugging

**Configuration:**
```csharp
damage_reduction_10: 0.10f (10% reduction)
```

---

#### Task 2.3: Implement Critical Strike Handlers
**Priority:** High
**Estimated Time:** 45-60 minutes

**Effect IDs:** `critical_chance_10`, `critical_chance_20`
**Used by:** 6 blessings across Lysa, Umbros

**Implementation:**
- Hook into attack/damage events
- RNG check (10% or 20% chance)
- On critical: multiply damage by 1.5x-2.0x
- Visual feedback (particle effect, sound)
- Prevent double-crits if player has multiple crit blessings (use highest)

**Configuration:**
```csharp
critical_chance_10: 0.10f (10% chance, 1.5x damage)
critical_chance_20: 0.20f (20% chance, 1.5x damage)
```

---

#### Task 2.4: Implement Headshot Bonus Handler
**Priority:** Medium
**Estimated Time:** 30-45 minutes

**Effect ID:** `headshot_bonus`
**Used by:** 1 blessing (Lysa - Master Huntress)

**Implementation:**
- Detect if attack hit entity's head hitbox
- Apply 2x damage multiplier on headshots
- Only applicable to ranged attacks
- Visual feedback on successful headshot

**Note:** Vintage Story's hitbox system may limit precision; research required.

---

#### Task 2.5: Implement Execute Threshold Handler
**Priority:** Medium
**Estimated Time:** 30-45 minutes

**Effect ID:** `execute_threshold`
**Used by:** 2 blessings (Morthen - Lord of Death, Umbros - Deadly Ambush)

**Implementation:**
- Hook into attack events
- Check target's health percentage
- If target health ≤ 15%, instant kill
- Visual/sound effect for execution
- Prevent execution on boss entities (optional balance rule)

**Configuration:**
```csharp
execute_threshold: 0.15f (15% health threshold)
```

---

### Phase 3: Damage Over Time & Auras (1-2 hours)

#### Task 3.1: Implement Poison DoT Handlers
**Priority:** High
**Estimated Time:** 45-60 minutes

**Effect IDs:** `poison_dot`, `poison_dot_strong`
**Used by:** 3 blessings (Morthen - Soul Reaper, Plague Bearer)

**Implementation:**
- Apply poison debuff to target on hit
- Track active poisons per entity (dictionary)
- Tick damage every 2 seconds for 10 seconds total
- Weak poison: 5% of base damage per tick
- Strong poison: 10% of base damage per tick
- Stackable or refresh duration on reapplication
- Visual: green particle effect on poisoned entities

**Configuration:**
```csharp
poison_dot: { tickDamage: 0.05f, duration: 10s, interval: 2s }
poison_dot_strong: { tickDamage: 0.10f, duration: 10s, interval: 2s }
```

---

#### Task 3.2: Implement Aura Handlers
**Priority:** Medium
**Estimated Time:** 45-60 minutes

**Effect IDs:** `plague_aura`, `death_aura`
**Used by:** 2 blessings (Morthen - Plague Bearer, Lord of Death)

**Implementation:**
- Create radius-based periodic damage zone around player
- Tick damage to enemies within radius every 1 second
- Plague Aura: 5 block radius, low damage
- Death Aura: 7 block radius, medium damage
- Exclude friendly entities (religion members)
- Visual: dark particle effects swirling around player

**Configuration:**
```csharp
plague_aura: { radius: 5, tickDamage: 2, interval: 1s }
death_aura: { radius: 7, tickDamage: 4, interval: 1s }
```

---

### Phase 4: Combat Mechanics (2-3 hours)

#### Task 4.1: Implement AoE Cleave Handler
**Priority:** High
**Estimated Time:** 60-90 minutes

**Effect ID:** `aoe_cleave`
**Used by:** 1 blessing (Khoras - Avatar of War)

**Implementation:**
- Hook into melee attack events
- On attack, detect all entities in 90-degree arc (3 block range)
- Apply damage to all entities in arc (70% of primary target damage)
- Cap maximum targets (3-5 enemies)
- Visual: sweep particle effect showing cleave arc

**Challenges:**
- Entity detection in arc/cone shape
- Performance with many entities
- Preventing self-damage

---

#### Task 4.2: Implement Multishot Handler
**Priority:** Medium
**Estimated Time:** 60-90 minutes

**Effect ID:** `multishot`
**Used by:** 1 blessing (Lysa - Avatar of the Hunt)

**Implementation:**
- Hook into ranged attack/arrow spawn events
- Spawn 2 additional projectiles at slight angles (+/- 10 degrees)
- Each projectile deals 60% of normal damage
- Consume only 1 arrow from inventory
- Visual: multiple projectile trails

**Challenges:**
- Vintage Story's projectile system compatibility
- Balancing damage to prevent OP behavior
- Performance with many projectiles

---

#### Task 4.3: Implement Stealth Bonus Handler
**Priority:** Medium
**Estimated Time:** 30-45 minutes

**Effect ID:** `stealth_bonus`
**Used by:** 4 blessings across Lysa, Umbros

**Implementation:**
- Reduce entity detection range for hostile mobs
- Decrease player visibility to other players (PvP)
- Modify entity AI awareness radius by -30%
- Optional: visual transparency effect on player

**Challenges:**
- Accessing and modifying entity AI detection
- PvP stealth mechanics balance

---

#### Task 4.4: Implement Tracking Vision Handler
**Priority:** Low
**Estimated Time:** 30-45 minutes

**Effect ID:** `tracking_vision`
**Used by:** 1 blessing (Lysa - Apex Predator)

**Implementation:**
- Highlight recent tracks/footprints of animals and players
- Show particle trail leading to tracked entities
- Increase tracking duration and visibility range
- Optional: minimap markers for tracked entities

**Challenges:**
- Vintage Story may not have built-in tracking system
- May require custom particle/visual solution

---

#### Task 4.5: Implement Animal Companion Handler
**Priority:** Low
**Estimated Time:** 60-90 minutes

**Effect ID:** `animal_companion`
**Used by:** 1 blessing (Lysa - Avatar of the Hunt)

**Implementation:**
- Spawn companion wolf/bear when blessing is unlocked
- Companion follows player and assists in combat
- Companion has 50% of player's health
- Respawn companion after 5 minutes if killed
- Despawn on player logout, respawn on login

**Challenges:**
- Entity AI integration
- Companion behavior (follow, attack, defend)
- Persistence across sessions
- Performance impact

---

### Phase 5: Religion Group Abilities (1-2 hours)

#### Task 5.1: Implement Religion War Cry Ability
**Priority:** Medium
**Estimated Time:** 30-45 minutes

**Effect ID:** `religion_war_cry`
**Used by:** 1 blessing (Khoras - Pantheon of War)

**Implementation:**
- Activatable ability (command or hotkey)
- Grant +20% damage buff to all religion members for 30 seconds
- 5 minute cooldown per activation
- Visual/audio feedback (war cry sound, buff particles)
- Show buff icon in UI

**Configuration:**
```csharp
war_cry: { damageBuff: 0.20f, duration: 30s, cooldown: 300s }
```

---

#### Task 5.2: Implement Religion Pack Tracking Ability
**Priority:** Medium
**Estimated Time:** 30-45 minutes

**Effect ID:** `religion_pack_tracking`
**Used by:** 1 blessing (Lysa - Hunter's Paradise)

**Implementation:**
- Show waypoints/markers for all religion members
- Display distance to each member
- Optional: minimap integration
- Update positions every 5 seconds
- Visual: colored particles/beams pointing to members

**Challenges:**
- UI/HUD integration for waypoint display
- Network syncing for multiplayer

---

#### Task 5.3: Implement Religion Death Mark Ability
**Priority:** Medium
**Estimated Time:** 30-45 minutes

**Effect ID:** `religion_death_mark`
**Used by:** 1 blessing (Morthen - Empire of Death)

**Implementation:**
- Activatable ability to mark enemy
- Marked enemy takes +15% damage from all religion members
- Duration: 20 seconds
- Cooldown: 60 seconds
- Visual: skull icon above marked enemy

**Configuration:**
```csharp
death_mark: { damageIncrease: 0.15f, duration: 20s, cooldown: 60s }
```

---

### Phase 6: Integration & Testing (1-2 hours)

#### Task 6.1: Integrate SpecialEffectManager with BlessingEffectSystem
**Priority:** Critical
**Estimated Time:** 30-45 minutes

**Implementation:**
- Modify `BlessingEffectSystem.ApplyBlessingsToPlayer()`:
  - After applying stat modifiers, apply special effects
  - Pass blessing's `SpecialEffects` list to `SpecialEffectManager`

- Modify `BlessingEffectSystem.RemoveBlessingsFromPlayer()`:
  - Before removing stat modifiers, remove special effects
  - Call `SpecialEffectManager.RemoveAllEffects(player)`

- Update refresh logic to reapply effects properly

**Key Changes:**
```csharp
// In ApplyBlessingsToPlayer, after stat application:
foreach (var blessing in activeBlessings) {
    foreach (var effectId in blessing.SpecialEffects) {
        _specialEffectManager.ApplyEffect(player, effectId, blessing);
    }
}
```

---

#### Task 6.2: Add Effect Cleanup on Blessing Removal/Player Logout
**Priority:** Critical
**Estimated Time:** 30-45 minutes

**Implementation:**
- Hook into player disconnect event
- Call `SpecialEffectManager.RemoveAllEffects(player)`
- Clean up any spawned entities (companions)
- Remove any active debuffs on other entities
- Save cooldown states to player data
- Restore cooldown states on player login

**Edge cases:**
- Sudden disconnect (crash/timeout)
- Server restart
- Blessing being locked after being unlocked

---

#### Task 6.3: Build and Test All Special Effect Handlers
**Priority:** Critical
**Estimated Time:** 30-45 minutes

**Testing checklist:**
- [ ] Lifesteal healing works correctly and displays feedback
- [ ] Damage reduction properly reduces incoming damage
- [ ] Critical strikes trigger at expected rate
- [ ] Headshot bonus applies extra damage
- [ ] Execute threshold instant kills low-health enemies
- [ ] Poison DoTs tick correctly and expire
- [ ] Auras damage nearby enemies periodically
- [ ] AoE Cleave hits multiple enemies
- [ ] Multishot fires multiple projectiles
- [ ] Stealth reduces detection range
- [ ] Tracking vision highlights tracks
- [ ] Animal companion spawns and fights
- [ ] Religion abilities activate and buff members
- [ ] Effects clean up on blessing removal
- [ ] Effects restore properly on player login
- [ ] No memory leaks or performance issues

**Build command:**
```bash
dotnet build PantheonWars/PantheonWars.csproj
```

---

## Technical Considerations

### Event Hooks Required

**Vintage Story API Events:**
- `OnEntityReceiveDamage` - Damage modification (lifesteal, crit, reduction)
- `OnEntityDeath` - Execute effects, death triggers
- `RegisterGameTickListener` - Periodic effects (DoTs, auras, cooldowns)
- Combat/Attack events - Cleave, multishot triggers
- Entity detection/AI - Stealth mechanics
- Player login/logout - Effect persistence and cleanup

### Data Storage Requirements

**Player-Specific Data:**
- Active effect tracking (Dictionary<string, ISpecialEffectHandler>)
- Active DoTs on entities (Dictionary<long, List<PoisonEffect>>)
- Aura sources and affected entities
- Companion entity references (long entityId)
- Ability cooldown timestamps

**Religion-Specific Data:**
- Cached religion member lists for group abilities
- Active group buff states
- Death mark targets and timestamps

### Performance Optimization

**Strategies:**
- Cache entity queries (nearby enemies for auras)
- Batch DoT damage ticks (process all poisons in single tick)
- Use spatial indexing for AoE detection
- Limit effect update frequency (not every frame)
- Unregister inactive handlers to prevent memory leaks

**Benchmarks:**
- Aura updates: Max once per second per player
- DoT ticks: Max once per 2 seconds per entity
- Entity detection: Use chunking to limit search radius

### Balance Considerations

**Damage Scaling:**
- Lifesteal should cap at reasonable values to prevent immortality
- Critical strikes should not stack multiplicatively
- Execute threshold should not work on boss entities
- AoE effects should have damage falloff or target caps

**Cooldowns:**
- Religion abilities should have significant cooldowns (5+ minutes)
- Prevent ability spam in PvP scenarios
- Consider global cooldown for multiple abilities

---

## Implementation Priority

### Must-Have (Phase 3.4 Completion):
1. Core architecture (SpecialEffectManager)
2. Lifesteal handlers (most commonly used)
3. Damage Reduction
4. Critical Strikes
5. Execute Threshold
6. Poison DoTs
7. Integration with BlessingEffectSystem

### Nice-to-Have (Can defer to Phase 3.5):
8. Auras (Plague, Death)
9. AoE Cleave
10. Multishot
11. Religion group abilities
12. Stealth Bonus

### Optional (Future Enhancement):
13. Tracking Vision
14. Animal Companion
15. Advanced visual effects
16. Sound effects for abilities

---

## Success Criteria

Phase 3.4 Special Effects implementation is complete when:

- [ ] All 20 tasks are implemented and tested
- [ ] Build succeeds with 0 errors
- [ ] All special effects function in-game without crashes
- [ ] Effects apply/remove correctly with blessing unlocks
- [ ] Player logout/login properly persists effect state
- [ ] No memory leaks or performance degradation
- [ ] Documentation updated with effect behaviors
- [ ] Ready for balance testing phase

**Next Phase:** Phase 3.5 - Integration & Polish (UI, data migration, testing)

---

## References

- **BlessingDefinitions.cs** - All blessing definitions with special effect lists
- **SpecialEffects.cs** - Effect ID constants
- **BlessingEffectSystem.cs** - Current stat modifier application system
- **balance_testing_guide.md** - Testing methodology for effects
- **phase3_task_breakdown.md** - Overall Phase 3 roadmap

---

**Document Status:** Ready for implementation
**Owner:** PantheonWars Development Team
**Last Review:** October 30, 2025
