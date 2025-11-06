# Phase 4: Religion-Based Deity System with Perk Trees

## Executive Summary

This document outlines a **fundamental redesign** of the Pantheon Wars deity system, moving from player-centric active abilities to religion-centric passive perk trees. This is a massive architectural shift that affects nearly every system in the mod.

### Key Changes

1. **Deity Assignment**: Moved from player-level to religion-level
2. **One Deity Rule**: Players serve the deity of their first religion only
3. **Ability System**: Replaced with passive perk trees
4. **Dual Ranking**: Both players (favor ranks) and religions (prestige ranks) progress independently
5. **Religion Perks**: Deity-specific perks that benefit the entire congregation
6. **Custom Religion System**: Custom implementation (not using Vintage Story's built-in groups)

---

## Table of Contents

1. [Architecture Comparison](#architecture-comparison)
2. [Core Concepts](#core-concepts)
3. [Data Models](#data-models)
4. [System Design](#system-design)
5. [Perk Tree System](#perk-tree-system)
6. [Implementation Phases](#implementation-phases)
7. [Migration Strategy](#migration-strategy)
8. [Deity Perk Trees](#deity-perk-trees)

---

## Architecture Comparison

### Current Design (Phase 1-3)

```
Player
  └─ Deity Assignment (DeityType)
  └─ Divine Favor (int)
  └─ Devotion Rank (DevotionRank)
  └─ Active Abilities (4 per deity)
      └─ Cooldowns
      └─ Favor Costs
```

**Flow**: Player → Deity → Abilities → PvP Combat

### New Design (Phase 4)

```
Player
  └─ Current Religion (single)
      └─ Religion Deity Assignment (DeityType)
      └─ Religion Prestige Rank (PrestigeRank)
      └─ Religion Perks (deity-specific)
  └─ Player Favor Rank (FavorRank)
  └─ Player Perks (favor-rank unlocked, deity-specific)
```

**Flow**: Player → Current Religion → Religion Deity → Player Favor Rank → Passive Perks (Player + Religion)

---

## Core Concepts

### 1. Religion-Based Deity System

**Rule**: A religion chooses a deity, not individual players.

- When a religion is created, it must select a deity to serve
- All members of that religion (the congregation) are considered followers of that deity
- Religions cannot change deities (permanent choice)
- **Custom Implementation**: We implement our own religion system, not using Vintage Story's built-in groups

### 2. Single Religion Per Player

**Rule**: A player can only be a member of ONE religion at a time.

- Players join a single religion that serves a specific deity
- To join a different religion, players must leave their current one
- Switching religions resets player favor and perks (prevents exploitation)
- The player's deity is directly determined by their current religion

**Example**:
```
Player Alice:
  - Current Religion: "Knights of Khoras" (Deity: Khoras)

Alice's Active Deity: Khoras
Alice gains perks from: Khoras only

If Alice wants to join "Hunters of Lysa":
  1. Must leave "Knights of Khoras" first
  2. Loses current favor/perks (or partial retention based on design)
  3. Joins "Hunters of Lysa"
  4. Active deity becomes Lysa
```

### 3. Dual Ranking System

#### Player Favor Ranks
- **Individual progression** based on favor earned
- Unlocks **player-specific perks** from the deity perk tree
- Ranks: `Initiate → Disciple → Zealot → Champion → Avatar`
- Tracked per player, persists across religions

#### Religion Prestige Ranks
- **Collective progression** based on religion achievements
- Unlocks **religion-wide perks** that benefit all congregation members
- Ranks: `Fledgling → Established → Renowned → Legendary → Mythic`
- Tracked per religion, shared by all members

### 4. Passive Perk Trees

**No more active abilities**. Instead, deities grant passive perks:

- **Player Perks**: Personal bonuses (damage, speed, resistances)
- **Religion Perks**: Congregation-wide benefits (shared buffs, territory bonuses)
- **Progression**: Perks unlock at specific favor/prestige ranks
- **Deity-Specific**: Each deity has unique perk trees

---

## Data Models

### New Models

#### ReligionData.cs
```csharp
public class ReligionData
{
    public string ReligionUID { get; set; }         // Unique identifier
    public string ReligionName { get; set; }        // Display name (e.g., "Knights of Khoras")
    public DeityType Deity { get; set; }            // Chosen deity (permanent)
    public string FounderUID { get; set; }          // Player who created the religion
    public List<string> MemberUIDs { get; set; }    // Player UIDs (ordered, first = founder)
    public PrestigeRank PrestigeRank { get; set; }  // Current prestige rank
    public int Prestige { get; set; }               // Current prestige points
    public int TotalPrestige { get; set; }          // Lifetime prestige earned
    public DateTime CreationDate { get; set; }
    public Dictionary<string, bool> UnlockedPerks { get; set; } // Religion perk unlocks
    public bool IsPublic { get; set; }              // Can anyone join, or invite-only?
    public string Description { get; set; }         // Religion description/manifesto
}
```

#### PlayerReligionData.cs
```csharp
public class PlayerReligionData
{
    public string PlayerUID { get; set; }
    public string? ReligionUID { get; set; }       // Current religion (null if none)
    public DeityType ActiveDeity { get; set; }     // Cached from current religion
    public FavorRank FavorRank { get; set; }       // Player's favor rank
    public int Favor { get; set; }                 // Current favor points
    public int TotalFavorEarned { get; set; }      // Lifetime favor (persists across religion changes)
    public Dictionary<string, bool> UnlockedPerks { get; set; } // Player perk unlocks
    public DateTime? LastReligionSwitch { get; set; } // Cooldown tracking for switching religions
    public int DataVersion { get; set; }           // For migrations
}
```

#### Perk.cs
```csharp
public class Perk
{
    public string PerkId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DeityType Deity { get; set; }
    public PerkType Type { get; set; }            // Player or Religion
    public PerkCategory Category { get; set; }    // Combat, Survival, Utility, etc.

    // Unlock requirements
    public int RequiredFavorRank { get; set; }    // For player perks (FavorRank enum value)
    public int RequiredPrestigeRank { get; set; } // For religion perks (PrestigeRank enum value)
    public List<string> PrerequisitePerks { get; set; } // Dependencies

    // Effects
    public Dictionary<string, float> StatModifiers { get; set; }
    public List<string> SpecialEffects { get; set; }
}
```

### Enums

#### PrestigeRank.cs
```csharp
/// <summary>
/// Religion prestige ranks - collective progression
/// </summary>
public enum PrestigeRank
{
    Fledgling = 0,    // 0-499 prestige
    Established = 1,  // 500-1999
    Renowned = 2,     // 2000-4999
    Legendary = 3,    // 5000-9999
    Mythic = 4        // 10000+
}
```

#### FavorRank.cs
```csharp
/// <summary>
/// Player favor ranks - individual progression
/// </summary>
public enum FavorRank
{
    Initiate = 0,     // 0-499 favor
    Disciple = 1,     // 500-1999
    Zealot = 2,       // 2000-4999
    Champion = 3,     // 5000-9999
    Avatar = 4        // 10000+
}
```

#### PerkType.cs
```csharp
public enum PerkType
{
    Player,    // Personal perks (unlocked by FavorRank)
    Religion   // Religion-wide perks (unlocked by PrestigeRank)
}
```

#### PerkCategory.cs
```csharp
public enum PerkCategory
{
    Combat,      // Damage, attack speed, etc.
    Defense,     // Resistances, health, shields
    Mobility,    // Movement speed, jumping, etc.
    Utility,     // Crafting, gathering, special mechanics
    Economic,    // Trade, resource generation
    Territory    // Land control, building bonuses
}
```

---

## System Design

### Core Systems

#### 1. ReligionManager.cs
**Purpose**: Manages all religions and congregation membership

**Responsibilities**:
- Create/delete religions
- Add/remove members
- Handle deity selection at religion creation
- Persist religion data
- Manage invitations and access control
- Enforce single-religion-per-player rule

**Key Methods**:
```csharp
ReligionData CreateReligion(string name, DeityType deity, string founderUID, bool isPublic)
void AddMember(string religionUID, string playerUID)
void RemoveMember(string religionUID, string playerUID)
ReligionData? GetPlayerReligion(string playerUID)
ReligionData? GetReligion(string religionUID)
DeityType GetPlayerActiveDeity(string playerUID)
bool CanJoinReligion(string religionUID, string playerUID)
void InvitePlayer(string religionUID, string playerUID, string inviterUID)
bool HasReligion(string playerUID)
List<ReligionData> GetAllReligions()
List<ReligionData> GetReligionsByDeity(DeityType deity)
```

#### 2. PlayerReligionDataManager.cs
**Purpose**: Manages player-religion relationships and player progression

**Responsibilities**:
- Track player's current religion
- Manage player favor and ranking
- Handle perk unlocks for players
- Sync active deity from current religion
- Handle religion switching with appropriate penalties/cooldowns

**Key Methods**:
```csharp
PlayerReligionData GetOrCreatePlayerData(string playerUID)
void AddFavor(string playerUID, int amount, string reason)
void UpdateFavorRank(string playerUID)
bool UnlockPlayerPerk(string playerUID, string perkId)
List<Perk> GetActivePlayerPerks(string playerUID)
void JoinReligion(string playerUID, string religionUID)
void LeaveReligion(string playerUID)
bool CanSwitchReligion(string playerUID)  // Check cooldown
void HandleReligionSwitch(string playerUID)  // Apply penalties
```

#### 3. ReligionPrestigeManager.cs
**Purpose**: Manages religion prestige and religion-level progression

**Responsibilities**:
- Award prestige to religions
- Update religion prestige ranks
- Handle religion perk unlocks
- Track religion achievements
- Distribute prestige rewards to congregation

**Key Methods**:
```csharp
void AddPrestige(string religionUID, int amount, string reason)
void UpdatePrestigeRank(string religionUID)
bool UnlockReligionPerk(string religionUID, string perkId)
List<Perk> GetActiveReligionPerks(string religionUID)
```

#### 4. PerkRegistry.cs
**Purpose**: Central registry of all perks for all deities

**Responsibilities**:
- Register all perks
- Provide perk lookup by ID
- Filter perks by deity/type/category
- Validate perk unlock requirements

**Key Methods**:
```csharp
void RegisterPerk(Perk perk)
Perk? GetPerk(string perkId)
List<Perk> GetPerksForDeity(DeityType deity, PerkType type)
bool CanUnlockPerk(PlayerReligionData playerData, ReligionData religionData, Perk perk)
```

#### 5. PerkEffectSystem.cs
**Purpose**: Applies perk effects to players and religions

**Responsibilities**:
- Calculate cumulative stat modifiers
- Apply passive effects
- Handle perk activation/deactivation
- Manage perk interactions
- Combine player and religion perk effects

**Key Methods**:
```csharp
Dictionary<string, float> GetPlayerStatModifiers(string playerUID)
Dictionary<string, float> GetReligionStatModifiers(string religionUID)
Dictionary<string, float> GetCombinedStatModifiers(string playerUID)
void ApplyPerksToPlayer(IPlayer player)
void RefreshPlayerPerks(string playerUID)
```

### System Interactions

```
Player kills enemy (PvP)
    ↓
FavorSystem.OnPlayerKill()
    ↓
PlayerReligionDataManager.AddFavor(playerUID, amount, "PvP Kill")
ReligionPrestigeManager.AddPrestige(religionUID, amount, "Member PvP Kill")
    ↓
PlayerReligionDataManager.UpdateFavorRank(playerUID)
ReligionPrestigeManager.UpdatePrestigeRank(religionUID)
    ↓
Check for new perk unlocks
    ↓
PerkEffectSystem.RefreshPlayerPerks(playerUID)
    ↓
Apply new stat modifiers (player + religion perks combined)
```

---

## Perk Tree System

### Perk Structure

Each deity has **two perk trees**:

1. **Player Perk Tree**: Unlocked by individual favor rank
2. **Religion Perk Tree**: Unlocked by religion prestige rank

### Perk Progression

#### Player Perks (5 tiers)
- **Tier 1 (Initiate)**: Basic passive bonuses
- **Tier 2 (Disciple)**: Enhanced combat stats
- **Tier 3 (Zealot)**: Specialized bonuses
- **Tier 4 (Champion)**: Powerful synergies
- **Tier 5 (Avatar)**: Ultimate passive abilities

#### Religion Perks (5 tiers)
- **Tier 1 (Fledgling)**: Basic congregation benefits
- **Tier 2 (Established)**: Congregation coordination bonuses
- **Tier 3 (Renowned)**: Territory and influence advantages
- **Tier 4 (Legendary)**: Dominance mechanics and powerful effects
- **Tier 5 (Mythic)**: World-changing effects for the faithful

### Perk Dependencies

Some perks require prerequisites:
```
Khoras (War) Player Tree:
  Tier 1: "Warrior's Resolve" (+5% melee damage)
    ↓
  Tier 2: "Battle Fury" (+10% melee damage, requires Warrior's Resolve)
    ↓
  Tier 3: "Berserker's Rage" (+15% melee damage at low HP, requires Battle Fury)
```

---

## Implementation Phases

### Phase 4.1: Foundation (Week 1-2)

**Goal**: Build custom religion system foundation

**Tasks**:
1. Create `ReligionData`, `PlayerReligionData`, `Perk` models
2. Create all new enums (`PrestigeRank`, `FavorRank`, `PerkType`, `PerkCategory`)
3. Implement `ReligionManager` (basic CRUD operations)
4. Implement `PlayerReligionDataManager` (player-religion relationships)
5. Add religion creation/management commands
6. Implement persistence for religions and player-religion data
7. Add invitation and access control systems

**Deliverables**:
- Players can create custom religions
- Players can join/leave religions (one at a time)
- Religions can select deities (permanent choice)
- Joining a religion sets active deity
- Public/private religion system works
- Invitation system functional
- Religion switching with cooldowns works
- Data persists across sessions

### Phase 4.2: Ranking Systems (Week 3)

**Goal**: Implement dual ranking progression

**Tasks**:
1. Implement `ReligionPrestigeManager`
2. Create prestige earning logic (religion achievements, member actions)
3. Implement favor earning (per-player, independent of religion)
4. Add rank progression algorithms
5. Create rank-up notifications
6. Add rank display to HUD

**Deliverables**:
- Players earn favor individually
- Religions earn prestige collectively
- Automatic rank progression for both systems
- HUD shows both player favor rank and religion prestige rank

### Phase 4.3: Perk System Core (Week 4-5)

**Goal**: Build perk system infrastructure

**Tasks**:
1. Implement `PerkRegistry`
2. Implement `PerkEffectSystem`
3. Create perk unlock validation
4. Add perk stat modifier calculation
5. Implement perk persistence
6. Create perk UI/commands

**Deliverables**:
- Perks can be registered and queried
- Perk unlock requirements work
- Stat modifiers apply to players
- Players can view available/unlocked perks

### Phase 4.4: Deity Perk Trees (Week 6-8)

**Goal**: Design and implement all deity perk trees

**Tasks**:
1. Design perk trees for all 8 deities (2 trees each = 16 trees)
2. Implement player perks for each deity (5 tiers × 8 deities = 40+ perks)
3. Implement religion perks for each deity (5 tiers × 8 deities = 40+ perks)
4. Balance perk effects
5. Test perk interactions and combinations
6. Document all perks with clear descriptions

**Deliverables**:
- All 8 deities have complete perk trees (player + religion)
- 80+ unique perks implemented
- Balanced and tested gameplay
- Full perk documentation

### Phase 4.5: Integration & Polish (Week 9-10)

**Goal**: Integrate everything and polish

**Tasks**:
1. Remove old ability system (deprecate `AbilitySystem`, `AbilityCooldownManager`, etc.)
2. Update all commands to use new religion/perk systems
3. Migrate existing player data to new format
4. Create perk tree visualization UI
5. Add religion management UI
6. Comprehensive testing
7. Update all documentation (README, guides, etc.)

**Deliverables**:
- Old ability system fully removed
- All systems integrated seamlessly
- Migration path for existing saves
- Religion management UI complete
- Perk tree UI complete
- Complete documentation

---

## Migration Strategy

### Data Migration

#### Existing PlayerDeityData → New PlayerReligionData

```csharp
// Migration logic
PlayerDeityData oldData = LoadOldData(playerUID);
PlayerReligionData newData = new PlayerReligionData
{
    PlayerUID = oldData.PlayerUID,
    ReligionUID = null,                  // Will be set when auto-creating religion
    ActiveDeity = oldData.DeityType,     // Preserve old deity
    FavorRank = ConvertDevotionToFavorRank(oldData.DevotionRank),
    Favor = oldData.DivineFavor,
    TotalFavorEarned = oldData.TotalFavorEarned,
    UnlockedPerks = new Dictionary<string, bool>(),
    LastReligionSwitch = null,
    DataVersion = 2
};
```

#### Creating Default Religion for Existing Players

**Option 1**: Auto-create solo religions
- Each existing player gets a solo religion with their deity
- Religion name: "{PlayerName}'s Followers of {Deity}"
- This becomes their primary religion
- Player is set as founder

**Option 2**: Reset deity assignments
- All players start fresh in Phase 4
- Must create/join a religion to select deity
- Old favor converts to new favor

**Recommendation**: Option 1 for continuity and player retention

### Deprecation Plan

1. **Phase 4.1**: Mark old systems as `[Obsolete]`
2. **Phase 4.3**: Disable old ability commands
3. **Phase 4.5**: Remove all old ability code

### Backward Compatibility

- **Save files**: Migration runs automatically on first load with version check
- **Commands**: Old deity/ability commands show migration message
- **UI**: Old UI elements replaced with new religion management UI
- **Auto-migration**: Existing players get solo religions created automatically

---

## Deity Perk Trees

### Template Structure

Each deity needs:
- **8-10 Player Perks** (Tier 1-5)
- **8-10 Religion Perks** (Tier 1-5)

### Example: Khoras (God of War)

#### Player Perk Tree

**Tier 1 (Initiate) - 0+ favor**
- `khoras_warriors_resolve`: +5% melee damage
- `khoras_armor_training`: +2% damage reduction

**Tier 2 (Disciple) - 500+ favor**
- `khoras_battle_fury`: +10% melee damage (requires Warrior's Resolve)
- `khoras_shield_mastery`: +5% block chance (requires Armor Training)

**Tier 3 (Zealot) - 2000+ favor**
- `khoras_berserker_rage`: +15% damage when below 40% HP (requires Battle Fury)
- `khoras_last_stand`: Cannot die for 5 seconds when receiving fatal blow (30min cooldown)

**Tier 4 (Champion) - 5000+ favor**
- `khoras_warlords_presence`: Nearby faithful gain +5% damage
- `khoras_indomitable`: Immune to knockback

**Tier 5 (Avatar) - 10000+ favor**
- `khoras_avatar_of_war`: All melee damage increased by 25%, gain lifesteal on kills

#### Religion Perk Tree

**Tier 1 (Fledgling) - 0+ prestige**
- `khoras_congregation_tactics`: All congregation members gain +3% damage when fighting together
- `khoras_shared_armament`: Congregation members can share weapons/armor from religion storage

**Tier 2 (Established) - 500+ prestige**
- `khoras_war_banner`: Religion territory gains +10% combat stats for all faithful
- `khoras_supply_lines`: Reduced hunger drain for congregation members in combat

**Tier 3 (Renowned) - 2000+ prestige**
- `khoras_legion`: Congregation members deal +5% damage per nearby faithful (max 25%)
- `khoras_fortifications`: Religion structures have +50% durability

**Tier 4 (Legendary) - 5000+ prestige**
- `khoras_conquest`: Killing enemy religion members grants religion prestige
- `khoras_armory`: Religion has shared storage for weapons/armor

**Tier 5 (Mythic) - 10000+ prestige**
- `khoras_divine_legion`: All congregation members gain Avatar-tier combat bonuses during holy wars

### All Deities Overview

Each deity's perks should reflect their domain:

- **Khoras (War)**: Melee damage, armor, group combat synergy
- **Lysa (Hunt)**: Ranged damage, tracking, mobility, ambush tactics
- **Morthen (Death)**: Life drain, debuffs, death-related mechanics
- **Aethra (Light)**: Healing, shields, anti-undead, support
- **Umbros (Shadows)**: Stealth, critical strikes, night bonuses
- **Tharos (Storms)**: AoE damage, elemental effects, weather control
- **Gaia (Earth)**: Durability, regeneration, resource gathering
- **Vex (Madness)**: Debuffs, confusion, unpredictable effects

---

## Technical Considerations

### Performance

- **Perk Calculation**: Cache stat modifiers, recalculate only on perk unlock/religion change
- **Religion Queries**: Index religions by deity for fast lookups
- **Persistence**: Batch save religion data, not per-action
- **Member Lookups**: Cache player's current religion for quick deity determination
- **Simple Lookups**: Single religion per player means O(1) deity lookup

### Scalability

- **Religions per Player**: 1 (enforced by design)
- **Max Congregation Size**: Recommend 10-100 members per religion
- **Perk Limits**: Consider max active perks to prevent stat inflation
- **Total Religions**: Should scale to hundreds of religions server-wide
- **Religion Switching**: Enforce cooldowns (e.g., 7 days) to prevent abuse

### Networking

- **Religion Sync**: Broadcast religion changes to all congregation members
- **Perk Updates**: Send perk effect packets to clients when perks unlock
- **Rank-ups**: Server-authoritative, client receives notifications
- **Invitation System**: Server validates invitations before allowing joins

### Testing

- **Unit Tests**: Perk unlock logic, stat calculations, religion membership
- **Integration Tests**: Full perk system with multiple religions
- **Balance Tests**: Ensure no perk combos are overpowered
- **Edge Cases**: Test religion switching, founder leaving, single-member religions, etc.
- **Switching Tests**: Verify cooldowns work, penalties apply correctly

---

## Commands Reference

### Religion Management

```
/religion create <name> <deity> [public/private]  - Create a new religion (leaves current if any)
/religion join <religionname>                     - Join a public religion or accept invitation (leaves current)
/religion leave                                   - Leave your current religion
/religion list [deity]                            - List all religions (optionally filter by deity)
/religion info <religionname>                     - Show detailed religion information
/religion members [religionname]                  - List congregation members (defaults to yours)
/religion invite <playername>                     - Invite player to your religion
/religion kick <playername>                       - Remove member from religion (founder only)
/religion disband                                 - Disband religion (founder only)
/religion description <text>                      - Set religion description/manifesto (founder only)
```

### Perk Management

```
/perks list                        - Show available perks for your deity
/perks player                      - Show your unlocked player perks
/perks religion                    - Show your religion's unlocked religion perks
/perks info <perkid>               - Detailed perk information
/perks tree [player/religion]      - Display full perk tree
/perks unlock <perkid>             - Unlock a perk (if requirements met)
/perks active                      - Show all active perks affecting you
```

### Status Commands

```
/deity status                      - Show your active deity and current religion
/favor                             - Show current favor and rank
/prestige                          - Show your religion's prestige and rank
/rank                              - Show both your favor rank and religion prestige rank
```

---

## UI Elements

### Required UI Components

1. **Religion Management Dialog**
   - Create/join/leave religion
   - Browse public religions
   - View congregation members
   - Religion prestige/rank display
   - Manage invitations
   - Religion switching warning (penalty notification)

2. **Perk Tree Viewer**
   - Visual tree layout for both player and religion perks
   - Unlock status indicators
   - Requirement tooltips
   - Stat preview
   - Toggle between player/religion trees

3. **Enhanced HUD**
   - Current religion name
   - Active deity
   - Player favor rank
   - Religion prestige rank
   - Active perk count (player + religion)

4. **Congregation Roster Panel**
   - Online/offline members
   - Member favor ranks
   - Quick actions (invite, kick)
   - Founder indicator
   - Total congregation size

---

## Success Metrics

### Phase Completion Criteria

**Phase 4.1**: ✅ Religions can be created, players can join, deity assignment works, invitations work
**Phase 4.2**: ✅ Both ranking systems functional, progression works
**Phase 4.3**: ✅ Perks can be unlocked and applied, effects stack properly
**Phase 4.4**: ✅ All deity perk trees complete and balanced
**Phase 4.5**: ✅ Old systems removed, migration complete, everything polished

### Gameplay Metrics

- Average congregation size
- Perk unlock rates (player vs religion)
- Deity distribution among religions
- Player retention with new system
- Balance: No single deity dominates
- Religion switching frequency
- Religion loyalty/retention rates

---

## Risks & Mitigation

### Risk: Players Resistant to Change
**Mitigation**: Provide smooth migration, highlight benefits of perk system, auto-create solo religions

### Risk: Balancing 80+ Perks
**Mitigation**: Start conservative, iterate based on playtesting, community feedback

### Risk: Religion System Complexity
**Mitigation**: Excellent UI/UX, clear documentation, helpful commands, tutorial messages

### Risk: Performance with Large Religions
**Mitigation**: Implement caching, optimize queries, set reasonable congregation size limits

### Risk: Religion Hopping for Benefits
**Mitigation**: Implement switching cooldowns, reset favor/perks on switch (or partial retention penalty)

---

## Conclusion

This redesign transforms Pantheon Wars from an individual player-deity system with active abilities into a religion-based community system with passive progression. It emphasizes:

- **Community Play**: Custom religions and congregation cooperation
- **Long-term Progression**: Persistent perk unlocks across dual ranking systems
- **Strategic Depth**: Perk tree choices and synergies
- **Deity Identity**: Unique playstyles through deity-specific perks
- **Player Agency**: Create and manage custom religions, not just join predefined groups
- **Commitment**: One religion per player encourages loyalty and meaningful choices

The shift from active abilities to passive perks reduces mechanical complexity while increasing strategic depth and replayability. The custom religion system (rather than using Vintage Story's built-in groups) provides complete control over the deity-worship mechanics. The single-religion-per-player rule creates meaningful commitment and prevents exploitation.

**Estimated Timeline**: 10 weeks for full implementation
**Complexity**: High - touches every major system and introduces custom religion management
**Impact**: Complete gameplay transformation with community-driven religious factions

---

**Next Steps**: Review this guide, approve the design, and begin Phase 4.1 implementation.
