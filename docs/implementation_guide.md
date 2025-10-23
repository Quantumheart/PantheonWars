# Pantheon Wars - Implementation Guide

**Version:** 0.2.0
**Last Updated:** October 2025
**Status:** Phase 2 Complete - Ready for Phase 3

This guide outlines the development roadmap for Pantheon Wars, broken down into five major implementation phases. Each phase builds upon the previous, gradually expanding the mod from a basic MVP to a full-featured PvP deity worship system.

---

## Phase Overview

| Phase | Status | Completion | Focus |
|-------|--------|-----------|-------|
| Phase 1 | ✅ Complete | 6/6 | Foundation (MVP) |
| Phase 2 | ✅ Complete | 4/4 | Combat Integration |
| Phase 3 | 🔲 Planned | 0/5 | Full Ability System |
| Phase 4 | 🔲 Planned | 0/4 | World Integration |
| Phase 5 | 🔲 Planned | 0/4 | Advanced Features |

---

## Phase 1: Foundation (MVP) - ✅ COMPLETE

**Goal:** Establish core systems and deliver a playable MVP with 2 deities.

### Tasks Completed

- [x] **Project structure setup**
  - Build system configured (CakeBuild)
  - Launch configurations for debugging
  - Proper namespace organization
  - Assembly configuration

- [x] **Deity registration system**
  - `DeityRegistry.cs` managing all deities
  - Complete deity data model with relationships
  - 2 fully defined deities (Khoras and Lysa)
  - Deity relationship system (Allied/Rival mechanics)

- [x] **Player deity selection GUI**
  - `DeitySelectionDialog.cs` with functional interface
  - Radio button selection with deity information
  - Hotkey activation (K)
  - Confirmation system

- [x] **Basic favor tracking**
  - `FavorSystem.cs` with PvP rewards
  - Deity relationship multipliers (2x rival, 0.5x allied)
  - Death penalties (5 favor loss)
  - `FavorHudElement.cs` for HUD display

- [x] **Data persistence**
  - `PlayerDataManager.cs` for save/load
  - `AbilityCooldownManager.cs` for cooldown persistence
  - World storage integration
  - Automatic save on disconnect/server save
  - Automatic load on join/server start

- [x] **1-2 deities with basic abilities**
  - Khoras (War): 4 abilities
  - Lysa (Hunt): 4 abilities
  - Complete ability framework with cooldowns
  - Favor costs and rank requirements
  - Full validation and execution system

### Deliverables

**Systems Implemented:**
- Deity Registry
- Ability Registry
- Player Data Manager
- Ability Cooldown Manager
- Favor System
- Ability System

**Commands Available:**
- **Deity:** `/deity list`, `/deity info <deity>`, `/deity select <deity>`, `/deity status`
- **Ability:** `/ability list`, `/ability info <ability>`, `/ability use <ability>`, `/ability cooldowns`
- **Favor:** `/favor`, `/favor info`, `/favor stats`, `/favor ranks`
- **Admin:** `/favor set <amount>`, `/favor add <amount>`, `/favor remove <amount>`, `/favor reset`, `/favor max`, `/favor settotal <amount>`

**GUI/HUD:**
- Deity selection dialog (Hotkey: K)
- Always-visible HUD showing deity, favor, and rank

**Playable Content:**
- 2 deities (Khoras and Lysa)
- 8 unique abilities with cooldowns and costs
- Full progression system (Initiate → Avatar)

---

## Phase 2: Combat Integration - ✅ COMPLETE (4/4)

**Goal:** Integrate abilities deeply with Vintage Story's combat and entity stat systems.

### Tasks Completed

- [x] **Buff/Debuff system implementation**
  - Created `ActiveEffect.cs` data model for tracking buffs/debuffs
  - Implemented `EntityBehaviorBuffTracker.cs` for entity-level effect tracking
  - Built `BuffManager.cs` for centralized buff application and management
  - Duration-based effects with automatic expiration (0.5s tick updates)
  - Stat modifier accumulation (multiple buffs stack additively)
  - Persistence to entity attributes (survives server restarts)
  - Damage modification hooks via `EntityBehaviorHealth`
  - Proper cleanup with player notifications on expiration

- [x] **Entity stat modification system**
  - Integration with Vintage Story's `entity.Stats` system
  - Support for multiple stat types:
    - `walkspeed` - Movement speed
    - `meleeWeaponsSpeed` / `rangedWeaponsSpeed` - Attack speed
    - `meleeDamageMultiplier` / `rangedDamageMultiplier` - Damage output
    - `receivedDamageMultiplier` - Damage reduction (defensive buffs)
    - `receivedDamageAmplification` - Damage amplification (debuffs)
    - `critChance` - Critical hit chance (for future expansion)
  - Real-time stat application and removal
  - Stat changes visible in game mechanics

