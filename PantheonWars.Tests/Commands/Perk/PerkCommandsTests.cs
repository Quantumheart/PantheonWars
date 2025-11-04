using System.Diagnostics.CodeAnalysis;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Constants;
using PantheonWars.Tests.Commands.Helpers;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PantheonWars.Tests.Commands.Perk;

[ExcludeFromCodeCoverage]
public class PerkCommandsTests : PerkCommandsTestHelpers
{
    public PerkCommandsTests()
    {
        _sut = InitializeMocksAndSut();
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
        mockSapi.Verify(c => c.Logger.Notification(LogMessageConstants.LogPerkCommandsRegistered), Times.Once);
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
    public void ShouldConfigureRequiresPlayerAndPrivilege()
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

        // Act
        perkCommands.RegisterCommands();

        // Assert - Verify that RequiresPlayer() and RequiresPrivilege() were called
        mockCommandBuilder.Verify(b => b.RequiresPlayer(), Times.Once,
            "RequiresPlayer() should be called once during command registration");
        mockCommandBuilder.Verify(b => b.RequiresPrivilege(Privilege.chat), Times.Once,
            "RequiresPrivilege(Privilege.chat) should be called once during command registration");
    }
}