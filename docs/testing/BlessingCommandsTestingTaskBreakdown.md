# BlessingCommands Testing Task Breakdown

## Overview

This document outlines the comprehensive testing strategy for `BlessingCommands.cs`, which manages all blessing-related commands in PantheonWars.

**File Under Test:** `PantheonWars/Commands/BlessingCommands.cs`
**Test File:** `PantheonWars.Tests/Commands/BlessingCommandsTests.cs`
**Lines of Code:** ~600
**Estimated Test Count:** 60-80 unit tests

## Class Structure

The `BlessingCommands` class has:
- **5 injected dependencies** (all interfaces - excellent for mocking):
  - `ICoreServerAPI`
  - `IBlessingRegistry`
  - `IPlayerReligionDataManager`
  - `IReligionManager`
  - `IBlessingEffectSystem`
- **1 public method:** `RegisterCommands()`
- **7 command handlers** (private methods):
  - `OnBlessingsList`
  - `OnBlessingsPlayer`
  - `OnBlessingsReligion`
  - `OnBlessingsInfo`
  - `OnBlessingsTree`
  - `OnBlessingsUnlock`
  - `OnBlessingsActive`
- **Extensive string constants** (already extracted - excellent for maintainability)

## Handling Private Methods

### Problem
Most command handlers are `private`, which makes direct unit testing challenging.

### Recommended Solution: `internal` + `InternalsVisibleTo`

**Implementation:**
1. Change all command handler methods from `private` to `internal`
2. Add `[assembly: InternalsVisibleTo("PantheonWars.Tests")]` to the project

**Rationale:**
- Standard C# testing pattern
- Keeps methods hidden from external consumers
- Allows clean, direct unit testing
- Well-supported by all testing frameworks

