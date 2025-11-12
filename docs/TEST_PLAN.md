# PantheonWars Comprehensive Test Plan

## Executive Summary

This document outlines a comprehensive testing strategy for the PantheonWars mod, focusing on:
- **Interface Extraction** for improved testability and dependency injection
- **Unit Testing** with mocking best practices using xUnit and Moq
- **Test Coverage** for all 120 classes in the codebase
- **Testing Infrastructure** improvements and patterns

**Current State:**
- 27 existing test files
- xUnit v3.1.0 + Moq v4.20.72 already configured
- 6 classes have interfaces, 10+ core classes need interface extraction

**Target State:**
- All core business logic classes tested with >80% coverage
- All classes have appropriate interfaces for dependency injection
- Comprehensive mocking strategy for external dependencies
- Automated test suite integrated into build process

---

**📊 Code Coverage:** See [CODE_COVERAGE.md](CODE_COVERAGE.md) for instructions on generating coverage reports.

## Table of Contents

1. [Interface Extraction Plan](#1-interface-extraction-plan)
2. [Testing Strategy](#2-testing-strategy)
3. [Mocking Best Practices](#3-mocking-best-practices)
4. [Test Plan by Component](#4-test-plan-by-component)
5. [Implementation Roadmap](#5-implementation-roadmap)
6. [Test Examples](#6-test-examples)

---

## 1. Interface Extraction Plan

### 1.1 Priority Classes Needing Interfaces

#### High Priority (Core Systems - 8 classes)

| Class | Location | Reason | Proposed Interface |
|-------|----------|--------|-------------------|
| `DeityRegistry` | Systems/DeityRegistry.cs | Central registry, needed by multiple systems | `IDeityRegistry` |
| `FavorSystem` | Systems/FavorSystem.cs | Complex business logic, event handling | `IFavorSystem` |
| `ReligionPrestigeManager` | Systems/ReligionPrestigeManager.cs | Complex progression logic | `IReligionPrestigeManager` |
| `BuffManager` | Systems/BuffSystem/BuffManager.cs | Buff/debuff management, used by abilities | `IBuffManager` |
| `RankRequirements` | Systems/RankRequirements.cs | Static utility class, needs instance methods | `IRankRequirements` |
| `BlessingDialogManager` | GUI/BlessingDialogManager.cs | UI state management, testable independently | `IBlessingDialogManager` |
| `OverlayCoordinator` | GUI/OverlayCoordinator.cs | UI coordination logic | `IOverlayCoordinator` |
| `BlessingDefinitions` | Systems/BlessingDefinitions.cs | Blessing factory/builder | `IBlessingDefinitions` |

#### Medium Priority (Legacy Systems - 3 classes)

| Class | Location | Reason | Proposed Interface |
|-------|----------|--------|-------------------|
| `AbilityRegistry` | Systems/AbilityRegistry.cs | Legacy, but still in use | `IAbilityRegistry` |
| `AbilitySystem` | Systems/AbilitySystem.cs | Legacy ability activation | `IAbilitySystem` |
| `AbilityCooldownManager` | Systems/AbilityCooldownManager.cs | Legacy cooldown tracking | `IAbilityCooldownManager` |

#### Low Priority (Commands - 5 classes)

| Class | Location | Reason | Proposed Interface |
|-------|----------|--------|-------------------|
| `BlessingCommands` | Commands/BlessingCommands.cs | Command pattern, testable logic | `IBlessingCommands` |
| `ReligionCommands` | Commands/ReligionCommands.cs | Command pattern, testable logic | `IReligionCommands` |
| `DeityCommands` | Commands/DeityCommands.cs | Legacy commands | `IDeityCommands` |
| `FavorCommands` | Commands/FavorCommands.cs | Command pattern | `IFavorCommands` |
| `AbilityCommands` | Commands/AbilityCommands.cs | Legacy commands | `IAbilityCommands` |

### 1.2 Interface Extraction Guidelines

**Naming Convention:**
```csharp
public interface I{ClassName}
{
    // Public methods only
}
```

**Location:**
- All interfaces should be in `Systems/Interfaces/` directory
- Follow existing pattern: `IBlessingRegistry`, `IPlayerDataManager`, etc.

**Principles:**
1. **Extract only public methods** that are used by other components
2. **Keep internal helper methods private** and not in the interface
3. **Use dependency injection** via constructor parameters
4. **Maintain backward compatibility** during extraction

---

## 2. Testing Strategy

### 2.1 Testing Pyramid

```
         /\
        /UI\         (10% - Integration Tests)
       /----\
      /Logic\        (60% - Unit Tests)
     /------\
    /  Data  \       (30% - Data/Model Tests)
   /----------\
```

### 2.2 Test Categories

#### Category 1: Unit Tests (Core Business Logic)
- **Target:** Systems, Managers, Registries
- **Coverage Goal:** 80%+
- **Approach:** Isolated unit tests with mocked dependencies

#### Category 2: Data Tests (Models & Serialization)
- **Target:** Data models, Network packets
- **Coverage Goal:** 90%+
- **Approach:** Property validation, serialization/deserialization

#### Category 3: Integration Tests (System Interaction)
- **Target:** Multi-system workflows
- **Coverage Goal:** 60%+
- **Approach:** Test system interactions with minimal mocking

#### Category 4: Command Tests (Command Handlers)
- **Target:** Command classes
- **Coverage Goal:** 70%+
- **Approach:** Test command logic with mocked systems

### 2.3 Test Organization

```
PantheonWars.Tests/
├── Systems/              # Core system tests
├── Models/               # Model/domain tests
├── Data/                 # Data persistence tests
├── Network/              # Network packet tests
├── Commands/             # Command handler tests
├── GUI/                  # UI logic tests (new)
├── BuffSystem/           # Buff system tests (new)
├── Abilities/            # Ability tests (new)
├── Integration/          # Integration tests (new)
└── Helpers/              # Test utilities and fixtures
```

---

## 3. Mocking Best Practices

### 3.1 When to Mock

**Always Mock:**
- External game APIs (`ICoreAPI`, `ICoreServerAPI`, `ICoreClientAPI`)
- Network communication
- File system operations
- Database/persistence layers
- Other system dependencies (via interfaces)

**Never Mock:**
- Data transfer objects (DTOs)
- Value objects
- Simple models
- The class under test

### 3.2 Moq Patterns

#### Pattern 1: Simple Method Mock
```csharp
var mockRegistry = new Mock<IDeityRegistry>();
mockRegistry
    .Setup(r => r.GetDeity(DeityType.Khoras))
    .Returns(new Deity(DeityType.Khoras, "Khoras", "War"));
```

#### Pattern 2: Verify Interactions
```csharp
mockRegistry.Verify(
    r => r.RegisterDeity(It.IsAny<Deity>()),
    Times.Once()
);
```

#### Pattern 3: Setup Property
```csharp
mockRegistry
    .SetupGet(r => r.DeityCount)
    .Returns(2);
```

#### Pattern 4: Callback for Side Effects
```csharp
mockDataManager
    .Setup(m => m.AddFavor(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
    .Callback<string, int, string>((uid, amount, reason) => {
        // Track side effects for verification
        favorAwarded += amount;
    });
```

### 3.3 Test Fixtures for Common Mocks

Create reusable test fixtures in `PantheonWars.Tests/Helpers/`:

```csharp
public class TestFixtures
{
    public static Mock<ICoreServerAPI> CreateMockServerAPI();
    public static Mock<IDeityRegistry> CreateMockDeityRegistry();
    public static PlayerReligionData CreateTestPlayerData();
    public static ReligionData CreateTestReligion();
}
```

---

## 4. Test Plan by Component

### 4.1 Core Systems (19 classes)

#### 4.1.1 DeityRegistry (NEW TESTS)

**Interface:** `IDeityRegistry`

**Test Class:** `DeityRegistryTests.cs`

**Test Cases:**
1. ✅ **Initialization Tests**
   - `Initialize_RegistersKhorasAndLysa_Successfully()`
   - `Initialize_LogsCorrectDeityCount()`

2. ✅ **Registration Tests**
   - `RegisterDeity_WithValidDeity_AddsToRegistry()`
   - `RegisterDeity_WithDuplicateType_LogsWarningAndSkips()`

3. ✅ **Retrieval Tests**
   - `GetDeity_WithValidType_ReturnsDeity()`
   - `GetDeity_WithInvalidType_ReturnsNull()`
   - `GetAllDeities_ReturnsAllRegisteredDeities()`
   - `HasDeity_WithRegisteredType_ReturnsTrue()`
   - `HasDeity_WithUnregisteredType_ReturnsFalse()`

4. ✅ **Relationship Tests**
   - `GetRelationship_BetweenAlliedDeities_ReturnsAllied()`
   - `GetRelationship_BetweenRivalDeities_ReturnsRival()`
   - `GetRelationship_WithSameDeity_ReturnsNeutral()`
   - `GetFavorMultiplier_ForAlliedDeities_Returns0Point5()`
   - `GetFavorMultiplier_ForRivalDeities_Returns2Point0()`

**Mocking Strategy:**
- Mock `ICoreAPI` for logging
- Use actual `Deity` objects (value objects)

---

#### 4.1.2 FavorSystem (NEW TESTS)

**Interface:** `IFavorSystem`

**Test Class:** `FavorSystemTests.cs` (UPDATE EXISTING)

**Test Cases:**
1. ✅ **PvP Kill Processing**
   - `ProcessPvPKill_BetweenRivalDeities_Awards2xFavor()`
   - `ProcessPvPKill_BetweenAlliedDeities_Awards0Point5xFavor()`
   - `ProcessPvPKill_SameDeity_AwardsHalfFavor()`
   - `ProcessPvPKill_AttackerWithoutDeity_AwardsNoFavor()`
   - `ProcessPvPKill_IncrementsKillCount()`

2. ✅ **Death Penalty Tests**
   - `ProcessDeathPenalty_RemovesCorrectFavorAmount()`
   - `ProcessDeathPenalty_WithZeroFavor_RemovesNothing()`
   - `ProcessDeathPenalty_SendsNotificationToPlayer()`

3. ✅ **Favor Calculation Tests**
   - `CalculateFavorReward_WithNoVictimDeity_ReturnsBaseFavor()`
   - `CalculateFavorReward_UsesDeityMultiplier_Correctly()`

4. ✅ **Passive Favor Generation** (NEW)
   - `AwardPassiveFavor_WithValidPlayer_AwardsFavor()`
   - `AwardPassiveFavor_WithoutDeity_AwardsNothing()`
   - `AwardPassiveFavor_AppliesRankMultiplier_Correctly()`
   - `AwardPassiveFavor_AppliesPrestigeMultiplier_Correctly()`

5. ✅ **Event Handling** (NEW)
   - `Initialize_SubscribesToPlayerDeathEvent()`
   - `Initialize_RegistersGameTickListener()`

**Mocking Strategy:**
```csharp
Mock<ICoreServerAPI> mockAPI
Mock<IDeityRegistry> mockDeityRegistry
Mock<IPlayerDataManager> mockPlayerDataManager
Mock<IPlayerReligionDataManager> mockPlayerReligionDataManager
Mock<ReligionManager> mockReligionManager (or extract interface)
Mock<IServerPlayer> mockPlayer
```

---

#### 4.1.3 ReligionPrestigeManager (NEW TESTS)

**Interface:** `IReligionPrestigeManager`

**Test Class:** `ReligionPrestigeManagerTests.cs`

**Test Cases:**
1. ✅ **Prestige Addition Tests**
   - `AddPrestige_IncreasesPrestige_Correctly()`
   - `AddPrestige_IncrementsTotal_Correctly()`
   - `AddPrestige_WithInvalidReligion_LogsError()`

2. ✅ **Rank Update Tests**
   - `UpdatePrestigeRank_AtEstablishedThreshold_RanksUp()`
   - `UpdatePrestigeRank_BelowThreshold_MaintainsRank()`
   - `UpdatePrestigeRank_SendsNotification_OnRankUp()`

3. ✅ **Blessing Unlock Tests**
   - `UnlockReligionBlessing_WithValidBlessing_UnlocksSuccessfully()`
   - `UnlockReligionBlessing_AlreadyUnlocked_ReturnsFalse()`
   - `UnlockReligionBlessing_TriggersEffectRefresh()`

4. ✅ **Progression Tests**
   - `GetPrestigeProgress_ReturnsCorrectValues()`
   - `GetPrestigeProgress_AtMaxRank_ReturnsMaxThreshold()`

5. ✅ **Blessing Availability Tests** (NEW)
   - `CheckForNewBlessingUnlocks_OnRankUp_NotifiesMembers()`
   - `GetActiveReligionBlessings_ReturnsOnlyUnlocked()`

**Mocking Strategy:**
```csharp
Mock<ICoreServerAPI> mockAPI
Mock<ReligionManager> mockReligionManager (or extract interface)
Mock<IBlessingRegistry> mockBlessingRegistry
Mock<IBlessingEffectSystem> mockBlessingEffectSystem
```

---

#### 4.1.4 BuffManager (NEW TESTS)

**Interface:** `IBuffManager`

**Test Class:** `BuffManagerTests.cs`

**Test Cases:**
1. ✅ **Effect Application Tests**
   - `ApplyEffect_WithValidParameters_AppliesSuccessfully()`
   - `ApplyEffect_WithNullTarget_ReturnsFalse()`
   - `ApplyEffect_WithEmptyEffectId_ReturnsFalse()`
   - `ApplyEffect_CreatesBuffTrackerIfMissing()`

2. ✅ **Simple Buff Tests**
   - `ApplySimpleBuff_CreatesSingleModifier()`

3. ✅ **Effect Removal Tests**
   - `RemoveEffect_WithExistingEffect_RemovesSuccessfully()`
   - `RemoveEffect_WithNonExistentEffect_ReturnsFalse()`

4. ✅ **Effect Query Tests**
   - `HasEffect_WithActiveEffect_ReturnsTrue()`
   - `GetActiveEffects_ReturnsAllEffects()`

5. ✅ **AoE Buff Tests**
   - `ApplyAoEBuff_AffectsAllPlayersInRadius()`
   - `ApplyAoEBuff_CanExcludeCaster()`
   - `ApplyAoEBuff_ReturnsCorrectCount()`

6. ✅ **Damage Multiplier Tests**
   - `GetOutgoingDamageMultiplier_WithBuffs_ReturnsCorrectValue()`
   - `GetReceivedDamageMultiplier_WithDebuffs_ReturnsCorrectValue()`

**Mocking Strategy:**
```csharp
Mock<ICoreServerAPI> mockAPI
Mock<EntityAgent> mockEntity
Mock<EntityBehaviorBuffTracker> mockBuffTracker
```

---

#### 4.1.5 RankRequirements (UPDATE TESTS)

**Interface:** `IRankRequirements`

**Note:** Convert from static class to instance-based service

**Test Class:** `RankRequirementsTests.cs` (EXISTS - UPDATE)

**Additional Test Cases:**
1. ✅ **Favor Rank Tests**
   - All existing tests still valid
   - Add tests for rank transitions

2. ✅ **Prestige Rank Tests**
   - All existing tests still valid
   - Add tests for rank thresholds

---

#### 4.1.6 BlessingRegistry (EXISTING - EXTEND TESTS)

**Interface:** `IBlessingRegistry` ✅ EXISTS

**Test Class:** `BlessingRegistryTests.cs` (CREATE)

**Test Cases:**
1. ✅ **Registration Tests**
   - `RegisterBlessing_WithValidBlessing_AddsSuccessfully()`
   - `GetBlessing_WithValidId_ReturnsBlessing()`

2. ✅ **Query Tests**
   - `GetBlessingsForDeity_FiltersCorrectly()`
   - `GetBlessingsForDeityAndKind_FiltersCorrectly()`

3. ✅ **Unlock Validation Tests**
   - `CanUnlockBlessing_WithMetRequirements_ReturnsTrue()`
   - `CanUnlockBlessing_WithUnmetRank_ReturnsFalseWithReason()`
   - `CanUnlockBlessing_WithUnmetPrerequisites_ReturnsFalseWithReason()`

---

#### 4.1.7 BlessingEffectSystem (EXISTING - EXTEND TESTS)

**Interface:** `IBlessingEffectSystem` ✅ EXISTS

**Test Class:** `BlessingEffectSystemTests.cs` (CREATE)

**Test Cases:**
1. ✅ **Effect Application Tests**
   - `RefreshPlayerBlessings_AppliesAllUnlockedBlessings()`
   - `RefreshReligionBlessings_AppliesAllMemberBlessings()`

2. ✅ **Stat Calculation Tests**
   - `GetCombinedStatModifiers_CombinesPlayerAndReligion()`
   - `GetActiveBlessings_ReturnsCorrectLists()`

---

#### 4.1.8 PlayerReligionDataManager (EXISTING - EXTEND TESTS)

**Interface:** `IPlayerReligionDataManager` ✅ EXISTS

**Test Class:** `PlayerReligionDataManagerTests.cs` (CREATE)

**Test Cases:**
1. ✅ **Data Management Tests**
   - `GetOrCreatePlayerData_CreatesIfMissing()`
   - `AddFavor_UpdatesFavorAndRank()`
   - `AddFractionalFavor_AccumulatesCorrectly()`

2. ✅ **Blessing Management Tests**
   - `UnlockPlayerBlessing_UnlocksSuccessfully()`
   - `IsBlessingUnlocked_ReturnsCorrectStatus()`

---

#### 4.1.9 ReligionManager (EXISTING - EXTEND TESTS)

**Interface:** `IReligionManager` ✅ EXISTS

**Test Class:** `ReligionManagerTests.cs` (CREATE)

**Test Cases:**
1. ✅ **Religion CRUD Tests**
   - `CreateReligion_WithValidData_CreatesSuccessfully()`
   - `GetReligion_WithValidUID_ReturnsReligion()`
   - `GetPlayerReligion_ReturnsCorrectReligion()`

2. ✅ **Membership Tests**
   - `AddMemberToReligion_AddsSuccessfully()`
   - `RemoveMemberFromReligion_RemovesSuccessfully()`
   - `IsPlayerInReligion_ReturnsCorrectStatus()`

---

### 4.2 Models & Data (16 classes)

#### 4.2.1 Model Tests (EXISTING - EXTEND)

**Existing Tests:**
- ✅ `BlessingTests.cs`
- ✅ `DeityTests.cs`
- ✅ `PlayerFavorProgressTests.cs`
- ✅ `ReligionPrestigeProgressTests.cs`

**New Test Classes Needed:**
- `BlessingNodeStateTests.cs` (NEW)
- `BlessingTooltipDataTests.cs` (NEW)

**Test Focus:**
- Property initialization
- Validation logic
- State transitions
- Calculated properties

---

#### 4.2.2 Data Persistence Tests (EXISTING)

**Existing Tests:**
- ✅ `PlayerReligionDataTests.cs`
- ✅ `ReligionDataTests.cs`
- ✅ `PlayerAbilityDataTests.cs`
- ✅ `PlayerDeityDataTests.cs`

**Test Focus:**
- Serialization/Deserialization (ProtoBuf)
- Data integrity
- Default values

---

### 4.3 Network Packets (17 classes)

#### Existing Tests (10 test files) ✅

**Test Pattern (Apply to remaining 7 packet classes):**

```csharp
[Fact]
public void Serialize_Deserialize_MaintainsData()
{
    // Arrange
    var packet = new SomePacket { /* data */ };

    // Act
    var serialized = ProtoBuf.Serializer.Serialize(packet);
    var deserialized = ProtoBuf.Serializer.Deserialize<SomePacket>(serialized);

    // Assert
    Assert.Equal(packet.Property, deserialized.Property);
}
```

**Missing Packet Tests:**
1. `BlessingDataRequestPacketTests.cs` (NEW)
2. `BlessingDataResponsePacketTests.cs` (NEW)
3. `BlessingUnlockRequestPacketTests.cs` (NEW)
4. `BlessingUnlockResponsePacketTests.cs` (NEW)
5. `PlayerReligionInfoRequestPacketTests.cs` (NEW)
6. `PlayerReligionInfoResponsePacketTests.cs` (NEW)
7. `ReligionStateChangedPacketTests.cs` (NEW)

---

### 4.4 Commands (5 classes)

#### 4.4.1 BlessingCommands (EXISTING - COMPLETE)

**Existing Tests:** ✅ Complete coverage in `Commands/Blessing/`

---

#### 4.4.2 Other Command Tests (NEW)

**New Test Classes Needed:**
1. `ReligionCommandsTests.cs` (NEW)
2. `DeityCommandsTests.cs` (NEW)
3. `FavorCommandsTests.cs` (NEW)
4. `AbilityCommandsTests.cs` (NEW)

**Test Pattern:**
```csharp
[Fact]
public void CommandHandler_WithValidArguments_ReturnsSuccess()
{
    // Arrange
    var mockAPI = new Mock<ICoreServerAPI>();
    var mockRegistry = new Mock<IDeityRegistry>();
    var commands = new DeityCommands(mockAPI.Object, mockRegistry.Object);
    var args = CreateMockCommandArgs();

    // Act
    var result = commands.OnDeityInfo(args);

    // Assert
    Assert.Equal(TextCommandResult.Success, result.Status);
}
```

---

### 4.5 GUI Components (35 classes)

#### 4.5.1 State Management Tests

**Test Classes:**
1. ✅ `BlessingDialogManagerTests.cs` (EXISTS)
2. `CreateReligionStateTests.cs` (NEW)
3. `ReligionBrowserStateTests.cs` (NEW)
4. `ReligionManagementStateTests.cs` (NEW)
5. `OverlayCoordinatorTests.cs` (NEW)

**Test Focus:**
- State initialization
- State transitions
- Property updates
- Validation logic

---

#### 4.5.2 Renderer Tests (14 classes)

**Approach:** Limited unit testing for renderers (mostly integration/visual testing)

**Test Focus:**
- Null safety
- Data formatting
- Conditional rendering logic

**Test Classes (Optional):**
- Focus on testable logic in renderers, not ImGui calls

---

#### 4.5.3 Component Tests (7 classes)

**Test Classes:**
1. `CheckboxTests.cs` (NEW - if contains logic)
2. `DropdownTests.cs` (NEW - if contains logic)
3. `TextInputTests.cs` (NEW - if contains logic)
4. `ScrollableListTests.cs` (NEW - if contains logic)

---

### 4.6 Abilities (8 legacy classes)

#### Test Classes (NEW)

1. `BattleCryAbilityTests.cs`
2. `BladeStormAbilityTests.cs`
3. `LastStandAbilityTests.cs`
4. `WarBannerAbilityTests.cs`
5. `ArrowRainAbilityTests.cs`
6. `HuntersMarkAbilityTests.cs`
7. `PredatorInstinctAbilityTests.cs`
8. `SwiftFeetAbilityTests.cs`

**Test Focus:**
- Activation logic
- Effect application
- Cooldown management
- Mana/resource consumption

---

### 4.7 Integration Tests (NEW)

#### Integration Test Scenarios

1. **Religion Creation Flow**
   - Create religion → Join religion → Unlock blessing → Apply effect

2. **PvP Favor Flow**
   - Player kills → Favor awarded → Rank up → Blessing unlocked

3. **Prestige Progression Flow**
   - Religion actions → Prestige gained → Rank up → Unlock religion blessing

4. **Buff Application Flow**
   - Ability activation → Buff applied → Stat modified → Effect expires

**Test Class:** `IntegrationTests.cs`

---

## 5. Implementation Roadmap

### Phase 1: Interface Extraction (Week 1-2)

**Priority Order:**
1. Extract `IDeityRegistry` ✅
2. Extract `IFavorSystem` ✅
3. Extract `IReligionPrestigeManager` ✅
4. Extract `IBuffManager` ✅
5. Extract `IRankRequirements` ✅
6. Extract `IBlessingDialogManager` ✅
7. Extract `IOverlayCoordinator` ✅
8. Extract `IBlessingDefinitions` ✅

**Checklist per Interface:**
- [ ] Create interface in `Systems/Interfaces/`
- [ ] Implement interface in existing class
- [ ] Update all consumers to use interface
- [ ] Update dependency injection in `PantheonWarsSystem.cs`
- [ ] Write unit tests for new interface

---

### Phase 2: Core System Tests (Week 3-4)

**Test Creation Order:**
1. `DeityRegistryTests.cs` ✅
2. `FavorSystemTests.cs` (extend existing) ✅
3. `ReligionPrestigeManagerTests.cs` ✅
4. `BuffManagerTests.cs` ✅
5. `BlessingRegistryTests.cs` ✅
6. `BlessingEffectSystemTests.cs` ✅
7. `ReligionManagerTests.cs` ✅
8. `PlayerReligionDataManagerTests.cs` ✅

---

### Phase 3: Command & GUI Tests (Week 5)

1. Complete command tests for remaining 4 command classes
2. Complete GUI state management tests
3. Add missing packet tests (7 classes)

---

### Phase 4: Ability & Integration Tests (Week 6)

1. Create tests for all 8 ability classes
2. Create integration test suite
3. Establish test coverage reporting

---

### Phase 5: CI/CD Integration (Week 7)

1. Configure automated test execution in build process
2. Set up code coverage reporting
3. Establish quality gates (minimum coverage thresholds)

---

## 6. Test Examples

### Example 1: Testing DeityRegistry with Mocking

**File:** `PantheonWars.Tests/Systems/DeityRegistryTests.cs`

```csharp
using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Models.Enum;
using PantheonWars.Systems;
using Vintagestory.API.Common;
using Xunit;

namespace PantheonWars.Tests.Systems;

[ExcludeFromCodeCoverage]
public class DeityRegistryTests
{
    private readonly Mock<ICoreAPI> _mockApi;
    private readonly Mock<ILogger> _mockLogger;
    private readonly DeityRegistry _registry;

    public DeityRegistryTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockApi = new Mock<ICoreAPI>();
        _mockApi.Setup(a => a.Logger).Returns(_mockLogger.Object);

        _registry = new DeityRegistry(_mockApi.Object);
    }

    [Fact]
    public void Initialize_RegistersKhorasAndLysa_Successfully()
    {
        // Act
        _registry.Initialize();

        // Assert
        Assert.NotNull(_registry.GetDeity(DeityType.Khoras));
        Assert.NotNull(_registry.GetDeity(DeityType.Lysa));
        Assert.Equal(2, _registry.GetAllDeities().Count());
    }

    [Fact]
    public void GetDeity_WithValidType_ReturnsCorrectDeity()
    {
        // Arrange
        _registry.Initialize();

        // Act
        var deity = _registry.GetDeity(DeityType.Khoras);

        // Assert
        Assert.NotNull(deity);
        Assert.Equal("Khoras", deity.Name);
        Assert.Equal("War", deity.Domain);
        Assert.Equal(DeityAlignment.Lawful, deity.Alignment);
    }

    [Fact]
    public void GetDeity_WithInvalidType_ReturnsNull()
    {
        // Arrange
        _registry.Initialize();

        // Act
        var deity = _registry.GetDeity(DeityType.Morthen); // Not yet registered

        // Assert
        Assert.Null(deity);
    }

    [Fact]
    public void GetRelationship_BetweenAlliedDeities_ReturnsAllied()
    {
        // Arrange
        _registry.Initialize();

        // Act
        var relationship = _registry.GetRelationship(DeityType.Khoras, DeityType.Lysa);

        // Assert
        Assert.Equal(DeityRelationshipType.Allied, relationship);
    }

    [Fact]
    public void GetFavorMultiplier_ForRivalDeities_Returns2Point0()
    {
        // Arrange
        _registry.Initialize();

        // Act
        var multiplier = _registry.GetFavorMultiplier(DeityType.Khoras, DeityType.Morthen);

        // Assert
        Assert.Equal(2.0f, multiplier);
    }

    [Fact]
    public void HasDeity_WithRegisteredType_ReturnsTrue()
    {
        // Arrange
        _registry.Initialize();

        // Act & Assert
        Assert.True(_registry.HasDeity(DeityType.Khoras));
    }
}
```

---

### Example 2: Testing FavorSystem with Multiple Mocks

**File:** `PantheonWars.Tests/Systems/FavorSystemTests.cs`

```csharp
using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Data;
using PantheonWars.Models.Enum;
using PantheonWars.Systems;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Xunit;

namespace PantheonWars.Tests.Systems;

[ExcludeFromCodeCoverage]
public class FavorSystemTests
{
    private readonly Mock<ICoreServerAPI> _mockAPI;
    private readonly Mock<IDeityRegistry> _mockDeityRegistry;
    private readonly Mock<IPlayerDataManager> _mockPlayerDataManager;
    private readonly Mock<IPlayerReligionDataManager> _mockPlayerReligionDataManager;
    private readonly Mock<IReligionManager> _mockReligionManager;
    private readonly FavorSystem _favorSystem;

    public FavorSystemTests()
    {
        _mockAPI = new Mock<ICoreServerAPI>();
        _mockDeityRegistry = new Mock<IDeityRegistry>();
        _mockPlayerDataManager = new Mock<IPlayerDataManager>();
        _mockPlayerReligionDataManager = new Mock<IPlayerReligionDataManager>();
        _mockReligionManager = new Mock<IReligionManager>();

        // Setup logger
        var mockLogger = new Mock<ILogger>();
        _mockAPI.Setup(a => a.Logger).Returns(mockLogger.Object);

        _favorSystem = new FavorSystem(
            _mockAPI.Object,
            _mockPlayerDataManager.Object,
            _mockPlayerReligionDataManager.Object,
            _mockDeityRegistry.Object,
            _mockReligionManager.Object
        );
    }

    [Fact]
    public void ProcessPvPKill_BetweenRivalDeities_Awards2xFavor()
    {
        // Arrange
        var attackerData = new PlayerReligionData { ActiveDeity = DeityType.Khoras, KillCount = 0 };
        var victimData = new PlayerReligionData { ActiveDeity = DeityType.Morthen };

        var mockAttacker = new Mock<IServerPlayer>();
        mockAttacker.Setup(p => p.PlayerUID).Returns("attacker-uid");
        mockAttacker.Setup(p => p.PlayerName).Returns("Attacker");

        var mockVictim = new Mock<IServerPlayer>();
        mockVictim.Setup(p => p.PlayerUID).Returns("victim-uid");
        mockVictim.Setup(p => p.PlayerName).Returns("Victim");

        _mockPlayerReligionDataManager
            .Setup(m => m.GetOrCreatePlayerData("attacker-uid"))
            .Returns(attackerData);

        _mockPlayerReligionDataManager
            .Setup(m => m.GetOrCreatePlayerData("victim-uid"))
            .Returns(victimData);

        _mockDeityRegistry
            .Setup(r => r.GetFavorMultiplier(DeityType.Khoras, DeityType.Morthen))
            .Returns(2.0f);

        // Act
        _favorSystem.ProcessPvPKill(mockAttacker.Object, mockVictim.Object);

        // Assert
        _mockPlayerReligionDataManager.Verify(
            m => m.AddFavor("attacker-uid", 20, It.IsAny<string>()), // 10 * 2.0 = 20
            Times.Once()
        );
        Assert.Equal(1, attackerData.KillCount);
    }

    [Fact]
    public void ProcessDeathPenalty_RemovesCorrectFavorAmount()
    {
        // Arrange
        var playerData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 10
        };

        var mockPlayer = new Mock<IServerPlayer>();
        mockPlayer.Setup(p => p.PlayerUID).Returns("player-uid");

        _mockPlayerReligionDataManager
            .Setup(m => m.GetOrCreatePlayerData("player-uid"))
            .Returns(playerData);

        // Act
        _favorSystem.ProcessDeathPenalty(mockPlayer.Object);

        // Assert
        _mockPlayerReligionDataManager.Verify(
            m => m.RemoveFavor("player-uid", 5, "Death penalty"),
            Times.Once()
        );
    }

    [Fact]
    public void CalculateFavorReward_WithSameDeity_ReturnsHalfFavor()
    {
        // Arrange & Act
        var reward = _favorSystem.CalculateFavorReward(DeityType.Khoras, DeityType.Khoras);

        // Assert
        Assert.Equal(5, reward); // BASE_KILL_FAVOR / 2
    }
}
```

---

### Example 3: Testing BuffManager with Entity Mocking

**File:** `PantheonWars.Tests/Systems/BuffManagerTests.cs`

```csharp
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Systems.BuffSystem;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Xunit;

namespace PantheonWars.Tests.Systems.BuffSystem;

[ExcludeFromCodeCoverage]
public class BuffManagerTests
{
    private readonly Mock<ICoreServerAPI> _mockAPI;
    private readonly BuffManager _buffManager;

    public BuffManagerTests()
    {
        _mockAPI = new Mock<ICoreServerAPI>();
        var mockLogger = new Mock<ILogger>();
        _mockAPI.Setup(a => a.Logger).Returns(mockLogger.Object);

        _buffManager = new BuffManager(_mockAPI.Object);
    }

    [Fact]
    public void ApplyEffect_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var mockEntity = new Mock<EntityAgent>(MockBehavior.Loose);
        var mockBuffTracker = new Mock<EntityBehaviorBuffTracker>(mockEntity.Object);

        mockEntity
            .Setup(e => e.GetBehavior<EntityBehaviorBuffTracker>())
            .Returns(mockBuffTracker.Object);

        var statModifiers = new Dictionary<string, float>
        {
            { "walkspeed", 0.2f }
        };

        // Act
        var result = _buffManager.ApplyEffect(
            mockEntity.Object,
            "test_buff",
            10.0f,
            "test_ability",
            "player-uid",
            statModifiers,
            true
        );

        // Assert
        Assert.True(result);
        mockBuffTracker.Verify(
            bt => bt.AddEffect(It.IsAny<ActiveEffect>()),
            Times.Once()
        );
    }

    [Fact]
    public void ApplyEffect_WithNullTarget_ReturnsFalse()
    {
        // Act
        var result = _buffManager.ApplyEffect(
            null,
            "test_buff",
            10.0f,
            "test_ability",
            "player-uid",
            new Dictionary<string, float>(),
            true
        );

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveEffect_WithExistingEffect_ReturnsTrue()
    {
        // Arrange
        var mockEntity = new Mock<EntityAgent>(MockBehavior.Loose);
        var mockBuffTracker = new Mock<EntityBehaviorBuffTracker>(mockEntity.Object);

        mockEntity
            .Setup(e => e.GetBehavior<EntityBehaviorBuffTracker>())
            .Returns(mockBuffTracker.Object);

        mockBuffTracker
            .Setup(bt => bt.RemoveEffect("test_buff"))
            .Returns(true);

        // Act
        var result = _buffManager.RemoveEffect(mockEntity.Object, "test_buff");

        // Assert
        Assert.True(result);
    }
}
```

---

### Example 4: Testing Command Handler

**File:** `PantheonWars.Tests/Commands/DeityCommandsTests.cs`

```csharp
using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using PantheonWars.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Xunit;

namespace PantheonWars.Tests.Commands;

[ExcludeFromCodeCoverage]
public class DeityCommandsTests
{
    private readonly Mock<ICoreServerAPI> _mockAPI;
    private readonly Mock<IDeityRegistry> _mockRegistry;
    private readonly DeityCommands _commands;

    public DeityCommandsTests()
    {
        _mockAPI = new Mock<ICoreServerAPI>();
        _mockRegistry = new Mock<IDeityRegistry>();

        _commands = new DeityCommands(_mockAPI.Object, _mockRegistry.Object);
    }

    [Fact]
    public void OnDeityInfo_WithValidDeity_ReturnsSuccess()
    {
        // Arrange
        var mockPlayer = new Mock<IServerPlayer>();
        var mockCaller = new Mock<Caller>();
        mockCaller.Setup(c => c.Player).Returns(mockPlayer.Object);

        var mockArgs = new Mock<TextCommandCallingArgs>();
        mockArgs.Setup(a => a.Caller).Returns(mockCaller.Object);
        mockArgs.Setup(a => a[0]).Returns("khoras");

        var deity = new Deity(DeityType.Khoras, "Khoras", "War")
        {
            Description = "God of War",
            Alignment = DeityAlignment.Lawful
        };

        _mockRegistry
            .Setup(r => r.GetDeity(DeityType.Khoras))
            .Returns(deity);

        // Act
        var result = _commands.OnDeityInfo(mockArgs.Object);

        // Assert
        Assert.Equal(TextCommandResult.Success, result.Status);
        Assert.Contains("Khoras", result.Message);
        Assert.Contains("War", result.Message);
    }
}
```

---

## 7. Test Coverage Goals

### Overall Coverage Targets

| Component | Target Coverage | Priority |
|-----------|----------------|----------|
| Core Systems | 85%+ | High |
| Models & Data | 90%+ | High |
| Network Packets | 95%+ | Medium |
| Commands | 75%+ | Medium |
| GUI State | 70%+ | Medium |
| Abilities | 80%+ | Low |
| Renderers | 40%+ | Low |

---

## 8. Continuous Integration

### Build Configuration

**Update CakeBuild or add GitHub Actions:**

```yaml
name: Test Suite

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

      - name: Code Coverage Report
        uses: codecov/codecov-action@v3
```

---

## 9. Test Maintenance Guidelines

### Naming Conventions

**Test Method Naming:**
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

**Examples:**
- `GetDeity_WithValidType_ReturnsDeity()`
- `ProcessPvPKill_BetweenAlliedDeities_AwardsReducedFavor()`
- `ApplyEffect_WithNullTarget_ReturnsFalse()`

### AAA Pattern

Always follow **Arrange-Act-Assert** pattern:

```csharp
[Fact]
public void ExampleTest()
{
    // Arrange - Set up test data and mocks
    var mockAPI = new Mock<ICoreAPI>();
    var system = new MySystem(mockAPI.Object);

    // Act - Execute the method under test
    var result = system.DoSomething();

    // Assert - Verify the outcome
    Assert.NotNull(result);
    mockAPI.Verify(a => a.SomeMethod(), Times.Once());
}
```

### Test Isolation

- Each test should be **independent**
- Use **setup/teardown** methods for common initialization
- **Never rely** on test execution order
- **Clean up** resources after tests

---

## 10. Success Criteria

### Phase Completion Criteria

**Phase 1 Complete When:** ✅ **COMPLETED**
- [x] All 8 high-priority interfaces extracted
- [x] All consumers updated to use interfaces
- [x] No compilation errors
- [x] Existing tests still pass

**Phase 2 Complete When:** ✅ **COMPLETED**
- [x] 8 core system test classes created
- [x] >80% coverage for core systems
- [x] All tests passing
- [x] Mocking best practices applied

**Phase 3 Complete When:**
- [ ] All command classes tested
- [ ] All packet classes tested
- [ ] GUI state management tested
- [ ] >70% coverage for commands and GUI

**Phase 4 Complete When:**
- [ ] All ability classes tested
- [ ] Integration test suite created
- [ ] >60% coverage for integration tests

**Phase 5 Complete When:**
- [ ] CI/CD pipeline configured
- [ ] Code coverage reporting enabled
- [ ] Quality gates enforced

---

## Conclusion

This comprehensive test plan provides a roadmap for achieving high test coverage across the PantheonWars codebase. By extracting interfaces, applying mocking best practices, and systematically testing each component, we'll create a robust, maintainable test suite that ensures code quality and prevents regressions.

**Next Steps:**
1. Review and approve this test plan
2. Begin Phase 1: Interface Extraction
3. Create test fixture utilities
4. Start implementing tests following the roadmap

---

**Document Version:** 1.0
**Created:** 2025-11-12
**Author:** Claude (AI Assistant)
**Status:** Draft - Pending Review
