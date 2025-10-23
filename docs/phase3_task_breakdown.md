# Phase 4 Implementation Task Breakdown

This document provides a detailed, actionable task list for implementing the Religion-Based Deity System with Perk Trees as outlined in `PHASE4_GROUP_DEITY_PERKS_GUIDE.md`.

---

## Phase 4.1: Foundation (Week 1-2)
**Goal**: Build custom religion system foundation

### Task 1.1: Create Data Models
**Estimated Time**: 2-3 hours

- [ ] Create `Models/PrestigeRank.cs` enum
  - [ ] Define 5 ranks: Fledgling, Established, Renowned, Legendary, Mythic
  - [ ] Add rank thresholds in comments
- [ ] Create `Models/FavorRank.cs` enum
  - [ ] Define 5 ranks: Initiate, Disciple, Zealot, Champion, Avatar
  - [ ] Add rank thresholds in comments
- [ ] Create `Models/PerkType.cs` enum
  - [ ] Define: Player, Religion
- [ ] Create `Models/PerkCategory.cs` enum
  - [ ] Define: Combat, Defense, Mobility, Utility, Economic, Territory
- [ ] Create `Models/Perk.cs` class
  - [ ] Add all properties: PerkId, Name, Description, Deity, Type, Category
  - [ ] Add unlock requirements: RequiredFavorRank, RequiredPrestigeRank
  - [ ] Add PrerequisitePerks list
  - [ ] Add StatModifiers dictionary
  - [ ] Add SpecialEffects list
  - [ ] Add constructor
- [ ] Create `Data/ReligionData.cs` class
  - [ ] Add all properties: ReligionUID, ReligionName, Deity, FounderUID
  - [ ] Add MemberUIDs list
  - [ ] Add PrestigeRank, Prestige, TotalPrestige
  - [ ] Add CreationDate
  - [ ] Add UnlockedPerks dictionary
  - [ ] Add IsPublic flag
  - [ ] Add Description string
  - [ ] Add constructors
  - [ ] Add helper methods (AddMember, RemoveMember, etc.)
- [ ] Create `Data/PlayerReligionData.cs` class
  - [ ] Add all properties: PlayerUID, ReligionUID (nullable)
  - [ ] Add ActiveDeity, FavorRank, Favor, TotalFavorEarned
  - [ ] Add UnlockedPerks dictionary
  - [ ] Add LastReligionSwitch (nullable DateTime)
  - [ ] Add DataVersion
  - [ ] Add constructors
  - [ ] Add helper methods (UpdateFavorRank, AddFavor, RemoveFavor, etc.)

### Task 1.2: Create ReligionManager System
**Estimated Time**: 4-5 hours

- [ ] Create `Systems/ReligionManager.cs`
  - [ ] Add private fields: ICoreServerAPI, Dictionary of religions
  - [ ] Add constructor with dependency injection
  - [ ] Implement `Initialize()` method
    - [ ] Register event handlers (SaveGameLoaded, GameWorldSave)
  - [ ] Implement `CreateReligion(name, deity, founderUID, isPublic)`
    - [ ] Generate unique ReligionUID
    - [ ] Validate deity type
    - [ ] Create ReligionData instance
    - [ ] Add founder as first member
    - [ ] Store in dictionary
    - [ ] Return ReligionData
  - [ ] Implement `AddMember(religionUID, playerUID)`
    - [ ] Validate religion exists
    - [ ] Add player to MemberUIDs
    - [ ] Trigger member join event
  - [ ] Implement `RemoveMember(religionUID, playerUID)`
    - [ ] Validate religion exists
    - [ ] Remove player from MemberUIDs
    - [ ] Handle founder leaving (transfer or disband)
    - [ ] Trigger member leave event
  - [ ] Implement `GetPlayerReligion(playerUID)`
    - [ ] Search all religions for player membership
    - [ ] Return ReligionData or null
  - [ ] Implement `GetReligion(religionUID)`
    - [ ] Return religion from dictionary or null
  - [ ] Implement `GetPlayerActiveDeity(playerUID)`
    - [ ] Get player's religion
    - [ ] Return deity or DeityType.None
  - [ ] Implement `CanJoinReligion(religionUID, playerUID)`
    - [ ] Check if player already in a religion
    - [ ] Check if religion is public or player has invitation
    - [ ] Return bool
  - [ ] Implement `InvitePlayer(religionUID, playerUID, inviterUID)`
    - [ ] Validate inviter is member
    - [ ] Store invitation (add invitation tracking system)
    - [ ] Notify player
  - [ ] Implement `HasReligion(playerUID)`
    - [ ] Check if player is in any religion
  - [ ] Implement `GetAllReligions()`
    - [ ] Return all religions
  - [ ] Implement `GetReligionsByDeity(deity)`
    - [ ] Filter and return religions by deity
  - [ ] Implement persistence methods
    - [ ] `LoadAllReligions()`
    - [ ] `SaveAllReligions()`
    - [ ] `LoadReligion(religionUID)`
    - [ ] `SaveReligion(religionUID)`