**Alternative Solutions:**
- Make handlers `public` (philosophically valid - they're exposed via command system anyway)
- Test indirectly through command registration (more integration-style)
- Use reflection (NOT recommended - brittle and slow)

## Testing Tools & Frameworks

**Recommended Stack:**
- **Test Framework:** xUnit or NUnit
- **Mocking:** Moq or NSubstitute
- **Assertions:** FluentAssertions
- **Coverage:** Coverlet or JetBrains dotCover

## Task Breakdown

### Phase 0: Preparation & Setup ✅ COMPLETED

#### Task 0.1: Configure Project for Testing ✅
- [x] Add `[assembly: InternalsVisibleTo("PantheonWars.Tests")]` to PantheonWars project
- [x] Change command handler methods from `private` to `internal`
- [x] Ensure test project references all necessary dependencies

#### Task 0.2: Create Test Infrastructure ✅
- [x] Create `BlessingCommandsTests.cs` in `PantheonWars.Tests/Commands/`
- [x] Add required NuGet packages (Moq/NSubstitute, FluentAssertions, etc.)
- [x] Set up test class structure and namespaces

#### Task 0.3: Create Test Helpers ✅
- [x] Create helper method to generate mock `ICoreServerAPI`
- [x] Create helper method to generate mock `IBlessingRegistry`
- [x] Create helper method to generate mock `IPlayerReligionDataManager`
- [x] Create helper method to generate mock `IReligionManager`
- [x] Create helper method to generate mock `IBlessingEffectSystem`
- [x] Create helper method to generate mock `TextCommandCallingArgs` with player context
- [x] Create factory methods for test data (`PlayerReligionData`, `Religion`, `Blessing` objects)

---

### Phase 1: Constructor Tests ✅ COMPLETED

#### Task 1.1: Null Parameter Validation ✅
- [x] Test constructor throws `ArgumentNullException` when `sapi` is null (line 49-57)
- [x] Test constructor throws `ArgumentNullException` when `blessingRegistry` is null (line 60-68)
- [x] Test constructor throws `ArgumentNullException` when `playerReligionDataManager` is null (line 71-79)
- [x] Test constructor throws `ArgumentNullException` when `religionManager` is null (line 82-90)
- [x] Test constructor throws `ArgumentNullException` when `blessingEffectSystem` is null (line 93-101)

#### Task 1.2: Successful Construction ✅
- [x] Test successful construction with all valid dependencies (line 104-115)

---

### Phase 2: RegisterCommands Tests ✅

#### Task 2.1: Basic Registration ✅
- [x] Test `RegisterCommands()` executes without throwing exceptions (line 91-132)
- [x] Verify logger notification is called with correct message

#### Task 2.2: Command Structure Verification ✅
- [x] Verify main command is created with name "blessings" (line 176)
- [x] Verify command has correct description (line 177-178)
- [x] Verify `RequiresPlayer()` is configured (line 230-231)
- [x] Verify `RequiresPrivilege(Privilege.chat)` is configured (line 232-233)

#### Task 2.3: Subcommand Registration ✅
- [x] Verify "list" subcommand is registered with correct description (line 179)
- [x] Verify "player" subcommand is registered with correct description (line 180)
- [x] Verify "religion" subcommand is registered with correct description (line 181)
- [x] Verify "info" subcommand is registered with correct description and parameters (line 182)
- [x] Verify "tree" subcommand is registered with correct description and optional parameters (line 183)
- [x] Verify "unlock" subcommand is registered with correct description and parameters (line 184)
- [x] Verify "active" subcommand is registered with correct description (line 185)

---

### Phase 3: OnBlessingsList Tests ✅ COMPLETE (16/17 tests)

#### Task 3.1: Error Cases ✅ COMPLETE
- [x] Test returns error when player is null (BlessingCommandListTests.cs:110-123)
- [x] Test returns error when player has no deity (DeityType.None) (BlessingCommandListTests.cs:126-145)

#### Task 3.2: Success Cases ✅ COMPLETE (6/6 tests)
- [x] Test successful listing with valid player and deity (BlessingCommandListTests.cs:19-107)
- [x] Test displays both player and religion blessings sections (BlessingCommandListTests.cs:19-107)
- [x] Test shows unlocked status correctly for player blessings (BlessingCommandListTests.cs:19-107)
- [x] Test shows unlocked status correctly for religion blessings (BlessingCommandListTests.cs:221-279)
- [x] Test displays correct favor ranks for player blessings (BlessingCommandListTests.cs:282-325)
- [x] Test displays correct prestige ranks for religion blessings (BlessingCommandListTests.cs:328-380)

#### Task 3.3: Edge Cases ✅ COMPLETE (4/4 tests)
- [x] Test with empty player blessings list (BlessingCommandListTests.cs:148-180, 185-218)
- [x] Test with empty religion blessings list (BlessingCommandListTests.cs:148-180)
- [x] Test blessing ID and description display (BlessingCommandListTests.cs:383-427)
- [x] Test output formatting matches expected format (BlessingCommandListTests.cs:430-512 - verifies section ordering)

#### Task 3.4: Dependency Interactions ✅ COMPLETE
- [x] Verify `GetBlessingsForDeity()` is called for player blessings (implicit in all success tests)
- [x] Verify `GetBlessingsForDeity()` is called for religion blessings (implicit in all success tests)
- [x] Verify `GetOrCreatePlayerData()` is called (implicit in all tests)

**Note:** The "player in religion vs not in religion" test was determined to be redundant, as the error case is already covered and the success cases implicitly test religion membership.

---

### Phase 4: OnBlessingsPlayer Tests ✅ COMPLETE

#### Task 4.1: Error Cases ✅
- [x] Test returns error when player is null (BlessingCommandPlayerTests.cs:22-35)

#### Task 4.2: Success Cases ✅
- [x] Test returns info message when player has no unlocked blessings (BlessingCommandPlayerTests.cs:38-62)
- [x] Test displays unlocked player blessings correctly (BlessingCommandPlayerTests.cs:65-115)
- [x] Test shows blessing count in header (BlessingCommandPlayerTests.cs:118-154)
- [x] Test displays blessing names and categories (BlessingCommandPlayerTests.cs:157-205)
- [x] Test displays blessing descriptions (BlessingCommandPlayerTests.cs:208-248)

#### Task 4.3: Stat Modifier Display ✅
- [x] Test stat modifiers are shown when present (BlessingCommandPlayerTests.cs:65-115)
- [x] Test stat modifiers are formatted correctly (percentage) (BlessingCommandPlayerTests.cs:251-295)
- [x] Test blessings without stat modifiers display correctly (BlessingCommandPlayerTests.cs:298-339)
- [x] Test multiple stat modifiers per blessing (BlessingCommandPlayerTests.cs:342-383)

#### Task 4.4: Dependency Interactions ✅
- [x] Verify `GetActiveBlessings()` is called (implicit in all tests, explicit in line 382)
- [x] Verify only player blessings are returned (not religion blessings) (BlessingCommandPlayerTests.cs:386-423)

---

### Phase 5: OnBlessingsReligion Tests ✅ COMPLETE

#### Task 5.1: Error Cases ✅
- [x] Test returns error when player is null (BlessingCommandReligionTests.cs:20-33)
- [x] Test returns error when player has no religion (BlessingCommandReligionTests.cs:36-58)

#### Task 5.2: Success Cases ✅
- [x] Test returns info message when religion has no unlocked blessings (BlessingCommandReligionTests.cs:61-94)
- [x] Test displays religion name in header (BlessingCommandReligionTests.cs:96-138)
- [x] Test displays unlocked religion blessings correctly (BlessingCommandReligionTests.cs:140-193)
- [x] Test shows blessing count in header (BlessingCommandReligionTests.cs:195-238)
- [x] Test displays blessing names and categories (BlessingCommandReligionTests.cs:240-283)
- [x] Test displays blessing descriptions (BlessingCommandReligionTests.cs:285-331)

#### Task 5.3: Stat Modifier Display ✅
- [x] Test stat modifiers show "for all members" label (BlessingCommandReligionTests.cs:333-383)
- [x] Test stat modifiers are formatted correctly (BlessingCommandReligionTests.cs:385-435)
- [x] Test multiple stat modifiers per blessing (BlessingCommandReligionTests.cs:437-491)

#### Task 5.4: Dependency Interactions ✅
- [x] Verify `GetOrCreatePlayerData()` is called (implicit in all tests)
- [x] Verify `GetReligion()` is called (implicit in all tests)
- [x] Verify `GetActiveBlessings()` is called (implicit in all tests)
- [x] Verify only religion blessings are returned (not player blessings) (BlessingCommandReligionTests.cs:493-540)

---

### Phase 6: OnBlessingsInfo Tests

#### Task 6.1: Error Cases
- [ ] Test returns error with null blessingId parameter
- [ ] Test returns error with empty blessingId parameter
- [ ] Test returns error when blessing doesn't exist

#### Task 6.2: Player Blessing Info Display
- [ ] Test displays player blessing name in header
- [ ] Test displays blessing ID
- [ ] Test displays deity name
- [ ] Test displays blessing type (Player)
- [ ] Test displays category
- [ ] Test displays description
- [ ] Test displays required favor rank

#### Task 6.3: Religion Blessing Info Display
- [ ] Test displays religion blessing name in header
- [ ] Test displays required prestige rank (not favor rank)

#### Task 6.4: Prerequisites Display
- [ ] Test shows prerequisites section when present
- [ ] Test resolves prerequisite names from IDs
- [ ] Test handles missing prerequisite gracefully (shows ID if name unavailable)
- [ ] Test with multiple prerequisites

#### Task 6.5: Stat Modifiers Display
- [ ] Test shows stat modifiers section when present
- [ ] Test formats modifiers as percentages
- [ ] Test with multiple stat modifiers
- [ ] Test with no stat modifiers (section not shown)

#### Task 6.6: Special Effects Display
- [ ] Test shows special effects section when present
- [ ] Test displays all special effects
- [ ] Test with no special effects (section not shown)

#### Task 6.7: Dependency Interactions
- [ ] Verify `GetBlessing()` is called with correct blessingId
- [ ] Verify `GetBlessing()` is called for each prerequisite

---

### Phase 7: OnBlessingsTree Tests

#### Task 7.1: Error Cases
- [ ] Test returns error when player is null
- [ ] Test returns error when player has no deity

#### Task 7.2: Player Tree Display
- [ ] Test defaults to player tree when type parameter is omitted
- [ ] Test displays player tree when type="player"
- [ ] Test groups blessings by FavorRank
- [ ] Test shows checked status for unlocked blessings
- [ ] Test shows unchecked status for locked blessings
- [ ] Test skips ranks with no blessings

#### Task 7.3: Religion Tree Display
- [ ] Test displays religion tree when type="religion"
- [ ] Test groups blessings by PrestigeRank
- [ ] Test checks religion's unlocked blessings (not player's)
- [ ] Test skips ranks with no blessings

