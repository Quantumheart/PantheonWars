# Deity System Migration Plan

## Overview

This document outlines the plan to complete the migration from the legacy Phase 1-2 deity system to the Phase 3 religion-based deity system. This migration resolves the architectural inconsistency where player deity assignment fails when joining religions.

**Status**: Planning Phase
**Priority**: Critical
**Estimated Time**: 2-3 hours with TDD
**Related Documents**:
- `/docs/topics/ui-design/BLESSING_UI_IMPLEMENTATION_PLAN.md`
- Phase 3 Religion System Implementation

---

## Problem Statement

### Current State

Two parallel deity tracking systems exist:

1. **Legacy System (Phase 1-2)**
   - Manager: `PlayerDataManager`
   - Data Model: `PlayerDeityData`
   - Key Property: `DeityType`
   - Validation: `HasDeity()` checks `DeityType != DeityType.None`

2. **Current System (Phase 3)**
   - Manager: `PlayerReligionDataManager`
   - Data Model: `PlayerReligionData`
   - Key Property: `ActiveDeity`
   - Validation: Checks `ActiveDeity != DeityType.None`

### The Issue

When `PlayerReligionDataManager.JoinReligion()` is called:
- ✅ Sets `PlayerReligionData.ActiveDeity` correctly
- ❌ Does NOT set `PlayerDeityData.DeityType`

**Result**: Systems checking `HasDeity()` fail even when player is in a religion with an active deity.

### Affected Systems

| System | File | Issue | Lines |
|--------|------|-------|-------|
| Favor Commands | `FavorCommands.cs` | 9 `HasDeity()` validation checks fail | 124, 144, 191, 268, 292, 317, 344, 363, 380 |
| Favor Generation | `FavorSystem.cs` | 5 checks fail - passive favor broken | Multiple |
| Ability Commands | `AbilityCommands.cs` | 3 validation checks fail | Multiple |
| Deity Commands | `DeityCommands.cs` | 3 validation checks fail | Multiple |

### Already Migrated Systems

- ✅ `BlessingCommands.cs` - Uses `PlayerReligionData.ActiveDeity`
- ✅ Blessing Dialog UI - Uses `PlayerReligionDataManager`
- ✅ Server blessing handlers - Correct implementation

---

## Migration Strategy

### Approach: Complete Migration (Option B)

We will complete the migration to Phase 3 by updating all legacy system dependencies to use `PlayerReligionDataManager` and `PlayerReligionData.ActiveDeity`.

**Why Option B over Option A (Bridge Pattern)?**
- Eliminates technical debt
- Prevents future bugs from system divergence
- Aligns with intended Phase 3 architecture
- Creates clean foundation for future features
- Only ~2-3 hours with TDD approach

---

## Implementation Phases

### Phase 1: Test Infrastructure Setup (30 minutes)

#### 1.1 Create Test Files

Create comprehensive test coverage BEFORE making changes (TDD approach):

**File**: `/PantheonWars.Tests/Systems/FavorSystemTests.cs`
```csharp
using Xunit;
using PantheonWars.Systems;
using PantheonWars.Models.Enum;

namespace PantheonWars.Tests.Systems;

public class FavorSystemTests
{
    [Fact]
    public void OnPlayerTick_PlayerInReligionWithDeity_GeneratesFavor()
    {
        // Test passive favor generation for religion members
    }

    [Fact]
    public void OnPlayerTick_PlayerNotInReligion_DoesNotGenerateFavor()
    {
        // Test that favor is not generated without religion
    }

    [Fact]
    public void AwardFavor_PlayerInReligion_AddsFavorAndUpdatesRank()
    {
        // Test favor award and rank progression
    }

    [Fact]
    public void AwardFavor_ReligionExists_IncreasesPrestige()
    {
        // Test prestige increase for religion
    }

    [Fact]
    public void CalculateFavorMultiplier_WithActiveBlessings_AppliesCorrectMultiplier()
    {
        // Test blessing multiplier effects
    }
}
```

**File**: `/PantheonWars.Tests/Commands/FavorCommandsIntegrationTests.cs`
```csharp
using Xunit;
using PantheonWars.Commands;
using PantheonWars.Models.Enum;

namespace PantheonWars.Tests.Commands;

public class FavorCommandsIntegrationTests
{
    [Fact]
    public void OnCheckFavor_PlayerInReligion_ReturnsDeityInfo()
    {
        // Test /favor command with religion membership
    }

    [Fact]
    public void OnCheckFavor_PlayerNotInReligion_ReturnsNotPledgedMessage()
    {
        // Test /favor command without religion
    }

    [Fact]
    public void OnFavorInfo_PlayerInReligion_ShowsDetailedProgression()
    {
        // Test /favor info with religion data
    }

    [Fact]
    public void OnFavorStats_PlayerInReligion_ShowsComprehensiveStats()
    {
        // Test /favor stats display
    }
}
```

