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
| Phase 3 | ⚠️ In Progress | 3.5/5 | Religion-Based Deity System with Blessing Trees |
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

> **⚠️ This phase has been cancelled and replaced with the new Phase 3: Religion-Based Deity System with Blessing Trees**
> The active ability system from Phases 1-2 will be removed in favor of passive blessing trees tied to a custom religion system.
> See the new Phase 3 section below for the updated design.
>
> **📊 SCOPE REDUCTION APPLIED:** Originally planned for 160 blessings (20 per deity), the system has been **reduced to 80 blessings (10 per deity)** for better balance, faster development, and more meaningful progression. See `ScopeReduction.md` for full rationale.

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

## Phase 3: Religion-Based Deity System with Blessing Trees - ⚠️ IN PROGRESS (~75-80% Complete)

**Goal:** Transform the mod from player-centric active abilities to religion-centric passive blessing trees.

### Executive Summary

This is a **fundamental redesign** that moves deity assignment from individual players to custom player-created religions. The active ability system is completely replaced with passive blessing trees that unlock based on dual ranking systems.

**📊 SCOPE REDUCTION:** Reduced from 160 blessings to **80 blessings total** (10 per deity: 6 player + 4 religion). This 50% reduction improves balance, accelerates development, and makes each blessing more meaningful. See `ScopeReduction.md` for rationale.

**✅ ALL 80 BLESSINGS FULLY DEFINED:** All 8 deities have complete blessing definitions in `BlessingDefinitions.cs` with proper stat modifiers and structure.

**See [phase3_group_deity_blessings_guide.md](phase3_group_deity_blessings_guide.md) and [phase3_task_breakdown.md](phase3_task_breakdown.md) for complete design specifications and implementation tasks.**

### Key Changes

1. **Custom Religion System**: Players create and join custom religions (not using VS built-in groups)
2. **Single Religion Per Player**: Players can only be in one religion at a time
3. **Religion-Based Deity**: A religion chooses a deity, and all members serve that deity
4. **Passive Blessing Trees**: Replace active abilities with passive blessings
5. **Dual Ranking**:
   - **Player Favor Ranks**: Individual progression (Initiate → Disciple → Zealot → Champion → Avatar)
   - **Religion Prestige Ranks**: Collective progression (Fledgling → Established → Renowned → Legendary → Mythic)
6. **80 Unique Blessings** (Reduced Scope): Each deity has 10 blessings total (6 player blessings + 4 religion blessings)
   - Player Blessings: Tier 1 (1) → Tier 2 (2 paths) → Tier 3 (2 specializations) → Tier 4 (1 capstone)
   - Religion Blessings: Tier 1 (1) → Tier 2 (1) → Tier 3 (1) → Tier 4 (1)

### Sub-Phases Progress

#### Phase 3.1: Foundation - ✅ COMPLETED (Week 1-2)
- [x] Create all data models (ReligionData, PlayerReligionData, Blessing, new enums)
- [x] Implement ReligionManager and PlayerReligionDataManager
- [x] Build religion commands system (/religion create, join, leave, etc.)
- [x] Implement persistence and invitation system
- [x] Add religion switching cooldowns
- [x] Integrate with PantheonWarsSystem

**Status**: All 10 religion commands functional, persistence working, cooldown system implemented.

#### Phase 3.2: Ranking Systems - ✅ COMPLETED (Week 3)
- [x] Implement ReligionPrestigeManager
- [x] Integrate favor/prestige earning with PvP
- [x] Create rank-up notifications for both systems
- [x] Update HUD to display both player favor rank and religion prestige rank
- [x] Network synchronization via PlayerReligionDataPacket

**Status**: Dual ranking system fully functional, PvP integration complete, HUD displaying all data.

#### Phase 3.3: Blessing System Core - ✅ COMPLETED (Week 4-5) - Completed Oct 24, 2025
- [x] Create BlessingRegistry and BlessingEffectSystem
- [x] Implement blessing unlock validation
- [x] Build blessing stat modifier system
- [x] Create blessing commands (/blessings list, unlock, tree, etc.)
- [x] Implement ApplyBlessingsToPlayer() using VS Stats API
- [x] Implement RemoveBlessingsFromPlayer() for cleanup
- [x] Test blessing application and effect stacking

**Status**: All 7 blessing commands implemented, stat application system complete, **all 80 blessings registered in BlessingRegistry**. Stat modifiers fully functional. Minor gap: ReligionPrestigeManager.CheckForNewBlessingUnlocks() is a placeholder (~1 hour fix).

