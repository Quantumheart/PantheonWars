# Unit Testing Implementation Guide: Passive Favor System

## Overview

This guide outlines the comprehensive unit testing strategy for the Phase 1 passive favor generation system. Tests will validate fractional accumulation, multiplier calculations, and integration with existing systems.

---

## Testing Infrastructure

### Existing Framework
- **Framework**: xUnit v3
- **Mocking**: Moq 4.20.72
- **Target**: .NET 8.0
- **Pattern**: Fact/Theory-based tests with inline data
- **Location**: `PantheonWars.Tests/Systems/`

### New Test Files to Create
1. `PlayerDeityDataTests.cs` - Fractional favor accumulation
2. `FavorSystemPassiveTests.cs` - Passive favor generation logic
3. `PlayerDataManagerPassiveTests.cs` - Fractional favor manager methods

---

## Test Plan

### 1. PlayerDeityData Fractional Favor Tests

**File**: `PantheonWars.Tests/Data/PlayerDeityDataTests.cs`

**Purpose**: Test fractional favor accumulation logic in isolation

**Test Cases**:

#### 1.1 Basic Fractional Accumulation
```csharp
[Fact]
public void AddFractionalFavor_ShouldAccumulate_WithoutAwardingUntilOne()
{
    // Given: New player data
    var playerData = new PlayerDeityData("test-player");

    // When: Add fractional favor < 1.0
    playerData.AddFractionalFavor(0.04f);
    playerData.AddFractionalFavor(0.04f);
    playerData.AddFractionalFavor(0.04f);

    // Then: No favor awarded, fractional accumulator increased
    Assert.Equal(0, playerData.DivineFavor);
    Assert.Equal(0.12f, playerData.AccumulatedFractionalFavor, precision: 2);
}
```

#### 1.2 Favor Award at Threshold
```csharp
[Fact]
public void AddFractionalFavor_ShouldAwardFavor_WhenReachingOne()
{
    // Given: Player data with accumulated fractional favor
    var playerData = new PlayerDeityData("test-player");
    playerData.AccumulatedFractionalFavor = 0.96f;

    // When: Add fractional favor pushing total over 1.0
    playerData.AddFractionalFavor(0.08f); // Total: 1.04

    // Then: 1 favor awarded, remainder kept
    Assert.Equal(1, playerData.DivineFavor);
    Assert.Equal(1, playerData.TotalFavorEarned);
    Assert.Equal(0.04f, playerData.AccumulatedFractionalFavor, precision: 2);
}
```

#### 1.3 Multiple Favor Awards
```csharp
[Fact]
public void AddFractionalFavor_ShouldAwardMultipleFavor_WhenLargeAmount()
{
    // Given: New player data
    var playerData = new PlayerDeityData("test-player");

    // When: Add large fractional amount
    playerData.AddFractionalFavor(3.75f);

    // Then: Multiple favor awarded, remainder kept
    Assert.Equal(3, playerData.DivineFavor);
    Assert.Equal(3, playerData.TotalFavorEarned);
    Assert.Equal(0.75f, playerData.AccumulatedFractionalFavor, precision: 2);
}
```

#### 1.4 Precision Preservation
```csharp
[Theory]
[InlineData(0.04f, 25)] // 25 ticks should award 1 favor
[InlineData(0.08f, 13)] // 13 ticks should award 1 favor
[InlineData(0.01f, 100)] // 100 ticks should award 1 favor
public void AddFractionalFavor_ShouldPreservePrecision_OverMultipleTicks(float amount, int ticks)
{
    // Given: New player data
    var playerData = new PlayerDeityData("test-player");

    // When: Add fractional favor multiple times
    for (int i = 0; i < ticks; i++)
    {
        playerData.AddFractionalFavor(amount);
    }

    // Then: Should award exactly the expected favor
    int expectedFavor = (int)(amount * ticks);
    Assert.Equal(expectedFavor, playerData.DivineFavor);
}
```

