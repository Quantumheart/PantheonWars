using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Constants;
using PantheonWars.Data;
using PantheonWars.Tests.Commands.Helpers;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PantheonWars.Tests.Commands.Perk;

[ExcludeFromCodeCoverage]
public class PerkCommandReligionTests : PerkCommandsTestHelpers
{
    public PerkCommandReligionTests()
    {
        _sut = InitializeMocksAndSut();
    }

    [Fact]
    public void OnPerksReligion_PlayerNull_ReturnsErrorPlayerNotFound()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Mock<Caller>().Object
        };

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        Assert.Equal(ErrorMessageConstants.ErrorPlayerNotFound, result.StatusMessage);
    }

    [Fact]
    public void OnPerksReligion_PlayerHasNoReligion_ReturnsErrorNoReligion()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new Mock<PlayerReligionData>().Object;

        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = player
            }
        };

        _playerReligionDataManager.Setup(p => p.GetOrCreatePlayerData(player.PlayerUID))
            .Returns(playerData);

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        Assert.Equal(ErrorMessageConstants.ErrorNoReligion, result.StatusMessage);
    }

    [Fact]
    public void OnPerksReligion_ReligionHasNoUnlockedPerks_ReturnsInfoMessage()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "foobar"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "foobar"
        };

        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = player
            }
        };

        _playerReligionDataManager.Setup(p => p.GetOrCreatePlayerData(player.PlayerUID))
            .Returns(playerData);
        _religionManager.Setup(r => r.GetReligion(It.IsAny<string>()))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), new List<Models.Perk>()));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        Assert.Equal(InfoMessageConstants.InfoNoReligionPerks, result.StatusMessage);
    }
}