#### Task 7.4: Prerequisites Display
- [ ] Test displays prerequisite names inline
- [ ] Test multiple prerequisites are comma-separated
- [ ] Test blessings without prerequisites don't show "Requires:" line

#### Task 7.5: Edge Cases
- [ ] Test with player not in religion (for religion tree)
- [ ] Test case-insensitive type parameter ("PLAYER", "Religion", etc.)
- [ ] Test with empty blessing tree

#### Task 7.6: Dependency Interactions
- [ ] Verify `GetBlessingsForDeity()` is called with correct deity and kind
- [ ] Verify `GetOrCreatePlayerData()` is called
- [ ] Verify `GetReligion()` is called for religion tree

---

### Phase 8: OnBlessingsUnlock Tests

#### Task 8.1: Error Cases - General
- [ ] Test returns error when player is null
- [ ] Test returns error with null/empty blessingId parameter
- [ ] Test returns error when blessing doesn't exist

#### Task 8.2: Player Blessing Unlock - Error Cases
- [ ] Test returns error when player not in religion
- [ ] Test returns error when cannot unlock (prerequisites not met)
- [ ] Test returns error when already unlocked
- [ ] Test returns error when insufficient favor rank

#### Task 8.3: Player Blessing Unlock - Success Case
- [ ] Test successfully unlocks player blessing
- [ ] Test returns success message with blessing name
- [ ] Verify `CanUnlockBlessing()` is called
- [ ] Verify `UnlockPlayerBlessing()` is called
- [ ] Verify `RefreshPlayerBlessings()` is called

