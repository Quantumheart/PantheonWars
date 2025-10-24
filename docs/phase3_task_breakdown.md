# Phase 3 Implementation Task Breakdown

This document provides a detailed, actionable task list for implementing the Religion-Based Deity System with Perk Trees as outlined in `PHASE4_GROUP_DEITY_PERKS_GUIDE.md`.

---

## Phase 3.1: Foundation (Week 1-2)
**Goal**: Build custom religion system foundation

### Task 1.1: Create Data Models ✅
**Estimated Time**: 2-3 hours
**Status**: COMPLETED

- [x] Create `Models/PrestigeRank.cs` enum
  - [x] Define 5 ranks: Fledgling, Established, Renowned, Legendary, Mythic
  - [x] Add rank thresholds in comments
- [x] Create `Models/FavorRank.cs` enum
  - [x] Define 5 ranks: Initiate, Disciple, Zealot, Champion, Avatar
  - [x] Add rank thresholds in comments
- [x] Create `Models/PerkType.cs` enum
  - [x] Define: Player, Religion
- [x] Create `Models/PerkCategory.cs` enum
  - [x] Define: Combat, Defense, Mobility, Utility, Economic, Territory
- [x] Create `Models/Perk.cs` class
  - [x] Add all properties: PerkId, Name, Description, Deity, Type, Category
  - [x] Add unlock requirements: RequiredFavorRank, RequiredPrestigeRank
  - [x] Add PrerequisitePerks list
  - [x] Add StatModifiers dictionary
  - [x] Add SpecialEffects list
  - [x] Add constructor
- [x] Create `Data/ReligionData.cs` class
  - [x] Add all properties: ReligionUID, ReligionName, Deity, FounderUID
  - [x] Add MemberUIDs list
  - [x] Add PrestigeRank, Prestige, TotalPrestige
  - [x] Add CreationDate
  - [x] Add UnlockedPerks dictionary
  - [x] Add IsPublic flag
  - [x] Add Description string
  - [x] Add constructors
  - [x] Add helper methods (AddMember, RemoveMember, etc.)
  - [x] Add protobuf-net serialization attributes ([ProtoContract], [ProtoMember])
- [x] Create `Data/PlayerReligionData.cs` class
  - [x] Add all properties: PlayerUID, ReligionUID (nullable)
  - [x] Add ActiveDeity, FavorRank, Favor, TotalFavorEarned
  - [x] Add UnlockedPerks dictionary
  - [x] Add LastReligionSwitch (nullable DateTime)
  - [x] Add DataVersion
  - [x] Add constructors
  - [x] Add helper methods (UpdateFavorRank, AddFavor, RemoveFavor, etc.)
  - [x] Add protobuf-net serialization attributes ([ProtoContract], [ProtoMember])

### Task 1.2: Create ReligionManager System ✅
**Estimated Time**: 4-5 hours
**Status**: COMPLETED