**File**: `/PantheonWars.Tests/Commands/AbilityCommandsIntegrationTests.cs`
```csharp
using Xunit;
using PantheonWars.Commands;

namespace PantheonWars.Tests.Commands;

public class AbilityCommandsIntegrationTests
{
    [Fact]
    public void OnUseAbility_PlayerInReligionWithDeity_ExecutesAbility()
    {
        // Test ability execution with religion deity
    }

    [Fact]
    public void OnUseAbility_PlayerNotInReligion_ReturnsError()
    {
        // Test ability execution without religion
    }
}
```

#### 1.2 Run Initial Tests

```bash
dotnet build PantheonWars.Tests
dotnet test PantheonWars.Tests --filter "FullyQualifiedName~FavorSystemTests|FavorCommandsIntegrationTests|AbilityCommandsIntegrationTests"
```

**Expected Result**: All tests should fail (red) - this is correct for TDD!

---

### Phase 2: Core System Migration (45 minutes)

#### 2.1 Update FavorSystem.cs

**File**: `/PantheonWars/Systems/FavorSystem.cs`

**Current Implementation Issues**:
```csharp
// OLD: Uses PlayerDataManager.HasDeity()
if (!playerData.HasDeity()) continue;
```

**Migration Changes**:

1. **Add PlayerReligionDataManager dependency**:
```csharp
public class FavorSystem
{
    private readonly PlayerDataManager _playerDataManager;
    private readonly PlayerReligionDataManager _religionDataManager; // ADD THIS
    private readonly DeityRegistry _deityRegistry;

    public FavorSystem(
        ICoreServerAPI sapi,
        PlayerDataManager playerDataManager,
        PlayerReligionDataManager religionDataManager, // ADD THIS
        DeityRegistry deityRegistry)
    {
        _sapi = sapi;
        _playerDataManager = playerDataManager;
        _religionDataManager = religionDataManager; // ADD THIS
        _deityRegistry = deityRegistry;
    }
}
```

2. **Update HasDeity() checks** (replace all 5 occurrences):
```csharp
// OLD
if (!playerData.HasDeity()) continue;

// NEW
var religionData = _religionDataManager.GetPlayerReligionData(playerUID);
if (religionData == null || religionData.ActiveDeity == DeityType.None) continue;
```

3. **Update deity retrieval**:
```csharp
// OLD
var deity = _deityRegistry.GetDeity(playerData.DeityType);

// NEW
var deity = _deityRegistry.GetDeity(religionData.ActiveDeity);
```

4. **Update favor award to use religion prestige**:
```csharp
// When awarding favor, also update religion prestige
public void AwardFavor(string playerUID, int amount, string reason)
{
    var religionData = _religionDataManager.GetPlayerReligionData(playerUID);
    if (religionData == null || religionData.ActiveDeity == DeityType.None)
    {
        _sapi.Logger.Warning($"Cannot award favor to {playerUID}: Not in a religion");
        return;
    }

    // Award favor to player (updates TotalFavorEarned and rank)
    var playerData = _playerDataManager.GetOrCreatePlayerData(player);
    var oldFavor = playerData.DivineFavor;
    playerData.DivineFavor += amount;
    playerData.TotalFavorEarned += amount;
    playerData.UpdateDevotionRank();

    // Update religion prestige if in a religion
    if (!string.IsNullOrEmpty(religionData.ReligionUID))
    {
        _religionDataManager.AddPrestigeToReligion(religionData.ReligionUID, amount / 2);
    }

    _sapi.Logger.Notification($"[FavorSystem] {playerUID} earned {amount} favor: {reason}");
}
```

**Estimated Lines Changed**: ~15 lines across 5 methods

#### 2.2 Update PantheonWarsModSystem.cs

**File**: `/PantheonWars/PantheonWarsModSystem.cs`

Update FavorSystem initialization to pass PlayerReligionDataManager:

```csharp
// In StartServerSide() method
_favorSystem = new FavorSystem(
    sapi,
    _playerDataManager,
    _playerReligionDataManager, // ADD THIS
    _deityRegistry
);
```

#### 2.3 Run Tests

```bash
dotnet test PantheonWars.Tests --filter "FullyQualifiedName~FavorSystemTests"
```

**Expected Result**: FavorSystemTests should now pass (green)

---

### Phase 3: Commands Migration (45 minutes)

#### 3.1 Update FavorCommands.cs

**File**: `/PantheonWars/Commands/FavorCommands.cs`

**Current Implementation Issues**:
- Uses `PlayerDataManager` only
- 9 occurrences of `HasDeity()` checks
- No access to religion data

