# PantheonWars - Comprehensive Test Plan

**Document Version:** 1.0
**Last Updated:** 2025-11-05
**Framework:** xUnit.net v3 with Moq 4.20.72

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current Test Coverage Analysis](#current-test-coverage-analysis)
3. [Testing Strategy](#testing-strategy)
4. [Interface Requirements for Mocking](#interface-requirements-for-mocking)
5. [Detailed Test Plans by Component](#detailed-test-plans-by-component)
6. [Test Priority Matrix](#test-priority-matrix)
7. [Test Infrastructure Requirements](#test-infrastructure-requirements)
8. [Implementation Roadmap](#implementation-roadmap)

---

## Executive Summary

### Current State
- **Total Source Files:** 92
- **Total Test Files:** 20
- **Overall Test Coverage:** ~24% (20 test files / 92 source files)
- **Testing Framework:** xUnit.net v3 with Moq for mocking

### Coverage by Component

| Component | Files | Tests | Coverage | Status |
|-----------|-------|-------|----------|--------|
| Commands/Perk | 5 | 5 | 100% | ✅ Good |
| Data | 4 | 3 | 75% | ✅ Good |
| Network Packets | 17 | 10 | 59% | ⚠️ Partial |
| Models | 14 | 2 | 14% | ❌ Poor |
| Systems | 19 | 1 | 5% | ❌ Poor |
| GUI | 15 | 1 | 7% | ❌ Poor |
| Abilities | 8 | 0 | 0% | ❌ None |
| Constants | 5 | 0 | 0% | ❌ None |
| BuffSystem | 3 | 0 | 0% | ❌ None |

### Critical Gaps
1. **Zero coverage** for Abilities (8 classes)
2. **Zero coverage** for BuffSystem (3 classes)
3. **Minimal coverage** for Systems (1/19 classes)
4. **Minimal coverage** for GUI components (1/15 files)
5. **Missing interfaces** prevent effective unit testing of 8 key system classes

---

## Current Test Coverage Analysis

### Well-Tested Components ✅

#### 1. Commands/Perk (100% Coverage)
**Location:** `PantheonWars.Tests/Commands/Perk/`

**Existing Tests:**
- `PerkCommandsTests.cs` - Constructor validation, command registration
- `PerkCommandListTests.cs` - List command functionality
- `PerkCommandPlayerTests.cs` - Player perk commands
- `PerkCommandReligionTests.cs` - Religion perk commands
- `PerkCommandsTestHelpers.cs` - Shared test utilities

**Quality:** Excellent - comprehensive constructor validation, fluent API testing, null checks

#### 2. Data Models (75% Coverage)
**Location:** `PantheonWars.Tests/Data/`

**Existing Tests:**
- `PlayerAbilityDataTests.cs` - Player ability data serialization/state
- `PlayerReligionDataTests.cs` - Player religion membership data
- `ReligionDataTests.cs` - Religion configuration data

**Missing:**
- `PlayerDeityData` tests (1 file untested)

#### 3. Network Packets (59% Coverage)
**Location:** `PantheonWars.Tests/Network/`

**Tested Packets (10/17):**
- ✅ CreateReligionRequest/Response
- ✅ EditDescriptionRequest/Response
- ✅ PlayerDataPacket
- ✅ PlayerReligionDataPacket
- ✅ ReligionActionRequest/Response
- ✅ ReligionListRequest/Response

**Untested Packets (7/17):**
- ❌ PerkDataRequest/Response
- ❌ PerkUnlockRequest/Response
- ❌ PlayerReligionInfoRequest/Response
- ❌ ReligionStateChangedPacket

### Poorly Tested Components ❌

#### 1. Systems (5% Coverage - 1/19 files)
**Location:** `PantheonWars/Systems/`

**Only Test:** `PerkDialogManagerTests.cs` (basic property/state tests)

**Critical Untested Systems:**
- `AbilitySystem` (201 lines) - Ability execution, validation, cooldowns
- `FavorSystem` (160 lines) - PvP favor rewards, death penalties
- `PerkEffectSystem` (423 lines) - Perk effect application
- `PlayerDataManager` (218 lines) - Player data persistence
- `PlayerReligionDataManager` (336 lines) - Religion data management
- `ReligionManager` (323 lines) - Religion lifecycle management
- `ReligionPrestigeManager` (309 lines) - Prestige progression
- `PvPManager` (213 lines) - PvP system management
- `AbilityCooldownManager` - Cooldown tracking and persistence
- `BuffManager` (220 lines) - Buff/debuff application
- `DeityRegistry` - Deity registration and lookup
- `AbilityRegistry` - Ability registration and lookup
- `PerkRegistry` - Perk registration and lookup

#### 2. Abilities (0% Coverage - 0/8 files)
**Location:** `PantheonWars/Abilities/`

**Untested Abilities:**

**Khoras (War Deity):**
- `BattleCryAbility` - Damage boost ability
- `BladeStormAbility` - AoE damage ability
- `LastStandAbility` - Defensive last stand
- `WarBannerAbility` - Team buff ability

**Lysa (Hunt Deity):**
- `ArrowRainAbility` - Ranged AoE ability
- `HuntersMarkAbility` - Target marking/debuff
- `PredatorInstinctAbility` - Speed/perception boost
- `SwiftFeetAbility` - Movement speed boost

#### 3. GUI Components (7% Coverage - 1/15 files)
**Location:** `PantheonWars/GUI/`

**Untested Dialogs:**
- `CreateReligionDialog` - Religion creation UI
- `DeitySelectionDialog` - Deity selection UI
- `EditDescriptionDialog` - Description editor
- `FavorHudElement` - Favor display HUD
- `InvitePlayerDialog` - Player invitation UI
- `PerkDialog` (429 lines) - Main perk tree UI
- `PerkTreeLayout` - Layout calculations
- `ReligionManagementDialog` (470 lines) - Religion management UI

**Untested Renderers (6 files):**
- `PerkUIRenderer` - Main UI renderer
- `PerkActionsRenderer` - Action button renderer
- `PerkInfoRenderer` (286 lines) - Info display
- `PerkNodeRenderer` - Perk node rendering
- `PerkTreeRenderer` (228 lines) - Tree rendering
- `ReligionHeaderRenderer` - Header rendering

#### 4. BuffSystem (0% Coverage - 0/3 files)
**Location:** `PantheonWars/Systems/BuffSystem/`

**Untested Classes:**
- `BuffManager` (220 lines) - Central buff management, AoE buffs, damage multipliers
- `EntityBehaviorBuffTracker` (288 lines) - Entity buff tracking, stat modification
- `ActiveEffect` - Active effect data model

#### 5. Models (14% Coverage - 2/14 files)
**Location:** `PantheonWars/Models/`

**Tested (2):**
- ✅ `Perk` - Basic perk model tests
- ✅ `Deity` - Basic deity tests

**Untested (12):**
- `Ability` (abstract class) - Base ability model
- `PerkNodeState` - Perk UI state
- `PerkTooltipData` (286 lines) - Tooltip data model
- 9 Enum types (DeityType, FavorRank, etc.)

#### 6. Commands (Partial Coverage)
**Location:** `PantheonWars/Commands/`

**Untested Commands:**
- `ReligionCommands` (482 lines) - 10 subcommands (create, join, leave, list, etc.)
- `FavorCommands` (394 lines) - Favor management
- `DeityCommands` - Deity operations
- `AbilityCommands` - Ability management

#### 7. Constants (0% Coverage)
**Location:** `PantheonWars/Constants/`

**Untested (5 files):**
- `PerkCommandConstants`
- `PerkIds`
- `SpecialEffects`
- `SystemConstants`
- `VintageStoryStats`

**Note:** Constants may not need dedicated tests, but should be validated through integration tests.

---

## Testing Strategy

### Test Levels

#### 1. Unit Tests (Priority: HIGH)
**Target:** Individual classes in isolation
- Systems (Managers, Registries)
- Data models
- Network packets
- Abilities
- BuffSystem

**Approach:**
- Mock all external dependencies (API, Managers, Registries)
- Test public methods and state changes
- Validate error handling and edge cases

#### 2. Integration Tests (Priority: MEDIUM)
**Target:** Component interactions
- Ability execution flow (AbilitySystem → BuffManager → Entity)
- Religion lifecycle (ReligionManager → PlayerReligionDataManager)
- Perk unlocking (PerkCommands → PerkRegistry → PerkEffectSystem)
- Favor calculation (FavorSystem → PlayerDataManager)

**Approach:**
- Use real implementations where possible
- Mock only external systems (ICoreAPI, IServerPlayer)
- Test workflows across multiple components

#### 3. UI Tests (Priority: LOW)
**Target:** GUI components
- Dialog state management
- Renderer output validation
- User interaction flows

**Approach:**
- Focus on state management and logic
- Mock rendering APIs (avoid visual testing)
- Test event handling and data binding

#### 4. End-to-End Tests (Priority: FUTURE)
**Target:** Complete user workflows
- Player joins server → selects deity → earns favor → unlocks perks
- Player creates religion → invites members → unlocks religion perks

**Approach:**
- Requires test server environment
- Full integration with Vintage Story API
- May need custom test harness

### Test Patterns

#### Constructor Validation Pattern
```csharp
[Fact]
public void Constructor_ThrowsWhenDependencyIsNull()
{
    Assert.Throws<ArgumentNullException>(() =>
        new MyClass(null, _dependency2.Object));
}
```

#### State Change Pattern
```csharp
[Fact]
public void Method_UpdatesStateCorrectly()
{
    // Arrange
    var sut = new MyClass();

    // Act
    sut.DoSomething(value);

    // Assert
    Assert.Equal(expectedState, sut.State);
}
```

#### Mock Verification Pattern
```csharp
[Fact]
public void Method_CallsDependencyCorrectly()
{
    // Arrange
    _mockDependency.Setup(x => x.Method(It.IsAny<string>()))
        .Returns(result);

    // Act
    _sut.DoSomething();

    // Assert
    _mockDependency.Verify(x => x.Method("expected"), Times.Once);
}
```

#### Error Handling Pattern
```csharp
[Fact]
public void Method_HandlesExceptionGracefully()
{
    // Arrange
    _mockDependency.Setup(x => x.Method())
        .Throws<Exception>();

    // Act & Assert
    var exception = Record.Exception(() => _sut.DoSomething());
    Assert.Null(exception); // OR Assert.IsType<CustomException>(exception)
}
```

---

## Interface Requirements for Mocking

### Current Interfaces ✅
The following interfaces already exist in `PantheonWars/Systems/Interfaces/`:

1. ✅ `IPlayerDataManager`
2. ✅ `IPlayerReligionDataManager`
3. ✅ `IReligionManager`
4. ✅ `IPvPManager`
5. ✅ `IPerkRegistry`
6. ✅ `IPerkEffectSystem`

### Required New Interfaces ⚠️

To enable comprehensive unit testing, the following classes need interfaces:

#### Priority 1: Critical for System Tests

**1. `IAbilitySystem`**
- **Location:** Create in `Systems/Interfaces/IAbilitySystem.cs`
- **Implementation:** `AbilitySystem`
- **Reason:** Required to test Commands that execute abilities
- **Methods to include:**
  ```csharp
  public interface IAbilitySystem
  {
      void Initialize();
      bool ExecuteAbility(IServerPlayer player, string abilityId);
      Ability? GetAbility(string abilityId);
      IEnumerable<Ability> GetPlayerAbilities(IServerPlayer player);
      float GetAbilityCooldown(IServerPlayer player, string abilityId);
  }
  ```

**2. `IFavorSystem`**
- **Location:** Create in `Systems/Interfaces/IFavorSystem.cs`
- **Implementation:** `FavorSystem`
- **Reason:** Required to test PvP reward calculations
- **Methods to include:**
  ```csharp
  public interface IFavorSystem
  {
      void Initialize();
      void ProcessPvPKill(IServerPlayer attacker, IServerPlayer victim);
      void ProcessDeathPenalty(IServerPlayer player);
      int CalculateFavorReward(DeityType attackerDeity, DeityType victimDeity);
      void AwardFavorForAction(IServerPlayer player, string actionType, int amount);
  }
  ```

**3. `IBuffManager`**
- **Location:** Create in `Systems/Interfaces/IBuffManager.cs`
- **Implementation:** `BuffManager`
- **Reason:** Required to test abilities that apply buffs/debuffs
- **Methods to include:**
  ```csharp
  public interface IBuffManager
  {
      bool ApplyEffect(EntityAgent target, string effectId, float duration,
          string sourceAbilityId, string casterPlayerUID,
          Dictionary<string, float> statModifiers, bool isBuff = true);
      bool ApplySimpleBuff(EntityAgent target, string effectId, float duration,
          string sourceAbilityId, string casterPlayerUID,
          string statName, float statValue);
      bool RemoveEffect(EntityAgent target, string effectId);
      bool HasEffect(EntityAgent target, string effectId);
      List<ActiveEffect> GetActiveEffects(EntityAgent target);
      int ApplyAoEBuff(EntityAgent caster, string effectId, float duration,
          string sourceAbilityId, float radius,
          Dictionary<string, float> statModifiers, bool affectCaster = true);
      float GetOutgoingDamageMultiplier(EntityAgent entity);
      float GetReceivedDamageMultiplier(EntityAgent entity);
  }
  ```

**4. `IAbilityCooldownManager`**
- **Location:** Create in `Systems/Interfaces/IAbilityCooldownManager.cs`
- **Implementation:** `AbilityCooldownManager`
- **Reason:** Required to test cooldown tracking in AbilitySystem
- **Methods to include:**
  ```csharp
  public interface IAbilityCooldownManager
  {
      void Initialize();
      PlayerAbilityData GetOrCreateAbilityData(string playerUID);
      PlayerAbilityData GetOrCreateAbilityData(IServerPlayer player);
      bool IsOnCooldown(string playerUID, string abilityId, float cooldownSeconds);
      float GetRemainingCooldown(string playerUID, string abilityId, float cooldownSeconds);
      void StartCooldown(string playerUID, string abilityId);
      void ClearCooldown(string playerUID, string abilityId);
      void ClearAllCooldowns(string playerUID);
  }
  ```

**5. `IReligionPrestigeManager`**
- **Location:** Create in `Systems/Interfaces/IReligionPrestigeManager.cs`
- **Implementation:** `ReligionPrestigeManager`
- **Reason:** Required to test prestige progression
- **Methods to include:**
  ```csharp
  public interface IReligionPrestigeManager
  {
      void Initialize();
      int CalculatePrestige(ReligionData religion);
      PrestigeRank GetPrestigeRank(int prestige);
      void UpdateReligionPrestige(string religionUID);
      void OnPlayerJoinReligion(string religionUID, string playerUID);
      void OnPlayerLeaveReligion(string religionUID, string playerUID);
      void OnReligionPerkUnlocked(string religionUID, string perkId);
  }
  ```

#### Priority 2: Important for Comprehensive Testing

**6. `IDeityRegistry`**
- **Location:** Create in `Systems/Interfaces/IDeityRegistry.cs`
- **Implementation:** `DeityRegistry`
- **Reason:** Required to test deity-related lookups
- **Methods to include:**
  ```csharp
  public interface IDeityRegistry
  {
      void Initialize();
      void RegisterDeity(Deity deity);
      Deity? GetDeity(DeityType deityType);
      IEnumerable<Deity> GetAllDeities();
      float GetFavorMultiplier(DeityType attackerDeity, DeityType victimDeity);
  }
  ```

**7. `IAbilityRegistry`**
- **Location:** Create in `Systems/Interfaces/IAbilityRegistry.cs`
- **Implementation:** `AbilityRegistry`
- **Reason:** Required to test ability registration and lookup
- **Methods to include:**
  ```csharp
  public interface IAbilityRegistry
  {
      void Initialize();
      void RegisterAbility(Ability ability);
      Ability? GetAbility(string abilityId);
      IEnumerable<Ability> GetAllAbilities();
      IEnumerable<Ability> GetAbilitiesForDeity(DeityType deityType);
  }
  ```

#### Priority 3: Optional for Command Testing

**8. `IReligionCommands`** (Optional)
- **Location:** Create in `Commands/Interfaces/IReligionCommands.cs`
- **Implementation:** `ReligionCommands`
- **Reason:** Only needed if ReligionCommands is injected into other components
- **Note:** Commands are typically tested directly, not mocked

**9. `IFavorCommands`** (Optional)
- Similar to IReligionCommands

**10. `IDeityCommands`** (Optional)
- Similar to IReligionCommands

**11. `IAbilityCommands`** (Optional)
- Similar to IReligionCommands

### Interface Implementation Checklist

| Interface | Priority | Status | Implementation Class | Lines |
|-----------|----------|--------|---------------------|-------|
| IPlayerDataManager | - | ✅ Exists | PlayerDataManager | 218 |
| IPlayerReligionDataManager | - | ✅ Exists | PlayerReligionDataManager | 336 |
| IReligionManager | - | ✅ Exists | ReligionManager | 323 |
| IPvPManager | - | ✅ Exists | PvPManager | 213 |
| IPerkRegistry | - | ✅ Exists | PerkRegistry | - |
| IPerkEffectSystem | - | ✅ Exists | PerkEffectSystem | 423 |
| IAbilitySystem | P1 | ❌ **NEEDED** | AbilitySystem | 201 |
| IFavorSystem | P1 | ❌ **NEEDED** | FavorSystem | 160 |
| IBuffManager | P1 | ❌ **NEEDED** | BuffManager | 220 |
| IAbilityCooldownManager | P1 | ❌ **NEEDED** | AbilityCooldownManager | 183 |
| IReligionPrestigeManager | P1 | ❌ **NEEDED** | ReligionPrestigeManager | 309 |
| IDeityRegistry | P2 | ❌ **NEEDED** | DeityRegistry | - |
| IAbilityRegistry | P2 | ❌ **NEEDED** | AbilityRegistry | - |

**Total Interfaces Needed: 7 (5 Priority 1, 2 Priority 2)**

---

## Detailed Test Plans by Component

### 1. Systems Tests

#### 1.1 AbilitySystem Tests
**File:** `PantheonWars.Tests/Systems/AbilitySystemTests.cs`

**Test Coverage:**

**Constructor Tests:**
- ✅ Constructor_ThrowsWhenSAPIIsNull
- ✅ Constructor_ThrowsWhenAbilityRegistryIsNull
- ✅ Constructor_ThrowsWhenPlayerDataManagerIsNull
- ✅ Constructor_ThrowsWhenCooldownManagerIsNull
- ✅ Constructor_AcceptsNullBuffManager
- ✅ Constructor_SetsDependenciesCorrectly

**ExecuteAbility Tests:**
- ✅ ExecuteAbility_ReturnsFalse_WhenPlayerIsNull
- ✅ ExecuteAbility_ReturnsFalse_WhenPlayerEntityIsNull
- ✅ ExecuteAbility_ReturnsFalse_WhenAbilityNotFound
- ✅ ExecuteAbility_ReturnsFalse_WhenPlayerHasNoDeity
- ✅ ExecuteAbility_ReturnsFalse_WhenAbilityDoesNotMatchDeity
- ✅ ExecuteAbility_ReturnsFalse_WhenDevotionRankTooLow
- ✅ ExecuteAbility_ReturnsFalse_WhenInsufficientFavor
- ✅ ExecuteAbility_ReturnsFalse_WhenOnCooldown
- ✅ ExecuteAbility_ReturnsFalse_WhenCustomValidationFails
- ✅ ExecuteAbility_ReturnsTrue_WhenAllValidationsPassed
- ✅ ExecuteAbility_ConsumesFavor_WhenSuccessful
- ✅ ExecuteAbility_StartsCooldown_WhenSuccessful
- ✅ ExecuteAbility_DoesNotConsumeFavor_WhenExecutionFails
- ✅ ExecuteAbility_HandlesExecutionException_Gracefully

**GetPlayerAbilities Tests:**
- ✅ GetPlayerAbilities_ReturnsEmpty_WhenPlayerHasNoDeity
- ✅ GetPlayerAbilities_ReturnsDeityAbilities_WhenPlayerHasDeity

**GetAbilityCooldown Tests:**
- ✅ GetAbilityCooldown_ReturnsZero_WhenAbilityNotFound
- ✅ GetAbilityCooldown_ReturnsRemaining_WhenOnCooldown
- ✅ GetAbilityCooldown_ReturnsZero_WhenNotOnCooldown

**Mocks Required:**
- Mock<ICoreServerAPI>
- Mock<IAbilityRegistry>
- Mock<IPlayerDataManager>
- Mock<IAbilityCooldownManager>
- Mock<IBuffManager> (optional)
- Mock<IServerPlayer>
- Mock<Ability>

**Estimated Tests:** 20

---

#### 1.2 FavorSystem Tests
**File:** `PantheonWars.Tests/Systems/FavorSystemTests.cs`

**Test Coverage:**

**Constructor Tests:**
- ✅ Constructor_ThrowsWhenSAPIIsNull
- ✅ Constructor_ThrowsWhenPlayerDataManagerIsNull
- ✅ Constructor_ThrowsWhenDeityRegistryIsNull

**ProcessPvPKill Tests:**
- ✅ ProcessPvPKill_DoesNothing_WhenAttackerHasNoDeity
- ✅ ProcessPvPKill_AwardsFavor_WhenAttackerHasDeity
- ✅ ProcessPvPKill_IncrementsKillCount
- ✅ ProcessPvPKill_SendsNotificationToAttacker
- ✅ ProcessPvPKill_SendsNotificationToVictim_WhenVictimHasDeity
- ✅ ProcessPvPKill_DoesNotNotifyVictim_WhenVictimHasNoDeity

**CalculateFavorReward Tests:**
- ✅ CalculateFavorReward_ReturnsBaseFavor_WhenVictimHasNoDeity
- ✅ CalculateFavorReward_ReturnsHalfFavor_WhenSameDeity
- ✅ CalculateFavorReward_AppliesMultiplier_WhenDifferentDeities
- ✅ CalculateFavorReward_UsesCorrectMultiplier_ForRivals
- ✅ CalculateFavorReward_UsesCorrectMultiplier_ForAllies

**ProcessDeathPenalty Tests:**
- ✅ ProcessDeathPenalty_DoesNothing_WhenPlayerHasNoDeity
- ✅ ProcessDeathPenalty_RemovesFavor_WhenPlayerHasDeity
- ✅ ProcessDeathPenalty_DoesNotReduceBelowZero
- ✅ ProcessDeathPenalty_SendsNotificationToPlayer

**AwardFavorForAction Tests:**
- ✅ AwardFavorForAction_DoesNothing_WhenPlayerHasNoDeity
- ✅ AwardFavorForAction_AddsFavor_WhenPlayerHasDeity
- ✅ AwardFavorForAction_SendsNotification

**Mocks Required:**
- Mock<ICoreServerAPI>
- Mock<IPlayerDataManager>
- Mock<IDeityRegistry>
- Mock<IServerPlayer>

**Estimated Tests:** 19

---

#### 1.3 BuffManager Tests
**File:** `PantheonWars.Tests/Systems/BuffSystem/BuffManagerTests.cs`

**Test Coverage:**

**Constructor Tests:**
- ✅ Constructor_ThrowsWhenSAPIIsNull

**ApplyEffect Tests:**
- ✅ ApplyEffect_ReturnsFalse_WhenTargetIsNull
- ✅ ApplyEffect_ReturnsFalse_WhenEffectIdIsEmpty
- ✅ ApplyEffect_ReturnsTrue_WhenSuccessful
- ✅ ApplyEffect_CreatesBuffTracker_WhenNotExists
- ✅ ApplyEffect_UsesExistingBuffTracker_WhenExists
- ✅ ApplyEffect_AddsStatModifiers_Correctly

**ApplySimpleBuff Tests:**
- ✅ ApplySimpleBuff_CallsApplyEffect_WithCorrectParameters
- ✅ ApplySimpleBuff_CreatesCorrectModifierDictionary

**RemoveEffect Tests:**
- ✅ RemoveEffect_ReturnsFalse_WhenTargetIsNull
- ✅ RemoveEffect_ReturnsFalse_WhenEffectIdIsEmpty
- ✅ RemoveEffect_ReturnsFalse_WhenNoBuffTracker
- ✅ RemoveEffect_ReturnsTrue_WhenSuccessful

**HasEffect Tests:**
- ✅ HasEffect_ReturnsFalse_WhenTargetIsNull
- ✅ HasEffect_ReturnsFalse_WhenNoBuffTracker
- ✅ HasEffect_ReturnsTrue_WhenEffectActive

**ApplyAoEBuff Tests:**
- ✅ ApplyAoEBuff_FindsEntitiesInRadius
- ✅ ApplyAoEBuff_SkipsCaster_WhenAffectCasterFalse
- ✅ ApplyAoEBuff_IncludesCaster_WhenAffectCasterTrue
- ✅ ApplyAoEBuff_ReturnsCorrectCount
- ✅ ApplyAoEBuff_NotifiesPlayers

**GetDamageMultiplier Tests:**
- ✅ GetOutgoingDamageMultiplier_ReturnsOne_WhenNoBuffTracker
- ✅ GetOutgoingDamageMultiplier_ReturnsBuff_WhenActive
- ✅ GetReceivedDamageMultiplier_ReturnsOne_WhenNoBuffTracker
- ✅ GetReceivedDamageMultiplier_ReturnsDebuff_WhenActive

**Mocks Required:**
- Mock<ICoreServerAPI>
- Mock<EntityAgent>
- Mock<EntityBehaviorBuffTracker> (challenging - may need integration test)

**Estimated Tests:** 24
**Note:** BuffManager tests may be challenging due to EntityAgent/EntityBehavior mocking. Consider integration tests.

---

#### 1.4 AbilityCooldownManager Tests
**File:** `PantheonWars.Tests/Systems/AbilityCooldownManagerTests.cs`

**Test Coverage:**

**Constructor Tests:**
- ✅ Constructor_ThrowsWhenSAPIIsNull

**GetOrCreateAbilityData Tests:**
- ✅ GetOrCreateAbilityData_CreatesNew_WhenNotExists
- ✅ GetOrCreateAbilityData_ReturnsExisting_WhenExists
- ✅ GetOrCreateAbilityData_ByPlayer_UsesPlayerUID

**IsOnCooldown Tests:**
- ✅ IsOnCooldown_ReturnsFalse_WhenNeverUsed
- ✅ IsOnCooldown_ReturnsTrue_WhenOnCooldown
- ✅ IsOnCooldown_ReturnsFalse_WhenCooldownExpired

**GetRemainingCooldown Tests:**
- ✅ GetRemainingCooldown_ReturnsZero_WhenNotOnCooldown
- ✅ GetRemainingCooldown_ReturnsRemaining_WhenOnCooldown
- ✅ GetRemainingCooldown_DecreasesOverTime

**StartCooldown Tests:**
- ✅ StartCooldown_RecordsTimestamp
- ✅ StartCooldown_CreatesDataIfNeeded

**ClearCooldown Tests:**
- ✅ ClearCooldown_RemovesSpecificAbility
- ✅ ClearCooldown_DoesNotAffectOtherAbilities

**ClearAllCooldowns Tests:**
- ✅ ClearAllCooldowns_RemovesAllAbilities

**Persistence Tests:** (Integration tests)
- ✅ OnPlayerJoin_LoadsPlayerData
- ✅ OnPlayerDisconnect_SavesPlayerData
- ✅ OnGameWorldSave_SavesAllPlayerData

**Mocks Required:**
- Mock<ICoreServerAPI>
- Mock<IServerPlayer>
- Mock<IWorldManagerAPI> (for save/load)

**Estimated Tests:** 17

---

#### 1.5 ReligionPrestigeManager Tests
**File:** `PantheonWars.Tests/Systems/ReligionPrestigeManagerTests.cs`

**Test Coverage:**

**Constructor Tests:**
- ✅ Constructor_ThrowsWhenDependencyIsNull

**CalculatePrestige Tests:**
- ✅ CalculatePrestige_ReturnsZero_ForNewReligion
- ✅ CalculatePrestige_IncludesMemberCount
- ✅ CalculatePrestige_IncludesActiveMemberBonus
- ✅ CalculatePrestige_IncludesPerksUnlocked
- ✅ CalculatePrestige_IncludesTotalFavor
- ✅ CalculatePrestige_CorrectFormula

**GetPrestigeRank Tests:**
- ✅ GetPrestigeRank_ReturnsFledgling_WhenLow
- ✅ GetPrestigeRank_ReturnsEstablished_WhenMedium
- ✅ GetPrestigeRank_ReturnsRenowned_WhenHigh
- ✅ GetPrestigeRank_ReturnsLegendary_WhenVeryHigh
- ✅ GetPrestigeRank_ReturnsMythic_WhenMaximum

**UpdateReligionPrestige Tests:**
- ✅ UpdateReligionPrestige_CalculatesAndStores
- ✅ UpdateReligionPrestige_UpdatesRank
- ✅ UpdateReligionPrestige_DoesNothing_WhenReligionNotFound

**Event Handler Tests:**
- ✅ OnPlayerJoinReligion_UpdatesPrestige
- ✅ OnPlayerLeaveReligion_UpdatesPrestige
- ✅ OnReligionPerkUnlocked_UpdatesPrestige

**Mocks Required:**
- Mock<IReligionManager>
- Mock<IPlayerReligionDataManager>

**Estimated Tests:** 18

---

#### 1.6 DeityRegistry Tests
**File:** `PantheonWars.Tests/Systems/DeityRegistryTests.cs`

**Test Coverage:**

**RegisterDeity Tests:**
- ✅ RegisterDeity_AddsDeityToRegistry
- ✅ RegisterDeity_OverwritesExisting_WhenSameType
- ✅ RegisterDeity_ThrowsWhen_DeityIsNull

**GetDeity Tests:**
- ✅ GetDeity_ReturnsNull_WhenNotRegistered
- ✅ GetDeity_ReturnsDeity_WhenRegistered

**GetAllDeities Tests:**
- ✅ GetAllDeities_ReturnsEmpty_WhenNoneRegistered
- ✅ GetAllDeities_ReturnsAll_WhenMultipleRegistered

**GetFavorMultiplier Tests:**
- ✅ GetFavorMultiplier_ReturnsOne_ForNeutral
- ✅ GetFavorMultiplier_ReturnsBonus_ForRival
- ✅ GetFavorMultiplier_ReturnsPenalty_ForAlly
- ✅ GetFavorMultiplier_HandlesMissingDeity

**Estimated Tests:** 11

---

#### 1.7 AbilityRegistry Tests
**File:** `PantheonWars.Tests/Systems/AbilityRegistryTests.cs`

**Test Coverage:**

**RegisterAbility Tests:**
- ✅ RegisterAbility_AddsAbilityToRegistry
- ✅ RegisterAbility_OverwritesExisting_WhenSameId
- ✅ RegisterAbility_ThrowsWhen_AbilityIsNull

**GetAbility Tests:**
- ✅ GetAbility_ReturnsNull_WhenNotRegistered
- ✅ GetAbility_ReturnsAbility_WhenRegistered

**GetAllAbilities Tests:**
- ✅ GetAllAbilities_ReturnsEmpty_WhenNoneRegistered
- ✅ GetAllAbilities_ReturnsAll_WhenMultipleRegistered

**GetAbilitiesForDeity Tests:**
- ✅ GetAbilitiesForDeity_ReturnsEmpty_WhenNoneMatch
- ✅ GetAbilitiesForDeity_ReturnsMatching_WhenExists
- ✅ GetAbilitiesForDeity_FiltersCorrectly

**Estimated Tests:** 10

---

#### 1.8 PerkRegistry Tests
**File:** `PantheonWars.Tests/Systems/PerkRegistryTests.cs`

**Status:** ✅ Should already be tested (has IPerkRegistry interface)

**Verify Existing Tests or Create:**
- Similar to AbilityRegistry tests

---

#### 1.9 PerkEffectSystem Tests
**File:** `PantheonWars.Tests/Systems/PerkEffectSystemTests.cs`

**Test Coverage:**

**ApplyPerkEffects Tests:**
- ✅ ApplyPerkEffects_AppliesStatModifiers
- ✅ ApplyPerkEffects_HandlesMissingPerk
- ✅ ApplyPerkEffects_AppliesMultiplePerks
- ✅ ApplyPerkEffects_DoesNotApplyInactivePerks

**RemovePerkEffects Tests:**
- ✅ RemovePerkEffects_RemovesStatModifiers
- ✅ RemovePerkEffects_DoesNotAffectOtherPerks

**GetPerkBonus Tests:**
- ✅ GetPerkBonus_ReturnsZero_WhenPerkNotActive
- ✅ GetPerkBonus_ReturnsValue_WhenPerkActive

**Mocks Required:**
- Mock<IPerkRegistry>
- Mock<IServerPlayer>

**Estimated Tests:** 8

---

### 2. Abilities Tests

All ability classes inherit from abstract `Ability` base class and implement:
- `CanExecute(IServerPlayer player, ICoreServerAPI sapi, out string failureReason)`
- `Execute(IServerPlayer player, ICoreServerAPI sapi, BuffManager buffManager)`

#### 2.1 BattleCryAbility Tests
**File:** `PantheonWars.Tests/Abilities/Khoras/BattleCryAbilityTests.cs`

**Test Coverage:**

**Properties Tests:**
- ✅ Ability_HasCorrectProperties (Name, Description, Deity, FavorCost, Cooldown, MinRank)

**CanExecute Tests:**
- ✅ CanExecute_ReturnsTrue_WhenValid
- ✅ CanExecute_ReturnsFalse_WhenPlayerIsNull
- ✅ CanExecute_ReturnsFalse_WhenEntityIsNull
- ✅ CanExecute_ReturnsFalse_WhenCustomConditionNotMet

**Execute Tests:**
- ✅ Execute_AppliesBuff_ToPlayer
- ✅ Execute_SendsSuccessMessage
- ✅ Execute_ReturnsFalse_WhenBuffManagerIsNull
- ✅ Execute_ReturnsFalse_WhenExecutionFails

**Mocks Required:**
- Mock<IServerPlayer>
- Mock<ICoreServerAPI>
- Mock<IBuffManager>
- Mock<EntityPlayer>

**Estimated Tests:** 9

**Pattern:** Repeat for all 8 abilities (72 total tests)

#### Ability Test Summary

| Ability | Deity | Tests | Special Considerations |
|---------|-------|-------|----------------------|
| BattleCryAbility | Khoras | 9 | Damage buff application |
| BladeStormAbility | Khoras | 9 | AoE damage to multiple targets |
| LastStandAbility | Khoras | 9 | Health threshold check |
| WarBannerAbility | Khoras | 9 | AoE buff to nearby allies |
| ArrowRainAbility | Lysa | 9 | Ranged AoE damage |
| HuntersMarkAbility | Lysa | 9 | Target debuff application |
| PredatorInstinctAbility | Lysa | 9 | Multiple stat buffs |
| SwiftFeetAbility | Lysa | 9 | Movement speed buff |

**Total Ability Tests:** 72

---

### 3. Commands Tests

#### 3.1 ReligionCommands Tests
**File:** `PantheonWars.Tests/Commands/ReligionCommandsTests.cs`

**Test Coverage:**

**Constructor Tests:**
- ✅ Constructor_ThrowsWhenSAPIIsNull
- ✅ Constructor_ThrowsWhenReligionManagerIsNull
- ✅ Constructor_ThrowsWhenPlayerReligionDataManagerIsNull

**RegisterCommands Tests:**
- ✅ RegisterCommands_ExecutesWithoutException
- ✅ RegisterCommands_ConfiguresSubCommandsCorrectly
- ✅ RegisterCommands_RegistersAllSubcommands (10 subcommands)

**OnCreateReligion Tests:**
- ✅ OnCreateReligion_CreatesReligion_WhenValid
- ✅ OnCreateReligion_SendsErrorMessage_WhenInvalidDeity
- ✅ OnCreateReligion_SendsErrorMessage_WhenAlreadyInReligion
- ✅ OnCreateReligion_SendsErrorMessage_WhenNameTaken

**OnJoinReligion Tests:**
- ✅ OnJoinReligion_JoinsReligion_WhenValid
- ✅ OnJoinReligion_SendsErrorMessage_WhenAlreadyInReligion
- ✅ OnJoinReligion_SendsErrorMessage_WhenReligionNotFound

**OnLeaveReligion Tests:**
- ✅ OnLeaveReligion_LeavesReligion_WhenValid
- ✅ OnLeaveReligion_SendsErrorMessage_WhenNotInReligion
- ✅ OnLeaveReligion_SendsErrorMessage_WhenFounder

**Additional Tests for:**
- OnListReligions (3 tests)
- OnReligionInfo (3 tests)
- OnListMembers (3 tests)
- OnInvitePlayer (4 tests)
- OnKickPlayer (4 tests)
- OnDisbandReligion (3 tests)
- OnSetDescription (3 tests)

**Mocks Required:**
- Mock<ICoreServerAPI>
- Mock<IReligionManager>
- Mock<IPlayerReligionDataManager>
- Mock<IServerNetworkChannel>

**Estimated Tests:** 40

---

#### 3.2 FavorCommands Tests
**File:** `PantheonWars.Tests/Commands/FavorCommandsTests.cs`

**Test Coverage:** Similar pattern to ReligionCommands

**Estimated Tests:** 25

---

#### 3.3 DeityCommands Tests
**File:** `PantheonWars.Tests/Commands/DeityCommandsTests.cs`

**Test Coverage:** Similar pattern to ReligionCommands

**Estimated Tests:** 20

---

#### 3.4 AbilityCommands Tests
**File:** `PantheonWars.Tests/Commands/AbilityCommandsTests.cs`

**Test Coverage:** Similar pattern to ReligionCommands

**Estimated Tests:** 20

---

### 4. Network Packet Tests

#### Missing Packet Tests (7 packets, ~14 tests each)

**4.1 PerkDataRequest/Response Tests**
- Serialization/deserialization
- Property validation
- Null handling

**4.2 PerkUnlockRequest/Response Tests**
- Similar pattern

**4.3 PlayerReligionInfoRequest/Response Tests**
- Similar pattern

**4.4 ReligionStateChangedPacket Tests**
- Similar pattern

**Estimated Total:** 49 tests

---

### 5. Data Model Tests

#### 5.1 PlayerDeityData Tests
**File:** `PantheonWars.Tests/Data/PlayerDeityDataTests.cs`

**Test Coverage:**
- Constructor tests
- Property initialization
- HasDeity validation
- Favor rank calculation
- Serialization/deserialization

**Estimated Tests:** 10

---

#### 5.2 Ability (Abstract) Tests
**File:** `PantheonWars.Tests/Models/AbilityTests.cs`

**Test Coverage:**
- Test with concrete implementation
- Property validation
- Abstract method contracts

**Estimated Tests:** 5

---

#### 5.3 PerkNodeState Tests
**File:** `PantheonWars.Tests/Models/PerkNodeStateTests.cs`

**Estimated Tests:** 5

---

#### 5.4 PerkTooltipData Tests
**File:** `PantheonWars.Tests/Models/PerkTooltipDataTests.cs`

**Estimated Tests:** 8

---

### 6. GUI Tests (Lower Priority)

**Strategy:** Focus on state management and logic, not rendering

#### 6.1 PerkDialog Tests
**File:** `PantheonWars.Tests/GUI/PerkDialogTests.cs`

**Test Coverage:**
- Dialog initialization
- State updates
- Event handling
- Data binding

**Estimated Tests:** 15

---

#### 6.2 ReligionManagementDialog Tests
**File:** `PantheonWars.Tests/GUI/ReligionManagementDialogTests.cs`

**Estimated Tests:** 15

---

#### 6.3 PerkTreeLayout Tests
**File:** `PantheonWars.Tests/GUI/PerkTreeLayoutTests.cs`

**Test Coverage:**
- Layout calculations
- Node positioning
- Dependency tree building

**Estimated Tests:** 10

---

**Other GUI Tests:** 5-10 tests each for remaining dialogs/renderers

**Total GUI Tests (Estimated):** 80-100

---

### 7. BuffSystem Tests

#### 7.1 EntityBehaviorBuffTracker Tests
**File:** `PantheonWars.Tests/Systems/BuffSystem/EntityBehaviorBuffTrackerTests.cs`

**Test Coverage:**
- AddEffect/RemoveEffect
- Effect expiration
- Stat modifier application
- Damage multiplier calculation
- Save/load state

**Estimated Tests:** 15

**Challenge:** Requires mocking EntityAgent/EntityBehavior - may need integration tests

---

#### 7.2 ActiveEffect Tests
**File:** `PantheonWars.Tests/Systems/BuffSystem/ActiveEffectTests.cs`

**Test Coverage:**
- Constructor validation
- Property initialization
- AddStatModifier
- IsExpired calculation
- Serialization

**Estimated Tests:** 8

---

### 8. Constants Tests

**Strategy:** Validate through integration tests, not dedicated unit tests

**Alternative:** Add validation tests to ensure no duplicates, correct formatting

**Estimated Tests:** 0-5 per file (Optional)

---

## Test Priority Matrix

### Priority 1: Critical Path (Must Have)
**Target:** Core game mechanics that affect gameplay

| Component | Tests | Effort | Impact | Risk if Untested |
|-----------|-------|--------|--------|------------------|
| AbilitySystem | 20 | Medium | High | Ability exploits, game crashes |
| FavorSystem | 19 | Medium | High | Broken progression, unfair rewards |
| BuffManager | 24 | High | High | Incorrect buffs, game imbalance |
| All Abilities (8) | 72 | High | High | Non-functional abilities |
| AbilityCooldownManager | 17 | Low | Medium | Cooldown exploits |
| **TOTAL P1** | **152** | **High** | **Critical** | **Game-breaking** |

### Priority 2: Important Systems (Should Have)
**Target:** Supporting systems that enable core features

| Component | Tests | Effort | Impact | Risk if Untested |
|-----------|-------|--------|--------|------------------|
| ReligionPrestigeManager | 18 | Medium | Medium | Incorrect prestige calculations |
| DeityRegistry | 11 | Low | Low | Missing deity data |
| AbilityRegistry | 10 | Low | Low | Missing ability data |
| ReligionCommands | 40 | High | Medium | Broken religion management |
| FavorCommands | 25 | Medium | Medium | Favor admin issues |
| DeityCommands | 20 | Medium | Low | Deity selection issues |
| AbilityCommands | 20 | Medium | Medium | Ability admin issues |
| Missing Network Packets | 49 | Medium | Medium | Network sync issues |
| **TOTAL P2** | **193** | **High** | **Important** | **Feature gaps** |

### Priority 3: Polish & Coverage (Nice to Have)
**Target:** Additional coverage for completeness

| Component | Tests | Effort | Impact | Risk if Untested |
|-----------|-------|--------|--------|------------------|
| PlayerDeityData | 10 | Low | Low | Minor data issues |
| BuffSystem (Tracker, ActiveEffect) | 23 | High | Medium | Buff edge cases |
| PerkEffectSystem | 8 | Low | Medium | Perk effect bugs |
| Additional Model Tests | 18 | Low | Low | Model edge cases |
| GUI Tests | 80-100 | Very High | Low | UI bugs (not gameplay) |
| **TOTAL P3** | **139-159** | **Very High** | **Low** | **Minor issues** |

### Recommended Implementation Order

**Phase 1: Critical Foundation (2-3 weeks)**
1. Create 7 required interfaces (IAbilitySystem, IFavorSystem, IBuffManager, IAbilityCooldownManager, IReligionPrestigeManager, IDeityRegistry, IAbilityRegistry)
2. AbilitySystem tests (20 tests)
3. FavorSystem tests (19 tests)
4. AbilityCooldownManager tests (17 tests)
5. All 8 Ability tests (72 tests)

**Phase 2: System Completion (2-3 weeks)**
6. BuffManager tests (24 tests) - may be integration tests
7. ReligionPrestigeManager tests (18 tests)
8. Registry tests (21 tests)
9. Missing network packet tests (49 tests)

**Phase 3: Command Coverage (1-2 weeks)**
10. ReligionCommands tests (40 tests)
11. FavorCommands tests (25 tests)
12. DeityCommands tests (20 tests)
13. AbilityCommands tests (20 tests)

**Phase 4: Polish (1-2 weeks)**
14. BuffSystem tests (23 tests)
15. Additional data model tests (28 tests)
16. PerkEffectSystem tests (8 tests)

**Phase 5: GUI (Optional, 2-3 weeks)**
17. GUI state management tests (80-100 tests)

**Total Estimated Effort:**
- Phase 1-4: 8-12 weeks
- Phase 5 (Optional): +2-3 weeks
- **Total:** 10-15 weeks for comprehensive coverage

---

## Test Infrastructure Requirements

### 1. Mock Helpers

**Create:** `PantheonWars.Tests/Helpers/MockHelpers.cs`

Centralized helper for creating common mocks:
```csharp
public static class MockHelpers
{
    public static Mock<IServerPlayer> CreateMockPlayer(
        string playerUID = "test-player",
        string playerName = "TestPlayer")
    {
        var mock = new Mock<IServerPlayer>();
        mock.Setup(p => p.PlayerUID).Returns(playerUID);
        mock.Setup(p => p.PlayerName).Returns(playerName);
        // ... common setup
        return mock;
    }

    public static Mock<ICoreServerAPI> CreateMockSAPI()
    {
        var mock = new Mock<ICoreServerAPI>();
        var logger = new Mock<ILogger>();
        mock.Setup(s => s.Logger).Returns(logger.Object);
        // ... common setup
        return mock;
    }

    // Similar for other common mocks
}
```

### 2. Test Data Builders

**Create:** `PantheonWars.Tests/Builders/`

Fluent builders for test data:
```csharp
public class PlayerDeityDataBuilder
{
    private string _playerUID = "test-uid";
    private DeityType _deityType = DeityType.Khoras;
    private int _favor = 100;

    public PlayerDeityDataBuilder WithPlayerUID(string uid)
    {
        _playerUID = uid;
        return this;
    }

    public PlayerDeityDataBuilder WithDeity(DeityType deity)
    {
        _deityType = deity;
        return this;
    }

    public PlayerDeityData Build()
    {
        return new PlayerDeityData(_playerUID)
        {
            DeityType = _deityType,
            DivineFavor = _favor
        };
    }
}
```

### 3. Test Fixtures

**Create:** `PantheonWars.Tests/Fixtures/`

Shared fixtures for complex setups:
```csharp
public class AbilitySystemFixture : IDisposable
{
    public Mock<ICoreServerAPI> MockSAPI { get; }
    public Mock<IAbilityRegistry> MockAbilityRegistry { get; }
    public Mock<IPlayerDataManager> MockPlayerDataManager { get; }
    public Mock<IAbilityCooldownManager> MockCooldownManager { get; }
    public AbilitySystem SystemUnderTest { get; }

    public AbilitySystemFixture()
    {
        // Initialize all mocks and SUT
    }

    public void Dispose()
    {
        // Cleanup
    }
}
```

### 4. Custom Assertions

**Create:** `PantheonWars.Tests/Assertions/`

Domain-specific assertions:
```csharp
public static class PerkAssertions
{
    public static void ShouldBeUnlocked(this Perk perk)
    {
        Assert.True(perk.IsUnlocked, $"Perk {perk.PerkId} should be unlocked");
    }

    public static void ShouldHavePrerequisite(this Perk perk, string prereqId)
    {
        Assert.Contains(prereqId, perk.Prerequisites);
    }
}
```

### 5. Integration Test Base

**Create:** `PantheonWars.Tests/Integration/IntegrationTestBase.cs`

Base class for integration tests that require more complex setup:
```csharp
public abstract class IntegrationTestBase : IDisposable
{
    protected Mock<ICoreServerAPI> SAPI;
    protected Dictionary<Type, object> Services;

    protected IntegrationTestBase()
    {
        // Setup common integration test infrastructure
    }

    protected T GetService<T>() where T : class
    {
        return (T)Services[typeof(T)];
    }

    public virtual void Dispose()
    {
        // Cleanup
    }
}
```

### 6. Test Categories

Add test categories for selective test execution:
```csharp
public static class TestCategories
{
    public const string Unit = "Unit";
    public const string Integration = "Integration";
    public const string Systems = "Systems";
    public const string Commands = "Commands";
    public const string Abilities = "Abilities";
    public const string Network = "Network";
    public const string GUI = "GUI";
    public const string Slow = "Slow";
}

// Usage:
[Fact, Trait("Category", TestCategories.Unit)]
public void MyUnitTest() { }
```

### 7. Code Coverage Configuration

**Update:** `PantheonWars.Tests/PantheonWars.Tests.csproj`

Add coverage configuration:
```xml
<PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>cobertura,json</CoverletOutputFormat>
    <Threshold>80</Threshold>
    <ThresholdType>line,branch</ThresholdType>
    <ExcludeByFile>**/Constants/**/*.cs,**/Enum/**/*.cs</ExcludeByFile>
</PropertyGroup>

<ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="coverlet.msbuild" Version="6.0.0" />
</ItemGroup>
```

### 8. Continuous Integration

**Create:** `.github/workflows/tests.yml` (if using GitHub)

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      - name: Upload coverage
        uses: codecov/codecov-action@v3
```

---

## Implementation Roadmap

### Week 1-2: Foundation
- [ ] Create 7 required interfaces
- [ ] Update existing classes to implement interfaces
- [ ] Create test infrastructure (MockHelpers, Builders, Fixtures)
- [ ] Setup code coverage tooling

### Week 3-4: Critical Systems Tests
- [ ] AbilitySystem tests (20)
- [ ] FavorSystem tests (19)
- [ ] AbilityCooldownManager tests (17)

### Week 5-7: Ability Tests
- [ ] BattleCryAbility tests (9)
- [ ] BladeStormAbility tests (9)
- [ ] LastStandAbility tests (9)
- [ ] WarBannerAbility tests (9)
- [ ] ArrowRainAbility tests (9)
- [ ] HuntersMarkAbility tests (9)
- [ ] PredatorInstinctAbility tests (9)
- [ ] SwiftFeetAbility tests (9)

### Week 8-9: Supporting Systems
- [ ] BuffManager tests (24)
- [ ] ReligionPrestigeManager tests (18)
- [ ] DeityRegistry tests (11)
- [ ] AbilityRegistry tests (10)
- [ ] PerkRegistry tests (verification)

### Week 10-11: Network & Commands
- [ ] Missing network packet tests (49)
- [ ] ReligionCommands tests (40)

### Week 12: Additional Commands
- [ ] FavorCommands tests (25)
- [ ] DeityCommands tests (20)
- [ ] AbilityCommands tests (20)

### Week 13-14: Polish & Data Models
- [ ] BuffSystem tests (23)
- [ ] PerkEffectSystem tests (8)
- [ ] PlayerDeityData tests (10)
- [ ] Additional model tests (18)

### Week 15+ (Optional): GUI Tests
- [ ] PerkDialog tests (15)
- [ ] ReligionManagementDialog tests (15)
- [ ] PerkTreeLayout tests (10)
- [ ] Other GUI tests (40-60)

---

## Success Metrics

### Coverage Targets

| Phase | Target Coverage | Test Count |
|-------|----------------|------------|
| Phase 1 (Critical) | 60% overall | ~150 tests |
| Phase 2 (Systems) | 70% overall | ~280 tests |
| Phase 3 (Commands) | 75% overall | ~385 tests |
| Phase 4 (Polish) | 80% overall | ~470 tests |
| Phase 5 (GUI) | 85% overall | ~550-570 tests |

### Quality Metrics

- **Code Coverage:** Minimum 80% line coverage for Systems, Commands, Abilities
- **Branch Coverage:** Minimum 70% for conditional logic
- **Test Execution Time:** < 30 seconds for all tests (excluding slow integration tests)
- **Test Reliability:** 0% flaky tests (no random failures)
- **Maintainability:** All tests follow established patterns and conventions

### Continuous Monitoring

- Weekly coverage reports
- Test execution metrics (pass rate, duration)
- New code must include tests (80% coverage minimum)
- Pull requests require passing tests

---

## Appendices

### Appendix A: Test Naming Conventions

**Pattern:** `MethodName_StateUnderTest_ExpectedBehavior`

**Examples:**
- `ExecuteAbility_WhenPlayerHasNoDeity_ReturnsFalse`
- `ApplyBuff_WithValidTarget_ReturnsTrue`
- `CalculateFavorReward_ForRivalDeity_AppliesMultiplier`

### Appendix B: Mock Setup Patterns

**Standard Mock Setup:**
```csharp
private Mock<IServiceInterface> _mockService;

public void Setup()
{
    _mockService = new Mock<IServiceInterface>();
    _mockService.Setup(s => s.Method(It.IsAny<Type>()))
        .Returns(expectedValue);
}
```

**Fluent API Mock:**
```csharp
mockCommandBuilder.Setup(b => b.WithDescription(It.IsAny<string>()))
    .Returns(mockCommandBuilder.Object);
```

### Appendix C: Common Testing Pitfalls

1. **Over-mocking:** Don't mock everything - use real objects for simple dependencies
2. **Testing Implementation:** Test behavior, not implementation details
3. **Brittle Tests:** Don't hardcode magic numbers - use constants
4. **Missing Cleanup:** Always implement IDisposable for tests with resources
5. **Slow Tests:** Keep tests fast - mock external dependencies

### Appendix D: Resources

- xUnit Documentation: https://xunit.net/
- Moq Documentation: https://github.com/moq/moq4
- .NET Testing Best Practices: https://learn.microsoft.com/en-us/dotnet/core/testing/
- Test-Driven Development: https://martinfowler.com/bliki/TestDrivenDevelopment.html

---

## Document Maintenance

This test plan should be updated:
- When new components are added to the codebase
- After each testing phase is completed
- When test infrastructure changes
- When coverage targets are adjusted
- Quarterly review for relevance

**Last Review:** 2025-11-05
**Next Review:** 2026-02-05
**Owner:** Development Team

---

**End of Test Plan**