### Task 1.3: Create PlayerReligionDataManager System
**Estimated Time**: 4-5 hours

- [ ] Create `Systems/PlayerReligionDataManager.cs`
  - [ ] Add private fields: ICoreServerAPI, Dictionary of player data
  - [ ] Add reference to ReligionManager
  - [ ] Add constructor with dependency injection
  - [ ] Implement `Initialize()` method
    - [ ] Register event handlers (PlayerJoin, PlayerDisconnect, etc.)
  - [ ] Implement `GetOrCreatePlayerData(playerUID)`
    - [ ] Check dictionary for existing data
    - [ ] Create new if not exists
    - [ ] Return PlayerReligionData
  - [ ] Implement `AddFavor(playerUID, amount, reason)`
    - [ ] Get player data
    - [ ] Add favor
    - [ ] Update total favor earned
    - [ ] Call UpdateFavorRank
    - [ ] Log reason
  - [ ] Implement `UpdateFavorRank(playerUID)`
    - [ ] Get player data
    - [ ] Calculate rank based on TotalFavorEarned
    - [ ] Update FavorRank
    - [ ] Check for new perk unlocks
    - [ ] Send rank-up notification if changed
  - [ ] Implement `UnlockPlayerPerk(playerUID, perkId)`
    - [ ] Validate perk exists
    - [ ] Check unlock requirements
    - [ ] Add to UnlockedPerks
    - [ ] Trigger perk effect refresh
    - [ ] Return success/failure
  - [ ] Implement `GetActivePlayerPerks(playerUID)`
    - [ ] Get player data
    - [ ] Get unlocked perks from UnlockedPerks dictionary
    - [ ] Return list of Perk objects
  - [ ] Implement `JoinReligion(playerUID, religionUID)`
    - [ ] Check if player already in a religion
    - [ ] If yes, call LeaveReligion first
    - [ ] Set ReligionUID
    - [ ] Update ActiveDeity from religion
    - [ ] Call ReligionManager.AddMember
    - [ ] Set LastReligionSwitch timestamp
  - [ ] Implement `LeaveReligion(playerUID)`
    - [ ] Get player data
    - [ ] Call ReligionManager.RemoveMember
    - [ ] Set ReligionUID to null
    - [ ] Set ActiveDeity to None
    - [ ] Clear religion-specific data
  - [ ] Implement `CanSwitchReligion(playerUID)`
    - [ ] Get player data
    - [ ] Check LastReligionSwitch timestamp
    - [ ] Compare against cooldown period (e.g., 7 days)
    - [ ] Return bool
  - [ ] Implement `HandleReligionSwitch(playerUID)`
    - [ ] Apply switching penalties
    - [ ] Reset favor to 0 (or apply retention percentage)
    - [ ] Clear unlocked perks
    - [ ] Reset favor rank to Initiate
  - [ ] Implement persistence methods
    - [ ] `LoadPlayerData(playerUID)`
    - [ ] `SavePlayerData(playerUID)`
    - [ ] `LoadAllPlayerData()`
    - [ ] `SaveAllPlayerData()`

### Task 1.4: Create Religion Commands
**Estimated Time**: 3-4 hours