**Migration Changes**:

1. **Add PlayerReligionDataManager dependency**:
```csharp
public class FavorCommands
{
    private readonly DeityRegistry _deityRegistry;
    private readonly PlayerDataManager _playerDataManager;
    private readonly PlayerReligionDataManager _religionDataManager; // ADD THIS
    private readonly ICoreServerAPI _sapi;

    public FavorCommands(
        ICoreServerAPI sapi,
        DeityRegistry deityRegistry,
        PlayerDataManager playerDataManager,
        PlayerReligionDataManager religionDataManager) // ADD THIS
    {
        _sapi = sapi;
        _deityRegistry = deityRegistry;
        _playerDataManager = playerDataManager;
        _religionDataManager = religionDataManager; // ADD THIS
    }
}
```

2. **Create helper method for validation**:
```csharp
/// <summary>
///     Get player's religion data and validate they have a deity
/// </summary>
private (PlayerReligionData? religionData, TextCommandResult? errorResult) ValidatePlayerHasDeity(IServerPlayer player)
{
    var religionData = _religionDataManager.GetPlayerReligionData(player.PlayerUID);

    if (religionData == null || religionData.ActiveDeity == DeityType.None)
    {
        return (null, TextCommandResult.Error("You are not in a religion or do not have an active deity."));
    }

    return (religionData, null);
}
```

3. **Update all command methods** (9 occurrences):

**Example - OnCheckFavor (lines 117-132)**:
```csharp
// OLD
private TextCommandResult OnCheckFavor(TextCommandCallingArgs args)
{
    var player = args.Caller.Player as IServerPlayer;
    if (player == null) return TextCommandResult.Error("Command must be used by a player");

    var playerData = _playerDataManager.GetOrCreatePlayerData(player);

    if (!playerData.HasDeity()) return TextCommandResult.Success("You have not pledged to any deity yet.");

    var deity = _deityRegistry.GetDeity(playerData.DeityType);
    var deityName = deity?.Name ?? playerData.DeityType.ToString();

    return TextCommandResult.Success(
        $"You have {playerData.DivineFavor} favor with {deityName} (Rank: {playerData.DevotionRank})"
    );
}

// NEW
private TextCommandResult OnCheckFavor(TextCommandCallingArgs args)
{
    var player = args.Caller.Player as IServerPlayer;
    if (player == null) return TextCommandResult.Error("Command must be used by a player");

    var (religionData, errorResult) = ValidatePlayerHasDeity(player);
    if (errorResult.HasValue) return errorResult.Value;

    var playerData = _playerDataManager.GetOrCreatePlayerData(player);
    var deity = _deityRegistry.GetDeity(religionData!.ActiveDeity);
    var deityName = deity?.Name ?? religionData.ActiveDeity.ToString();
    var religionName = religionData.ReligionName ?? "Unknown Religion";

    return TextCommandResult.Success(
        $"[{religionName}] You have {playerData.DivineFavor} favor with {deityName} (Rank: {playerData.DevotionRank})"
    );
}
```

**Apply same pattern to**:
- `OnFavorInfo()` (line 137)
- `OnFavorStats()` (line 184)
- `OnSetFavor()` (line 261)
- `OnAddFavor()` (line 285)
- `OnRemoveFavor()` (line 310)
- `OnResetFavor()` (line 337)
- `OnMaxFavor()` (line 355)
- `OnSetTotalFavor()` (line 373)

**Note**: `OnListRanks()` does NOT need migration - it's informational only and doesn't require deity check.

**Estimated Lines Changed**: ~50 lines across 9 methods

#### 3.2 Update AbilityCommands.cs

**File**: `/PantheonWars/Commands/AbilityCommands.cs`

**Migration Changes**:

1. Add `PlayerReligionDataManager` dependency
2. Replace 3 `HasDeity()` checks with religion validation
3. Use `religionData.ActiveDeity` instead of `playerData.DeityType`

```csharp
// Similar pattern to FavorCommands.cs
private (PlayerReligionData? religionData, TextCommandResult? errorResult) ValidatePlayerHasDeity(IServerPlayer player)
{
    var religionData = _religionDataManager.GetPlayerReligionData(player.PlayerUID);

    if (religionData == null || religionData.ActiveDeity == DeityType.None)
    {
        return (null, TextCommandResult.Error("You must be in a religion with an active deity to use abilities."));
    }

    return (religionData, null);
}
```

**Estimated Lines Changed**: ~20 lines across 3 methods

#### 3.3 Update DeityCommands.cs

**File**: `/PantheonWars/Commands/DeityCommands.cs`

**Migration Changes**:

1. Add `PlayerReligionDataManager` dependency
2. Replace 3 `HasDeity()` checks with religion validation
3. Use `religionData.ActiveDeity` for deity info display