- [x] Create `Systems/ReligionManager.cs`
  - [x] Add private fields: ICoreServerAPI, Dictionary of religions, Dictionary of invitations
  - [x] Add constructor with dependency injection
  - [x] Implement `Initialize()` method
    - [x] Register event handlers (SaveGameLoaded, GameWorldSave)
  - [x] Implement `CreateReligion(name, deity, founderUID, isPublic)`
    - [x] Generate unique ReligionUID using Guid.NewGuid()
    - [x] Validate deity type
    - [x] Create ReligionData instance
    - [x] Add founder as first member
    - [x] Store in dictionary
    - [x] Return ReligionData
  - [x] Implement `AddMember(religionUID, playerUID)`
    - [x] Validate religion exists
    - [x] Add player to MemberUIDs
  - [x] Implement `RemoveMember(religionUID, playerUID)`
    - [x] Validate religion exists
    - [x] Remove player from MemberUIDs
    - [x] Handle founder leaving (transfer to next member)
    - [x] Auto-disband if no members remain
  - [x] Implement `GetPlayerReligion(playerUID)`
    - [x] Search all religions for player membership
    - [x] Return ReligionData or null
  - [x] Implement `GetReligion(religionUID)`
    - [x] Return religion from dictionary or null
  - [x] Implement `GetReligionByName(name)`
    - [x] Search by name (case-insensitive)
  - [x] Implement `GetPlayerActiveDeity(playerUID)`
    - [x] Get player's religion
    - [x] Return deity or DeityType.None
  - [x] Implement `CanJoinReligion(religionUID, playerUID)`
    - [x] Check if player already in a religion
    - [x] Check if religion is public or player has invitation
    - [x] Return bool
  - [x] Implement `InvitePlayer(religionUID, playerUID, inviterUID)`
    - [x] Validate inviter is member
    - [x] Store invitation in Dictionary
    - [x] Log invitation
  - [x] Implement `HasInvitation(playerUID, religionUID)`
  - [x] Implement `RemoveInvitation(playerUID, religionUID)`
  - [x] Implement `GetPlayerInvitations(playerUID)`
  - [x] Implement `HasReligion(playerUID)`
    - [x] Check if player is in any religion
  - [x] Implement `GetAllReligions()`
    - [x] Return all religions
  - [x] Implement `GetReligionsByDeity(deity)`
    - [x] Filter and return religions by deity
  - [x] Implement `DeleteReligion(religionUID, requesterUID)`
    - [x] Validate requester is founder
  - [x] Implement persistence methods
    - [x] `LoadAllReligions()` using SerializerUtil
    - [x] `SaveAllReligions()` using SerializerUtil

### Task 1.3: Create PlayerReligionDataManager System ✅
**Estimated Time**: 4-5 hours
**Status**: COMPLETED

- [x] Create `Systems/PlayerReligionDataManager.cs`
  - [x] Add private fields: ICoreServerAPI, Dictionary of player data
  - [x] Add reference to ReligionManager
  - [x] Add constructor with dependency injection
  - [x] Implement `Initialize()` method
    - [x] Register event handlers (PlayerJoin, PlayerDisconnect, SaveGameLoaded, GameWorldSave)
  - [x] Implement `GetOrCreatePlayerData(playerUID)`
    - [x] Check dictionary for existing data
    - [x] Create new if not exists
    - [x] Return PlayerReligionData
  - [x] Implement `AddFavor(playerUID, amount, reason)`
    - [x] Get player data
    - [x] Add favor using PlayerReligionData.AddFavor()
    - [x] Check for rank up and send notification
    - [x] Log reason
  - [x] Implement `UpdateFavorRank(playerUID)`
    - [x] Get player data
    - [x] Calculate rank based on TotalFavorEarned
    - [x] Update FavorRank
    - [x] Send rank-up notification if changed
  - [x] Implement `UnlockPlayerPerk(playerUID, perkId)`
    - [x] Check if already unlocked
    - [x] Add to UnlockedPerks
    - [x] Return success/failure
  - [x] Implement `GetActivePlayerPerks(playerUID)`
    - [x] Get player data
    - [x] Get unlocked perks from UnlockedPerks dictionary
    - [x] Return list of perk IDs
  - [x] Implement `JoinReligion(playerUID, religionUID)`
    - [x] Check if player already in a religion
    - [x] If yes, call LeaveReligion first
    - [x] Set ReligionUID
    - [x] Update ActiveDeity from religion
    - [x] Call ReligionManager.AddMember
    - [x] Set LastReligionSwitch timestamp
  - [x] Implement `LeaveReligion(playerUID)`
    - [x] Get player data
    - [x] Call ReligionManager.RemoveMember
    - [x] Set ReligionUID to null
    - [x] Set ActiveDeity to None
  - [x] Implement `CanSwitchReligion(playerUID)`
    - [x] Get player data
    - [x] Check LastReligionSwitch timestamp
    - [x] Compare against 7-day cooldown (RELIGION_SWITCH_COOLDOWN_DAYS constant)
    - [x] Return bool
  - [x] Implement `GetSwitchCooldownRemaining(playerUID)`
    - [x] Calculate remaining cooldown time
  - [x] Implement `HandleReligionSwitch(playerUID)`
    - [x] Apply switching penalties using ApplySwitchPenalty()
    - [x] Reset favor to 0
    - [x] Clear unlocked perks
    - [x] Reset favor rank to Initiate
  - [x] Implement persistence methods
    - [x] `LoadPlayerData(playerUID)` using SerializerUtil
    - [x] `SavePlayerData(playerUID)` using SerializerUtil
    - [x] `LoadAllPlayerData()` (placeholder for future batch loading)
    - [x] `SaveAllPlayerData()` using SerializerUtil