- [ ] Create `Commands/ReligionCommands.cs`
  - [ ] Add constructor with ReligionManager and PlayerReligionDataManager dependencies
  - [ ] Implement `/religion create <name> <deity> [public/private]`
    - [ ] Parse arguments
    - [ ] Validate deity name
    - [ ] Check if player already in religion (auto-leave with confirmation)
    - [ ] Call ReligionManager.CreateReligion
    - [ ] Call PlayerReligionDataManager.JoinReligion
    - [ ] Send success message
  - [ ] Implement `/religion join <religionname>`
    - [ ] Find religion by name
    - [ ] Check CanJoinReligion
    - [ ] Check CanSwitchReligion (cooldown)
    - [ ] Call HandleReligionSwitch if switching
    - [ ] Call PlayerReligionDataManager.JoinReligion
    - [ ] Send success message
  - [ ] Implement `/religion leave`
    - [ ] Check if player in a religion
    - [ ] Call PlayerReligionDataManager.LeaveReligion
    - [ ] Send confirmation message
  - [ ] Implement `/religion list [deity]`
    - [ ] Get all religions or filter by deity
    - [ ] Format and display list with member counts
  - [ ] Implement `/religion info <religionname>`
    - [ ] Get religion data
    - [ ] Display: name, deity, founder, member count, prestige rank, description
  - [ ] Implement `/religion members [religionname]`
    - [ ] Default to player's current religion
    - [ ] Get religion data
    - [ ] Display member list with online status and favor ranks
  - [ ] Implement `/religion invite <playername>`
    - [ ] Check if player is in a religion
    - [ ] Check if player is founder or has permission
    - [ ] Call ReligionManager.InvitePlayer
    - [ ] Send invitation to target player
  - [ ] Implement `/religion kick <playername>`
    - [ ] Check if player is founder
    - [ ] Call ReligionManager.RemoveMember
    - [ ] Send notification
  - [ ] Implement `/religion disband`
    - [ ] Check if player is founder
    - [ ] Confirm action (require confirmation)
    - [ ] Remove all members
    - [ ] Delete religion
  - [ ] Implement `/religion description <text>`
    - [ ] Check if player is founder
    - [ ] Update religion description
    - [ ] Send confirmation
  - [ ] Register all commands in main mod class

### Task 1.5: Update PantheonWarsSystem Integration
**Estimated Time**: 2 hours

- [ ] Update `PantheonWarsSystem.cs`
  - [ ] Add ReligionManager field
  - [ ] Add PlayerReligionDataManager field
  - [ ] Initialize ReligionManager in StartServerSide
  - [ ] Initialize PlayerReligionDataManager in StartServerSide
  - [ ] Register ReligionCommands
  - [ ] Add proper initialization order

### Task 1.6: Implement Persistence
**Estimated Time**: 3 hours

- [ ] Test religion data serialization
  - [ ] Create test religion
  - [ ] Save to world storage
  - [ ] Load from world storage
  - [ ] Verify data integrity
- [ ] Test player religion data serialization
  - [ ] Create test player data
  - [ ] Save to world storage
  - [ ] Load from world storage
  - [ ] Verify data integrity
- [ ] Implement batch save/load for performance
  - [ ] Batch religion saves
  - [ ] Batch player data saves

### Task 1.7: Testing Phase 4.1
**Estimated Time**: 2-3 hours

- [ ] Test religion creation
  - [ ] Create public religion
  - [ ] Create private religion
  - [ ] Verify founder is added as member
- [ ] Test religion joining
  - [ ] Join public religion
  - [ ] Join private religion (should fail without invite)
  - [ ] Verify player leaves previous religion when joining new one
- [ ] Test invitations
  - [ ] Send invitation
  - [ ] Accept invitation
  - [ ] Verify invitation system works
- [ ] Test switching cooldown
  - [ ] Switch religions
  - [ ] Verify cooldown prevents immediate switch
- [ ] Test persistence
  - [ ] Create religion, save game, reload
  - [ ] Verify religion data persists
  - [ ] Verify player data persists
- [ ] Test edge cases
  - [ ] Founder leaving religion
  - [ ] Single-member religion
  - [ ] Religion disbanding

---

## Phase 4.2: Ranking Systems (Week 3)
**Goal**: Implement dual ranking progression

### Task 2.1: Create ReligionPrestigeManager System
**Estimated Time**: 3-4 hours

