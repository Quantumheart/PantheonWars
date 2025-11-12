using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Data;
using PantheonWars.Models.Enum;
using PantheonWars.Systems;
using PantheonWars.Systems.Interfaces;
using PantheonWars.Tests.Helpers;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Xunit;

namespace PantheonWars.Tests.Systems;

/// <summary>
///     Unit tests for PlayerReligionDataManager
///     Tests player data management, favor tracking, and religion membership
/// </summary>
[ExcludeFromCodeCoverage]
public class PlayerReligionDataManagerTests
{
    private readonly Mock<ICoreServerAPI> _mockAPI;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IReligionManager> _mockReligionManager;
    private readonly PlayerReligionDataManager _dataManager;

    public PlayerReligionDataManagerTests()
    {
        _mockAPI = TestFixtures.CreateMockServerAPI();
        _mockLogger = new Mock<ILogger>();
        _mockAPI.Setup(a => a.Logger).Returns(_mockLogger.Object);

        _mockReligionManager = new Mock<IReligionManager>();

        _dataManager = new PlayerReligionDataManager(_mockAPI.Object, _mockReligionManager.Object);
    }

    #region Initialization Tests

    [Fact]
    public void Initialize_RegistersEventHandlers()
    {
        // Arrange
        var mockEventAPI = new Mock<IServerEventAPI>();
        _mockAPI.Setup(a => a.Event).Returns(mockEventAPI.Object);

        // Act
        _dataManager.Initialize();

        // Assert
        mockEventAPI.VerifyAdd(e => e.PlayerJoin += It.IsAny<PlayerDelegate>(), Times.Once());
        mockEventAPI.VerifyAdd(e => e.PlayerDisconnect += It.IsAny<PlayerDelegate>(), Times.Once());
        mockEventAPI.VerifyAdd(e => e.SaveGameLoaded += It.IsAny<Action>(), Times.Once());
        mockEventAPI.VerifyAdd(e => e.GameWorldSave += It.IsAny<Action>(), Times.Once());
    }

    [Fact]
    public void Initialize_LogsNotification()
    {
        // Arrange
        var mockEventAPI = new Mock<IServerEventAPI>();
        _mockAPI.Setup(a => a.Event).Returns(mockEventAPI.Object);

        // Act
        _dataManager.Initialize();

        // Assert
        _mockLogger.Verify(
            l => l.Notification(It.Is<string>(s => s.Contains("Initializing") && s.Contains("Player Religion Data Manager"))),
            Times.Once()
        );
    }

    #endregion

    #region GetOrCreatePlayerData Tests

    [Fact]
    public void GetOrCreatePlayerData_CreatesNewData_WhenNotExists()
    {
        // Act
        var data = _dataManager.GetOrCreatePlayerData("new-player-uid");

        // Assert
        Assert.NotNull(data);
        Assert.Equal("new-player-uid", data.PlayerUID);
        Assert.Equal(DeityType.None, data.ActiveDeity);
        Assert.Equal(0, data.Favor);
    }

    [Fact]
    public void GetOrCreatePlayerData_ReturnsExistingData_WhenExists()
    {
        // Arrange
        var firstCall = _dataManager.GetOrCreatePlayerData("player-uid");
        firstCall.Favor = 100;

        // Act
        var secondCall = _dataManager.GetOrCreatePlayerData("player-uid");

        // Assert
        Assert.Same(firstCall, secondCall);
        Assert.Equal(100, secondCall.Favor);
    }

    [Fact]
    public void GetOrCreatePlayerData_LogsDebugOnCreation()
    {
        // Act
        _dataManager.GetOrCreatePlayerData("new-player-uid");

        // Assert
        _mockLogger.Verify(
            l => l.Debug(It.Is<string>(s => s.Contains("Created new player religion data") && s.Contains("new-player-uid"))),
            Times.Once()
        );
    }

    #endregion

    #region AddFavor Tests

    [Fact]
    public void AddFavor_IncreasesFavorAmount()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.Favor = 50;