### Task 1.4: Create Religion Commands ✅
**Estimated Time**: 3-4 hours
**Status**: COMPLETED

- [x] Create `Commands/ReligionCommands.cs`
  - [x] Add constructor with ReligionManager and PlayerReligionDataManager dependencies
  - [x] Implement `/religion create <name> <deity> [public/private]`
    - [x] Parse arguments
    - [x] Validate deity name using Enum.TryParse
    - [x] Check if player already in religion
    - [x] Check if religion name already exists
    - [x] Call ReligionManager.CreateReligion
    - [x] Auto-join founder using PlayerReligionDataManager.JoinReligion
    - [x] Send success message
  - [x] Implement `/religion join <religionname>`
    - [x] Find religion by name
    - [x] Check CanSwitchReligion (7-day cooldown)
    - [x] Check CanJoinReligion (public or invitation)
    - [x] Call HandleReligionSwitch if switching
    - [x] Call PlayerReligionDataManager.JoinReligion
    - [x] Remove invitation if exists
    - [x] Send success message
  - [x] Implement `/religion leave`
    - [x] Check if player in a religion
    - [x] Get religion name for confirmation message
    - [x] Call PlayerReligionDataManager.LeaveReligion
    - [x] Send confirmation message
  - [x] Implement `/religion list [deity]`
    - [x] Get all religions or filter by optional deity parameter
    - [x] Format and display list with member counts, prestige rank, visibility
    - [x] Sort by TotalPrestige descending
  - [x] Implement `/religion info [name]`
    - [x] Default to player's current religion if no name provided
    - [x] Get religion data
    - [x] Display: name, deity, visibility, members, prestige rank/points, creation date, founder, description
  - [x] Implement `/religion members`
    - [x] Get player's current religion
    - [x] Display member list with favor ranks and favor amounts
    - [x] Show founder designation
  - [x] Implement `/religion invite <playername>`
    - [x] Check if player is in a religion
    - [x] Validate inviter is a member
    - [x] Find target player by name
    - [x] Check if target is already a member
    - [x] Call ReligionManager.InvitePlayer
    - [x] Send invitation notification to target player
  - [x] Implement `/religion kick <playername>`
    - [x] Check if player is founder
    - [x] Find target player by name
    - [x] Check if target is a member
    - [x] Prevent self-kick
    - [x] Call PlayerReligionDataManager.LeaveReligion
    - [x] Send notification to kicked player if online
  - [x] Implement `/religion disband`
    - [x] Check if player is founder
    - [x] Get religion name for confirmation
    - [x] Remove all members using LeaveReligion
    - [x] Notify all members except founder
    - [x] Delete religion using ReligionManager.DeleteReligion
  - [x] Implement `/religion description <text>`
    - [x] Check if player is founder
    - [x] Parse text using All() parser
    - [x] Update religion description
    - [x] Send confirmation
  - [x] Register all commands in PantheonWarsSystem

### Task 1.5: Update PantheonWarsSystem Integration ✅
**Estimated Time**: 2 hours
**Status**: COMPLETED