- [ ] Create `Systems/ReligionPrestigeManager.cs`
  - [ ] Add private fields: ICoreServerAPI, reference to ReligionManager
  - [ ] Add constructor
  - [ ] Implement `Initialize()` method
  - [ ] Implement `AddPrestige(religionUID, amount, reason)`
    - [ ] Get religion data
    - [ ] Add prestige
    - [ ] Add to total prestige
    - [ ] Call UpdatePrestigeRank
    - [ ] Log reason
  - [ ] Implement `UpdatePrestigeRank(religionUID)`
    - [ ] Get religion data
    - [ ] Calculate rank based on TotalPrestige
    - [ ] Update PrestigeRank
    - [ ] Check for new perk unlocks
    - [ ] Notify all members if rank changed
  - [ ] Implement `UnlockReligionPerk(religionUID, perkId)`
    - [ ] Validate perk exists
    - [ ] Check unlock requirements
    - [ ] Add to religion's UnlockedPerks
    - [ ] Trigger perk effect refresh for all members
    - [ ] Return success/failure
  - [ ] Implement `GetActiveReligionPerks(religionUID)`
    - [ ] Get religion data
    - [ ] Get unlocked perks from UnlockedPerks dictionary
    - [ ] Return list of Perk objects

### Task 2.2: Integrate Favor/Prestige Earning
**Estimated Time**: 3 hours

- [ ] Update PvP kill handler
  - [ ] Hook into player death event
  - [ ] Calculate favor reward based on deity relationships
  - [ ] Call PlayerReligionDataManager.AddFavor
  - [ ] Calculate prestige reward for killer's religion
  - [ ] Call ReligionPrestigeManager.AddPrestige
  - [ ] Send kill notification with favor/prestige earned
- [ ] Add favor earning for other actions (optional)
  - [ ] Deity-aligned actions (e.g., Lysa followers hunting)
  - [ ] Territory control
  - [ ] Quests/achievements

### Task 2.3: Create Rank-Up Notification System
**Estimated Time**: 2 hours

- [ ] Create rank-up notification for player favor
  - [ ] Detect rank increase in UpdateFavorRank
  - [ ] Send server message to player
  - [ ] Add visual/sound effects (if possible)
- [ ] Create rank-up notification for religion prestige
  - [ ] Detect rank increase in UpdatePrestigeRank
  - [ ] Broadcast to all religion members
  - [ ] Add visual/sound effects (if possible)

### Task 2.4: Update HUD for Ranks
**Estimated Time**: 3-4 hours

- [ ] Update or create `GUI/FavorHudElement.cs`
  - [ ] Display current religion name
  - [ ] Display active deity
  - [ ] Display player favor rank
  - [ ] Display player favor amount
  - [ ] Display religion prestige rank
  - [ ] Display religion prestige amount
  - [ ] Add visual rank icons/indicators
  - [ ] Update on rank changes

### Task 2.5: Testing Phase 4.2
**Estimated Time**: 2 hours

- [ ] Test favor earning
  - [ ] Kill enemy players
  - [ ] Verify favor is awarded
  - [ ] Check deity relationship multipliers
- [ ] Test prestige earning
  - [ ] Kill enemy players
  - [ ] Verify religion prestige increases
- [ ] Test favor rank progression
  - [ ] Earn enough favor to rank up
  - [ ] Verify rank increases automatically
  - [ ] Check notification appears
- [ ] Test prestige rank progression
  - [ ] Earn enough prestige to rank up
  - [ ] Verify all members are notified
- [ ] Test HUD display
  - [ ] Verify all information displays correctly
  - [ ] Test updates on rank changes

---

## Phase 4.3: Perk System Core (Week 4-5)
**Goal**: Build perk system infrastructure

### Task 3.1: Create PerkRegistry System
**Estimated Time**: 3-4 hours

- [ ] Create `Systems/PerkRegistry.cs`
  - [ ] Add private fields: ICoreAPI, Dictionary of perks
  - [ ] Add constructor
  - [ ] Implement `Initialize()` method
  - [ ] Implement `RegisterPerk(perk)`
    - [ ] Validate perk data
    - [ ] Add to dictionary
    - [ ] Log registration
  - [ ] Implement `GetPerk(perkId)`
    - [ ] Return perk from dictionary or null
  - [ ] Implement `GetPerksForDeity(deity, type)`
    - [ ] Filter perks by deity and type
    - [ ] Return list
  - [ ] Implement `CanUnlockPerk(playerData, religionData, perk)`
    - [ ] Check if perk already unlocked
    - [ ] Check favor/prestige rank requirements
    - [ ] Check prerequisite perks
    - [ ] Return bool with reason
  - [ ] Create sample perks for testing
    - [ ] Create 2-3 player perks for Khoras (Tier 1)
    - [ ] Create 2-3 religion perks for Khoras (Tier 1)