**Estimated Lines Changed**: ~20 lines across 3 methods

#### 3.4 Update command registrations in PantheonWarsModSystem.cs

**File**: `/PantheonWars/PantheonWarsModSystem.cs`

```csharp
// Update command initializations
var favorCommands = new FavorCommands(
    sapi,
    _deityRegistry,
    _playerDataManager,
    _playerReligionDataManager // ADD THIS
);

var abilityCommands = new AbilityCommands(
    sapi,
    _playerDataManager,
    _religionDataManager, // ADD THIS
    _abilityRegistry
);

var deityCommands = new DeityCommands(
    sapi,
    _deityRegistry,
    _playerDataManager,
    _religionDataManager // ADD THIS
);
```

#### 3.5 Run Tests

```bash
dotnet test PantheonWars.Tests --filter "FullyQualifiedName~FavorCommandsIntegrationTests|AbilityCommandsIntegrationTests"
```

**Expected Result**: All command tests should pass (green)

---

### Phase 4: Legacy System Deprecation (15 minutes)

#### 4.1 Mark Legacy Methods as Obsolete

**File**: `/PantheonWars/Systems/PlayerDataManager.cs`

```csharp
/// <summary>
///     Check if player has pledged to a deity
///     DEPRECATED: Use PlayerReligionDataManager.GetPlayerReligionData() and check ActiveDeity instead
/// </summary>
[Obsolete("Use PlayerReligionDataManager.GetPlayerReligionData() and check ActiveDeity != DeityType.None instead. This method will be removed in v2.0.0")]
public bool HasDeity(string playerUID)
{
    var data = GetPlayerDeityData(playerUID);
    return data != null && data.DeityType != DeityType.None;
}

/// <summary>
///     Set player's deity
///     DEPRECATED: Use PlayerReligionDataManager.JoinReligion() instead
/// </summary>
[Obsolete("Use PlayerReligionDataManager.JoinReligion() instead. This method will be removed in v2.0.0")]
public void SetDeity(string playerUID, DeityType deity)
{
    var data = GetOrCreatePlayerDeityData(playerUID);
    data.DeityType = deity;
    data.PledgeDate = DateTime.UtcNow;
}
```

**File**: `/PantheonWars/Models/PlayerDeityData.cs`

Add XML doc comment at class level:
```csharp
/// <summary>
///     Player deity data (LEGACY - Phase 1-2)
///     DEPRECATED: Use PlayerReligionData from Phase 3 religion system instead
///     This model will be removed in v2.0.0
/// </summary>
[Obsolete("Use PlayerReligionData instead. This class will be removed in v2.0.0")]
public class PlayerDeityData
{
    // ... existing properties
}
```

#### 4.2 Add Migration Notes

**File**: `/docs/topics/architecture/MIGRATION_NOTES.md` (NEW)

```markdown
# Migration Notes

## Phase 3 Religion System Migration

### v1.5.0 - Deity System Migration (2024-11-11)

**Breaking Changes**: None (backward compatibility maintained)

**Deprecated APIs**:
- `PlayerDataManager.HasDeity()` - Use `PlayerReligionDataManager.GetPlayerReligionData()` and check `ActiveDeity != DeityType.None`
- `PlayerDataManager.SetDeity()` - Use `PlayerReligionDataManager.JoinReligion()`
- `PlayerDeityData` model - Use `PlayerReligionData` instead

**Migration Path**:
```csharp
// OLD
if (playerDataManager.HasDeity(playerUID))
{
    var deity = playerData.DeityType;
}

// NEW
var religionData = religionDataManager.GetPlayerReligionData(playerUID);
if (religionData != null && religionData.ActiveDeity != DeityType.None)
{
    var deity = religionData.ActiveDeity;
}
```

**Timeline**:
- v1.5.0: Legacy APIs marked as obsolete (warnings)
- v1.8.0: Legacy APIs marked as errors
- v2.0.0: Legacy APIs removed entirely
```

#### 4.3 Update Changelogs

**File**: `/CHANGELOG.md`

Add entry:
```markdown
## [Unreleased]

### Changed
- **[BREAKING - Deprecated]** Migrated deity system from Phase 1-2 to Phase 3 religion-based architecture
  - `PlayerDataManager.HasDeity()` is now obsolete - use `PlayerReligionDataManager.GetPlayerReligionData()` instead
  - `PlayerDataManager.SetDeity()` is now obsolete - use `PlayerReligionDataManager.JoinReligion()` instead
  - `PlayerDeityData` is now obsolete - use `PlayerReligionData` instead
  - All favor, ability, and deity commands now use religion-based deity system
  - Passive favor generation now correctly works for religion members

### Fixed
- Fixed player deity not being recognized after joining religion
- Fixed passive favor generation not working for religion members
- Fixed favor commands showing "not pledged" error for religion members
- Fixed ability commands not working for religion members
```

