using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Constants;
using PantheonWars.Data;
using PantheonWars.Models;
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

    [Fact]
    public void OnPerksReligion_DisplaysReligionNameInHeader()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk { Name = "Holy Blessing", Category = PerkCategory.Utility }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains("Divine Order", message);
        Assert.Contains(string.Format(FormatStringConstants.HeaderReligionPerksWithName, "Divine Order", 1), message);
    }

    [Fact]
    public void OnPerksReligion_DisplaysUnlockedReligionPerksCorrectly()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Holy Blessing",
                Category = PerkCategory.Utility,
                Description = "Grants divine protection to all members.",
                StatModifiers = new Dictionary<string, float>
                {
                    { "Defense", 0.15f }
                }
            }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatPerkNameCategory, "Holy Blessing", "Utility"), message);
        Assert.Contains(string.Format(FormatStringConstants.FormatDescription, "Grants divine protection to all members."), message);
        Assert.Contains(FormatStringConstants.LabelEffectsForAllMembers, message);
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Defense", 15), message);
    }

    [Fact]
    public void OnPerksReligion_ShowsPerkCountInHeader()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk { Name = "Holy Blessing", Category = PerkCategory.Utility },
            new Models.Perk { Name = "Sacred Shield", Category = PerkCategory.Defense },
            new Models.Perk { Name = "Divine Wrath", Category = PerkCategory.Combat }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.HeaderReligionPerksWithName, "Divine Order", 3), message);
    }

    [Fact]
    public void OnPerksReligion_DisplaysPerkNamesAndCategories()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk { Name = "Holy Blessing", Category = PerkCategory.Utility },
            new Models.Perk { Name = "Sacred Shield", Category = PerkCategory.Defense }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatPerkNameCategory, "Holy Blessing", "Utility"), message);
        Assert.Contains(string.Format(FormatStringConstants.FormatPerkNameCategory, "Sacred Shield", "Defense"), message);
    }

    [Fact]
    public void OnPerksReligion_DisplaysPerkDescriptions()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Holy Blessing",
                Category = PerkCategory.Utility,
                Description = "Grants divine protection to all members."
            }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatDescription, "Grants divine protection to all members."), message);
    }

    [Fact]
    public void OnPerksReligion_StatModifiersShowForAllMembersLabel()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Holy Blessing",
                Category = PerkCategory.Utility,
                Description = "Grants divine protection.",
                StatModifiers = new Dictionary<string, float>
                {
                    { "Defense", 0.10f }
                }
            }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(FormatStringConstants.LabelEffectsForAllMembers, message);
    }

    [Fact]
    public void OnPerksReligion_StatModifiersFormattedCorrectly()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Holy Blessing",
                Category = PerkCategory.Utility,
                Description = "Grants divine protection.",
                StatModifiers = new Dictionary<string, float>
                {
                    { "Defense", 0.25f }
                }
            }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Defense", 25), message);
    }

    [Fact]
    public void OnPerksReligion_MultipleStatModifiersPerPerk()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Divine Empowerment",
                Category = PerkCategory.Combat,
                Description = "Empowers all members.",
                StatModifiers = new Dictionary<string, float>
                {
                    { "Attack", 0.20f },
                    { "Defense", 0.15f },
                    { "Speed", 0.10f }
                }
            }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((new List<Models.Perk>(), religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Attack", 20), message);
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Defense", 15), message);
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Speed", 10), message);
    }

    [Fact]
    public void OnPerksReligion_ReturnsOnlyReligionPerks()
    {
        // Arrange
        var player = new Mock<IServerPlayer>().Object;
        var playerData = new PlayerReligionData()
        {
            ReligionUID = "test-religion-uid"
        };
        var religion = new ReligionData()
        {
            ReligionUID = "test-religion-uid",
            ReligionName = "Divine Order"
        };

        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk { Name = "Personal Blessing", Category = PerkCategory.Utility }
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk { Name = "Holy Blessing", Category = PerkCategory.Utility }
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
        _religionManager.Setup(r => r.GetReligion(playerData.ReligionUID))
            .Returns(religion);
        _perkEffectSystem.Setup(p => p.GetActivePerks(player.PlayerUID))
            .Returns((playerPerks, religionPerks));

        // Act
        var result = _sut!.OnPerksReligion(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains("Holy Blessing", message);
        Assert.DoesNotContain("Personal Blessing", message);
    }
}