using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Constants;
using PantheonWars.Models;
using PantheonWars.Systems.Interfaces;
using PantheonWars.Tests.Commands.Helpers;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PantheonWars.Tests.Commands.Perk;

[ExcludeFromCodeCoverage]
public class PerkCommandPlayer : PerkCommandsTestHelpers
{
    public PerkCommandPlayer()
    {
        _sut = InitializeMocksAndSut();
    }

    [Fact]
    public void OnPerksPlayer_PlayerNull_ReturnsError()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Mock<Caller>().Object
        };

        // Act
        var result = _sut!.OnPerksPlayer(args);

        // Assert
        Assert.Equal(ErrorMessageConstants.ErrorPlayerNotFound, result.StatusMessage);
    }

    [Fact]
    public void OnPerksPlayer_PlayerHasNoUnlockedPerks_ReturnsInfoMessage()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        _perkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns(new ValueTuple<List<Models.Perk>, List<Models.Perk>>()
            {
                Item1 = new List<Models.Perk>(),
                Item2 = new List<Models.Perk>()
            }); // Simulate no perks


        // Act
        var result = _sut!.OnPerksPlayer(args);

        // Assert
        Assert.Equal(InfoMessageConstants.InfoNoPlayerPerks, result.StatusMessage);
    }

    [Fact]
    public void OnPerksPlayer_PlayerHasUnlockedPerks_ReturnsFormattedList()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Divine Strike",
                Category = PerkCategory.Combat,
                Description = "Deals extra damage to enemies.",
                StatModifiers = new Dictionary<string, float>
                {
                    { "Damage", 0.2f }
                }
            }
        };

        _perkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns(new ValueTuple<List<Models.Perk>, List<Models.Perk>>()
            {
                Item1 = playerPerks
            });

        var sut = new PerkCommands(
            _mockSapi.Object,
            _perkRegistry.Object,
            _playerReligionDataManager.Object,
            _religionManager.Object,
            _perkEffectSystem.Object);

        // Act
        var result = sut.OnPerksPlayer(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.HeaderUnlockedPlayerPerks, 1), message);
        Assert.Contains(string.Format(FormatStringConstants.FormatPerkNameCategory, "Divine Strike", "Combat"),
            message);
        Assert.Contains(string.Format(FormatStringConstants.FormatDescription, "Deals extra damage to enemies."),
            message);
        Assert.Contains(FormatStringConstants.LabelEffects, message);
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Damage", 20), message);
    }

    [Fact]
    public void OnPerksPlayer_HeaderDisplaysPerkCount()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk { Name = "Divine Strike" },
            new Models.Perk { Name = "Faithful Guardian" }
        };

        _perkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns(new ValueTuple<List<Models.Perk>, List<Models.Perk>>()
            {
                Item1 = playerPerks
            });

        var sut = new PerkCommands(
            _mockSapi.Object,
            _perkRegistry.Object,
            _playerReligionDataManager.Object,
            _religionManager.Object,
            _perkEffectSystem.Object);

        // Act
        var result = sut.OnPerksPlayer(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.HeaderUnlockedPlayerPerks, 2), message);
    }

    [Fact]
    public void OnPerksPlayer_DisplayPerkNamesAndCategories()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        var mockPerkEffectSystem = new Mock<IPerkEffectSystem>();
        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Divine Strike",
                Category = PerkCategory.Combat
            },
            new Models.Perk
            {
                Name = "Faithful Guardian",
                Category = PerkCategory.Defense
            }
        };

        mockPerkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns(new ValueTuple<List<Models.Perk>, List<Models.Perk>>()
            {
                Item1 = playerPerks
            });

        var sut = new PerkCommands(
            _mockSapi.Object,
            _perkRegistry.Object,
            _playerReligionDataManager.Object,
            _religionManager.Object,
            mockPerkEffectSystem.Object);

        // Act
        var result = sut.OnPerksPlayer(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatPerkNameCategory, "Divine Strike", "Combat"),
            message);
        Assert.Contains(string.Format(FormatStringConstants.FormatPerkNameCategory, "Faithful Guardian", "Defense"),
            message);
    }

    [Fact]
    public void OnPerksPlayer_DisplayPerkDescriptions()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Divine Strike",
                Description = "Deals extra damage to enemies."
            }
        };

        _perkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns(new ValueTuple<List<Models.Perk>, List<Models.Perk>>()
            {
                Item1 = playerPerks
            });

        var sut = new PerkCommands(
            _mockSapi.Object,
            _perkRegistry.Object,
            _playerReligionDataManager.Object,
            _religionManager.Object,
            _perkEffectSystem.Object);

        // Act
        var result = sut.OnPerksPlayer(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatDescription, "Deals extra damage to enemies."),
            message);
    }

    [Fact]
    public void OnPerksPlayer_DisplayStatModifiersAsPercentage()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Divine Strike",
                Category = PerkCategory.Combat,
                Description = "Deals extra damage to enemies.",
                StatModifiers = new Dictionary<string, float>
                {
                    { "Damage", 0.2f }
                }
            }
        };

        _perkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns(new ValueTuple<List<Models.Perk>, List<Models.Perk>>()
            {
                Item1 = playerPerks
            });

        var sut = new PerkCommands(
            _mockSapi.Object,
            _perkRegistry.Object,
            _playerReligionDataManager.Object,
            _religionManager.Object,
            _perkEffectSystem.Object);

        // Act
        var result = sut.OnPerksPlayer(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Damage", 20), message);
    }

    [Fact]
    public void OnPerksPlayer_NoStatModifiers_DoesNotShowEffectsLabel()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        var mockPerkEffectSystem = new Mock<IPerkEffectSystem>();
        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Faithful Guardian",
                Category = PerkCategory.Defense,
                Description = "Reduces incoming damage."
            }
        };

        mockPerkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns(new ValueTuple<List<Models.Perk>, List<Models.Perk>>()
            {
                Item1 = playerPerks
            });

        var sut = new PerkCommands(
            _mockSapi.Object,
            _perkRegistry.Object,
            _playerReligionDataManager.Object,
            _religionManager.Object,
            mockPerkEffectSystem.Object);

        // Act
        var result = sut.OnPerksPlayer(args);

        // Assert
        var message = result.StatusMessage;
        Assert.DoesNotContain(FormatStringConstants.LabelEffects, message);
    }

    [Fact]
    public void OnPerksPlayer_MultipleStatModifiers_DisplayAll()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Divine Strike",
                Category = PerkCategory.Combat,
                Description = "Deals extra damage to enemies.",
                StatModifiers = new Dictionary<string, float>
                {
                    { "Damage", 0.2f },
                    { "Crit Chance", 0.15f }
                }
            }
        };

        _perkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns(new ValueTuple<List<Models.Perk>, List<Models.Perk>>()
            {
                Item1 = playerPerks
            });
        

        // Act
        var result = _sut!.OnPerksPlayer(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Damage", 20), message);
        Assert.Contains(string.Format(FormatStringConstants.FormatStatModifier, "Crit Chance", 15), message);
        _perkEffectSystem.Verify(p => p.GetActivePerks(args.Caller.Player.PlayerUID), Times.Once);
    }

    [Fact]
    public void OnPerksPlayer_ReturnsOnlyPlayerPerks()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };


        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk { Name = "Divine Strike", Category = PerkCategory.Combat, Description = "Deals extra damage." }
        };

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                Name = "Religion Boon", Category = PerkCategory.Utility, Description = "Bonus for religion members."
            }
        };

        _perkEffectSystem.Setup(p => p.GetActivePerks(It.IsAny<string>()))
            .Returns((playerPerks, religionPerks));
        

        // Act
        var result = _sut!.OnPerksPlayer(args);

        // Assert
        var message = result.StatusMessage;
        Assert.Contains(string.Format(FormatStringConstants.HeaderUnlockedPlayerPerks, 1), message);
        Assert.Contains("Divine Strike", message);
        Assert.DoesNotContain("Religion Boon", message);
    }
}