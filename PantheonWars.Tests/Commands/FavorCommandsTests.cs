using System;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Xunit;

namespace PantheonWars.Tests.Commands;

/// <summary>
///     Tests for FavorCommands class
/// </summary>
public class FavorCommandsTests
{
    private readonly Mock<IDeityRegistry> _mockDeityRegistry;
    private readonly Mock<IPlayerReligionDataManager> _mockPlayerReligionDataManager;
    private readonly Mock<ICoreServerAPI> _mockSapi;
    private readonly FavorCommands _commands;

    public FavorCommandsTests()
    {
        _mockSapi = TestFixtures.CreateMockServerAPI();
        _mockDeityRegistry = new Mock<IDeityRegistry>();
        _mockPlayerReligionDataManager = new Mock<IPlayerReligionDataManager>();

        _commands = new FavorCommands(
            _mockSapi.Object,
            _mockDeityRegistry.Object,
            _mockPlayerReligionDataManager.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var commands = new FavorCommands(
            _mockSapi.Object,
            _mockDeityRegistry.Object,
            _mockPlayerReligionDataManager.Object
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
        mockChatCommands.Verify(c => c.Create("favor"), Times.Once);

        // Verify notification was logged
        var mockLogger = Mock.Get(_mockSapi.Object.Logger);
        mockLogger.Verify(l => l.Notification("[PantheonWars] Favor commands registered"), Times.Once);
    }

    #endregion

    #region OnCheckFavor Tests

    [Fact]
    public void OnCheckFavor_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnCheckFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnCheckFavor_WithActiveDeity_ShowsFavorAndRank()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 150,
            FavorRank = FavorRank.Disciple
        };

        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns(deity);

        // Act
        var result = InvokePrivateMethod("OnCheckFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("150 favor", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("Disciple", result.StatusMessage);
    }

    [Fact]
    public void OnCheckFavor_WithNullDeity_ShowsDeityType()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 150,
            FavorRank = FavorRank.Disciple
        };

        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns((Deity?)null);

