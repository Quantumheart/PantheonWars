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
| Phase 3 | 🔲 Planned | 0/5 | Religion-Based Deity System with Perk Trees |
| Phase 4 | 🔲 Planned | 0/4 | Advanced Features |

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

## Phase 3 (OLD): Full Ability System - ⚠️ DEPRECATED

> **⚠️ This phase has been cancelled and replaced with the new Phase 3: Religion-Based Deity System with Perk Trees**
> The active ability system from Phases 1-2 will be removed in favor of passive perk trees tied to a custom religion system.
> See the new Phase 3 section below for the updated design.

**Original Goal:** Expand to all 8 deities with complete ability sets and visual effects.

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

## Phase 3: Religion-Based Deity System with Perk Trees - 🔲 Planned

**Goal:** Transform the mod from player-centric active abilities to religion-centric passive perk trees.

### Executive Summary

This is a **fundamental redesign** that moves deity assignment from individual players to custom player-created religions. The active ability system is completely replaced with passive perk trees that unlock based on dual ranking systems.

**See [phase3_group_deity_perks_guide.md](phase3_group_deity_perks_guide.md) and [phase3_task_breakdown.md](phase3_task_breakdown.md) for complete design specifications and implementation tasks.**

### Key Changes

1. **Custom Religion System**: Players create and join custom religions (not using VS built-in groups)
2. **Single Religion Per Player**: Players can only be in one religion at a time
3. **Religion-Based Deity**: A religion chooses a deity, and all members serve that deity
4. **Passive Perk Trees**: Replace active abilities with passive perks
5. **Dual Ranking**:
   - **Player Favor Ranks**: Individual progression (Initiate → Disciple → Zealot → Champion → Avatar)
   - **Religion Prestige Ranks**: Collective progression (Fledgling → Established → Renowned → Legendary → Mythic)
6. **80+ Unique Perks**: Each deity has 2 perk trees (player perks + religion perks)

### Sub-Phases

#### Phase 3.1: Foundation (Week 1-2)
- Create all data models (ReligionData, PlayerReligionData, Perk, new enums)
- Implement ReligionManager and PlayerReligionDataManager
- Build religion commands system (/religion create, join, leave, etc.)
- Implement persistence and invitation system
- Add religion switching cooldowns

#### Phase 3.2: Ranking Systems (Week 3)
- Implement ReligionPrestigeManager
- Integrate favor/prestige earning with PvP
- Create rank-up notifications for both systems
- Update HUD to display both player favor rank and religion prestige rank

#### Phase 3.3: Perk System Core (Week 4-5)
- Create PerkRegistry and PerkEffectSystem
- Implement perk unlock validation
- Build perk stat modifier system
- Create perk commands (/perks list, unlock, tree, etc.)
- Test perk application and effect stacking

#### Phase 3.4: Deity Perk Trees (Week 6-8)
- Design perk trees for all 8 deities (player + religion trees)
- Implement 80+ unique perks with deity-specific themes
- Code perk effects (stat modifiers and special effects)
- Balance testing across all deities
- Document all perks

#### Phase 3.5: Integration & Polish (Week 9-10)
- Remove old ability system (AbilitySystem, AbilityCooldownManager, all ability files)
- Implement data migration from Phase 1-2 format
- Create perk tree visualization UI
- Build religion management UI (browse, create, manage)
- Update HUD with religion/perk information
- Comprehensive end-to-end testing
- Update all documentation

### Deliverables

**New Systems:**
- ReligionManager - Manage all religions and congregations
- PlayerReligionDataManager - Track player-religion relationships
- ReligionPrestigeManager - Handle collective religion progression
- PerkRegistry - Central registry for all perks
- PerkEffectSystem - Apply passive perk effects to players

**New Data Models:**
- ReligionData - Store religion information (name, deity, members, prestige, perks)
- PlayerReligionData - Store player's religion membership and favor
- Perk - Define perk properties, requirements, and effects
- New Enums: PrestigeRank, FavorRank, PerkType, PerkCategory

