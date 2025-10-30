using Moq;
using PantheonWars.Constants;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Tests.Commands.Helpers;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PantheonWars.Tests.Commands.Perk;

public class PerkCommandListTests : PerkCommandsTestHelpers
{
    public PerkCommandListTests()
    {
        _sut = InitializeMocksAndSut();
    }

    [Fact]
    public void OnPerksList_PlayerInReligionWithPerks_ReturnsFormattedList()
    {
        // Arrange
        var args = new TextCommandCallingArgs
        {
            Caller = new Caller()
            {
                Player = new Mock<IServerPlayer>().Object
            }
        };

        var playerData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Aethra,
            ReligionUID = "religion-uid"
        };

        var playerPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                PerkId = "perk1",
                Name = "Divine Strike",
                Description = "Deals extra damage to enemies.",
                RequiredFavorRank = (int)FavorRank.Initiate,
                Kind = PerkKind.Player
            },
            new Models.Perk
            {
                PerkId = "perk2",
                Name = "Faithful Guardian",
                Description = "Reduces damage taken by 20%.",
                RequiredFavorRank = (int)FavorRank.Initiate,
                Kind = PerkKind.Player
            }
        };

        foreach (var playerPerk in playerPerks)
        {
            playerData.UnlockPerk(playerPerk.PerkId);
        }

        var religionPerks = new List<Models.Perk>
        {
            new Models.Perk
            {
                PerkId = "religionperk1",
                Name = "Sacred Flame",
                Description = "Inflicts divine fire on enemies.",
                RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                Kind = PerkKind.Religion
            }
        };

        _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
            .Returns(playerData);

        _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Player))
            .Returns(playerPerks);
        _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
            .Returns(religionPerks);

        var religion = new ReligionData()
        {
            ReligionUID = "religion-uid",
            UnlockedPerks = new Dictionary<string, bool>
            {
                { "religionperk1", true }
            }
        };

        _religionManager.Setup(rm => rm.GetReligion("religion-uid"))
            .Returns(religion);

        _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
            .Returns(playerData);

        // Act
        var result = _sut.OnPerksList(args);

        // Assert
        Assert.Contains(string.Format(FormatStringConstants.HeaderPerksForDeity, DeityType.Aethra),
            result.StatusMessage);
        Assert.Contains(FormatStringConstants.HeaderPlayerPerks, result.StatusMessage);
        Assert.Contains("Divine Strike [UNLOCKED]", result.StatusMessage);
        Assert.Contains("Faithful Guardian", result.StatusMessage);
        Assert.Contains(FormatStringConstants.HeaderReligionPerks, result.StatusMessage);
        Assert.Contains("Sacred Flame [UNLOCKED]", result.StatusMessage);
    }
    
            [Fact]
        public void OnPerksList_PlayerNotFound_ReturnsError()
        {
            // Arrange
            var args = new TextCommandCallingArgs
            {
                Caller = new Mock<Caller>().Object
            };

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Equal(ErrorMessageConstants.ErrorPlayerNotFound, result.StatusMessage);
        }
        
        [Fact]
        public void OnPerksList_PlayerNotInReligion_ReturnsError()
        {
            // Arrange
            var args = new TextCommandCallingArgs
            {
                Caller = new Caller()
                {
                    Player = new Mock<IServerPlayer>().Object
                }
            };

            _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
                .Returns(new PlayerReligionData { ActiveDeity = DeityType.None });

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Equal(ErrorMessageConstants.ErrorMustJoinReligion, result.StatusMessage);
        }
        
        [Fact]
        public void OnPerksList_PlayerInReligionWithNoPerks_ReturnsSuccess()
        {
            // Arrange
            var args = new TextCommandCallingArgs
            {
                Caller = new Caller()
                {
                    Player = new Mock<IServerPlayer>().Object
                }
            };

            var playerData = new PlayerReligionData
            {
                ActiveDeity = DeityType.Aethra,
                ReligionUID = "religion-uid"
            };

            _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
                .Returns(playerData);

            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Player))
                .Returns(new List<Models.Perk>());
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(new List<Models.Perk>());

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Contains(string.Format(FormatStringConstants.HeaderPerksForDeity, DeityType.Aethra), result.StatusMessage);
            Assert.Contains(FormatStringConstants.HeaderPlayerPerks, result.StatusMessage);
            Assert.Contains(FormatStringConstants.HeaderReligionPerks, result.StatusMessage);
        }



        [Fact]
        public void OnPerksList_PlayerInReligionWithNoPerks_ReturnsSuccessMessage()
        {
            // Arrange
            var args = new TextCommandCallingArgs
            {
                Caller = new Caller()
                {
                    Player = new Mock<IServerPlayer>().Object
                }
            };

            var playerData = new PlayerReligionData
            {
                ActiveDeity = DeityType.Aethra,
                ReligionUID = "religion-uid"
            };

            _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
                .Returns(playerData);

            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Player))
                .Returns(new List<Models.Perk>());
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(new List<Models.Perk>());

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Contains(string.Format(FormatStringConstants.HeaderPerksForDeity, DeityType.Aethra),
                result.StatusMessage);
            Assert.Contains(FormatStringConstants.HeaderPlayerPerks, result.StatusMessage);
            Assert.Contains(FormatStringConstants.HeaderReligionPerks, result.StatusMessage);
        }

        [Fact]
        public void OnPerksList_PlayerInReligionWithReligionPerks_ReturnsFormattedList()
        {
            // Arrange
            var args = new TextCommandCallingArgs
            {
                Caller = new Caller()
                {
                    Player = new Mock<IServerPlayer>().Object
                }
            };

            var playerData = new PlayerReligionData
            {
                ActiveDeity = DeityType.Aethra,
                ReligionUID = "religion-uid",
            };

            var religionPerks = new List<Models.Perk>
            {
                new Models.Perk
                {
                    PerkId = "religionperk1",
                    Name = "Sacred Flame",
                    Description = "Inflicts divine fire on enemies.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    Kind = PerkKind.Religion
                }
            };
            
            _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
                .Returns(playerData);

            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(religionPerks);
            
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Player))
                .Returns(new List<Models.Perk>());

            var religion = new ReligionData()
            {
                ReligionUID = "religion-uid",
                UnlockedPerks = new Dictionary<string, bool>
                {
                    { "religionperk1", true }
                }
            };

            _religionManager.Setup(rm => rm.GetReligion("religion-uid"))
                .Returns(religion);

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Contains(string.Format(FormatStringConstants.HeaderPerksForDeity, DeityType.Aethra),
                result.StatusMessage);
            Assert.Contains(FormatStringConstants.HeaderReligionPerks, result.StatusMessage);
            Assert.Contains("Sacred Flame [UNLOCKED]", result.StatusMessage);
        }
}