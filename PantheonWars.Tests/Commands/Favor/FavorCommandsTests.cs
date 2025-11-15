using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Tests.Commands.Helpers;

namespace PantheonWars.Tests.Commands.Favor;

/// <summary>
/// Tests for the FavorCommands class registration and initialization
/// </summary>
[ExcludeFromCodeCoverage]
public class FavorCommandsTests : FavorCommandsTestHelpers
{
    public FavorCommandsTests()
    {
        _sut = InitializeMocksAndSut();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Act
        var favorCommands = InitializeMocksAndSut();

        // Assert
        Assert.NotNull(favorCommands);
    }

    [Fact]
    public void Constructor_WithNullSapi_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PantheonWars.Commands.FavorCommands(
            null!,
            _deityRegistry.Object,
            _playerReligionDataManager.Object));
    }

    [Fact]
    public void Constructor_WithNullDeityRegistry_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PantheonWars.Commands.FavorCommands(
            _mockSapi.Object,
            null!,
            _playerReligionDataManager.Object));
    }

    [Fact]
    public void Constructor_WithNullPlayerReligionDataManager_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PantheonWars.Commands.FavorCommands(
            _mockSapi.Object,
            _deityRegistry.Object,
            null!));
    }

    #endregion

    #region Command Registration Tests

    [Fact]
    public void RegisterCommands_Always_CallsChatCommandsCreate()
    {
        // Arrange
        var mockCommandBuilder = new Mock<IChatCommandBuilder>();
        mockCommandBuilder.Setup(b => b.WithDescription(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
        mockCommandBuilder.Setup(b => b.RequiresPlayer()).Returns(mockCommandBuilder.Object);
        mockCommandBuilder.Setup(b => b.RequiresPrivilege(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
        mockCommandBuilder.Setup(b => b.HandleWith(It.IsAny<CommandDelegate>())).Returns(mockCommandBuilder.Object);
        mockCommandBuilder.Setup(b => b.BeginSubCommand(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
        mockCommandBuilder.Setup(b => b.EndSubCommand()).Returns(mockCommandBuilder.Object);
        mockCommandBuilder.Setup(b => b.WithArgs(It.IsAny<ICommandArgumentParser>())).Returns(mockCommandBuilder.Object);

        _mockChatCommands.Setup(c => c.Create("favor")).Returns(mockCommandBuilder.Object);
        _mockChatCommands.Setup(c => c.Parsers).Returns(new Mock<IChatCommandArgumentParsers>().Object);

        var mockParsers = new Mock<IChatCommandArgumentParsers>();
        var mockIntParser = new Mock<ICommandArgumentParser>();
        mockParsers.Setup(p => p.Int(It.IsAny<string>())).Returns(mockIntParser.Object);
        _mockChatCommands.Setup(c => c.Parsers).Returns(mockParsers.Object);

        // Act
        _sut!.RegisterCommands();

        // Assert
        _mockChatCommands.Verify(c => c.Create("favor"), Times.Once);
        _mockLogger.Verify(l => l.Notification(It.Is<string>(s => s.Contains("Favor commands registered"))), Times.Once);
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public void GetCurrentFavorRank_WithZeroFavor_ReturnsInitiate()
    {
        // This test validates the GetCurrentFavorRank helper method behavior
        // Since it's private, we test it indirectly through commands that use it

        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", favor: 0, totalFavor: 0, rank: FavorRank.Initiate);
        var args = CreateCommandArgs(mockPlayer.Object);

        var mockDeity = CreateMockDeity(Models.Enum.DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(Models.Enum.DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnFavorInfo(args);

        // Assert
        Assert.Equal(Vintagestory.API.Common.EnumCommandStatus.Success, result.Status);
        Assert.Contains("Initiate", result.StatusMessage);
    }

    [Fact]
    public void GetCurrentFavorRank_With600TotalFavor_ReturnsDisciple()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", favor: 600, totalFavor: 600, rank: FavorRank.Disciple);
        var args = CreateCommandArgs(mockPlayer.Object);

        var mockDeity = CreateMockDeity(Models.Enum.DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(Models.Enum.DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnFavorInfo(args);

        // Assert
        Assert.Equal(Vintagestory.API.Common.EnumCommandStatus.Success, result.Status);
        Assert.Contains("Disciple", result.StatusMessage);
    }

    [Fact]
    public void GetCurrentFavorRank_With3000TotalFavor_ReturnsZealot()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", favor: 3000, totalFavor: 3000, rank: FavorRank.Zealot);
        var args = CreateCommandArgs(mockPlayer.Object);

        var mockDeity = CreateMockDeity(Models.Enum.DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(Models.Enum.DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnFavorInfo(args);

        // Assert
        Assert.Equal(Vintagestory.API.Common.EnumCommandStatus.Success, result.Status);
        Assert.Contains("Zealot", result.StatusMessage);
    }

    [Fact]
    public void GetCurrentFavorRank_With7000TotalFavor_ReturnsChampion()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", favor: 7000, totalFavor: 7000, rank: FavorRank.Champion);
        var args = CreateCommandArgs(mockPlayer.Object);

        var mockDeity = CreateMockDeity(Models.Enum.DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(Models.Enum.DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnFavorInfo(args);

        // Assert
        Assert.Equal(Vintagestory.API.Common.EnumCommandStatus.Success, result.Status);
        Assert.Contains("Champion", result.StatusMessage);
    }

    [Fact]
    public void GetCurrentFavorRank_With15000TotalFavor_ReturnsAvatar()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", favor: 15000, totalFavor: 15000, rank: FavorRank.Avatar);
        var args = CreateCommandArgs(mockPlayer.Object);

        var mockDeity = CreateMockDeity(Models.Enum.DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(Models.Enum.DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnFavorInfo(args);

        // Assert
        Assert.Equal(Vintagestory.API.Common.EnumCommandStatus.Success, result.Status);
        Assert.Contains("Avatar", result.StatusMessage);
    }

    #endregion
}
