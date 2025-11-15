using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Models.Enum;
using PantheonWars.Tests.Commands.Helpers;
using Vintagestory.API.Common;

namespace PantheonWars.Tests.Commands.Favor;

/// <summary>
/// Tests for admin favor commands (add, remove, reset, max, settotal)
/// </summary>
[ExcludeFromCodeCoverage]
public class FavorCommandAdminTests : FavorCommandsTestHelpers
{
    public FavorCommandAdminTests()
    {
        _sut = InitializeMocksAndSut();
    }

    #region /favor add tests

    [Fact]
    public void OnAddFavor_WithValidAmount_AddsFavor()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Khoras, favor: 100, totalFavor: 500);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "50");
        SetupParsers(args, 50);

        var mockDeity = CreateMockDeity(DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Khoras)).Returns(mockDeity.Object);
        _playerReligionDataManager.Setup(m => m.AddFavor("player-1", 50, It.IsAny<string>()))
            .Callback(() => playerData.Favor += 50);

        // Act
        var result = _sut!.OnAddFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Added 50 favor", result.StatusMessage);
        Assert.Contains("100", result.StatusMessage);
        Assert.Contains("150", result.StatusMessage);
    }

    [Fact]
    public void OnAddFavor_WithZeroOrNegative_ReturnsError()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Khoras);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "0");
        SetupParsers(args, 0);

        var mockDeity = CreateMockDeity(DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnAddFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("greater than 0", result.StatusMessage);
    }

    #endregion

    #region /favor remove tests

    [Fact]
    public void OnRemoveFavor_WithValidAmount_RemovesFavor()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Lysa, favor: 200, totalFavor: 500);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "50");
        SetupParsers(args, 50);

        var mockDeity = CreateMockDeity(DeityType.Lysa, "Lysa");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Lysa)).Returns(mockDeity.Object);
        _playerReligionDataManager.Setup(m => m.RemoveFavor("player-1", 50, It.IsAny<string>()))
            .Callback(() => playerData.Favor = Math.Max(0, playerData.Favor - 50))
            .Returns(true);

        // Act
        var result = _sut!.OnRemoveFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Removed", result.StatusMessage);
        Assert.Contains("200", result.StatusMessage);
        Assert.Contains("150", result.StatusMessage);
    }

    [Fact]
    public void OnRemoveFavor_WithMoreThanCurrent_RemovesOnlyAvailable()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Morthen, favor: 50, totalFavor: 500);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "100");
        SetupParsers(args, 100);

        var mockDeity = CreateMockDeity(DeityType.Morthen, "Morthen");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Morthen)).Returns(mockDeity.Object);
        _playerReligionDataManager.Setup(m => m.RemoveFavor("player-1", 100, It.IsAny<string>()))
            .Callback(() => playerData.Favor = 0)
            .Returns(true);

        // Act
        var result = _sut!.OnRemoveFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("50", result.StatusMessage); // Actual removed
        Assert.Contains("→ 0", result.StatusMessage);
    }

    [Fact]
    public void OnRemoveFavor_WithZeroOrNegative_ReturnsError()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Khoras);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "-10");
        SetupParsers(args, -10);

        var mockDeity = CreateMockDeity(DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnRemoveFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("greater than 0", result.StatusMessage);
    }

    #endregion

    #region /favor reset tests

    [Fact]
    public void OnResetFavor_Always_ResetsFavorToZero()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Aethra, favor: 5000, totalFavor: 10000);
        var args = CreateAdminCommandArgs(mockPlayer.Object);

        var mockDeity = CreateMockDeity(DeityType.Aethra, "Aethra");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Aethra)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnResetFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("reset to 0", result.StatusMessage);
        Assert.Contains("5,000", result.StatusMessage); // Old value
        Assert.Equal(0, playerData.Favor);
    }

    #endregion

    #region /favor max tests

    [Fact]
    public void OnMaxFavor_Always_SetsFavorToMaximum()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Umbros, favor: 100, totalFavor: 500);
        var args = CreateAdminCommandArgs(mockPlayer.Object);

        var mockDeity = CreateMockDeity(DeityType.Umbros, "Umbros");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Umbros)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnMaxFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("99,999", result.StatusMessage);
        Assert.Contains("100", result.StatusMessage); // Old value
        Assert.Equal(99999, playerData.Favor);
    }

    #endregion

    #region /favor settotal tests

    [Fact]
    public void OnSetTotalFavor_WithValidAmount_SetsTotalAndUpdatesRank()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Tharos, favor: 100, totalFavor: 100, rank: FavorRank.Initiate);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "5000");
        SetupParsers(args, 5000);

        var mockDeity = CreateMockDeity(DeityType.Tharos, "Tharos");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Tharos)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnSetTotalFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("5,000", result.StatusMessage);
        Assert.Contains("100", result.StatusMessage); // Old total
        Assert.Contains("Rank updated", result.StatusMessage);
        Assert.Equal(5000, playerData.TotalFavorEarned);
        Assert.Equal(FavorRank.Champion, playerData.FavorRank);
    }

    [Fact]
    public void OnSetTotalFavor_WithSameRank_ShowsRankUnchanged()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Gaia, favor: 100, totalFavor: 600, rank: FavorRank.Disciple);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "700");
        SetupParsers(args, 700);

        var mockDeity = CreateMockDeity(DeityType.Gaia, "Gaia");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Gaia)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnSetTotalFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Rank unchanged", result.StatusMessage);
        Assert.Contains("Disciple", result.StatusMessage);
        Assert.Equal(700, playerData.TotalFavorEarned);
    }

    [Fact]
    public void OnSetTotalFavor_WithNegativeAmount_ReturnsError()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Khoras);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "-100");
        SetupParsers(args, -100);

        var mockDeity = CreateMockDeity(DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnSetTotalFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("cannot be negative", result.StatusMessage);
    }

    [Fact]
    public void OnSetTotalFavor_WithTooLargeAmount_ReturnsError()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.Khoras);
        var args = CreateAdminCommandArgs(mockPlayer.Object, "1000000");
        SetupParsers(args, 1000000);

        var mockDeity = CreateMockDeity(DeityType.Khoras, "Khoras");

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);
        _deityRegistry.Setup(d => d.GetDeity(DeityType.Khoras)).Returns(mockDeity.Object);

        // Act
        var result = _sut!.OnSetTotalFavor(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("cannot exceed 999,999", result.StatusMessage);
    }

    #endregion

    #region Common error cases

    [Fact]
    public void AdminCommands_WithoutDeity_ReturnError()
    {
        // Arrange
        var mockPlayer = CreateMockPlayer("player-1", "TestPlayer");
        var playerData = CreatePlayerData("player-1", DeityType.None);
        var args = CreateAdminCommandArgs(mockPlayer.Object);

        _playerReligionDataManager.Setup(m => m.GetOrCreatePlayerData("player-1")).Returns(playerData);

        // Act & Assert
        var resetResult = _sut!.OnResetFavor(args);
        Assert.Equal(EnumCommandStatus.Error, resetResult.Status);

        var maxResult = _sut.OnMaxFavor(args);
        Assert.Equal(EnumCommandStatus.Error, maxResult.Status);
    }

    #endregion
}
