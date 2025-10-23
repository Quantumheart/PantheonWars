# Pantheon Wars - Implementation Guide

**Version:** 0.1.0
**Last Updated:** October 2025

This guide outlines the development roadmap for Pantheon Wars, broken down into five major implementation phases. Each phase builds upon the previous, gradually expanding the mod from a basic MVP to a full-featured PvP deity worship system.

---

## Phase Overview

| Phase | Status | Completion | Focus |
|-------|--------|-----------|-------|
| Phase 1 | ✅ Complete | 6/6 | Foundation (MVP) |
| Phase 2 | 🟡 In Progress | 3/4 | Combat Integration |
| Phase 3 | 🔲 Planned | 0/4 | Full Ability System |
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
- `/deity list`, `/deity info <deity>`, `/deity select <deity>`, `/deity status`, `/favor`
- `/ability list`, `/ability info <ability>`, `/ability use <ability>`, `/ability cooldowns`

**GUI/HUD:**
- Deity selection dialog (Hotkey: K)
- Always-visible HUD showing deity, favor, and rank

**Playable Content:**
- 2 deities (Khoras and Lysa)
- 8 unique abilities with cooldowns and costs
- Full progression system (Initiate → Avatar)

---

## Phase 2: Combat Integration - 🟡 In Progress (3/4)

**Goal:** Integrate abilities deeply with Vintage Story's combat and entity stat systems.

### Tasks

- [ ] **Damage system hooks** ⚠️ **CRITICAL - NOT IMPLEMENTED**
  - Hook into `EntityAgent.ReceiveDamage()` to modify incoming damage
  - Hook into damage dealing to apply buff effects
  - Implement actual entity stats modifications
  - Track active buff/debuff states on entities

- [x] **Death penalties/favor loss**
  - Players lose 5 favor on death
  - Minimum favor floor (0)
  - Notifications on penalty

- [x] **Rival deity bonuses**
  - 2x favor for killing rival deity followers
  - 0.5x favor for killing allied deity followers
  - 0.5x favor for same deity (discourage infighting)

- [x] **Ability execution framework**
  - Complete validation system (deity, favor, cooldown, rank)
  - Server-authoritative execution
  - Error messaging and feedback
  - Cooldown tracking and persistence

### Current Status

**What's Working:**
- ✅ Ability framework executes properly
- ✅ Cooldowns and favor costs work
- ✅ Favor rewards/penalties work
- ✅ Relationship multipliers work

**What's Missing:**
- ❌ Abilities use MVP placeholder effects (chat notifications only)
- ❌ No actual damage modification
- ❌ No actual stat changes (attack speed, movement speed, etc.)
- ❌ No buff/debuff tracking on entities
- ❌ No visual indicators for active effects

### Next Steps for Phase 2

1. Implement entity stat modification system
2. Add buff/debuff tracking to entities
3. Hook into damage calculation system
4. Add visual indicators for active buffs/debuffs
5. Replace placeholder effects with real gameplay modifications

---

## Phase 3: Full Ability System - 🔲 Planned

**Goal:** Expand to all 8 deities with complete ability sets and visual effects.

### Planned Tasks

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

### Immediate (Phase 2 Completion)
1. Implement damage system hooks
2. Add real stat modifications for abilities
3. Implement buff/debuff tracking
4. Add visual feedback for active effects

### Short-term (Phase 3)
1. Design remaining 6 deities
2. Create 24 new abilities
3. Implement particle effects system
4. Balance all 32 abilities

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

- **Phase 2 Help:** Experience with Vintage Story combat/entity systems needed
- **Phase 3 Help:** Ability design, balancing, and particle effect creation
- **Phase 4 Help:** World generation, structure design, and territorial gameplay
- **Phase 5 Help:** Event systems, leaderboards, and advanced PvP mechanics

See the main [README.md](../README.md) for contribution guidelines.

---

## Version History

- **v0.1.0** - Phase 1 complete, Phase 2 in progress
- *Future versions will be documented here*