---

### Phase 5: Integration Testing (15 minutes)

#### 5.1 Manual Testing Checklist

**Test Environment Setup**:
1. Start fresh server with clean world
2. Create 2-3 test players

**Test Case 1: Religion Joining Flow**
```
1. Player creates religion with Khoras deity
   - Expected: Religion created successfully

2. Run `/favor` command
   - Expected: Shows favor with Khoras (not "no deity" error)

3. Check favor is being awarded
   - Expected: Favor increases over time (passive generation)

4. Run `/favor info`
   - Expected: Shows detailed progression with religion name
```

**Test Case 2: Favor Commands**
```
1. Join existing religion
2. Run `/favor get` - Should show deity and favor
3. Run `/favor info` - Should show rank progression
4. Run `/favor stats` - Should show comprehensive stats
5. Run `/favor ranks` - Should list all ranks (no deity required)

Admin commands (requires root privilege):
6. Run `/favor add 50` - Should increase favor
7. Run `/favor set 100` - Should set favor to 100
8. Run `/favor settotal 500` - Should update rank to Champion
```

**Test Case 3: Ability Commands**
```
1. Join religion with deity
2. Run `/ability list` - Should show available abilities
3. Run `/ability use <ability>` - Should execute ability
4. Leave religion
5. Run `/ability use <ability>` - Should show error about no deity
```

**Test Case 4: Cross-System Integration**
```
1. Join religion
2. Kill enemy player
3. Check favor increased (FavorSystem working)
4. Check religion prestige increased
5. Run `/favor stats` - Verify kill count incremented
6. Open blessing dialog - Verify deity and favor display correctly
```

**Test Case 5: Backward Compatibility**
```
1. Load save from before migration (if available)
2. Verify existing religions still work
3. Verify existing favor values preserved
4. Verify player can still interact with all systems
```

#### 5.2 Run Full Test Suite

```bash
# Run all tests
dotnet test PantheonWars.Tests

# Expected: All 50+ tests pass (35 existing + ~15 new)
```

#### 5.3 Check for Obsolete Warnings

```bash
# Build and check for obsolete API usage
dotnet build | grep -i "obsolete\|deprecated"

# Expected: Warnings in legacy code only, no warnings in new/migrated code
```

---

### Phase 6: Documentation Updates (15 minutes)

#### 6.1 Update Architecture Diagrams

**File**: `/docs/topics/architecture/SYSTEM_ARCHITECTURE.md`

Add section:
```markdown
## Deity and Religion System Architecture

### Phase 3 (Current)

```
┌─────────────────────────────────────────────────────────────┐
│                    Deity & Religion System                   │
└─────────────────────────────────────────────────────────────┘

┌──────────────────────┐         ┌──────────────────────────┐
│  PlayerReligionData  │         │  Religion                │
│  ────────────────    │◄────────│  ────────────            │
│  - ReligionUID       │         │  - UID                   │
│  - ActiveDeity       │         │  - Deity (DeityType)     │
│  - ReligionName      │         │  - Prestige              │
│  - Role              │         │  - PrestigeRank          │
└──────────────────────┘         └──────────────────────────┘
          ▲                                   ▲
          │                                   │
          │ Managed by                        │ Managed by
          │                                   │
┌─────────┴──────────────────┐   ┌───────────┴──────────────┐
│ PlayerReligionDataManager  │   │  ReligionManager         │
│ ─────────────────────────  │   │  ───────────────         │
│ - GetPlayerReligionData()  │   │  - CreateReligion()      │
│ - JoinReligion()           │   │  - GetReligion()         │
│ - LeaveReligion()          │   │  - AddPrestige()         │
└────────────────────────────┘   └──────────────────────────┘
          ▲                                   ▲
          │                                   │
          └───────────┬───────────────────────┘
                      │
                      │ Used by
                      │
          ┌───────────┴────────────┐
          │   Consuming Systems    │
          │   ─────────────────    │
          │   - FavorSystem        │
          │   - FavorCommands      │
          │   - AbilityCommands    │
          │   - DeityCommands      │
          │   - BlessingCommands   │
          │   - BlessingDialog     │
          └────────────────────────┘

### Legacy Phase 1-2 (DEPRECATED)

┌──────────────────────┐
│  PlayerDeityData     │◄─── DEPRECATED in v1.5.0
│  ────────────────    │     Removed in v2.0.0
│  - DeityType         │
│  - PledgeDate        │
└──────────────────────┘
          ▲
          │ Managed by (DEPRECATED)
          │
