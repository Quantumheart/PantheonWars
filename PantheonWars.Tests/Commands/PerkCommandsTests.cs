using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Constants;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Server;


namespace PantheonWars.Tests.Commands
{
    [ExcludeFromCodeCoverage]
    public class PerkCommandsTests
    {
        private readonly Mock<ICoreServerAPI> _mockSapi;
        private readonly Mock<ICoreAPI> _mockApi;
        private readonly Mock<IPerkRegistry> _perkRegistry;
        private readonly Mock<IPlayerReligionDataManager> _playerReligionDataManager;
        private readonly Mock<IReligionManager> _religionManager;
        private readonly Mock<IPerkEffectSystem> _perkEffectSystem;
        private readonly PerkCommands _sut;

        public PerkCommandsTests()
        {
            _mockSapi = new Mock<ICoreServerAPI>();
            _mockApi = new Mock<ICoreAPI>();

            // Setup mock logger
            var mockLogger = new Mock<ILogger>();
            _mockApi.Setup(api => api.Logger).Returns(mockLogger.Object);
            _mockSapi.Setup(sapi => sapi.Logger).Returns(mockLogger.Object);
            _perkRegistry = new Mock<IPerkRegistry>();
            _religionManager = new Mock<IReligionManager>();
            _playerReligionDataManager = new Mock<IPlayerReligionDataManager>();
            _perkEffectSystem = new Mock<IPerkEffectSystem>();

            _sut = new PerkCommands(
                _mockSapi.Object,
                _perkRegistry.Object,
                _playerReligionDataManager.Object,
                _religionManager.Object,
                _perkEffectSystem.Object);
        }


        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenSAPIIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                null,
                _perkRegistry.Object,
                _playerReligionDataManager.Object,
                _religionManager.Object,
                _perkEffectSystem.Object));
        }

        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenPerkRegistryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                _mockSapi.Object,
                null,
                _playerReligionDataManager.Object,
                _religionManager.Object,
                _perkEffectSystem.Object));
        }

        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenPlayerReligionManagerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                _mockSapi.Object,
                _perkRegistry.Object,
                null,
                _religionManager.Object,
                _perkEffectSystem.Object));
        }

        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenReligionManagerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                _mockSapi.Object,
                _perkRegistry.Object,
                _playerReligionDataManager.Object,
                null,
                _perkEffectSystem.Object));
        }

        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenPerkEffectSystemIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                _mockSapi.Object,
                _perkRegistry.Object,
                _playerReligionDataManager.Object,
                _religionManager.Object,
                null));
        }

        [Fact]
        public void PerkCommands_Constructor_SetsDependenciesCorrectly()
        {
            // Verify that the constructor injects the dependencies.
            var commands = new PerkCommands(
                _mockSapi.Object,
                _perkRegistry.Object,
                _playerReligionDataManager.Object,
                _religionManager.Object,
                _perkEffectSystem.Object);

            Assert.NotNull(commands);
        }

        [Fact]
        public void RegisterCommands_ExecutesWithoutException()
        {
            // Arrange
            var mockSapi = new Mock<ICoreServerAPI>();
            var mockChatCommands = new Mock<IChatCommandApi>();
            var mockCommandBuilder = new Mock<IChatCommand>();
            var mockLogger = new Mock<ILogger>();

            // Use real CommandArgumentParsers instead of mocking it
            var realParsers = new CommandArgumentParsers(mockSapi.Object);
            mockChatCommands.Setup(c => c.Parsers).Returns(realParsers);

            // Setup fluent builder chain - all methods return the builder itself
            mockCommandBuilder.Setup(b => b.WithDescription(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.RequiresPlayer()).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.RequiresPrivilege(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.BeginSubCommand(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.WithArgs(It.IsAny<ICommandArgumentParser>()))
                .Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.HandleWith(It.IsAny<OnCommandDelegate>()))
                .Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.EndSubCommand()).Returns(mockCommandBuilder.Object);

            mockChatCommands.Setup(c => c.Create(It.IsAny<string>())).Returns(mockCommandBuilder.Object);

            mockSapi.Setup(s => s.ChatCommands).Returns(mockChatCommands.Object);
            mockSapi.Setup(s => s.Logger).Returns(mockLogger.Object);

            // Create PerkCommands with properly configured mock
            var perkCommands = new PerkCommands(
                mockSapi.Object,
                _perkRegistry.Object,
                _playerReligionDataManager.Object,
                _religionManager.Object,
                _perkEffectSystem.Object);

            // Act & Assert - should not throw any exceptions
            var exception = Record.Exception(() => perkCommands.RegisterCommands());

            Assert.Null(exception);
            mockChatCommands.Verify(c => c.Create("perks"), Times.Once);
        }


        [Fact]
        public void RegisterCommands_ConfiguresSubCommandsCorrectly()
        {
            // Arrange
            var mockSapi = new Mock<ICoreServerAPI>();
            var mockChatCommands = new Mock<IChatCommandApi>();
            var mockCommandBuilder = new Mock<IChatCommand>();
            var mockLogger = new Mock<ILogger>();

            // Use real CommandArgumentParsers instead of mocking it
            var realParsers = new CommandArgumentParsers(mockSapi.Object);
            mockChatCommands.Setup(c => c.Parsers).Returns(realParsers);

            // Setup fluent builder chain - all methods return the builder itself
            // Each method in the chain must be mocked separately for fluent APIs
            mockCommandBuilder.Setup(b => b.WithDescription(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.RequiresPlayer()).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.RequiresPrivilege(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.BeginSubCommand(It.IsAny<string>())).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.WithArgs(It.IsAny<ICommandArgumentParser>()))
                .Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.HandleWith(It.IsAny<OnCommandDelegate>()))
                .Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.EndSubCommand()).Returns(mockCommandBuilder.Object);

            mockChatCommands.Setup(c => c.Create(It.IsAny<string>())).Returns(mockCommandBuilder.Object);

            mockSapi.Setup(s => s.ChatCommands).Returns(mockChatCommands.Object);
            mockSapi.Setup(s => s.Logger).Returns(mockLogger.Object);

            // Create PerkCommands with properly configured mock
            var perkCommands = new PerkCommands(
                mockSapi.Object,
                _perkRegistry.Object,
                _playerReligionDataManager.Object,
                _religionManager.Object,
                _perkEffectSystem.Object);

            // Act
            perkCommands.RegisterCommands();

            // Verify subcommand configurations
            mockChatCommands.Verify(c => c.Create(PerkCommandConstants.CommandName), Times.Once);
            mockCommandBuilder.Verify(b => b.WithDescription(PerkDescriptionConstants.CommandDescription),
                Times.Once);
            mockCommandBuilder.Verify(b => b.BeginSubCommand(PerkCommandConstants.SubCommandList), Times.Once);
            mockCommandBuilder.Verify(b => b.BeginSubCommand(PerkCommandConstants.SubCommandPlayer), Times.Once);
            mockCommandBuilder.Verify(b => b.BeginSubCommand(PerkCommandConstants.SubCommandReligion), Times.Once);
            mockCommandBuilder.Verify(b => b.BeginSubCommand(PerkCommandConstants.SubCommandInfo), Times.Once);
            mockCommandBuilder.Verify(b => b.BeginSubCommand(PerkCommandConstants.SubCommandTree), Times.Once);
            mockCommandBuilder.Verify(b => b.BeginSubCommand(PerkCommandConstants.SubCommandUnlock), Times.Once);
            mockCommandBuilder.Verify(b => b.BeginSubCommand(PerkCommandConstants.SubCommandActive), Times.Once);
            mockCommandBuilder.Verify(b => b.EndSubCommand(), Times.Exactly(7)); // 7 subcommands
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
                .Returns(new List<Perk>());
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(new List<Perk>());

            // Act
            var result = _sut.OnPerksList(args);

            // Assert
            Assert.Contains(string.Format(FormatStringConstants.HeaderPerksForDeity, DeityType.Aethra), result.StatusMessage);
            Assert.Contains(FormatStringConstants.HeaderPlayerPerks, result.StatusMessage);
            Assert.Contains(FormatStringConstants.HeaderReligionPerks, result.StatusMessage);
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

            var playerPerks = new List<Perk>
            {
                new Perk
                {
                    PerkId = "perk1",
                    Name = "Divine Strike",
                    Description = "Deals extra damage to enemies.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    Kind = PerkKind.Player
                },
                new Perk
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

            var religionPerks = new List<Perk>
            {
                new Perk
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
                .Returns(new List<Perk>());
            _perkRegistry.Setup(pr => pr.GetPerksForDeity(DeityType.Aethra, PerkKind.Religion))
                .Returns(new List<Perk>());

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

            var religionPerks = new List<Perk>
            {
                new Perk
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
                .Returns(new List<Perk>());

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
}