### Task 3.2: Create PerkEffectSystem
**Estimated Time**: 4-5 hours

- [ ] Create `Systems/PerkEffectSystem.cs`
  - [ ] Add private fields: references to other managers
  - [ ] Add stat modifier cache (Dictionary<playerUID, modifiers>)
  - [ ] Add constructor
  - [ ] Implement `Initialize()` method
  - [ ] Implement `GetPlayerStatModifiers(playerUID)`
    - [ ] Get player's unlocked perks
    - [ ] Combine all stat modifiers
    - [ ] Return dictionary
  - [ ] Implement `GetReligionStatModifiers(religionUID)`
    - [ ] Get religion's unlocked perks
    - [ ] Combine all stat modifiers
    - [ ] Return dictionary
  - [ ] Implement `GetCombinedStatModifiers(playerUID)`
    - [ ] Get player modifiers
    - [ ] Get religion modifiers
    - [ ] Combine both
    - [ ] Return dictionary
  - [ ] Implement `ApplyPerksToPlayer(player)`
    - [ ] Get combined modifiers
    - [ ] Apply to player stats (if Vintage Story supports runtime stat modification)
    - [ ] Store applied modifiers for removal later
  - [ ] Implement `RefreshPlayerPerks(playerUID)`
    - [ ] Clear cached modifiers
    - [ ] Recalculate from scratch
    - [ ] Reapply to player
  - [ ] Add event handlers
    - [ ] On perk unlock: refresh player perks
    - [ ] On religion join/leave: refresh player perks
    - [ ] On player login: apply perks

### Task 3.3: Create Perk Unlock Validation
**Estimated Time**: 2 hours

- [ ] Implement unlock requirement checking
  - [ ] Check player favor rank for player perks
  - [ ] Check religion prestige rank for religion perks
  - [ ] Check prerequisite perks are unlocked
  - [ ] Return validation result with error messages
- [ ] Add unlock confirmation
  - [ ] Confirm perk unlock
  - [ ] Apply perk effects immediately

### Task 3.4: Implement Perk Persistence
**Estimated Time**: 2 hours

- [ ] Verify UnlockedPerks serialization in PlayerReligionData
  - [ ] Test saving/loading unlocked player perks
- [ ] Verify UnlockedPerks serialization in ReligionData
  - [ ] Test saving/loading unlocked religion perks
- [ ] Add migration for existing save files
  - [ ] Initialize empty UnlockedPerks dictionaries

### Task 3.5: Create Perk Commands
**Estimated Time**: 3 hours

- [ ] Create `Commands/PerkCommands.cs`
  - [ ] Implement `/perks list`
    - [ ] Get player's deity
    - [ ] Show all available perks for deity
    - [ ] Indicate which are unlocked
  - [ ] Implement `/perks player`
    - [ ] Show player's unlocked player perks
    - [ ] Display perk effects
  - [ ] Implement `/perks religion`
    - [ ] Show religion's unlocked religion perks
    - [ ] Display perk effects
  - [ ] Implement `/perks info <perkid>`
    - [ ] Display detailed perk information
    - [ ] Show unlock requirements
    - [ ] Show prerequisites
    - [ ] Show stat modifiers and effects
  - [ ] Implement `/perks tree [player/religion]`
    - [ ] Display perk tree in text format
    - [ ] Show unlock status
    - [ ] Show requirements
  - [ ] Implement `/perks unlock <perkid>`
    - [ ] Validate unlock requirements
    - [ ] Unlock perk
    - [ ] Apply effects
    - [ ] Send confirmation
  - [ ] Implement `/perks active`
    - [ ] Show all active perks affecting player
    - [ ] Display combined stat modifiers
  - [ ] Register commands

### Task 3.6: Testing Phase 4.3
**Estimated Time**: 3 hours

- [ ] Test perk registration
  - [ ] Register sample perks
  - [ ] Verify they appear in registry
- [ ] Test perk unlock requirements
  - [ ] Try unlocking perk without requirements (should fail)
  - [ ] Earn required rank
  - [ ] Unlock perk successfully
- [ ] Test perk effects
  - [ ] Unlock perk with stat modifiers
  - [ ] Verify modifiers are applied
  - [ ] Test combined player + religion modifiers