#### Task 8.4: Religion Blessing Unlock - Error Cases
- [ ] Test returns error when player not in religion
- [ ] Test returns error when player is not founder
- [ ] Test returns error when cannot unlock (prerequisites not met)
- [ ] Test returns error when insufficient prestige rank

#### Task 8.5: Religion Blessing Unlock - Success Case
- [ ] Test successfully unlocks religion blessing (founder)
- [ ] Test blessing is added to religion's unlocked blessings
- [ ] Test returns success message with blessing name
- [ ] Verify `CanUnlockBlessing()` is called
- [ ] Verify `RefreshReligionBlessings()` is called

#### Task 8.6: Religion Blessing Unlock - Notifications
- [ ] Test all religion members receive notification
- [ ] Test notification contains correct blessing name
- [ ] Test notification uses correct chat type (Notification)
- [ ] Test handles offline members gracefully

#### Task 8.7: Edge Cases
- [ ] Test unlock with exactly minimum required rank
- [ ] Test unlock with rank higher than required
- [ ] Test concurrent unlock attempts (if applicable)

---

### Phase 9: OnBlessingsActive Tests

#### Task 9.1: Error Cases
- [ ] Test returns error when player is null

#### Task 9.2: Display - No Active Blessings
- [ ] Test displays "None" when no player blessings
- [ ] Test displays "None" when no religion blessings
- [ ] Test displays "No active modifiers" when no stat modifiers

#### Task 9.3: Display - Player Blessings Only
- [ ] Test displays player blessings section with count
- [ ] Test lists all active player blessings by name
- [ ] Test religion section shows "None"

#### Task 9.4: Display - Religion Blessings Only
- [ ] Test displays religion blessings section with count
- [ ] Test lists all active religion blessings by name
- [ ] Test player section shows "None"

#### Task 9.5: Display - Both Blessing Types
- [ ] Test displays both player and religion blessings
- [ ] Test correct counts for each section

