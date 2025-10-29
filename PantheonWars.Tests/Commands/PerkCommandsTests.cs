using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Commands;
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
            mockCommandBuilder.Setup(b => b.WithArgs(It.IsAny<ICommandArgumentParser>())).Returns(mockCommandBuilder.Object);
            mockCommandBuilder.Setup(b => b.HandleWith(It.IsAny<OnCommandDelegate>())).Returns(mockCommandBuilder.Object);
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

    }
}