- [ ] Test perk persistence
  - [ ] Unlock perks
  - [ ] Save and reload
  - [ ] Verify perks remain unlocked
- [ ] Test perk commands
  - [ ] Test all perk commands
  - [ ] Verify output is correct

---

## Phase 4.4: Deity Perk Trees (Week 6-8)
**Goal**: Design and implement all deity perk trees

### Task 4.1: Design Perk Trees for All Deities
**Estimated Time**: 8-10 hours (design work)

- [ ] Khoras (War) - Player Perks
  - [ ] Design Tier 1 (Initiate) - 2 perks
  - [ ] Design Tier 2 (Disciple) - 2 perks
  - [ ] Design Tier 3 (Zealot) - 2 perks
  - [ ] Design Tier 4 (Champion) - 2 perks
  - [ ] Design Tier 5 (Avatar) - 2 perks
- [ ] Khoras (War) - Religion Perks
  - [ ] Design Tier 1 (Fledgling) - 2 perks
  - [ ] Design Tier 2 (Established) - 2 perks
  - [ ] Design Tier 3 (Renowned) - 2 perks
  - [ ] Design Tier 4 (Legendary) - 2 perks
  - [ ] Design Tier 5 (Mythic) - 2 perks
- [ ] Lysa (Hunt) - Player Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Lysa (Hunt) - Religion Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Morthen (Death) - Player Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Morthen (Death) - Religion Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Aethra (Light) - Player Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Aethra (Light) - Religion Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Umbros (Shadows) - Player Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Umbros (Shadows) - Religion Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Tharos (Storms) - Player Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Tharos (Storms) - Religion Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Gaia (Earth) - Player Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Gaia (Earth) - Religion Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Vex (Madness) - Player Perks
  - [ ] Design all 5 tiers (10 perks total)
- [ ] Vex (Madness) - Religion Perks
  - [ ] Design all 5 tiers (10 perks total)

### Task 4.2: Implement Deity Perk Definitions
**Estimated Time**: 8-10 hours

- [ ] Create perk definition files or system
  - [ ] Decide on approach (hardcoded, JSON, or both)
- [ ] Implement Khoras perks (20 perks)
  - [ ] Create Perk objects for all player perks
  - [ ] Create Perk objects for all religion perks
  - [ ] Register in PerkRegistry
- [ ] Implement Lysa perks (20 perks)
- [ ] Implement Morthen perks (20 perks)
- [ ] Implement Aethra perks (20 perks)
- [ ] Implement Umbros perks (20 perks)
- [ ] Implement Tharos perks (20 perks)
- [ ] Implement Gaia perks (20 perks)
- [ ] Implement Vex perks (20 perks)

### Task 4.3: Implement Perk Effects
**Estimated Time**: 12-15 hours

- [ ] Implement stat modifier effects
  - [ ] Damage modifiers
  - [ ] Defense/resistance modifiers
  - [ ] Speed/movement modifiers
  - [ ] Health modifiers
  - [ ] Other basic stat modifiers
- [ ] Implement special effects (per deity)
  - [ ] Khoras special effects (e.g., Last Stand, Lifesteal)
  - [ ] Lysa special effects (e.g., Tracking, Critical hits)
  - [ ] Morthen special effects (e.g., Life drain, DoT)
  - [ ] Aethra special effects (e.g., Healing, Shields)
  - [ ] Umbros special effects (e.g., Stealth, Backstab)
  - [ ] Tharos special effects (e.g., AoE damage, Lightning)
  - [ ] Gaia special effects (e.g., Regeneration, Durability)
  - [ ] Vex special effects (e.g., Confusion, Random effects)
- [ ] Create perk effect handlers
  - [ ] Create handler system for complex effects
  - [ ] Implement handlers for each special effect type

### Task 4.4: Balance Testing
**Estimated Time**: 6-8 hours

- [ ] Create test scenarios for each deity
  - [ ] Set up test players with max perks
  - [ ] Test combat effectiveness
- [ ] Balance perk values
  - [ ] Adjust stat modifier percentages
  - [ ] Ensure no deity is overpowered
  - [ ] Ensure all deities are viable
- [ ] Test perk combinations
  - [ ] Test player + religion perk stacking
  - [ ] Identify overpowered combinations
  - [ ] Adjust as needed
- [ ] Playtest with multiple players (if possible)
  - [ ] Get feedback on balance
  - [ ] Iterate on perk values

