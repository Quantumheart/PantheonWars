using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PantheonWars.Tests.Commands.Helpers;

[ExcludeFromCodeCoverage]
public class PerkCommandsTestHelpers
{
    protected Mock<ICoreServerAPI> _mockSapi;
    protected Mock<ICoreAPI> _mockApi;
    protected Mock<IPerkRegistry> _perkRegistry;
    protected Mock<IPlayerReligionDataManager> _playerReligionDataManager;
    protected Mock<IReligionManager> _religionManager;
    protected Mock<IPerkEffectSystem> _perkEffectSystem;
    protected PerkCommands? _sut;

    protected PerkCommandsTestHelpers()
    {
        _mockSapi = new Mock<ICoreServerAPI>();
        _mockApi = new Mock<ICoreAPI>();

        var mockLogger = new Mock<ILogger>();
        _mockApi.Setup(api => api.Logger).Returns(mockLogger.Object);
        _mockSapi.Setup(sapi => sapi.Logger).Returns(mockLogger.Object);

        _perkRegistry = new Mock<IPerkRegistry>();
        _religionManager = new Mock<IReligionManager>();
        _playerReligionDataManager = new Mock<IPlayerReligionDataManager>();
        _perkEffectSystem = new Mock<IPerkEffectSystem>();
    }
    
    protected PerkCommands InitializeMocksAndSut()
    {

        return new PerkCommands(
            _mockSapi.Object,
            _perkRegistry.Object,
            _playerReligionDataManager.Object,
            _religionManager.Object,
            _perkEffectSystem.Object);
    }
}