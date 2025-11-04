using Moq;
using PantheonWars.Constants;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
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

        var playerPerks = new List<PantheonWars.Models.Perk>
        {
            new PantheonWars.Models.Perk
            {
                PerkId = "perk1",
                Name = "Divine Strike",
                Description = "Deals extra damage to enemies.",
                RequiredFavorRank = (int)FavorRank.Initiate,
                Kind = PerkKind.Player
            },
            new PantheonWars.Models.Perk
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

        var religionPerks = new List<PantheonWars.Models.Perk>
        {
            new PantheonWars.Models.Perk
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
                .Returns(new List<PantheonWars.Models.Perk>());
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(new List<PantheonWars.Models.Perk>());

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
                .Returns(new List<PantheonWars.Models.Perk>());
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(new List<PantheonWars.Models.Perk>());

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

            var religionPerks = new List<PantheonWars.Models.Perk>
            {
                new PantheonWars.Models.Perk
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
                .Returns(new List<PantheonWars.Models.Perk>());

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

        [Fact]
        public void OnPerksList_DisplaysFavorRankCorrectly()
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

            var playerPerks = new List<PantheonWars.Models.Perk>
            {
                new PantheonWars.Models.Perk
                {
                    PerkId = "perk_zealot",
                    Name = "Zealot Perk",
                    Description = "A perk for zealots.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    Kind = PerkKind.Player
                }
            };

            _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
                .Returns(playerData);

            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Player))
                .Returns(playerPerks);
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(new List<PantheonWars.Models.Perk>());

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Contains("Zealot Perk", result.StatusMessage);
            Assert.Contains(string.Format(FormatStringConstants.FormatRequiredRank, FavorRank.Zealot), result.StatusMessage);
        }

        [Fact]
        public void OnPerksList_DisplaysPrestigeRankCorrectly()
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

            var religionPerks = new List<PantheonWars.Models.Perk>
            {
                new PantheonWars.Models.Perk
                {
                    PerkId = "perk_established",
                    Name = "Established Perk",
                    Description = "A perk for established religions.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    Kind = PerkKind.Religion
                }
            };

            _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
                .Returns(playerData);

            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Player))
                .Returns(new List<PantheonWars.Models.Perk>());
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(religionPerks);

            var religion = new ReligionData()
            {
                ReligionUID = "religion-uid",
                UnlockedPerks = new Dictionary<string, bool>()
            };

            _religionManager.Setup(rm => rm.GetReligion("religion-uid"))
                .Returns(religion);

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Contains("Established Perk", result.StatusMessage);
            Assert.Contains(string.Format(FormatStringConstants.FormatRequiredRank, PrestigeRank.Established), result.StatusMessage);
        }

        [Fact]
        public void OnPerksList_DisplaysPerkIdAndDescription()
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

            var playerPerks = new List<PantheonWars.Models.Perk>
            {
                new PantheonWars.Models.Perk
                {
                    PerkId = "unique_perk_id_123",
                    Name = "Test Perk",
                    Description = "This is a unique test description.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    Kind = PerkKind.Player
                }
            };

            _playerReligionDataManager.Setup(prdm => prdm.GetOrCreatePlayerData(It.IsAny<string>()))
                .Returns(playerData);

            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Player))
                .Returns(playerPerks);
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(new List<PantheonWars.Models.Perk>());

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Contains("Test Perk", result.StatusMessage);
            Assert.Contains(string.Format(FormatStringConstants.FormatPerkId, "unique_perk_id_123"), result.StatusMessage);
            Assert.Contains(string.Format(FormatStringConstants.FormatDescription, "This is a unique test description."), result.StatusMessage);
        }

        [Fact]
        public void OnPerksList_OutputFormatContainsAllSections()
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

            var playerPerks = new List<PantheonWars.Models.Perk>
            {
                new PantheonWars.Models.Perk
                {
                    PerkId = "player_perk",
                    Name = "Player Perk",
                    Description = "Player description",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    Kind = PerkKind.Player
                }
            };

            var religionPerks = new List<PantheonWars.Models.Perk>
            {
                new PantheonWars.Models.Perk
                {
                    PerkId = "religion_perk",
                    Name = "Religion Perk",
                    Description = "Religion description",
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
                UnlockedPerks = new Dictionary<string, bool>()
            };

            _religionManager.Setup(rm => rm.GetReligion("religion-uid"))
                .Returns(religion);

            // Act
            var result = _sut.OnPerksList(args);

            // Assert - Verify all expected sections are present
            var message = result.StatusMessage;

            // Header
            Assert.Contains(string.Format(FormatStringConstants.HeaderPerksForDeity, DeityType.Aethra), message);

            // Player perks section
            Assert.Contains(FormatStringConstants.HeaderPlayerPerks, message);
            Assert.Contains("Player Perk", message);

            // Religion perks section
            Assert.Contains(FormatStringConstants.HeaderReligionPerks, message);
            Assert.Contains("Religion Perk", message);

            // Verify section ordering: header appears before player perks, player perks before religion perks
            var headerIndex = message.IndexOf(string.Format(FormatStringConstants.HeaderPerksForDeity, DeityType.Aethra));
            var playerPerksIndex = message.IndexOf(FormatStringConstants.HeaderPlayerPerks);
            var religionPerksIndex = message.IndexOf(FormatStringConstants.HeaderReligionPerks);

            Assert.True(headerIndex < playerPerksIndex, "Header should appear before player perks section");
            Assert.True(playerPerksIndex < religionPerksIndex, "Player perks section should appear before religion perks section");
        }
}