        // Act
        _dataManager.AddFavor("player-uid", 25, "Test reason");

        // Assert
        Assert.Equal(75, data.Favor);
        Assert.Equal(25, data.TotalFavorEarned); // Total also increased
    }

    [Fact]
    public void AddFavor_WithReason_LogsDebugMessage()
    {
        // Act
        _dataManager.AddFavor("player-uid", 10, "PvP kill");

        // Assert
        _mockLogger.Verify(
            l => l.Debug(It.Is<string>(s => s.Contains("player-uid") && s.Contains("10") && s.Contains("PvP kill"))),
            Times.Once()
        );
    }

    [Fact]
    public void AddFavor_ThatCausesRankUp_FiresEvent()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.Favor = 90; // Close to Disciple threshold (100)

        var eventFired = false;
        _dataManager.OnPlayerDataChanged += (playerUID) => eventFired = true;

        var mockWorld = new Mock<IServerWorldAccessor>();
        _mockAPI.Setup(a => a.World).Returns(mockWorld.Object);

        // Act
        _dataManager.AddFavor("player-uid", 520); // Should rank up

        // Assert
        Assert.Equal(FavorRank.Disciple, data.FavorRank);
        Assert.True(eventFired);
    }

    [Fact]
    public void AddFavor_FiresOnPlayerDataChangedEvent()
    {
        // Arrange
        string? firedPlayerUID = null;
        _dataManager.OnPlayerDataChanged += (playerUID) => firedPlayerUID = playerUID;

        // Act
        _dataManager.AddFavor("player-uid", 10);

        // Assert
        Assert.Equal("player-uid", firedPlayerUID);
    }

    #endregion

    #region AddFractionalFavor Tests

    [Fact]
    public void AddFractionalFavor_AccumulatesCorrectly()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");

        // Act
        _dataManager.AddFractionalFavor("player-uid", 0.3f);
        _dataManager.AddFractionalFavor("player-uid", 0.3f);
        _dataManager.AddFractionalFavor("player-uid", 0.5f); // Should award 1 favor

        // Assert
        Assert.Equal(1, data.Favor);
        Assert.True(data.AccumulatedFractionalFavor < 1.0f);
    }

    [Fact]
    public void AddFractionalFavor_WhenAwardingFullFavor_FiresEvent()
    {
        // Arrange
        var eventFired = false;
        _dataManager.OnPlayerDataChanged += (_) => eventFired = true;

        // Act - Add enough fractional favor to award 1 full favor
        _dataManager.AddFractionalFavor("player-uid", 1.2f);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void AddFractionalFavor_WithoutAwardingFullFavor_DoesNotFireEvent()
    {
        // Arrange
        var eventFired = false;
        _dataManager.OnPlayerDataChanged += (_) => eventFired = true;

        // Act - Add fractional favor that doesn't reach 1.0
        _dataManager.AddFractionalFavor("player-uid", 0.3f);

        // Assert
        Assert.False(eventFired);
    }

    #endregion

    #region RemoveFavor Tests

    [Fact]
    public void RemoveFavor_DecreasesFavorAmount()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.Favor = 50;

        // Act
        var success = _dataManager.RemoveFavor("player-uid", 20, "Blessing unlock");

        // Assert
        Assert.True(success);
        Assert.Equal(30, data.Favor);
    }

    [Fact]
    public void RemoveFavor_WithInsufficientFavor_ReturnsFalse()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.Favor = 10;

        // Act
        var success = _dataManager.RemoveFavor("player-uid", 20);

        // Assert
        Assert.False(success);
        Assert.Equal(10, data.Favor); // Unchanged
    }

    [Fact]
    public void RemoveFavor_OnSuccess_FiresEvent()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.Favor = 50;

        var eventFired = false;
        _dataManager.OnPlayerDataChanged += (_) => eventFired = true;

        // Act
        _dataManager.RemoveFavor("player-uid", 10);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void RemoveFavor_WithReason_LogsDebugMessage()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.Favor = 50;

        // Act
        _dataManager.RemoveFavor("player-uid", 10, "Blessing unlock");

        // Assert
        _mockLogger.Verify(
            l => l.Debug(It.Is<string>(s => s.Contains("player-uid") && s.Contains("spent 10 favor") && s.Contains("Blessing unlock"))),
            Times.Once()
        );
    }

    #endregion

    #region UpdateFavorRank Tests

    [Fact]
    public void UpdateFavorRank_UpdatesRankCorrectly()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.TotalFavorEarned = 500; // Should be Disciple rank

        // Act
        _dataManager.UpdateFavorRank("player-uid");

        // Assert
        Assert.Equal(FavorRank.Disciple, data.FavorRank);
    }

    [Fact]
    public void UpdateFavorRank_WithRankChange_LogsNotification()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.TotalFavorEarned = 500;
        data.FavorRank = FavorRank.Initiate;

        // Act
        _dataManager.UpdateFavorRank("player-uid");

        // Assert
        _mockLogger.Verify(
            l => l.Notification(It.Is<string>(s => s.Contains("rank changed") && s.Contains("Initiate") && s.Contains("Disciple"))),
            Times.Once()
        );
    }

    #endregion

    #region UnlockPlayerBlessing Tests

    [Fact]
    public void UnlockPlayerBlessing_UnlocksNewBlessing_ReturnsTrue()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");

        // Act
        var result = _dataManager.UnlockPlayerBlessing("player-uid", "blessing_id");

        // Assert
        Assert.True(result);
        Assert.True(data.IsBlessingUnlocked("blessing_id"));
    }

    [Fact]
    public void UnlockPlayerBlessing_AlreadyUnlocked_ReturnsFalse()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.UnlockBlessing("blessing_id");

        // Act
        var result = _dataManager.UnlockPlayerBlessing("player-uid", "blessing_id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UnlockPlayerBlessing_LogsNotification()
    {
        // Act
        _dataManager.UnlockPlayerBlessing("player-uid", "blessing_id");

        // Assert
        _mockLogger.Verify(
            l => l.Notification(It.Is<string>(s => s.Contains("player-uid") && s.Contains("unlocked blessing") && s.Contains("blessing_id"))),
            Times.Once()
        );
    }

    #endregion

    #region GetActivePlayerBlessings Tests

    [Fact]
    public void GetActivePlayerBlessings_ReturnsUnlockedBlessings()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.UnlockBlessing("blessing1");
        data.UnlockBlessing("blessing2");

        // Act
        var blessings = _dataManager.GetActivePlayerBlessings("player-uid");

        // Assert
        Assert.Equal(2, blessings.Count);
        Assert.Contains("blessing1", blessings);
        Assert.Contains("blessing2", blessings);
    }

    [Fact]
    public void GetActivePlayerBlessings_WithNoUnlockedBlessings_ReturnsEmptyList()
    {
        // Act
        var blessings = _dataManager.GetActivePlayerBlessings("player-uid");

        // Assert
        Assert.Empty(blessings);
    }

    #endregion

    #region JoinReligion Tests

    [Fact]
    public void JoinReligion_SetsReligionAndDeity()
    {
        // Arrange
        var religion = TestFixtures.CreateTestReligion("religion-uid", "Test Religion", DeityType.Khoras);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var data = _dataManager.GetOrCreatePlayerData("player-uid");

        // Act
        _dataManager.JoinReligion("player-uid", "religion-uid");

        // Assert
        Assert.Equal("religion-uid", data.ReligionUID);
        Assert.Equal(DeityType.Khoras, data.ActiveDeity);
        Assert.NotNull(data.LastReligionSwitch);
    }

    [Fact]
    public void JoinReligion_AddsPlayerToReligion()
    {
        // Arrange
        var religion = TestFixtures.CreateTestReligion("religion-uid", "Test Religion", DeityType.Khoras);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        // Act
        _dataManager.JoinReligion("player-uid", "religion-uid");

        // Assert
        _mockReligionManager.Verify(m => m.AddMember("religion-uid", "player-uid"), Times.Once());
    }

    [Fact]
    public void JoinReligion_WhenAlreadyInReligion_LeavesCurrentFirst()
    {
        // Arrange
        var oldReligion = TestFixtures.CreateTestReligion("old-religion-uid", "Old Religion", DeityType.Lysa);
        var newReligion = TestFixtures.CreateTestReligion("new-religion-uid", "New Religion", DeityType.Khoras);

        _mockReligionManager.Setup(m => m.GetReligion("old-religion-uid")).Returns(oldReligion);
        _mockReligionManager.Setup(m => m.GetReligion("new-religion-uid")).Returns(newReligion);

        var mockWorld = new Mock<IServerWorldAccessor>();
        _mockAPI.Setup(a => a.World).Returns(mockWorld.Object);
        _mockAPI.Setup(a => a.World.PlayerByUid("player-uid")).Returns(new TestPlayer());

        int count =0;
        _dataManager.OnPlayerLeavesReligion += (player, uid) => count++;

        // Join first religion
        _dataManager.JoinReligion("player-uid", "old-religion-uid");

        // Act - Join second religion
        _dataManager.JoinReligion("player-uid", "new-religion-uid");

        // Assert
        _mockReligionManager.Verify(m => m.RemoveMember("old-religion-uid", "player-uid"), Times.Once());
        _mockReligionManager.Verify(m => m.AddMember("new-religion-uid", "player-uid"), Times.Once());
    }

    [Fact]
    public void JoinReligion_WithInvalidReligion_LogsError()
    {
        // Arrange
        _mockReligionManager.Setup(m => m.GetReligion("invalid-uid")).Returns((ReligionData)null!);

        // Act
        _dataManager.JoinReligion("player-uid", "invalid-uid");

        // Assert
        _mockLogger.Verify(
            l => l.Error(It.Is<string>(s => s.Contains("Cannot join non-existent religion"))),
            Times.Once()
        );
    }

    #endregion

    #region LeaveReligion Tests

    [Fact]
    public void LeaveReligion_ClearsPlayerData()
    {
        // Arrange
        var religion = TestFixtures.CreateTestReligion("religion-uid", "Test Religion", DeityType.Khoras);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = new Mock<IServerWorldAccessor>();
        _mockAPI.Setup(a => a.World).Returns(mockWorld.Object);

        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        _dataManager.JoinReligion("player-uid", "religion-uid");
        data.Favor = 100;

        // Act
        _dataManager.LeaveReligion("player-uid");

        // Assert
        Assert.Null(data.ReligionUID);
        Assert.Equal(DeityType.None, data.ActiveDeity);
        Assert.Equal(0, data.Favor);
        Assert.Equal(0, data.TotalFavorEarned);
        Assert.Equal(FavorRank.Initiate, data.FavorRank);
    }

    [Fact]
    public void LeaveReligion_RemovesPlayerFromReligion()
    {
        // Arrange
        var religion = TestFixtures.CreateTestReligion("religion-uid", "Test Religion", DeityType.Khoras);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = new Mock<IServerWorldAccessor>();
        _mockAPI.Setup(a => a.World).Returns(mockWorld.Object);

        _dataManager.JoinReligion("player-uid", "religion-uid");

        // Act
        _dataManager.LeaveReligion("player-uid");

        // Assert
        _mockReligionManager.Verify(m => m.RemoveMember("religion-uid", "player-uid"), Times.Once());
    }

    [Fact]
    public void LeaveReligion_WithNoReligion_DoesNothing()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        Assert.Null(data.ReligionUID);

        // Act & Assert - Should not throw
        _dataManager.LeaveReligion("player-uid");
    }

    #endregion

    #region CanSwitchReligion Tests

    [Fact]
    public void CanSwitchReligion_FirstTime_ReturnsTrue()
    {
        // Arrange
        _dataManager.GetOrCreatePlayerData("player-uid");

        // Act
        var canSwitch = _dataManager.CanSwitchReligion("player-uid");

        // Assert
        Assert.True(canSwitch);
    }

    [Fact]
    public void CanSwitchReligion_WithinCooldown_ReturnsFalse()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.LastReligionSwitch = DateTime.UtcNow.AddDays(-3); // 3 days ago (cooldown is 7 days)

        // Act
        var canSwitch = _dataManager.CanSwitchReligion("player-uid");

        // Assert
        Assert.False(canSwitch);
    }

    [Fact]
    public void CanSwitchReligion_AfterCooldown_ReturnsTrue()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.LastReligionSwitch = DateTime.UtcNow.AddDays(-8); // 8 days ago (cooldown is 7 days)

        // Act
        var canSwitch = _dataManager.CanSwitchReligion("player-uid");

        // Assert
        Assert.True(canSwitch);
    }

    #endregion

    #region GetSwitchCooldownRemaining Tests

    [Fact]
    public void GetSwitchCooldownRemaining_NeverSwitched_ReturnsNull()
    {
        // Arrange
        _dataManager.GetOrCreatePlayerData("player-uid");

        // Act
        var cooldown = _dataManager.GetSwitchCooldownRemaining("player-uid");

        // Assert
        Assert.Null(cooldown);
    }

    [Fact]
    public void GetSwitchCooldownRemaining_WithinCooldown_ReturnsTimeRemaining()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.LastReligionSwitch = DateTime.UtcNow.AddDays(-3); // 3 days ago

        // Act
        var cooldown = _dataManager.GetSwitchCooldownRemaining("player-uid");

        // Assert
        Assert.NotNull(cooldown);
        Assert.True(cooldown.Value.TotalDays > 3); // More than 3 days remaining
        Assert.True(cooldown.Value.TotalDays < 5); // Less than 5 days remaining
    }

    [Fact]
    public void GetSwitchCooldownRemaining_AfterCooldown_ReturnsZero()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.LastReligionSwitch = DateTime.UtcNow.AddDays(-10); // 10 days ago

        // Act
        var cooldown = _dataManager.GetSwitchCooldownRemaining("player-uid");

        // Assert
        Assert.Equal(TimeSpan.Zero, cooldown);
    }

    #endregion

    #region HandleReligionSwitch Tests

    [Fact]
    public void HandleReligionSwitch_AppliesPenalty()
    {
        // Arrange
        var data = _dataManager.GetOrCreatePlayerData("player-uid");
        data.Favor = 100;
        data.UnlockBlessing("blessing1");

        // Act
        _dataManager.HandleReligionSwitch("player-uid");

        // Assert
        Assert.Equal(0, data.Favor);
        Assert.Empty(data.UnlockedBlessings.Where(kvp => kvp.Value)); // All blessings locked
    }

    [Fact]
    public void HandleReligionSwitch_LogsNotification()
    {
        // Act
        _dataManager.HandleReligionSwitch("player-uid");

        // Assert
        _mockLogger.Verify(
            l => l.Notification(It.Is<string>(s => s.Contains("Applying religion switch penalty") && s.Contains("player-uid"))),
            Times.Once()
        );
    }

    #endregion
}


internal class TestPlayer : IPlayer
{
    public PlayerGroupMembership[] GetGroups()
    {
        throw new NotImplementedException();
    }

    public PlayerGroupMembership GetGroup(int groupId)
    {
        throw new NotImplementedException();
    }

    public bool HasPrivilege(string privilegeCode)
    {
        throw new NotImplementedException();
    }

    public IPlayerRole Role { get; set; }
    public PlayerGroupMembership[] Groups { get; }
    public List<Entitlement> Entitlements { get; }
    public BlockSelection CurrentBlockSelection { get; }
    public EntitySelection CurrentEntitySelection { get; }
    public string PlayerName { get; }
    public string PlayerUID { get; } = "player-uid";
    public int ClientId { get; }
    public EntityPlayer Entity { get; }
    public IWorldPlayerData WorldData { get; }
    public IPlayerInventoryManager InventoryManager { get; }
    public string[] Privileges { get; }
    public bool ImmersiveFpMode { get; }
}