### Task 4.5: Document All Perks
**Estimated Time**: 4-5 hours

- [ ] Create perk documentation file
  - [ ] List all perks by deity
  - [ ] Include descriptions
  - [ ] Include unlock requirements
  - [ ] Include stat modifiers
- [ ] Update README with perk information
  - [ ] Link to perk documentation
  - [ ] Provide overview of perk system

---

## Phase 4.5: Integration & Polish (Week 9-10)
**Goal**: Integrate everything and polish

### Task 5.1: Remove Old Ability System
**Estimated Time**: 3-4 hours

- [ ] Mark old systems as obsolete
  - [ ] Add [Obsolete] attributes to old classes
- [ ] Remove old ability command registrations
  - [ ] Remove from PantheonWarsSystem
- [ ] Delete old ability files
  - [ ] Delete AbilitySystem.cs
  - [ ] Delete AbilityCooldownManager.cs
  - [ ] Delete AbilityCommands.cs
  - [ ] Delete PlayerAbilityData.cs
  - [ ] Delete all ability implementation files (Khoras/, Lysa/, etc.)
- [ ] Remove old ability references
  - [ ] Search codebase for references
  - [ ] Remove or update references

### Task 5.2: Update Commands to New Systems
**Estimated Time**: 2-3 hours

- [ ] Update `/deity` commands
  - [ ] Update `/deity status` to show religion info
  - [ ] Remove ability-related commands
- [ ] Update `/favor` command
  - [ ] Show new favor rank system
  - [ ] Show progress to next rank
- [ ] Add `/prestige` command (if not already added)
  - [ ] Show religion prestige
  - [ ] Show progress to next rank
- [ ] Update `/rank` command
  - [ ] Show both player favor rank and religion prestige rank

### Task 5.3: Implement Data Migration
**Estimated Time**: 4-5 hours

- [ ] Create migration system
  - [ ] Detect old PlayerDeityData format
  - [ ] Convert to new PlayerReligionData format
  - [ ] Create solo religion for existing players
- [ ] Implement migration logic
  - [ ] Convert DevotionRank to FavorRank
  - [ ] Preserve favor values
  - [ ] Auto-create religion with format: "{PlayerName}'s Followers of {Deity}"
  - [ ] Set player as founder
  - [ ] Join player to auto-created religion
- [ ] Test migration
  - [ ] Create old format save
  - [ ] Run migration
  - [ ] Verify data integrity
  - [ ] Verify player can access new systems

### Task 5.4: Create Perk Tree Visualization UI
**Estimated Time**: 8-10 hours

- [ ] Design perk tree layout
  - [ ] Sketch UI mockup
  - [ ] Plan tree structure display
- [ ] Create `GUI/PerkTreeDialog.cs`
  - [ ] Create dialog window
  - [ ] Display perk tree structure
  - [ ] Show player/religion tabs
  - [ ] Display unlock status (locked/unlocked/available)
  - [ ] Show perk requirements on hover
  - [ ] Show perk effects on hover
  - [ ] Add unlock button for available perks
  - [ ] Add visual tree connections (prerequisites)
  - [ ] Add rank tier separators
- [ ] Register perk tree dialog
  - [ ] Add hotkey to open (e.g., "P" for perks)
  - [ ] Add command to open
- [ ] Test UI
  - [ ] Test with different screen resolutions
  - [ ] Test with all deities
  - [ ] Test unlock interactions

### Task 5.5: Create Religion Management UI
**Estimated Time**: 8-10 hours

- [ ] Design religion management UI
  - [ ] Sketch UI mockup
  - [ ] Plan different views (browse, manage, create)
- [ ] Create `GUI/ReligionManagementDialog.cs`
  - [ ] Create dialog window
  - [ ] Add "Browse Religions" tab
    - [ ] List all public religions
    - [ ] Show deity filter dropdown
    - [ ] Show member counts, prestige ranks
    - [ ] Add join button
  - [ ] Add "My Religion" tab
    - [ ] Show current religion details
    - [ ] Show member list with ranks
    - [ ] Show prestige progress bar
    - [ ] Add leave button
    - [ ] Add invite button (if founder)
    - [ ] Add kick button (if founder)
    - [ ] Add description edit (if founder)
  - [ ] Add "Create Religion" button
    - [ ] Open creation dialog
    - [ ] Input name, deity, public/private
    - [ ] Create and auto-join
  - [ ] Add switching warning dialog
    - [ ] Warn about cooldown
    - [ ] Warn about favor/perk loss
    - [ ] Require confirmation