┌─────────┴──────────────────┐
│  PlayerDataManager         │
│  ─────────────────────     │
│  - HasDeity() [Obsolete]   │
│  - SetDeity() [Obsolete]   │
└────────────────────────────┘
```

**Migration Timeline**:
- **v1.5.0** (Current): Phase 3 system active, Phase 1-2 marked obsolete
- **v1.8.0** (Planned): Obsolete APIs emit errors instead of warnings
- **v2.0.0** (Planned): Complete removal of Phase 1-2 system
```

#### 6.2 Update API Documentation

**File**: `/docs/topics/api/FAVOR_SYSTEM_API.md`

Update with new examples:
```markdown
## FavorSystem API

### Awarding Favor

```csharp
// Get player's religion data first
var religionData = _religionDataManager.GetPlayerReligionData(playerUID);
if (religionData == null || religionData.ActiveDeity == DeityType.None)
{
    // Player not in religion - cannot award favor
    return;
}

// Award favor (also increases religion prestige)
_favorSystem.AwardFavor(playerUID, 10, "Defeated enemy");
```

### Checking Deity Status

```csharp
// NEW (Phase 3) - Recommended
var religionData = _religionDataManager.GetPlayerReligionData(playerUID);
bool hasDeity = religionData != null && religionData.ActiveDeity != DeityType.None;
if (hasDeity)
{
    var deity = religionData.ActiveDeity;
    // ... use deity
}

// OLD (Phase 1-2) - DEPRECATED
var playerData = _playerDataManager.GetOrCreatePlayerData(player);
if (playerData.HasDeity()) // ⚠️ OBSOLETE - generates warning
{
    var deity = playerData.DeityType; // ⚠️ DEPRECATED
    // ...
}
```
```

#### 6.3 Create Migration Guide for Mod Developers

**File**: `/docs/topics/guides/MIGRATION_GUIDE_V1_5.md` (NEW)

```markdown
# Migration Guide: v1.4 → v1.5 (Deity System)

This guide helps third-party mod developers migrate from the legacy Phase 1-2 deity system to the Phase 3 religion-based deity system.

## Overview

**What Changed**: Player deity tracking moved from `PlayerDataManager` to `PlayerReligionDataManager`.

**Impact**: Low - Backward compatibility maintained, but deprecated APIs will be removed in v2.0.0.

## Quick Migration Checklist

- [ ] Replace `PlayerDataManager.HasDeity()` calls
- [ ] Replace `PlayerDataManager.SetDeity()` calls
- [ ] Update deity type retrieval to use `PlayerReligionData.ActiveDeity`
- [ ] Test with religion membership instead of direct deity pledge
- [ ] Remove obsolete API warnings from build output

## Step-by-Step Migration

### 1. Update Dependencies

If your mod depends on PantheonWars, update your constructor to include `PlayerReligionDataManager`:

**Before**:
```csharp
public class MyMod
{
    private readonly PlayerDataManager _playerDataManager;

    public MyMod(PlayerDataManager playerDataManager)
    {
        _playerDataManager = playerDataManager;
    }
}
```

**After**:
```csharp
public class MyMod
{
    private readonly PlayerDataManager _playerDataManager;
    private readonly PlayerReligionDataManager _religionDataManager; // ADD

    public MyMod(
        PlayerDataManager playerDataManager,
        PlayerReligionDataManager religionDataManager) // ADD
    {
        _playerDataManager = playerDataManager;
        _religionDataManager = religionDataManager; // ADD
    }
}
```

### 2. Replace HasDeity() Checks

**Before**:
```csharp
if (_playerDataManager.HasDeity(playerUID))
{
    var deity = playerData.DeityType;
    // ... use deity
}
```

**After**:
```csharp
var religionData = _religionDataManager.GetPlayerReligionData(playerUID);
if (religionData != null && religionData.ActiveDeity != DeityType.None)
{
    var deity = religionData.ActiveDeity;
    // ... use deity
}
```

### 3. Replace SetDeity() Calls

**Before**:
```csharp
_playerDataManager.SetDeity(playerUID, DeityType.Khoras);
```

**After**:
```csharp
// Players now join religions instead of pledging directly to deities
// If you need to programmatically set deity, have player join/create religion:

// Option 1: Create new religion for player
var religion = new Religion
{
    UID = Guid.NewGuid().ToString(),
    Name = "My Religion",
    Deity = DeityType.Khoras,
    LeaderUID = playerUID,
    // ... other properties
};
_religionManager.CreateReligion(religion);
_religionDataManager.JoinReligion(playerUID, religion);

