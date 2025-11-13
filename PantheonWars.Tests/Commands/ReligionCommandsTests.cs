using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Data;
using PantheonWars.Models.Enum;
using PantheonWars.Network;
using PantheonWars.Systems;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Xunit;

namespace PantheonWars.Tests.Commands;

/// <summary>
///     Tests for ReligionCommands class
/// </summary>
public class ReligionCommandsTests
{
    private readonly Mock<IPlayerReligionDataManager> _mockPlayerDataManager;
    private readonly Mock<ReligionManager> _mockReligionManager;
    private readonly Mock<ICoreServerAPI> _mockSapi;
    private readonly Mock<IServerNetworkChannel> _mockServerChannel;
    private readonly ReligionCommands _commands;

    public ReligionCommandsTests()
    {
        _mockSapi = TestFixtures.CreateMockServerAPI();
        _mockReligionManager = new Mock<ReligionManager>(_mockSapi.Object);
        _mockPlayerDataManager = new Mock<IPlayerReligionDataManager>();
        _mockServerChannel = new Mock<IServerNetworkChannel>();

        _commands = new ReligionCommands(
            _mockSapi.Object,
            _mockReligionManager.Object,
            _mockPlayerDataManager.Object,
            _mockServerChannel.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var commands = new ReligionCommands(
            _mockSapi.Object,
            _mockReligionManager.Object,
            _mockPlayerDataManager.Object,
            _mockServerChannel.Object
        );

        // Assert
        Assert.NotNull(commands);
    }

    [Fact]
    public void Constructor_WithNullServerChannel_CreatesInstance()
    {
        // Arrange & Act
        var commands = new ReligionCommands(
            _mockSapi.Object,
            _mockReligionManager.Object,
            _mockPlayerDataManager.Object,
            null
        );

        // Assert
        Assert.NotNull(commands);
    }

    #endregion

    #region RegisterCommands Tests

    [Fact]
    public void RegisterCommands_RegistersAllSubCommands()
    {
        // Act
        _commands.RegisterCommands();

        // Assert - Verify the main command was created
        var mockChatCommands = Mock.Get(_mockSapi.Object.ChatCommands);
        mockChatCommands.Verify(c => c.Create("religion"), Times.Once);

        // Verify notification was logged
        var mockLogger = Mock.Get(_mockSapi.Object.Logger);
        mockLogger.Verify(l => l.Notification("[PantheonWars] Religion commands registered"), Times.Once);
    }

    #endregion

    #region OnCreateReligion Tests

    [Fact]
    public void OnCreateReligion_WithPlayerAlreadyInReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "New Religion", "Khoras", "public");

        var playerData = new PlayerReligionData { ReligionUID = "existing-religion" };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokePrivateMethod("OnCreateReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("already in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnCreateReligion_WithInvalidDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "New Religion", "InvalidDeity", "public");

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokePrivateMethod("OnCreateReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("Invalid deity", result.StatusMessage);
    }

    [Fact]
    public void OnCreateReligion_WithExistingName_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Existing", "Khoras", "public");

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        var existingReligion = new ReligionData("Existing", DeityType.Khoras, "other-player", true);
        _mockReligionManager.Setup(m => m.GetReligionByName("Existing")).Returns(existingReligion);

        // Act
        var result = InvokePrivateMethod("OnCreateReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("already exists", result.StatusMessage);
    }

