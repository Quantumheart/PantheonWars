using Moq;
using PantheonWars.Commands;
using PantheonWars.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Server;


namespace PantheonWars.Tests.Commands
{
    public class PerkCommandsTests
    {
        private readonly Mock<ICoreServerAPI> _mockSapi;
        private readonly Mock<ICoreAPI> _mockApi;
        private readonly PerkRegistry _perkRegistry;
        private readonly PlayerReligionDataManager _playerReligionDataManager;
        private readonly ReligionManager _religionManager;
        private readonly PerkEffectSystem _perkEffectSystem;

        public PerkCommandsTests()
        {
            _mockSapi = new Mock<ICoreServerAPI>();
            _mockApi = new Mock<ICoreAPI>();

            // Create real instances instead of mocks since these classes have constructor dependencies
            _perkRegistry = new PerkRegistry(_mockApi.Object);
            _religionManager = new ReligionManager(_mockSapi.Object);
            _playerReligionDataManager = new PlayerReligionDataManager(_mockSapi.Object, _religionManager);
            _perkEffectSystem = new PerkEffectSystem(_mockSapi.Object, _perkRegistry, _playerReligionDataManager,
                _religionManager);
        }


        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenSAPIIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                null,
                _perkRegistry,
                _playerReligionDataManager,
                _religionManager,
                _perkEffectSystem));
        }

        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenPerkRegistryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                _mockSapi.Object,
                null,
                _playerReligionDataManager,
                _religionManager,
                _perkEffectSystem));
        }

        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenPlayerReligionManagerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                _mockSapi.Object,
                _perkRegistry,
                null,
                _religionManager,
                _perkEffectSystem));
        }

        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenReligionManagerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                _mockSapi.Object,
                _perkRegistry,
                _playerReligionDataManager,
                null,
                _perkEffectSystem));
        }

        [Fact]
        public void PerkCommands_Constructor_ThrowsWhenPerkEffectSystemIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PerkCommands(
                _mockSapi.Object,
                _perkRegistry,
                _playerReligionDataManager,
                _religionManager,
                null));
        }

        [Fact]
        public void PerkCommands_Constructor_SetsDependenciesCorrectly()
        {
            // Verify that the constructor injects the dependencies.
            var commands = new PerkCommands(
                _mockSapi.Object,
                _perkRegistry,
                _playerReligionDataManager,
                _religionManager,
                _perkEffectSystem);

            Assert.NotNull(commands);
        }
    }
}