// Option 2: Join existing religion
var existingReligion = _religionManager.GetReligionByUID(religionUID);
if (existingReligion != null)
{
    _religionDataManager.JoinReligion(playerUID, existingReligion);
}
```

### 4. Access Religion Information

New capabilities available with Phase 3:

```csharp
var religionData = _religionDataManager.GetPlayerReligionData(playerUID);
if (religionData != null)
{
    // Access deity
    var deity = religionData.ActiveDeity;

    // Access religion info
    var religionName = religionData.ReligionName;
    var role = religionData.Role; // "Leader" or "Member"
    var religionUID = religionData.ReligionUID;

    // Get full religion object
    var religion = _religionManager.GetReligionByUID(religionData.ReligionUID);
    if (religion != null)
    {
        var prestige = religion.Prestige;
        var prestigeRank = religion.PrestigeRank;
        var memberCount = religion.MemberUIDs.Count;
    }
}
```

## Common Patterns

### Pattern 1: Validation Helper

Create reusable validation method:

```csharp
private bool TryGetPlayerDeity(
    string playerUID,
    out DeityType deity,
    out PlayerReligionData? religionData)
{
    religionData = _religionDataManager.GetPlayerReligionData(playerUID);

    if (religionData != null && religionData.ActiveDeity != DeityType.None)
    {
        deity = religionData.ActiveDeity;
        return true;
    }

    deity = DeityType.None;
    return false;
}

// Usage
if (TryGetPlayerDeity(playerUID, out var deity, out var religionData))
{
    // Player has deity through religion
    Console.WriteLine($"Player deity: {deity} from {religionData.ReligionName}");
}
```

### Pattern 2: Backward Compatibility Bridge

If you need to support both old and new PantheonWars versions:

```csharp
private bool PlayerHasDeity(string playerUID, out DeityType deity)
{
    // Try Phase 3 first
    if (_religionDataManager != null)
    {
        var religionData = _religionDataManager.GetPlayerReligionData(playerUID);
        if (religionData != null && religionData.ActiveDeity != DeityType.None)
        {
            deity = religionData.ActiveDeity;
            return true;
        }
    }

    // Fallback to Phase 1-2 (for old PantheonWars versions)
    #pragma warning disable CS0618 // Suppress obsolete warning
    if (_playerDataManager.HasDeity(playerUID))
    {
        var playerData = _playerDataManager.GetPlayerDeityData(playerUID);
        deity = playerData.DeityType;
        return true;
    }
    #pragma warning restore CS0618

    deity = DeityType.None;
    return false;
}
```

## Testing Your Migration

1. **Build your mod** - Check for obsolete warnings:
```bash
dotnet build | grep -i "obsolete"
```

2. **Test deity detection**:
   - Create test religion
   - Join with player
   - Verify your mod recognizes deity

3. **Test edge cases**:
   - Player with no religion (should return None)
   - Player leaving religion (deity should become None)
   - Player switching religions (deity should update)

## Timeline

- **v1.5.0** (Current): Legacy APIs deprecated, warnings issued
- **v1.8.0** (Q2 2025): Deprecated APIs emit errors
- **v2.0.0** (Q4 2025): Legacy APIs removed entirely

**Recommendation**: Migrate before v1.8.0 to avoid breaking changes.

## Support

If you encounter issues during migration:
1. Check `/docs/topics/architecture/DEITY_SYSTEM_MIGRATION_PLAN.md`
2. Review example code in PantheonWars commands (FavorCommands.cs, etc.)
3. Open issue on GitHub with migration question

## Summary

| Old API | New API | Status |
|---------|---------|--------|
| `PlayerDataManager.HasDeity()` | `PlayerReligionDataManager.GetPlayerReligionData()` | Deprecated |
| `PlayerDataManager.SetDeity()` | `PlayerReligionDataManager.JoinReligion()` | Deprecated |
| `PlayerDeityData.DeityType` | `PlayerReligionData.ActiveDeity` | Deprecated |
| Direct deity pledge | Religion membership | Required |
```

---

## Success Criteria

### Functional Requirements

- ✅ All favor commands work for religion members
- ✅ Passive favor generation works for religion members
- ✅ Ability commands work for religion members
- ✅ Deity commands work for religion members
- ✅ Religion prestige increases when favor is awarded
- ✅ Blessing dialog displays deity correctly
- ✅ No "not pledged to deity" errors for religion members

### Technical Requirements

- ✅ All existing tests pass
- ✅ All new tests pass (15+ new tests)
- ✅ Zero compilation errors
- ✅ Obsolete warnings only in legacy code
- ✅ No breaking changes to public APIs
- ✅ Backward compatibility maintained

### Documentation Requirements

- ✅ Migration plan documented
- ✅ API documentation updated
- ✅ Architecture diagrams updated
- ✅ Migration guide for third-party developers
- ✅ Changelog updated
- ✅ Deprecation timeline documented

---

## Timeline Estimate