#### Phase 3.4: Deity Blessing Trees - ✅ 90% COMPLETE (Week 6-8)
- [x] Design blessing trees for Khoras (War) - 10 blessings ✅ (6 player + 4 religion)
- [x] Design blessing trees for Lysa (Hunt) - 10 blessings ✅ (6 player + 4 religion)
- [x] Design blessing trees for Morthen (Death) - 10 blessings ✅ (6 player + 4 religion)
- [x] Design blessing trees for Aethra (Light) - 10 blessings ✅ (6 player + 4 religion)
- [x] Design blessing trees for Umbros (Shadows) - 10 blessings ✅ (6 player + 4 religion)
- [x] Design blessing trees for Tharos (Storms) - 10 blessings ✅ (6 player + 4 religion)
- [x] Design blessing trees for Gaia (Earth) - 10 blessings ✅ (6 player + 4 religion)
- [x] Design blessing trees for Vex (Madness) - 10 blessings ✅ (6 player + 4 religion)
- [ ] Balance testing across all deities
- [ ] Document all blessings in user-facing format

**Status**: **All 80 blessings fully defined in BlessingDefinitions.cs** (8/8 deities complete). Stat modifiers fully working and registered in BlessingRegistry. Special effects defined but handlers not yet implemented.

**Remaining Work**:
- Special effect handlers not yet implemented (lifesteal, poison_dot, critical_strike, stealth_bonus, tracking_vision, aoe_cleave, damage_reduction, etc.) - ~8-10 hours
- Balance testing pending (~4-6 hours)
- User-facing blessing documentation needed (~2-3 hours)

#### Phase 3.5: Integration & Polish - ⚠️ 30% COMPLETE (Week 9-10)
- [ ] Remove old ability system (AbilitySystem, AbilityCooldownManager, all ability files)
- [ ] Implement data migration from Phase 1-2 format
- [ ] Create blessing tree visualization UI (BlessingTreeDialog) ❌ Not started
- [x] Build religion management UI (ReligionManagementDialog) ✅ Complete
- [x] Build supporting dialogs (CreateReligionDialog, InvitePlayerDialog, EditDescriptionDialog) ✅ Complete
- [x] Network packet system for religion actions ✅ Complete
- [x] Update HUD with religion/blessing information ✅ Complete (done in Phase 3.2)
- [ ] Comprehensive end-to-end testing
- [ ] Update all documentation

**Status**: Religion management GUI fully functional with tabbed interface, network sync, and all supporting dialogs. Remaining work: BlessingTreeDialog, old system removal, data migration, testing, and documentation.

### Deliverables

**New Systems (Implemented):**
- ✅ ReligionManager - Manage all religions and congregations (353 lines)
- ✅ PlayerReligionDataManager - Track player-religion relationships (365 lines)
- ✅ ReligionPrestigeManager - Handle collective religion progression (244 lines)
- ✅ BlessingRegistry - Central registry for all blessings (191 lines, **80/80 blessings registered**)
- ✅ BlessingEffectSystem - Apply passive blessing effects to players (471 lines)

**New Data Models (Implemented):**
- ✅ ReligionData - Store religion information (name, deity, members, prestige, blessings)
- ✅ PlayerReligionData - Store player's religion membership and favor
- ✅ Blessing - Define blessing properties, requirements, and effects
- ✅ New Enums: PrestigeRank, FavorRank, BlessingType, BlessingCategory

**New Commands (Implemented):**
- ✅ **Religion**: `/religion create`, `/religion join`, `/religion leave`, `/religion list`, `/religion info`, `/religion members`, `/religion invite`, `/religion kick`, `/religion disband`, `/religion description`
- ✅ **Blessings**: `/blessings list`, `/blessings player`, `/blessings religion`, `/blessings info`, `/blessings tree`, `/blessings unlock`, `/blessings active`

**New UI:**
- ✅ Religion Management Dialog (create/browse/join/manage religions) - Fully functional with tabs
- ✅ Create Religion Dialog, Invite Player Dialog, Edit Description Dialog
- ❌ Blessing Tree Viewer (visual blessing tree with unlock status) - Not yet started
- ✅ Enhanced HUD (religion name, deity, both ranks, favor/prestige display)