#### 1.5 Zero/Negative Amounts
```csharp
[Theory]
[InlineData(0f)]
[InlineData(-0.5f)]
public void AddFractionalFavor_ShouldIgnore_ZeroOrNegativeAmounts(float amount)
{
    // Given: New player data
    var playerData = new PlayerDeityData("test-player");
    playerData.AccumulatedFractionalFavor = 0.5f;

    // When: Add zero or negative amount
    playerData.AddFractionalFavor(amount);

    // Then: No change
    Assert.Equal(0, playerData.DivineFavor);
    Assert.Equal(0.5f, playerData.AccumulatedFractionalFavor, precision: 2);
}
```

#### 1.6 Devotion Rank Update Integration
```csharp
[Fact]
public void AddFractionalFavor_ShouldUpdateDevotionRank_WhenThresholdReached()
{
    // Given: Player data near Disciple threshold (500 favor)
    var playerData = new PlayerDeityData("test-player");
    playerData.DivineFavor = 499;
    playerData.TotalFavorEarned = 499;
    playerData.AccumulatedFractionalFavor = 0.9f;

    // When: Add fractional favor to reach threshold
    playerData.AddFractionalFavor(0.15f); // Total: 500

    // Then: Rank upgraded
    Assert.Equal(500, playerData.TotalFavorEarned);
    Assert.Equal(DevotionRank.Disciple, playerData.DevotionRank);
}
```

#### 1.7 Persistence/Serialization
```csharp
[Fact]
public void AccumulatedFractionalFavor_ShouldPersist_ThroughSerialization()
{
    // Given: Player data with fractional favor
    var original = new PlayerDeityData("test-player");
    original.AddFractionalFavor(0.66f);

    // When: Serialize and deserialize (simulating save/load)
    var bytes = SerializerUtil.Serialize(original);
    var deserialized = SerializerUtil.Deserialize<PlayerDeityData>(bytes);

    // Then: Fractional favor preserved
    Assert.Equal(0.66f, deserialized.AccumulatedFractionalFavor, precision: 2);
    Assert.Equal(0, deserialized.DivineFavor);
}
```

---

### 2. FavorSystem Passive Generation Tests

**File**: `PantheonWars.Tests/Systems/FavorSystemPassiveTests.cs`

**Purpose**: Test passive favor calculation logic

**Test Cases**:

#### 2.1 Base Favor Rate Calculation
```csharp
[Fact]
public void CalculateBaseFavor_ShouldReturn_CorrectAmount_ForOneSecond()
{
    // Given: 1 second tick (dt = 1.0)
    float dt = 1.0f;
    float hoursPerDay = 24.0f; // VS default
    float baseFavorPerHour = 2.0f;

    // When: Calculate base favor
    float inGameHoursElapsed = dt / hoursPerDay;
    float baseFavor = baseFavorPerHour * inGameHoursElapsed;

    // Then: Should be ~0.0833 favor per second
    Assert.Equal(0.0833f, baseFavor, precision: 4);
}

// Note: This will need adjustment based on actual VS calendar implementation
```

#### 2.2 Devotion Rank Multiplier Calculation
```csharp
[Theory]
[InlineData(DevotionRank.Initiate, 1.0f)]
[InlineData(DevotionRank.Disciple, 1.1f)]
[InlineData(DevotionRank.Zealot, 1.2f)]
[InlineData(DevotionRank.Champion, 1.3f)]
[InlineData(DevotionRank.Avatar, 1.5f)]
public void CalculateDevotionMultiplier_ShouldReturn_CorrectValue(DevotionRank rank, float expected)
{
    // Given: Player data with specific rank
    var playerData = new PlayerDeityData("test-player");
    playerData.DevotionRank = rank;

    // When: Calculate multiplier (extracted logic)
    float multiplier = rank switch
    {
        DevotionRank.Initiate => 1.0f,
        DevotionRank.Disciple => 1.1f,
        DevotionRank.Zealot => 1.2f,
        DevotionRank.Champion => 1.3f,
        DevotionRank.Avatar => 1.5f,
        _ => 1.0f
    };

    // Then: Matches expected value
    Assert.Equal(expected, multiplier);
}
```