| Phase | Task | Estimated Time | Cumulative |
|-------|------|----------------|------------|
| 1 | Test Infrastructure Setup | 30 minutes | 0:30 |
| 2 | Core System Migration (FavorSystem) | 45 minutes | 1:15 |
| 3 | Commands Migration (3 files) | 45 minutes | 2:00 |
| 4 | Legacy System Deprecation | 15 minutes | 2:15 |
| 5 | Integration Testing | 15 minutes | 2:30 |
| 6 | Documentation Updates | 15 minutes | 2:45 |
| | **Buffer** | 15 minutes | 3:00 |

**Total Estimated Time**: 2.5 - 3 hours

---

## Risk Assessment

### Low Risk
- ✅ Well-defined scope
- ✅ TDD approach reduces bugs
- ✅ Backward compatibility maintained
- ✅ Clear rollback path (revert commits)

### Medium Risk
- ⚠️ Integration testing may reveal edge cases
- ⚠️ Third-party mods may use deprecated APIs
- **Mitigation**: Comprehensive test suite, thorough documentation

### High Risk
- ❌ None identified

---

## Rollback Plan

If critical issues are discovered after migration:

1. **Immediate rollback** via git:
```bash
git revert <migration-commit-hash>
git push
```

2. **Notify users** about rollback in patch notes

3. **Investigate issue** in development environment

4. **Re-apply migration** with fixes

**Rollback Risk**: Low - All changes are additive, no data loss

---

## Post-Migration Tasks

### Immediate (v1.5.0)
- [ ] Monitor server logs for unexpected errors
- [ ] Gather player feedback on favor system
- [ ] Address any edge cases discovered

### Short-term (v1.6.0 - v1.7.0)
- [ ] Add migration helpers for third-party mods
- [ ] Optimize religion data queries if needed
- [ ] Add caching if performance issues detected

### Long-term (v1.8.0 - v2.0.0)
- [ ] v1.8.0: Change obsolete warnings to errors
- [ ] v1.9.0: Final compatibility check before removal
- [ ] v2.0.0: Remove all Phase 1-2 legacy code
- [ ] v2.0.0: Clean up PlayerDataManager entirely

---

## Questions and Answers

### Q: Will existing player data be lost?
**A**: No. Both systems will coexist. When players join religions, their deity will be set correctly. Favor and progression data remain intact.

### Q: Do we need a data migration script?
**A**: No. The migration is code-only. When existing players join religions (which sets `ActiveDeity`), the new system will work automatically.

### Q: What happens to players who already pledged to deities in old system?
**A**: Their old pledge data remains but won't be used by favor/ability systems. When they join a religion, `ActiveDeity` will be set and systems will work correctly.

### Q: Will this affect multiplayer servers?
**A**: No breaking changes. Servers can update without player action. Players in existing religions will work immediately.

### Q: Can we remove Phase 1-2 code immediately?
**A**: No. We maintain backward compatibility until v2.0.0 to avoid breaking third-party mods and give developers time to migrate.

---

## Approval Checklist

Before beginning implementation:

- [ ] Plan reviewed by lead developer
- [ ] Estimated time acceptable (2.5-3 hours)
- [ ] TDD approach approved
- [ ] Test coverage requirements clear (15+ new tests)
- [ ] Documentation scope approved
- [ ] Rollback plan acceptable
- [ ] Timeline for legacy removal agreed (v2.0.0)

---

## Implementation Notes

### Code Style
- Follow existing PantheonWars code conventions
- Use XML doc comments for all public methods
- Mark deprecated APIs with `[Obsolete]` attribute
- Add TODO comments for v2.0.0 removal

### Testing Strategy
- Write tests BEFORE implementation (TDD)
- Aim for 90%+ code coverage on changed files
- Include edge case tests (no religion, switching religions, etc.)
- Test both success and failure paths

### Git Workflow
```bash
# Create feature branch
git checkout -b feature/deity-system-migration

# Commit after each phase
git commit -m "Phase 1: Add test infrastructure for deity migration"
git commit -m "Phase 2: Migrate FavorSystem to Phase 3 religion system"
# ... etc

# Final commit
git commit -m "Complete deity system migration to Phase 3"

# Push and create PR
git push origin feature/deity-system-migration
```

---

## Conclusion

This migration completes the transition from the legacy Phase 1-2 deity system to the Phase 3 religion-based architecture. By following this plan:

1. **Player experience improves** - No more "not pledged to deity" errors
2. **Code quality improves** - Single source of truth for deity data
3. **Future development easier** - Clean architecture for new features
4. **Third-party mods supported** - Clear migration path with docs

**Estimated effort**: 2.5-3 hours with TDD approach

**Risk level**: Low - Backward compatibility maintained, comprehensive testing

**Ready to begin implementation?** Start with Phase 1 (Test Infrastructure Setup).