**Content (Complete):**
- ✅ **8/8 deities with complete blessing trees** (Khoras, Lysa, Morthen, Aethra, Umbros, Tharos, Gaia, Vex)
- ✅ **48/48 player blessings fully defined** (100%) - 6 blessings per deity
- ✅ **32/32 religion blessings fully defined** (100%) - 4 blessings per deity
- ✅ **Total: 80/80 blessings (100% defined and registered)**
- ✅ Stat modifiers functional and tested
- ⚠️ Special effects defined but handlers not yet implemented
- ✅ Public/private religion system
- ✅ Invitation system
- ✅ Religion switching with 7-day cooldown

### Design Principles

- **Community Focus**: Encourage player cooperation through religions
- **Meaningful Choice**: Religion choice is significant due to switching penalties
- **Long-term Progression**: Blessings persist and accumulate over time
- **Strategic Depth**: Blessing choices and combinations create unique builds
- **Deity Identity**: Each deity has unique blessing themes and playstyles

### Technical Notes

- **Single Religion Enforcement**: Simpler than multi-religion, prevents exploits
- **Custom Implementation**: Not using Vintage Story's built-in groups for full control
- **O(1) Deity Lookup**: Single religion per player enables fast deity determination
- **Cooldown System**: 7-day cooldown on religion switching prevents abuse
- **Stat Modifier Stacking**: Player blessings + religion blessings combine additively
- **Migration Path**: Existing Phase 1-2 saves auto-migrate to new format

### Timeline

**Original Estimate (160 blessings)**: 10-12 weeks (121-154 hours)
**Revised Estimate (80 blessings)**: 8-9 weeks (~95-115 hours) - **34 hours saved**

**Completed**:
- Phase 3.1: ✅ ~16-22 hours (Foundation)
- Phase 3.2: ✅ ~10-12 hours (Ranking Systems)
- Phase 3.3: ✅ ~17-21 hours (Blessing System Core)
- Phase 3.4: ✅ ~20-24 hours (Deity Blessing Trees - all 80 blessings defined)

**In Progress**:
- Phase 3.4 (Final): ⚠️ ~8-10 hours remaining (Special effect handlers + balance testing)
- Phase 3.5: ⚠️ ~12-15 hours completed / ~40-51 hours total (30% - Integration & Polish)

**Remaining**:
- Phase 3.4 Final: ~8-10 hours (special effects handlers + balance testing + documentation)
- Phase 3.5 Remainder: ~25-36 hours (BlessingTreeDialog, old system removal, migration, testing)

**Total Progress**: ~75-80% complete (~75-89 hours completed, ~22-28 hours remaining)

### Current Status Summary (Updated: Nov 3, 2025)

**What's Working:**
- ✅ Religion system fully functional (create, join, leave, manage)
- ✅ Favor/Prestige progression working with PvP integration
- ✅ Blessing unlocking and persistence working
- ✅ All 17 commands working (10 religion + 7 blessing)
- ✅ HUD displaying religion and rank data
- ✅ Network sync working (PlayerReligionDataPacket + 5 other packets)
- ✅ Stat application system **FULLY IMPLEMENTED** (ApplyBlessingsToPlayer/RemoveBlessingsFromPlayer)
- ✅ **All 80/80 blessings designed and registered in BlessingRegistry** (100% complete)
- ✅ Religion Management GUI with full tabbed interface
- ✅ All supporting dialogs (Create/Invite/Edit)

**📊 Scope Reduction Benefits:**
- ✅ Reduced from 160 → 80 blessings (50% reduction)
- ✅ Saves ~34 hours of development time
- ✅ Each blessing becomes more meaningful and impactful
- ✅ Easier to balance with fewer interactions
- ✅ More achievable goal for players (can max out a deity)

**Remaining Gaps:**
1. **Special Effects Incomplete** - Stat modifiers work perfectly, but special effect handlers need implementation (lifesteal, poison_dot, critical_strike, stealth_bonus, tracking_vision, aoe_cleave, etc.) - ~8-10 hours
2. **Blessing Tree GUI Incomplete** - Have command-based tree view, need visual BlessingTreeDialog - ~6-8 hours
3. **No Balance Testing** - Need to test all 8 deities for blessing value balance - ~4-6 hours
4. **Old System Not Removed** - AbilitySystem coexists but not yet cleaned up - ~3-4 hours
5. **No Data Migration** - Need migration from Phase 1-2 format - ~4-5 hours
6. **Minor: ReligionPrestigeManager.CheckForNewBlessingUnlocks()** - Placeholder function needs 1-hour fix