- [ ] Register religion management dialog
  - [ ] Add hotkey to open (reuse "K" or add new key)
  - [ ] Update existing deity selection dialog to open religion dialog
- [ ] Test UI
  - [ ] Test all interactions
  - [ ] Test founder permissions
  - [ ] Test member permissions
  - [ ] Test switching warnings

### Task 5.6: Update Enhanced HUD
**Estimated Time**: 3-4 hours

- [ ] Update `GUI/FavorHudElement.cs` (if not done in 2.4)
  - [ ] Display current religion name
  - [ ] Display active deity icon/name
  - [ ] Display player favor rank with progress bar
  - [ ] Display religion prestige rank with progress bar
  - [ ] Display active perk count (e.g., "Perks: 5P + 3R")
  - [ ] Add tooltips on hover
  - [ ] Update styling for readability
- [ ] Test HUD updates
  - [ ] Verify real-time updates
  - [ ] Test with different religions/deities

### Task 5.7: Comprehensive Testing
**Estimated Time**: 8-10 hours

- [ ] End-to-end testing
  - [ ] Fresh player joins server
  - [ ] Creates religion
  - [ ] Invites other players
  - [ ] Earns favor and prestige
  - [ ] Unlocks perks
  - [ ] Tests perk effects
  - [ ] Switches religions
- [ ] Multi-religion testing
  - [ ] Create multiple religions of same deity
  - [ ] Create religions of different deities
  - [ ] Test PvP between different deities
  - [ ] Test favor/prestige earning
- [ ] Edge case testing
  - [ ] Founder leaves religion
  - [ ] Single-member religion
  - [ ] Religion with max members
  - [ ] Player with max perks unlocked
  - [ ] Switching religions on cooldown
- [ ] Performance testing
  - [ ] Test with 10+ religions
  - [ ] Test with 50+ players
  - [ ] Monitor server performance
  - [ ] Optimize if needed
- [ ] Regression testing
  - [ ] Ensure old features still work
  - [ ] Test deity selection
  - [ ] Test favor system
  - [ ] Test PvP mechanics

### Task 5.8: Documentation Updates
**Estimated Time**: 4-5 hours

- [ ] Update README.md
  - [ ] Update feature list
  - [ ] Update phase status
  - [ ] Add religion system overview
  - [ ] Add perk system overview
  - [ ] Update commands list
  - [ ] Update screenshots (if applicable)
- [ ] Update implementation_guide.md
  - [ ] Mark Phase 4 as complete
  - [ ] Update status badges
- [ ] Create user guide
  - [ ] How to create/join religions
  - [ ] How to earn favor/prestige
  - [ ] How to unlock perks
  - [ ] Perk tree overview
  - [ ] Tips for each deity
- [ ] Update code comments
  - [ ] Add XML documentation comments
  - [ ] Update outdated comments
  - [ ] Add explanatory comments for complex logic

---

## Summary

### Total Estimated Time: 10-12 weeks

**Phase 4.1**: 16-22 hours (Week 1-2)
**Phase 4.2**: 10-12 hours (Week 3)
**Phase 4.3**: 17-21 hours (Week 4-5)
**Phase 4.4**: 38-48 hours (Week 6-8)
**Phase 4.5**: 40-51 hours (Week 9-10)

**Grand Total**: ~121-154 hours

### Task Count: 200+ individual tasks

### Critical Path
1. Data models must be created first
2. ReligionManager depends on data models
3. PlayerReligionDataManager depends on ReligionManager
4. Commands depend on both managers
5. Perk system depends on both data models and managers
6. UI depends on all backend systems being complete

### Recommended Approach
- Complete each phase fully before moving to next
- Test thoroughly at each phase boundary
- Keep Phase 4 branch separate until fully tested
- Consider alpha/beta testing phases between major phases

---

## Notes

- All time estimates are approximate and may vary based on experience
- Some tasks can be done in parallel (e.g., different deity perk implementations)
- Testing time may increase if bugs are discovered
- UI work may take longer depending on Vintage Story GUI complexity
- Perk effect implementations may vary widely in complexity
- Balance testing should be iterative throughout Phase 4.4

---

**Ready to begin Phase 4.1 upon approval.**