#### Task 9.6: Combined Stat Modifiers
- [ ] Test displays combined modifiers from all active blessings
- [ ] Test formatting matches `FormatStatModifiers()` output
- [ ] Test with overlapping modifiers from player and religion blessings

#### Task 9.7: Dependency Interactions
- [ ] Verify `GetActiveBlessings()` is called
- [ ] Verify `GetCombinedStatModifiers()` is called
- [ ] Verify `FormatStatModifiers()` is called

---

### Phase 10: Integration & Edge Cases

#### Task 10.1: Large Data Sets
- [ ] Test with 50+ blessings in list
- [ ] Test with 20+ unlocked blessings
- [ ] Test with deep prerequisite chains

#### Task 10.2: Special Characters & Formatting
- [ ] Test blessing names with special characters
- [ ] Test blessing descriptions with special characters
- [ ] Test very long blessing names
- [ ] Test very long descriptions
- [ ] Test Unicode characters in blessing data

#### Task 10.3: Data Integrity
- [ ] Test with malformed PlayerReligionData
- [ ] Test with missing religion data
- [ ] Test with inconsistent unlock states
- [ ] Test with circular prerequisite references

#### Task 10.4: Boundary Conditions
- [ ] Test with minimum favor/prestige ranks
- [ ] Test with maximum favor/prestige ranks
- [ ] Test with stat modifiers at 0%
- [ ] Test with very large stat modifiers (999%)

---

### Phase 11: Code Coverage & Analysis

#### Task 11.1: Coverage Measurement
- [ ] Run code coverage tool (Coverlet/dotCover)
- [ ] Generate coverage report
- [ ] Verify >80% line coverage
- [ ] Verify >75% branch coverage

#### Task 11.2: Coverage Gaps
- [ ] Identify uncovered code paths
- [ ] Add tests for uncovered branches
- [ ] Document intentionally untested code (if any)

#### Task 11.3: Code Review
- [ ] Review test code for clarity and maintainability
- [ ] Ensure test names clearly describe what they test
- [ ] Verify all assertions have meaningful failure messages
- [ ] Check for test code duplication (refactor if needed)

---

## Testing Checklist

### Before Starting
- [ ] Review BlessingCommands.cs implementation thoroughly
- [ ] Understand all dependencies and their interfaces
- [ ] Set up test project with all required tools
- [ ] Create mock/helper infrastructure

### During Development
- [ ] Follow AAA pattern (Arrange, Act, Assert)
- [ ] Write descriptive test method names
- [ ] Test one thing per test method
- [ ] Use meaningful assertion messages
- [ ] Mock only what's necessary
- [ ] Verify all mock interactions

### After Completion
- [ ] Run all tests and verify they pass
- [ ] Check code coverage meets targets
- [ ] Review for test maintainability
- [ ] Document any known limitations
- [ ] Update this document with findings

## Success Criteria

- ✅ All 60-80 tests pass consistently
- ✅ Code coverage >80% for BlessingCommands.cs
- ✅ All command handlers have comprehensive tests
- ✅ Error cases and edge cases are covered
- ✅ Tests run in <5 seconds
- ✅ Tests are isolated and don't depend on execution order
- ✅ Mock interactions are verified
- ✅ Test code is clean and maintainable

## Notes & Considerations

1. **TextCommandResult:** May need to create test utilities to inspect result status and messages
2. **StringBuilder Output:** Tests will need to parse or match formatted string output
3. **Async Operations:** Currently none, but be aware if added in future
4. **Chat System Integration:** Notification sending may require special mock setup
5. **Enum Iteration:** Tests for tree display iterate over all rank enums - ensure comprehensive coverage

## Related Documentation

- See existing test: `PantheonWars.Tests/Data/PlayerReligionDataTests.cs` for patterns
- VintageStory Command API documentation
- BlessingRegistry implementation for mock data generation
- Religion and PlayerReligionData models for test data setup

---

**Document Version:** 1.5
**Last Updated:** 2025-10-30
**Status:** In Progress - Phase 5 COMPLETE, Phase 6 next