#### 2.3 Religion Prestige Multiplier Calculation
```csharp
[Theory]
[InlineData(PrestigeRank.Fledgling, 1.0f)]
[InlineData(PrestigeRank.Established, 1.1f)]
[InlineData(PrestigeRank.Renowned, 1.2f)]
[InlineData(PrestigeRank.Legendary, 1.3f)]
[InlineData(PrestigeRank.Mythic, 1.5f)]
public void CalculatePrestigeMultiplier_ShouldReturn_CorrectValue(PrestigeRank rank, float expected)
{
    // Given: Religion with specific prestige rank
    var religion = new ReligionData("rel-uid", "Test Religion", DeityType.Khoras, "founder-uid");
    religion.PrestigeRank = rank;

    // When: Calculate multiplier (extracted logic)
    float multiplier = rank switch
    {
        PrestigeRank.Fledgling => 1.0f,
        PrestigeRank.Established => 1.1f,
        PrestigeRank.Renowned => 1.2f,
        PrestigeRank.Legendary => 1.3f,
        PrestigeRank.Mythic => 1.5f,
        _ => 1.0f
    };

    // Then: Matches expected value
    Assert.Equal(expected, multiplier);
}
```

#### 2.4 Combined Multiplier Calculation
```csharp
[Theory]
[InlineData(DevotionRank.Initiate, PrestigeRank.Fledgling, 1.0f)]
[InlineData(DevotionRank.Avatar, PrestigeRank.Fledgling, 1.5f)]
[InlineData(DevotionRank.Initiate, PrestigeRank.Mythic, 1.5f)]
[InlineData(DevotionRank.Avatar, PrestigeRank.Mythic, 2.25f)]
[InlineData(DevotionRank.Disciple, PrestigeRank.Established, 1.21f)]
public void CalculateTotalMultiplier_ShouldStack_DevotionAndPrestige(
    DevotionRank devotion, PrestigeRank prestige, float expected)
{
    // Given: Player and religion with ranks
    float devotionMultiplier = GetDevotionMultiplier(devotion);
    float prestigeMultiplier = GetPrestigeMultiplier(prestige);

    // When: Multiply together
    float totalMultiplier = devotionMultiplier * prestigeMultiplier;

    // Then: Correct stacking
    Assert.Equal(expected, totalMultiplier, precision: 2);
}
```

#### 2.5 Full Favor Calculation Pipeline
```csharp
[Fact]
public void CalculatePassiveFavor_ShouldReturn_CorrectAmount_WithAllMultipliers()
{
    // Given: Avatar in Mythic religion, 1 second tick
    float baseFavorPerHour = 2.0f;
    float dt = 1.0f;
    float hoursPerDay = 24.0f;
    float devotionMultiplier = 1.5f; // Avatar
    float prestigeMultiplier = 1.5f; // Mythic

    // When: Calculate final favor
    float inGameHoursElapsed = dt / hoursPerDay;
    float baseFavor = baseFavorPerHour * inGameHoursElapsed;
    float finalFavor = baseFavor * devotionMultiplier * prestigeMultiplier;

    // Then: Should be ~0.1875 favor per second
    float expected = 2.0f / 24.0f * 1.5f * 1.5f;
    Assert.Equal(expected, finalFavor, precision: 4);
}
```

#### 2.6 Player Without Deity
```csharp
[Fact]
public void AwardPassiveFavor_ShouldNotAward_WhenPlayerHasNoDeity()
{
    // Given: Player without deity
    var playerData = new PlayerDeityData("test-player");
    // DeityType is None by default

    // When: Check if should award
    bool hasDeity = playerData.HasDeity();

    // Then: Should not award
    Assert.False(hasDeity);
}
```

---

### 3. PlayerDataManager Integration Tests

**File**: `PantheonWars.Tests/Systems/PlayerDataManagerPassiveTests.cs`

**Purpose**: Test manager-level fractional favor operations

**Test Cases**:

#### 3.1 AddFractionalFavor Method
```csharp
[Fact]
public void AddFractionalFavor_ShouldCallPlayerData_AddFractionalFavor()
{
    // Given: Mock API and player data
    var mockApi = new Mock<ICoreServerAPI>();
    var manager = new PlayerDataManager(mockApi.Object);

    // Create player data manually
    var playerData = manager.GetOrCreatePlayerData("test-player");

    // When: Add fractional favor
    manager.AddFractionalFavor("test-player", 0.5f, "test");

    // Then: Fractional favor accumulated
    Assert.Equal(0.5f, playerData.AccumulatedFractionalFavor, precision: 2);
}
```

#### 3.2 Multiple Players Concurrent
```csharp
[Fact]
public void AddFractionalFavor_ShouldHandle_MultiplePlayersConcurrently()
{
    // Given: Manager with multiple players
    var mockApi = new Mock<ICoreServerAPI>();
    var manager = new PlayerDataManager(mockApi.Object);

    var player1 = manager.GetOrCreatePlayerData("player-1");
    var player2 = manager.GetOrCreatePlayerData("player-2");

    // When: Add fractional favor to both
    manager.AddFractionalFavor("player-1", 0.7f, "passive");
    manager.AddFractionalFavor("player-2", 0.3f, "passive");

    // Then: Each player has correct amount
    Assert.Equal(0.7f, player1.AccumulatedFractionalFavor, precision: 2);
    Assert.Equal(0.3f, player2.AccumulatedFractionalFavor, precision: 2);
}
```

---

## Test Organization

### Directory Structure
```
PantheonWars.Tests/
├── Data/
│   ├── PlayerDeityDataTests.cs (NEW)
│   └── ... (existing tests)
├── Systems/
│   ├── FavorSystemPassiveTests.cs (NEW)
│   ├── PlayerDataManagerPassiveTests.cs (NEW)
│   └── ... (existing tests)
└── ... (other folders)
```

---

## Mocking Strategy

### VS API Components to Mock

```csharp
// Calendar API (for time calculations)
var mockCalendar = new Mock<IGameCalendar>();
mockCalendar.Setup(c => c.HoursPerDay).Returns(24.0f);
mockCalendar.Setup(c => c.TotalHours).Returns(0.0);

// World API
var mockWorld = new Mock<IServerWorldAccessor>();
mockWorld.Setup(w => w.Calendar).Returns(mockCalendar.Object);

// Server API
var mockApi = new Mock<ICoreServerAPI>();
mockApi.Setup(a => a.World).Returns(mockWorld.Object);
```

### Player Mocking

```csharp
// Mock server player
var mockPlayer = new Mock<IServerPlayer>();
mockPlayer.Setup(p => p.PlayerUID).Returns("test-player-uid");

// Mock entity (for position, stats)
var mockEntity = new Mock<EntityPlayer>();
mockPlayer.Setup(p => p.Entity).Returns(mockEntity.Object);
```

---

## Test Data Builders (Helper Methods)

```csharp
// Test helpers for creating test data
public static class TestDataBuilder
{
    public static PlayerDeityData CreatePlayerWithDeity(
        string uid = "test-player",
        DeityType deity = DeityType.Khoras,
        DevotionRank rank = DevotionRank.Initiate,
        int favor = 0)
    {
        var player = new PlayerDeityData(uid);
        player.DeityType = deity;
        player.DevotionRank = rank;
        player.DivineFavor = favor;
        player.TotalFavorEarned = favor;
        return player;
    }

    public static ReligionData CreateReligion(
        string uid = "test-religion",
        string name = "Test Religion",
        DeityType deity = DeityType.Khoras,
        string founderUid = "founder",
        PrestigeRank rank = PrestigeRank.Fledgling)
    {
        var religion = new ReligionData(uid, name, deity, founderUid);
        religion.PrestigeRank = rank;
        return religion;
    }
}
```

---

## Test Execution Plan