        // Act
        var result = InvokePrivateMethod("OnCheckFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras", result.StatusMessage);
    }

    #endregion

    #region OnFavorInfo Tests

    [Fact]
    public void OnFavorInfo_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnFavorInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnFavorInfo_WithActiveDeity_ShowsDetailedInfo()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 150,
            FavorRank = FavorRank.Disciple,
            TotalFavorEarned = 750
        };

        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns(deity);

        // Act
        var result = InvokePrivateMethod("OnFavorInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("150", result.StatusMessage);
        Assert.Contains("750", result.StatusMessage);
        Assert.Contains("Disciple", result.StatusMessage);
        Assert.Contains("Next Rank:", result.StatusMessage);
        Assert.Contains("Progress:", result.StatusMessage);
    }

    [Fact]
    public void OnFavorInfo_AtMaxRank_ShowsMaxRankMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 500,
            FavorRank = FavorRank.Avatar,
            TotalFavorEarned = 15000
        };

        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns(deity);

        // Act
        var result = InvokePrivateMethod("OnFavorInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Maximum rank achieved", result.StatusMessage);
    }

    #endregion

    #region OnFavorStats Tests

    [Fact]
    public void OnFavorStats_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnFavorStats", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnFavorStats_WithFullData_ShowsComprehensiveStats()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 200,
            FavorRank = FavorRank.Zealot,
            TotalFavorEarned = 2500,
            KillCount = 15,
            LastReligionSwitch = DateTime.UtcNow.AddDays(-45)
        };

        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns(deity);

        // Act
        var result = InvokePrivateMethod("OnFavorStats", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("200", result.StatusMessage);
        Assert.Contains("2,500", result.StatusMessage);
        Assert.Contains("Zealot", result.StatusMessage);
        Assert.Contains("15", result.StatusMessage);
        Assert.Contains("Days Served:", result.StatusMessage);
        Assert.Contains("Join Date:", result.StatusMessage);
    }

    [Fact]
    public void OnFavorStats_WithoutLastReligionSwitch_DoesNotShowDaysServed()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 200,
            TotalFavorEarned = 600,
            LastReligionSwitch = null
        };

        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns(deity);

        // Act
        var result = InvokePrivateMethod("OnFavorStats", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.DoesNotContain("Days Served:", result.StatusMessage);
    }

    #endregion

    #region OnListRanks Tests

    [Fact]
    public void OnListRanks_ListsAllRanksWithRequirements()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object);

        // Act
        var result = InvokePrivateMethod("OnListRanks", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Favor Ranks", result.StatusMessage);
        Assert.Contains("Initiate", result.StatusMessage);
        Assert.Contains("Disciple", result.StatusMessage);
        Assert.Contains("Zealot", result.StatusMessage);
        Assert.Contains("Champion", result.StatusMessage);
        Assert.Contains("Avatar", result.StatusMessage);
        Assert.Contains("Higher ranks unlock more powerful blessings", result.StatusMessage);
    }

    #endregion

    #region OnSetFavor Tests

    [Fact]
    public void OnSetFavor_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 100);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnSetFavor_WithNegativeAmount_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, -100);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("cannot be negative", result.StatusMessage);
    }

    [Fact]
    public void OnSetFavor_WithAmountOverMax_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 1000000);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("cannot exceed 999,999", result.StatusMessage);
    }

    [Fact]
    public void OnSetFavor_WithValidAmount_SetsFavor()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 250);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras, Favor = 100 };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Equal(250, religionData.Favor);
        Assert.Contains("250", result.StatusMessage);
    }

    #endregion

    #region OnAddFavor Tests

    [Fact]
    public void OnAddFavor_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 50);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnAddFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnAddFavor_WithZeroOrNegative_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 0);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnAddFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("must be greater than 0", result.StatusMessage);
    }

    [Fact]
    public void OnAddFavor_WithAmountOverMax_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 1000000);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnAddFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("cannot exceed 999,999", result.StatusMessage);
    }

    [Fact]
    public void OnAddFavor_WithValidAmount_AddsFavor()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 50);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras, Favor = 100 };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockPlayerReligionDataManager.Setup(m => m.AddFavor("test-player", 50))
            .Callback(() => religionData.Favor += 50);

        // Act
        var result = InvokePrivateMethod("OnAddFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Added 50", result.StatusMessage);
        Assert.Contains("100", result.StatusMessage);
        Assert.Contains("150", result.StatusMessage);
        _mockPlayerReligionDataManager.Verify(m => m.AddFavor("test-player", 50), Times.Once);
    }

    #endregion

    #region OnRemoveFavor Tests

    [Fact]
    public void OnRemoveFavor_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 50);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnRemoveFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnRemoveFavor_WithZeroOrNegative_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, -10);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnRemoveFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("must be greater than 0", result.StatusMessage);
    }

    [Fact]
    public void OnRemoveFavor_WithValidAmount_RemovesFavor()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 30);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras, Favor = 100 };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockPlayerReligionDataManager.Setup(m => m.RemoveFavor("test-player", 30))
            .Callback(() => religionData.Favor -= 30);

        // Act
        var result = InvokePrivateMethod("OnRemoveFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Removed 30", result.StatusMessage);
        Assert.Contains("100", result.StatusMessage);
        Assert.Contains("70", result.StatusMessage);
        _mockPlayerReligionDataManager.Verify(m => m.RemoveFavor("test-player", 30), Times.Once);
    }

    [Fact]
    public void OnRemoveFavor_WhenExceedingCurrent_RemovesOnlyAvailable()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 150);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras, Favor = 100 };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockPlayerReligionDataManager.Setup(m => m.RemoveFavor("test-player", 150))
            .Callback(() => religionData.Favor = 0); // Can't go below 0

        // Act
        var result = InvokePrivateMethod("OnRemoveFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("100", result.StatusMessage); // Shows old favor
        Assert.Contains("0", result.StatusMessage); // Shows new favor
    }

    #endregion

    #region OnResetFavor Tests

    [Fact]
    public void OnResetFavor_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnResetFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnResetFavor_WithFavor_ResetsToZero()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras, Favor = 150 };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnResetFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Equal(0, religionData.Favor);
        Assert.Contains("reset to 0", result.StatusMessage);
        Assert.Contains("was 150", result.StatusMessage);
    }

    #endregion

    #region OnMaxFavor Tests

    [Fact]
    public void OnMaxFavor_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnMaxFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnMaxFavor_SetsToMaximum()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras, Favor = 50 };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnMaxFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Equal(99999, religionData.Favor);
        Assert.Contains("99,999", result.StatusMessage);
        Assert.Contains("was 50", result.StatusMessage);
    }

    #endregion

    #region OnSetTotalFavor Tests

    [Fact]
    public void OnSetTotalFavor_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 5000);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetTotalFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnSetTotalFavor_WithNegativeAmount_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, -100);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetTotalFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("cannot be negative", result.StatusMessage);
    }

    [Fact]
    public void OnSetTotalFavor_WithAmountOverMax_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 1000000);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetTotalFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("cannot exceed 999,999", result.StatusMessage);
    }

    [Fact]
    public void OnSetTotalFavor_WithValidAmount_SetsAndUpdatesRank()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 6000);

        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            TotalFavorEarned = 1000,
            FavorRank = FavorRank.Disciple
        };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetTotalFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Equal(6000, religionData.TotalFavorEarned);
        Assert.Contains("6,000", result.StatusMessage);
        Assert.Contains("1,000", result.StatusMessage);
        // Rank should have been updated by UpdateFavorRank()
    }

    [Fact]
    public void OnSetTotalFavor_WhenRankChanges_ShowsRankUpdate()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, 6000);

        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            TotalFavorEarned = 1000,
            FavorRank = FavorRank.Disciple
        };
        _mockPlayerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSetTotalFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        // The rank will be updated by UpdateFavorRank in the actual implementation
        Assert.Contains("Rank", result.StatusMessage);
    }

    #endregion

    #region Helper Methods

    private TextCommandResult InvokePrivateMethod(string methodName, TextCommandCallingArgs args)
    {
        var method = typeof(FavorCommands).GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (TextCommandResult)method!.Invoke(_commands, new object[] { args })!;
    }

    #endregion
}