---

## Current Test Coverage Summary

**Total Tests Implemented:** 41 / ~70 estimated (58.6%)
**All Tests Passing:** ✅ Yes (41/41)

### Test Files:
- **BlessingCommandsTests.cs**: 9 tests (constructor + RegisterCommands - COMPLETE)
- **BlessingCommandListTests.cs**: 10 tests (OnBlessingsList command - COMPLETE)
- **BlessingCommandPlayerTests.cs**: 10 tests (OnBlessingsPlayer command - COMPLETE)
- **BlessingCommandReligionTests.cs**: 12 tests (OnBlessingsReligion command - COMPLETE)
- **PlayerReligionDataTests.cs**: 13 tests (supporting data structure - not counted in totals)

### Progress by Phase:
- ✅ **Phase 0:** Preparation & Setup - COMPLETE (100%)
- ✅ **Phase 1:** Constructor Tests - COMPLETE (6/6 tests - 100%)
- ✅ **Phase 2:** RegisterCommands Tests - COMPLETE (9/9 tests - 100%)
- ✅ **Phase 3:** OnBlessingsList Tests - COMPLETE (10/10 tests - 100%)
- ✅ **Phase 4:** OnBlessingsPlayer Tests - COMPLETE (10/10 tests - 100%)
- ✅ **Phase 5:** OnBlessingsReligion Tests - COMPLETE (12/12 tests - 100%)
- ❌ **Phase 6:** OnBlessingsInfo Tests - NOT STARTED (0/21 tests)
- ❌ **Phase 7:** OnBlessingsTree Tests - NOT STARTED (0/16 tests)
- ❌ **Phase 8:** OnBlessingsUnlock Tests - NOT STARTED (0/17 tests)
- ❌ **Phase 9:** OnBlessingsActive Tests - NOT STARTED (0/11 tests)
- ❌ **Phase 10:** Integration & Edge Cases - NOT STARTED (0/17 tests)
- ❌ **Phase 11:** Code Coverage & Analysis - NOT STARTED

### Command Handler Coverage:
- ✅ **OnBlessingsList** - COMPLETE coverage (error cases, success cases, edge cases, formatting)
- ✅ **OnBlessingsPlayer** - COMPLETE coverage (error cases, success cases, stat modifiers, dependency verification)
- ✅ **OnBlessingsReligion** - COMPLETE coverage (error cases, success cases, stat modifiers, "for all members" label)
- ❌ **OnBlessingsInfo** - No coverage
- ❌ **OnBlessingsTree** - No coverage
- ❌ **OnBlessingsUnlock** - No coverage
- ❌ **OnBlessingsActive** - No coverage

### Recent Completion Summary:

#### Phase 5 (OnBlessingsReligion) - COMPLETED ✅
Implemented 9 comprehensive tests covering:
1. ✅ Error handling for null player
2. ✅ Error handling for player with no religion
3. ✅ Info message when religion has no unlocked blessings
4. ✅ Religion name displayed in header
5. ✅ Unlocked religion blessings displayed correctly
6. ✅ Blessing count shown in header
7. ✅ Blessing names and categories displayed
8. ✅ Blessing descriptions displayed
9. ✅ Stat modifiers show "for all members" label
10. ✅ Stat modifiers formatted as percentages
11. ✅ Multiple stat modifiers per blessing displayed
12. ✅ Only religion blessings returned (not player blessings)

**Key Achievement:** Phase 5 validates that religion-wide blessings are properly displayed with the distinctive "for all members" label, differentiating them from player-specific blessings.

### Next Steps:
1. **PRIORITY**: Implement Phase 6 (OnBlessingsInfo) - 21 tests for detailed blessing info command
2. Implement Phase 7 (OnBlessingsTree) - 16 tests for blessing tree display command
3. Implement Phase 8 (OnBlessingsUnlock) - 17 tests for blessing unlock command
4. Implement Phase 9 (OnBlessingsActive) - 11 tests for active blessings display command
5. Implement Phase 10 (Integration & Edge Cases) - 17 tests
6. Run code coverage analysis (Phase 11)