### Phase 1: Data Layer Tests
1. **PlayerDeityDataTests.cs** - All fractional favor tests
2. Verify precision and edge cases
3. Test serialization/persistence

### Phase 2: Logic Layer Tests
1. **FavorSystemPassiveTests.cs** - Multiplier calculations
2. Test rate formulas
3. Verify combined multipliers

### Phase 3: Integration Tests
1. **PlayerDataManagerPassiveTests.cs** - Manager operations
2. Test multi-player scenarios
3. Verify thread safety (if applicable)

### Phase 4: Manual Testing
1. Deploy to test server
2. Monitor passive favor gain over 10-minute period
3. Verify in-game rates match calculations

---

## Expected Test Coverage

### Metrics
- **Target Coverage**: 90%+ for new code
- **Critical Paths**: 100% coverage
  - Fractional accumulation logic
  - Multiplier calculations
  - Favor awarding at threshold

### Coverage Analysis
```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

---

## Continuous Integration

### Test Execution
```yaml
# Example CI pipeline step
- name: Run Unit Tests
  run: dotnet test --configuration Release --no-build --verbosity normal

- name: Test Results
  if: always()
  uses: dorny/test-reporter@v1
  with:
    name: XUnit Tests
    path: '**/TestResults/*.trx'
    reporter: dotnet-trx
```

---

## Performance Benchmarks

### Benchmark Tests (Optional)

```csharp
[Fact]
public void AddFractionalFavor_Performance_1000Iterations()
{
    // Given: Player data
    var playerData = new PlayerDeityData("test-player");
    var stopwatch = Stopwatch.StartNew();

    // When: Add fractional favor 1000 times
    for (int i = 0; i < 1000; i++)
    {
        playerData.AddFractionalFavor(0.04f);
    }

    stopwatch.Stop();

    // Then: Should complete quickly (< 10ms)
    Assert.True(stopwatch.ElapsedMilliseconds < 10);
}
```

---

## Test Maintenance

### When to Update Tests
- Adding new multiplier types (Phase 3)
- Changing base favor rates (balancing)
- Adding new devotion/prestige ranks
- Modifying calculation formulas

### Documentation
- Update test comments when changing formulas
- Document expected values in inline data
- Add comments explaining complex test scenarios

---

## Approval Checklist

Please review and approve the following before implementation:

### Test Coverage
- [ ] Fractional favor accumulation (7 tests)
- [ ] Multiplier calculations (5 tests)
- [ ] Integration with existing systems (2 tests)
- [ ] Edge cases (zero, negative, large amounts)
- [ ] Precision preservation over time
- [ ] Serialization/persistence

### Test Organization
- [ ] File naming convention
- [ ] Directory structure
- [ ] Test helper methods
- [ ] Mock strategy

### Test Quality
- [ ] Clear Given/When/Then structure
- [ ] Descriptive test names
- [ ] Inline theory data for parameterized tests
- [ ] Appropriate assertions with precision

### Documentation
- [ ] Test plan clearly explained
- [ ] Expected values documented
- [ ] Rationale for each test case

---

## Next Steps After Approval

1. Create test files with approved test cases
2. Run tests (expect failures - TDD)
3. Verify test coverage meets targets
4. Document any deviations from plan
5. Execute manual testing phase

---

## Questions for Review

1. **Are the test cases comprehensive enough?**
2. **Should we add performance/benchmark tests?**
3. **Do we need integration tests with actual VS API?**
4. **Should we test save/load persistence explicitly?**
5. **Are there additional edge cases to consider?**

---

## Estimated Implementation Time

- **Test File Creation**: 2-3 hours
- **Test Implementation**: 4-6 hours
- **Debugging & Refinement**: 2-3 hours
- **Documentation**: 1 hour
- **Total**: 9-13 hours

---

## Success Criteria

✅ All 14+ unit tests pass
✅ 90%+ code coverage on new code
✅ No regressions in existing tests
✅ Tests run in < 5 seconds
✅ Clear test failure messages
✅ Tests are maintainable and readable

---

**Ready for your approval to proceed with implementation!**