- [x] Update `PantheonWarsSystem.cs`
  - [x] Add ReligionManager field
  - [x] Add PlayerReligionDataManager field
  - [x] Initialize ReligionManager in StartServerSide (before PlayerReligionDataManager)
  - [x] Initialize PlayerReligionDataManager in StartServerSide (after ReligionManager)
  - [x] Initialize ReligionCommands with dependencies
  - [x] Register ReligionCommands.RegisterCommands()
  - [x] Proper initialization order: ReligionManager → PlayerReligionDataManager → Commands

### Task 1.6: Implement Persistence ✅
**Estimated Time**: 3 hours
**Status**: COMPLETED

- [x] Add protobuf-net serialization attributes to all data classes
  - [x] PlayerDeityData.cs - [ProtoContract] and [ProtoMember(1-9)]
  - [x] ReligionData.cs - [ProtoContract] and [ProtoMember(1-12)]
  - [x] PlayerReligionData.cs - [ProtoContract] and [ProtoMember(1-9)]
- [x] Implement religion data serialization in ReligionManager
  - [x] Use SerializerUtil.Serialize/Deserialize
  - [x] Use WorldManager.SaveGame.StoreData/GetData
  - [x] Save all religions as list to single key
  - [x] Load all religions on SaveGameLoaded event
  - [x] Save all religions on GameWorldSave event
- [x] Implement player religion data serialization in PlayerReligionDataManager
  - [x] Use SerializerUtil.Serialize/Deserialize
  - [x] Use WorldManager.SaveGame.StoreData/GetData
  - [x] Save per-player with unique key: "pantheonwars_playerreligiondata_{playerUID}"
  - [x] Load on player join
  - [x] Save on player disconnect
  - [x] Batch save all players on GameWorldSave event

### Task 1.7: Testing Phase 3.1
**Estimated Time**: 2-3 hours

- [x] Test religion creation
  - [x] Create public religion
  - [x] Create private religion
  - [x] Verify founder is added as member
- [ ] Test religion joining
  - [ ] Join public religion
  - [ ] Join private religion (should fail without invite)
  - [ ] Verify player leaves previous religion when joining new one
- [ ] Test invitations
  - [ ] Send invitation
  - [ ] Accept invitation
  - [ ] Verify invitation system works
- [x] Test switching cooldown
  - [ ] Switch religions
  - [ ] Verify cooldown prevents immediate switch
- [x] Test persistence
  - [x] Create religion, save game, reload
  - [x] Verify religion data persists
  - [x] Verify player data persists
- [x] Test edge cases
  - [x] Founder leaving religion
  - [x] Single-member religion
  - [x] Religion disbanding

---

## Phase 3.2: Ranking Systems (Week 3)
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

### Task 2.5: Testing Phase 3.2
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

## Phase 3.3: Perk System Core (Week 4-5)
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

### Task 3.6: Testing Phase 3.3
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

## Phase 3.4: Deity Perk Trees (Week 6-8)
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

## Phase 3.5: Integration & Polish (Week 9-10)
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

**Phase 3.1**: 16-22 hours (Week 1-2) ✅ **COMPLETED**
**Phase 3.2**: 10-12 hours (Week 3)
**Phase 3.3**: 17-21 hours (Week 4-5)
**Phase 3.4**: 38-48 hours (Week 6-8)
**Phase 3.5**: 40-51 hours (Week 9-10)

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
- Keep Phase 3 branch separate until fully tested
- Consider alpha/beta testing phases between major phases

---

## Notes

- All time estimates are approximate and may vary based on experience
- Some tasks can be done in parallel (e.g., different deity perk implementations)
- Testing time may increase if bugs are discovered
- UI work may take longer depending on Vintage Story GUI complexity
- Perk effect implementations may vary widely in complexity
- Balance testing should be iterative throughout Phase 3.4

---

## Phase 3.1 Status: ✅ COMPLETED

**Completed Tasks:**
- All data models created with protobuf serialization
- ReligionManager system fully implemented
- PlayerReligionDataManager system fully implemented
- All 10 religion commands implemented
- PantheonWarsSystem integration complete
- Persistence fully functional

**Next Phase:** Phase 3.2 - Ranking Systems (Prestige, Favor earning, HUD updates)