**Recommended Next Steps:**
1. Implement special effect handlers for complex blessing mechanics (~8-10 hours) - **HIGHEST PRIORITY**
2. Fix ReligionPrestigeManager.CheckForNewBlessingUnlocks() placeholder (~1 hour)
3. Create BlessingTreeDialog visual interface (~6-8 hours)
4. Balance testing with all 8 deities (~4-6 hours)
5. Remove old ability system (~3-4 hours)
6. Implement data migration (~4-5 hours)
7. User-facing blessing documentation (~2-3 hours)
8. Comprehensive testing (~4-6 hours)

---

## Phase 4 (OLD): World Integration - 🔲 REMOVED

> **This phase has been removed from the roadmap.**
> The focus has shifted to the religion/blessing system. World integration features (shrines, temples, territory control) may be revisited in the future as part of religion mechanics.

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

### Immediate (Phase 3.4 Final - Current Focus)
1. ✅ ~~Complete in-game testing of stat application system~~ **DONE**
2. ✅ ~~Complete all 8 deity blessing designs~~ **DONE - All 80 blessings defined**
3. **Implement special effect handlers** (~8-10 hours) - **HIGHEST PRIORITY**
   - Lifesteal (3%, 10%, 15%, 20% variants)
   - Poison DoT, Plague Aura, Death Aura
   - Critical Strike (10%, 20% chance)
   - Stealth Bonus, Tracking Vision
   - AoE Cleave, Multishot
   - Damage Reduction (10%), Execute Threshold
   - Headshot Bonus, Animal Companion
4. **Fix ReligionPrestigeManager.CheckForNewBlessingUnlocks()** (~1 hour)
5. **Balance testing across all 8 deities** (~4-6 hours)
6. **Document all 80 blessings in user-facing format** (~2-3 hours)

### Short-term (Phase 3.5 Completion)
1. Create BlessingTreeDialog visual interface (~6-8 hours)
2. Remove old ability system (AbilitySystem, AbilityCooldownManager, all abilities) (~3-4 hours)
3. Implement data migration from Phase 1-2 format (~4-5 hours)
4. Comprehensive end-to-end testing (~4-6 hours)
5. Update all documentation (~3-4 hours)
6. Performance testing with multiple religions and players (~2-3 hours)
7. Bug fixes and refinement (~2-3 hours)

### Long-term (Phase 4)
1. Design divine duel system
2. Create crusade event framework
3. Implement relic system
4. Add apostate mechanics

---

## Contributing to Development

If you're interested in contributing to Pantheon Wars development, consider:

- **Phase 3.4 Final (URGENT - ~15-20 hours remaining):**
  - ✅ ~~Designing all 8 deity blessing trees~~ **COMPLETE - All 80 blessings defined**
  - Implementing special effect handlers (lifesteal, poison, critical strikes, stealth, tracking, etc.) - **HIGHEST PRIORITY**
  - Balance testing and blessing value refinement
  - Fixing ReligionPrestigeManager.CheckForNewBlessingUnlocks() placeholder
  - User-facing blessing documentation
- **Phase 3.5 Help (~25-36 hours remaining):**
  - UI development (BlessingTreeDialog for visual blessing tree)
  - ✅ ~~ReligionManagementDialog~~ **COMPLETE**
  - Data migration implementation from Phase 1-2 to Phase 3
  - Old ability system removal
  - Comprehensive testing
  - Documentation updates
- **Phase 4 Help (Future):**
  - Event systems, leaderboards, advanced PvP mechanics

See the main [README.md](../README.md) for contribution guidelines.

---

## Version History

- **v0.3.9** (Nov 3, 2025) - Phase 3.4 blessing definitions complete (All 80 blessings defined and registered)
- **v0.3.7** (Oct 30, 2025) - Phase 3.5 GUI work (ReligionManagementDialog + supporting dialogs)
- **v0.3.5** (Oct 24, 2025) - Phase 3.3 complete (Blessing system core, stat application implemented)
- **v0.3.2** - Phase 3.2 complete (Dual ranking systems, prestige manager)
- **v0.3.1** - Phase 3.1 complete (Religion system foundation, 10 commands)
- **v0.2.0** - Phase 2 complete (Buff/debuff system, all abilities functional)
- **v0.1.0** - Phase 1 complete (MVP foundation, 2 deities, 8 basic abilities)
- *Future versions will be documented here*