- [x] **Death penalties and favor rewards**
  - Players lose 5 favor on death
  - Minimum favor floor (0)
  - Death notifications to player
  - Favor gain on PvP kills with relationship multipliers:
    - Rival deity: 2x favor (20 instead of 10)
    - Allied deity: 0.5x favor (5 instead of 10)
    - Same deity: 0.5x favor (discourages infighting)
    - No deity: 1.0x favor (base 10)

- [x] **Complete ability implementation (8/8 abilities)**
  - **Khoras (War):**
    - War Banner: Real AoE +20% damage buff for 15s
    - Battle Cry: Real +30% attack speed buff for 10s
    - Last Stand: Real 50% damage reduction for 12s (requires <30% HP)
    - Blade Storm: Working damage ability (5 damage, 4-block radius)
  - **Lysa (Hunt):**
    - Hunter's Mark: Real +25% damage taken debuff for 20s (raycast targeting)
    - Swift Feet: Real +50% movement speed buff for 8s
    - Arrow Rain: Working AoE damage ability (4 damage, 5-block radius)
    - Predator Instinct: Real +25% crit, +10% damage buff for 15s with entity detection

### Systems Implemented

**New Files Created:**
- `Systems/BuffSystem/ActiveEffect.cs` - Buff/debuff data model
- `Systems/BuffSystem/EntityBehaviorBuffTracker.cs` - Entity behavior for tracking effects
- `Systems/BuffSystem/BuffManager.cs` - Central buff management

**Modified Systems:**
- `PantheonWarsSystem.cs` - Registered `EntityBehaviorBuffTracker`, integrated `BuffManager`
- `AbilitySystem.cs` - Updated to pass `BuffManager` to ability execution
- `Models/Ability.cs` - Added `BuffManager` parameter to `Execute()` method
- All 8 ability files - Replaced MVP stubs with real stat modifications

### Technical Achievements

- **Architecture:** Followed xskills mod patterns for VS stat integration
- **Performance:** Efficient 0.5s tick-based updates for duration management
- **Persistence:** Effects survive server restarts via entity attributes
- **Scalability:** Easy to add new stat types and effect behaviors
- **Separation of Concerns:** BuffManager handles all effect logic separately from abilities
- **Compilation:** Clean build with 0 errors, 7 warnings (nullable annotations only)

### What's Working

- ✅ All 8 abilities have real gameplay effects (not MVP placeholders)
- ✅ Damage modification working (buffs increase damage, debuffs reduce resistance)
- ✅ Stat modifications working (attack speed, movement speed, damage resistance)
- ✅ Buff/debuff tracking on entities with duration management
- ✅ Automatic expiration with notifications
- ✅ Multiple buff stacking (additive accumulation)
- ✅ Favor rewards/penalties with deity relationship multipliers
- ✅ Ready for in-game testing

---

## Phase 3: Full Ability System - 🔲 Planned

**Goal:** Expand to all 8 deities with complete ability sets and visual effects.

### Planned Tasks

- [ ] **Hotkey-based ability activation system**
  - Replace command-based activation (`/ability use`) with hotkey binding
  - Single equipped ability slot (player can only have 1 ability equipped at a time)
  - Hotkey to activate equipped ability (default: R or configurable)
  - Ability equip/swap GUI or command (`/ability equip <ability_id>`)
  - Swap cooldown system (prevent rapid ability switching mid-combat)
  - Swap cooldown duration: 30-60 seconds (configurable)
  - Visual indicator showing currently equipped ability in HUD
  - Persistent equipped ability (saved with player data)
  - Equip validation (must own deity, meet rank requirements)
  - Clear feedback when trying to activate with no equipped ability

- [ ] **All 8 deities implemented**
  - Morthen (Death) - 4 abilities
  - Aethra (Light) - 4 abilities
  - Umbros (Shadows) - 4 abilities
  - Tharos (Storms) - 4 abilities
  - Gaia (Earth) - 4 abilities
  - Vex (Madness) - 4 abilities

- [ ] **Complete ability sets**
  - 32 total abilities (8 deities × 4 abilities)
  - Unique mechanics per deity theme
  - Balanced cooldowns and costs
  - Synergies within deity ability sets

- [ ] **Visual effects**
  - Particle effects for ability activation
  - Visual indicators for buffs/debuffs
  - Deity-themed color schemes
  - Animation hooks where applicable

- [ ] **Devotion progression**
  - Rank-specific ability unlocks
  - Enhanced effects at higher ranks
  - Rank titles and visual indicators
  - Rank-based favor multipliers

### Design Notes

- Each deity should have a unique playstyle identity
- Abilities should synergize with deity themes
- Visual clarity important for PvP readability
- Balance across all 8 deities