    [Fact]
    public void OnCreateReligion_WithValidParameters_CreatesReligionAndAutoJoins()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "New Religion", "Khoras", "public");

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligionByName("New Religion")).Returns((ReligionData?)null);

        var newReligion = new ReligionData("New Religion", DeityType.Khoras, "test-player", true)
        {
            ReligionUID = "new-religion-uid"
        };
        _mockReligionManager.Setup(m => m.CreateReligion("New Religion", DeityType.Khoras, "test-player", true))
            .Returns(newReligion);

        // Act
        var result = InvokePrivateMethod("OnCreateReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("New Religion", result.StatusMessage);
        Assert.Contains("created", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
        _mockPlayerDataManager.Verify(m => m.JoinReligion("test-player", "new-religion-uid"), Times.Once);
    }

    [Fact]
    public void OnCreateReligion_WithPrivateVisibility_CreatesPrivateReligion()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Secret Religion", "Lunara", "private");

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligionByName("Secret Religion")).Returns((ReligionData?)null);

        var newReligion = new ReligionData("Secret Religion", DeityType.Lunara, "test-player", false)
        {
            ReligionUID = "secret-religion-uid"
        };
        _mockReligionManager.Setup(m => m.CreateReligion("Secret Religion", DeityType.Lunara, "test-player", false))
            .Returns(newReligion);

        // Act
        var result = InvokePrivateMethod("OnCreateReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        _mockReligionManager.Verify(m => m.CreateReligion("Secret Religion", DeityType.Lunara, "test-player", false), Times.Once);
    }

    #endregion

    #region OnJoinReligion Tests

    [Fact]
    public void OnJoinReligion_WhenCannotSwitch_ReturnsErrorWithCooldown()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Target Religion");

        _mockPlayerDataManager.Setup(m => m.CanSwitchReligion("test-player")).Returns(false);
        _mockPlayerDataManager.Setup(m => m.GetSwitchCooldownRemaining("test-player"))
            .Returns(TimeSpan.FromDays(5).Add(TimeSpan.FromHours(3)));

        // Act
        var result = InvokePrivateMethod("OnJoinReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("must wait 5 days", result.StatusMessage);
        Assert.Contains("3 hours", result.StatusMessage);
    }

    [Fact]
    public void OnJoinReligion_WithNonExistentReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "NonExistent");

        _mockPlayerDataManager.Setup(m => m.CanSwitchReligion("test-player")).Returns(true);
        _mockReligionManager.Setup(m => m.GetReligionByName("NonExistent")).Returns((ReligionData?)null);

        // Act
        var result = InvokePrivateMethod("OnJoinReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("NonExistent", result.StatusMessage);
        Assert.Contains("not found", result.StatusMessage);
    }

    [Fact]
    public void OnJoinReligion_WhenCannotJoinPrivateReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Private Religion");

        var religion = new ReligionData("Private Religion", DeityType.Khoras, "founder-uid", false)
        {
            ReligionUID = "religion-uid"
        };

        _mockPlayerDataManager.Setup(m => m.CanSwitchReligion("test-player")).Returns(true);
        _mockReligionManager.Setup(m => m.GetReligionByName("Private Religion")).Returns(religion);
        _mockReligionManager.Setup(m => m.CanJoinReligion("religion-uid", "test-player")).Returns(false);

        // Act
        var result = InvokePrivateMethod("OnJoinReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("private", result.StatusMessage);
        Assert.Contains("not been invited", result.StatusMessage);
    }

    [Fact]
    public void OnJoinReligion_WithValidReligion_JoinsAndRemovesInvitation()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Target Religion");

        var playerData = new PlayerReligionData { ReligionUID = null };
        var religion = new ReligionData("Target Religion", DeityType.Khoras, "founder-uid", true)
        {
            ReligionUID = "religion-uid"
        };

        _mockPlayerDataManager.Setup(m => m.CanSwitchReligion("test-player")).Returns(true);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligionByName("Target Religion")).Returns(religion);
        _mockReligionManager.Setup(m => m.CanJoinReligion("religion-uid", "test-player")).Returns(true);

        // Act
        var result = InvokePrivateMethod("OnJoinReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("joined Target Religion", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
        _mockPlayerDataManager.Verify(m => m.JoinReligion("test-player", "religion-uid"), Times.Once);
        _mockReligionManager.Verify(m => m.RemoveInvitation("test-player", "religion-uid"), Times.Once);
    }

    [Fact]
    public void OnJoinReligion_WhenSwitchingFromExisting_AppliesPenalty()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "New Religion");

        var playerData = new PlayerReligionData { ReligionUID = "old-religion" };
        var religion = new ReligionData("New Religion", DeityType.Lunara, "founder-uid", true)
        {
            ReligionUID = "new-religion-uid"
        };

        _mockPlayerDataManager.Setup(m => m.CanSwitchReligion("test-player")).Returns(true);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligionByName("New Religion")).Returns(religion);
        _mockReligionManager.Setup(m => m.CanJoinReligion("new-religion-uid", "test-player")).Returns(true);

        // Act
        var result = InvokePrivateMethod("OnJoinReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        _mockPlayerDataManager.Verify(m => m.HandleReligionSwitch("test-player"), Times.Once);
        _mockPlayerDataManager.Verify(m => m.JoinReligion("test-player", "new-religion-uid"), Times.Once);
    }

    #endregion

    #region OnLeaveReligion Tests

    [Fact]
    public void OnLeaveReligion_WithNoReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokePrivateMethod("OnLeaveReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in any religion", result.StatusMessage);
    }

    [Fact]
    public void OnLeaveReligion_WithValidReligion_Leaves()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        // Act
        var result = InvokePrivateMethod("OnLeaveReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("left Test Religion", result.StatusMessage);
        _mockPlayerDataManager.Verify(m => m.LeaveReligion("test-player"), Times.Once);
    }

    [Fact]
    public void OnLeaveReligion_WithNullReligion_LeavesAndShowsUnknown()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns((ReligionData?)null);

        // Act
        var result = InvokePrivateMethod("OnLeaveReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Unknown", result.StatusMessage);
        _mockPlayerDataManager.Verify(m => m.LeaveReligion("test-player"), Times.Once);
    }

    #endregion

    #region OnListReligions Tests

    [Fact]
    public void OnListReligions_WithNoReligions_ReturnsInfoMessage()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object);

        _mockReligionManager.Setup(m => m.GetAllReligions()).Returns(new List<ReligionData>());

        // Act
        var result = InvokePrivateMethod("OnListReligions", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("No religions found", result.StatusMessage);
    }

    [Fact]
    public void OnListReligions_WithMultipleReligions_ListsAll()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object);

        var religion1 = new ReligionData("Religion A", DeityType.Khoras, "founder1", true)
        {
            TotalPrestige = 500,
            PrestigeRank = PrestigeRank.Prominent
        };
        religion1.MemberUIDs.Add("founder1");
        religion1.MemberUIDs.Add("member1");

        var religion2 = new ReligionData("Religion B", DeityType.Lunara, "founder2", false)
        {
            TotalPrestige = 200,
            PrestigeRank = PrestigeRank.Emerging
        };
        religion2.MemberUIDs.Add("founder2");

        _mockReligionManager.Setup(m => m.GetAllReligions()).Returns(new List<ReligionData> { religion1, religion2 });

        // Act
        var result = InvokePrivateMethod("OnListReligions", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Religion A", result.StatusMessage);
        Assert.Contains("Religion B", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("Lunara", result.StatusMessage);
        Assert.Contains("Public", result.StatusMessage);
        Assert.Contains("Private", result.StatusMessage);
        Assert.Contains("2 members", result.StatusMessage);
        Assert.Contains("1 members", result.StatusMessage);
    }

    [Fact]
    public void OnListReligions_WithDeityFilter_FiltersCorrectly()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object, "Khoras");

        var khorasReligion = new ReligionData("Khoras Religion", DeityType.Khoras, "founder1", true);
        khorasReligion.MemberUIDs.Add("founder1");

        _mockReligionManager.Setup(m => m.GetReligionsByDeity(DeityType.Khoras))
            .Returns(new List<ReligionData> { khorasReligion });

        // Act
        var result = InvokePrivateMethod("OnListReligions", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras Religion", result.StatusMessage);
    }

    [Fact]
    public void OnListReligions_WithInvalidDeityFilter_ReturnsError()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object, "InvalidDeity");

        // Act
        var result = InvokePrivateMethod("OnListReligions", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("Invalid deity", result.StatusMessage);
    }

    #endregion

    #region OnReligionInfo Tests

    [Fact]
    public void OnReligionInfo_WithNameParameter_ShowsSpecifiedReligion()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Target Religion");

        var religion = new ReligionData("Target Religion", DeityType.Khoras, "founder-uid", true)
        {
            Description = "A test religion",
            Prestige = 150,
            TotalPrestige = 500,
            PrestigeRank = PrestigeRank.Prominent,
            CreationDate = DateTime.UtcNow.AddDays(-30)
        };
        religion.MemberUIDs.Add("founder-uid");

        var mockFounder = TestFixtures.CreateMockServerPlayer("founder-uid");
        mockFounder.Setup(p => p.PlayerName).Returns("FounderName");

        _mockReligionManager.Setup(m => m.GetReligionByName("Target Religion")).Returns(religion);
        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.PlayerByUid("founder-uid")).Returns(mockFounder.Object);

        // Act
        var result = InvokePrivateMethod("OnReligionInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Target Religion", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("A test religion", result.StatusMessage);
        Assert.Contains("FounderName", result.StatusMessage);
        Assert.Contains("Public", result.StatusMessage);
    }

    [Fact]
    public void OnReligionInfo_WithoutParameter_ShowsPlayerReligion()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("My Religion", DeityType.Lunara, "test-player", true);
        religion.MemberUIDs.Add("test-player");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.PlayerByUid("test-player")).Returns(mockPlayer.Object);

        // Act
        var result = InvokePrivateMethod("OnReligionInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("My Religion", result.StatusMessage);
    }

    [Fact]
    public void OnReligionInfo_WithNonExistentName_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "NonExistent");

        _mockReligionManager.Setup(m => m.GetReligionByName("NonExistent")).Returns((ReligionData?)null);

        // Act
        var result = InvokePrivateMethod("OnReligionInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("NonExistent", result.StatusMessage);
        Assert.Contains("not found", result.StatusMessage);
    }

    [Fact]
    public void OnReligionInfo_WithoutParameterAndNoReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokePrivateMethod("OnReligionInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in any religion", result.StatusMessage);
    }

    #endregion

    #region OnListMembers Tests

    [Fact]
    public void OnListMembers_WithNoReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokePrivateMethod("OnListMembers", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in any religion", result.StatusMessage);
    }

    [Fact]
    public void OnListMembers_WithMembers_ListsAllWithDetails()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);
        religion.MemberUIDs.Add("founder-uid");
        religion.MemberUIDs.Add("member-uid");

        var founderData = new PlayerReligionData { Favor = 200, FavorRank = FavorRank.Disciple };
        var memberData = new PlayerReligionData { Favor = 50, FavorRank = FavorRank.Initiate };

        var mockFounder = TestFixtures.CreateMockServerPlayer("founder-uid");
        mockFounder.Setup(p => p.PlayerName).Returns("FounderName");

        var mockMember = TestFixtures.CreateMockServerPlayer("member-uid");
        mockMember.Setup(p => p.PlayerName).Returns("MemberName");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("founder-uid")).Returns(founderData);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("member-uid")).Returns(memberData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.PlayerByUid("founder-uid")).Returns(mockFounder.Object);
        mockWorld.Setup(w => w.PlayerByUid("member-uid")).Returns(mockMember.Object);

        // Act
        var result = InvokePrivateMethod("OnListMembers", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("FounderName", result.StatusMessage);
        Assert.Contains("MemberName", result.StatusMessage);
        Assert.Contains("Founder", result.StatusMessage);
        Assert.Contains("Member", result.StatusMessage);
        Assert.Contains("Disciple", result.StatusMessage);
        Assert.Contains("Initiate", result.StatusMessage);
        Assert.Contains("200", result.StatusMessage);
        Assert.Contains("50", result.StatusMessage);
    }

    #endregion

    #region OnInvitePlayer Tests

    [Fact]
    public void OnInvitePlayer_WithNoReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "TargetPlayer");

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokePrivateMethod("OnInvitePlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in any religion", result.StatusMessage);
    }

    [Fact]
    public void OnInvitePlayer_WithTargetNotOnline_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "OfflinePlayer");

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "test-player", true);

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.AllOnlinePlayers).Returns(Array.Empty<IPlayer>());

        // Act
        var result = InvokePrivateMethod("OnInvitePlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not found online", result.StatusMessage);
    }

    [Fact]
    public void OnInvitePlayer_WithTargetAlreadyMember_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var mockTarget = TestFixtures.CreateMockServerPlayer("target-uid");
        mockTarget.Setup(p => p.PlayerName).Returns("TargetPlayer");

        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "TargetPlayer");

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "test-player", true);
        religion.MemberUIDs.Add("test-player");
        religion.MemberUIDs.Add("target-uid");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.AllOnlinePlayers).Returns(new IPlayer[] { mockTarget.Object });

        // Act
        var result = InvokePrivateMethod("OnInvitePlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("already a member", result.StatusMessage);
    }

    [Fact]
    public void OnInvitePlayer_WithValidTarget_SendsInvitation()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var mockTarget = TestFixtures.CreateMockServerPlayer("target-uid");
        mockTarget.Setup(p => p.PlayerName).Returns("TargetPlayer");

        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "TargetPlayer");

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "test-player", true)
        {
            ReligionUID = "religion-uid"
        };
        religion.MemberUIDs.Add("test-player");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.AllOnlinePlayers).Returns(new IPlayer[] { mockTarget.Object });

        // Act
        var result = InvokePrivateMethod("OnInvitePlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Invitation sent to TargetPlayer", result.StatusMessage);
        _mockReligionManager.Verify(m => m.InvitePlayer("religion-uid", "target-uid", "test-player"), Times.Once);
        mockTarget.Verify(p => p.SendMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<EnumChatType>()), Times.Once);
    }

    #endregion

    #region OnKickPlayer Tests

    [Fact]
    public void OnKickPlayer_AsNonFounder_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "TargetPlayer");

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);
        religion.MemberUIDs.Add("founder-uid");
        religion.MemberUIDs.Add("test-player");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        // Act
        var result = InvokePrivateMethod("OnKickPlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("Only the founder", result.StatusMessage);
    }

    [Fact]
    public void OnKickPlayer_KickingSelf_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("founder-uid");
        mockPlayer.Setup(p => p.PlayerName).Returns("Founder");

        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Founder");

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);
        religion.MemberUIDs.Add("founder-uid");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("founder-uid")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.AllPlayers).Returns(new IPlayer[] { mockPlayer.Object });

        // Act
        var result = InvokePrivateMethod("OnKickPlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("cannot kick yourself", result.StatusMessage);
    }

    [Fact]
    public void OnKickPlayer_WithValidTarget_RemovesFromReligion()
    {
        // Arrange
        var mockFounder = TestFixtures.CreateMockServerPlayer("founder-uid");
        var mockTarget = TestFixtures.CreateMockServerPlayer("target-uid");
        mockTarget.Setup(p => p.PlayerName).Returns("TargetPlayer");

        var args = TestFixtures.CreateCommandArgs(mockFounder.Object, "TargetPlayer");

        var founderData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);
        religion.MemberUIDs.Add("founder-uid");
        religion.MemberUIDs.Add("target-uid");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("founder-uid")).Returns(founderData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.AllPlayers).Returns(new IPlayer[] { mockFounder.Object, mockTarget.Object });

        // Act
        var result = InvokePrivateMethod("OnKickPlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("TargetPlayer has been removed", result.StatusMessage);
        _mockPlayerDataManager.Verify(m => m.LeaveReligion("target-uid"), Times.Once);
        mockTarget.Verify(p => p.SendMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<EnumChatType>()), Times.Once);
    }

    #endregion

    #region OnDisbandReligion Tests

    [Fact]
    public void OnDisbandReligion_AsNonFounder_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        // Act
        var result = InvokePrivateMethod("OnDisbandReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("Only the founder", result.StatusMessage);
    }

    [Fact]
    public void OnDisbandReligion_AsFounder_DisbandAndNotifiesMembers()
    {
        // Arrange
        var mockFounder = TestFixtures.CreateMockServerPlayer("founder-uid");
        var mockMember = TestFixtures.CreateMockServerPlayer("member-uid");
        var args = TestFixtures.CreateCommandArgs(mockFounder.Object);

        var founderData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true)
        {
            ReligionUID = "religion-uid"
        };
        religion.MemberUIDs.Add("founder-uid");
        religion.MemberUIDs.Add("member-uid");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("founder-uid")).Returns(founderData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.PlayerByUid("founder-uid")).Returns(mockFounder.Object);
        mockWorld.Setup(w => w.PlayerByUid("member-uid")).Returns(mockMember.Object);

        // Act
        var result = InvokePrivateMethod("OnDisbandReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Test Religion has been disbanded", result.StatusMessage);
        _mockPlayerDataManager.Verify(m => m.LeaveReligion("founder-uid"), Times.Once);
        _mockPlayerDataManager.Verify(m => m.LeaveReligion("member-uid"), Times.Once);
        _mockReligionManager.Verify(m => m.DeleteReligion("religion-uid", "founder-uid"), Times.Once);
        mockMember.Verify(p => p.SendMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<EnumChatType>()), Times.Once);
        _mockServerChannel.Verify(c => c.SendPacket(It.IsAny<ReligionStateChangedPacket>(), mockFounder.Object), Times.Once);
        _mockServerChannel.Verify(c => c.SendPacket(It.IsAny<ReligionStateChangedPacket>(), mockMember.Object), Times.Once);
    }

    #endregion

    #region OnSetDescription Tests

    [Fact]
    public void OnSetDescription_WithNoReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "New description");

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokePrivateMethod("OnSetDescription", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in any religion", result.StatusMessage);
    }

    [Fact]
    public void OnSetDescription_AsNonFounder_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "New description");

        var playerData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        // Act
        var result = InvokePrivateMethod("OnSetDescription", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("Only the founder", result.StatusMessage);
    }

    [Fact]
    public void OnSetDescription_AsFounder_SetsDescription()
    {
        // Arrange
        var mockFounder = TestFixtures.CreateMockServerPlayer("founder-uid");
        var args = TestFixtures.CreateCommandArgs(mockFounder.Object, "A great religion");

        var founderData = new PlayerReligionData { ReligionUID = "religion-uid" };
        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("founder-uid")).Returns(founderData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-uid")).Returns(religion);

        // Act
        var result = InvokePrivateMethod("OnSetDescription", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Equal("A great religion", religion.Description);
        Assert.Contains("Description set", result.StatusMessage);
    }

    #endregion

    #region Helper Methods

    private TextCommandResult InvokePrivateMethod(string methodName, TextCommandCallingArgs args)
    {
        var method = typeof(ReligionCommands).GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (TextCommandResult)method!.Invoke(_commands, new object[] { args })!;
    }

    #endregion
}