**New Commands:**
- **Religion**: `/religion create`, `/religion join`, `/religion leave`, `/religion list`, `/religion info`, `/religion members`, `/religion invite`, `/religion kick`, `/religion disband`, `/religion description`
- **Perks**: `/perks list`, `/perks player`, `/perks religion`, `/perks info`, `/perks tree`, `/perks unlock`, `/perks active`

**New UI:**
- Religion Management Dialog (create/browse/join/manage religions)
- Perk Tree Viewer (visual perk tree with unlock status)
- Enhanced HUD (religion name, deity, both ranks, active perk count)

**Content:**
- 8 deities with complete perk trees
- 40+ player perks (5 tiers × 8 deities)
- 40+ religion perks (5 tiers × 8 deities)
- Public/private religion system
- Invitation system
- Religion switching with cooldowns

### Design Principles

- **Community Focus**: Encourage player cooperation through religions
- **Meaningful Choice**: Religion choice is significant due to switching penalties
- **Long-term Progression**: Perks persist and accumulate over time
- **Strategic Depth**: Perk choices and combinations create unique builds
- **Deity Identity**: Each deity has unique perk themes and playstyles

### Technical Notes

- **Single Religion Enforcement**: Simpler than multi-religion, prevents exploits
- **Custom Implementation**: Not using Vintage Story's built-in groups for full control
- **O(1) Deity Lookup**: Single religion per player enables fast deity determination
- **Cooldown System**: 7-day cooldown on religion switching prevents abuse
- **Stat Modifier Stacking**: Player perks + religion perks combine additively
- **Migration Path**: Existing Phase 1-2 saves auto-migrate to new format

### Estimated Timeline

**Total**: 10-12 weeks (121-154 hours)
- Phase 3.1: 16-22 hours
- Phase 3.2: 10-12 hours
- Phase 3.3: 17-21 hours
- Phase 3.4: 38-48 hours
- Phase 3.5: 40-51 hours

---

## Phase 4 (OLD): World Integration - 🔲 REMOVED

> **This phase has been removed from the roadmap.**
> The focus has shifted to the religion/perk system. World integration features (shrines, temples, territory control) may be revisited in the future as part of religion mechanics.

---

## Phase 4: Advanced Features - 🔲 Planned

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

### Immediate (Phase 3.1 - Next Up)
1. Create custom religion system data models
2. Implement ReligionManager and PlayerReligionDataManager
3. Build religion commands and invitation system
4. Implement persistence for religions and player data
5. Add religion switching cooldowns

### Short-term (Phase 3.2-3.3)
1. Implement dual ranking systems (Player Favor + Religion Prestige)
2. Create PerkRegistry and PerkEffectSystem
3. Build perk unlock validation and stat modifier system
4. Implement perk commands and HUD updates
5. Test perk application and effect stacking

### Medium-term (Phase 3.4-3.5)
1. Design and implement 80+ perks for all 8 deities
2. Code perk effects and balance testing
3. Create Perk Tree Viewer UI
4. Build Religion Management UI
5. Remove old ability system and migrate data

### Long-term (Phase 4)
1. Design divine duel system
2. Create crusade event framework
3. Implement relic system
4. Add apostate mechanics

---

## Contributing to Development

If you're interested in contributing to Pantheon Wars development, consider:

- **Phase 3 Help:** Religion system design, perk balancing, UI development, data migration
- **Phase 4 Help:** Event systems, leaderboards, advanced PvP mechanics

See the main [README.md](../README.md) for contribution guidelines.

---

## Version History

- **v0.2.0** - Phase 2 complete (buff/debuff system, all abilities functional)
- **v0.1.0** - Phase 1 complete (MVP foundation, 2 deities, 8 basic abilities)
- *Future versions will be documented here*