**Hotkey System Design Considerations:**
- Single equipped ability forces strategic choice (can't use all 4 at once)
- Swap cooldown prevents ability cycling during combat
- Encourages pre-planning: equip the right ability for the situation
- Rewards player skill: knowing when to swap vs when to commit
- PvP balance: opponents can predict equipped ability from combat patterns
- Suggested swap cooldown: 45 seconds (balance between flexibility and commitment)

### Technical Implementation Requirements

**New Systems Needed:**

1. **AbilitySlotManager.cs**
   - Track currently equipped ability per player
   - Validate equip requests (deity ownership, rank requirements)
   - Persist equipped ability to PlayerDeityData
   - Provide getter for equipped ability ID

2. **AbilitySwapCooldownManager.cs**
   - Track last ability swap timestamp per player
   - Enforce swap cooldown (default: 45 seconds, configurable)
   - Persist swap cooldown across server restarts
   - Provide remaining cooldown calculation

3. **Hotkey Registration**
   - Register ability activation hotkey (client-side)
   - Default binding: R key
   - Send network packet to server on hotkey press
   - Server validates and executes equipped ability

4. **HUD Updates**
   - Add equipped ability display to FavorHudElement
   - Show ability name, icon (if available), and cooldown
   - Visual indication when swap cooldown is active
   - Update display on ability equip/swap

5. **Network Protocol**
   - Client → Server: "Activate equipped ability" packet
   - Client → Server: "Equip ability" packet with ability ID
   - Server → Client: "Equipped ability updated" packet
   - Server → Client: "Swap cooldown active" error packet

6. **Data Model Updates**
   - Add `EquippedAbilityId` to PlayerDeityData
   - Add `LastAbilitySwapTime` to PlayerDeityData or separate manager
   - Ensure persistence on save/load

7. **Command Updates**
   - Deprecate `/ability use <ability_id>` (keep for testing/admin)
   - Add `/ability equip <ability_id>` command
   - Add `/ability equipped` command to show current
   - Update `/ability list` to highlight equipped ability

---

## Phase 4: World Integration - 🔲 Planned

**Goal:** Add persistent world structures and territory control mechanics.

### Planned Tasks

- [ ] **Shrine blocks**
  - Craftable shrine blocks for each deity
  - Shrine blessing mechanics
  - Shrine defense/attack gameplay
  - Visual indicators for shrine ownership

- [ ] **Temple world generation**
  - Procedural temple generation for each deity
  - Unique architecture per deity theme
  - Loot tables and rewards
  - Boss encounters in temples

- [ ] **Sacred ground system**
  - Territory control mechanics
  - Blessing zones around controlled areas
  - Favor bonuses in controlled territory
  - Visual boundaries and indicators

- [ ] **Temple capture mechanics**
  - Siege mechanics for temple control
  - Capture point system
  - Defense bonuses for controlling deity
  - Rewards for successful captures

### Design Notes

- World structures should encourage PvP conflict
- Temples as focal points for deity worship
- Balance between solo and group activities
- Long-term goals for player investment

---

## Phase 5: Advanced Features - 🔲 Planned

**Goal:** Add end-game features and advanced PvP mechanics.

### Planned Tasks

- [ ] **Divine duels**
  - Formal 1v1 challenge system
  - Favor stakes (winner takes portion)
  - Arena creation or designated duel zones
  - Spectator mode
  - Leaderboards

- [ ] **Crusade events**
  - Server-wide deity war events
  - Team-based objectives
  - Massive favor rewards
  - Temporary alliance mechanics
  - Event-specific rewards

- [ ] **Relic system**
  - Rare, powerful artifacts
  - Temporary dominance bonuses
  - Artifact hunting mechanics
  - Artifact theft/defense gameplay
  - Time-limited ownership

- [ ] **Apostate mechanics**
  - Deity switching penalties
  - Apostate status and consequences
  - Redemption mechanics
  - Hunted by former deity followers
  - Path back to good standing

### Design Notes

- End-game content for veteran players
- High-stakes PvP encounters
- Server community events
- Long-term replayability

---

## Development Priorities

### Immediate (Phase 3 - Next Up)
1. Design and implement hotkey-based ability activation system
2. Add equipped ability slot with swap cooldown
3. Update HUD to show equipped ability
4. Design remaining 6 deities
5. Create 24 new abilities

### Short-term (Phase 3 Completion)
1. Implement particle effects system for abilities
2. Add visual buff/debuff indicators in HUD
3. Balance all 32 abilities across 8 deities
4. Add deity-themed visual effects

### Medium-term (Phase 4)
1. Design temple structures
2. Implement world generation hooks
3. Create territory control system
4. Add shrine blocks and mechanics

### Long-term (Phase 5)
1. Design duel system
2. Create event framework
3. Implement relic system
4. Add apostate mechanics

---

## Contributing to Development

If you're interested in contributing to Pantheon Wars development, consider:

- **Phase 3 Help:** Hotkey system implementation, ability design, balancing, and particle effects
- **Phase 4 Help:** World generation, structure design, and territorial gameplay
- **Phase 5 Help:** Event systems, leaderboards, and advanced PvP mechanics

See the main [README.md](../README.md) for contribution guidelines.

---

## Version History

- **v0.2.0** - Phase 2 complete (buff/debuff system, all abilities functional)
- **v0.1.0** - Phase 1 complete (MVP foundation, 2 deities, 8 basic abilities)
- *Future versions will be documented here*
