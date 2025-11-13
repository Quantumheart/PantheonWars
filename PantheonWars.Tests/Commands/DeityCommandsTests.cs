using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using PantheonWars.Systems;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Xunit;

namespace PantheonWars.Tests.Commands;

/// <summary>
///     Tests for DeityCommands class
/// </summary>
public class DeityCommandsTests
{
    private readonly Mock<IDeityRegistry> _mockDeityRegistry;
    private readonly Mock<PlayerDataManager> _mockPlayerDataManager;
    private readonly Mock<IPlayerReligionDataManager> _mockReligionDataManager;
    private readonly Mock<ICoreServerAPI> _mockSapi;
    private readonly DeityCommands _commands;

    public DeityCommandsTests()
    {
        _mockSapi = TestFixtures.CreateMockServerAPI();
        _mockDeityRegistry = new Mock<IDeityRegistry>();
        _mockPlayerDataManager = new Mock<PlayerDataManager>(_mockSapi.Object);
        _mockReligionDataManager = new Mock<IPlayerReligionDataManager>();

        _commands = new DeityCommands(
            _mockSapi.Object,
            _mockDeityRegistry.Object,
            _mockPlayerDataManager.Object,
            _mockReligionDataManager.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var commands = new DeityCommands(
            _mockSapi.Object,
            _mockDeityRegistry.Object,
            _mockPlayerDataManager.Object,
            _mockReligionDataManager.Object
        );

        // Assert
        Assert.NotNull(commands);
    }

    #endregion

    #region RegisterCommands Tests

    [Fact]
    public void RegisterCommands_RegistersAllSubCommands()
    {
        // Act
        _commands.RegisterCommands();

        // Assert - Verify the main command was created
        var mockChatCommands = Mock.Get(_mockSapi.Object.ChatCommands);
        mockChatCommands.Verify(c => c.Create("deity"), Times.Once);

        // Verify notification was logged
        var mockLogger = Mock.Get(_mockSapi.Object.Logger);
        mockLogger.Verify(l => l.Notification("[PantheonWars] Deity commands registered"), Times.Once);
    }

    #endregion

    #region OnListDeities Tests

    [Fact]
    public void OnListDeities_WithNoDeities_ReturnsEmptyList()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object);

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(Enumerable.Empty<Deity>());

        // Act
        var result = InvokePrivateMethod("OnListDeities", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Available Deities", result.StatusMessage);
    }

    [Fact]
    public void OnListDeities_WithMultipleDeities_ListsAllDeities()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object);

        var deities = new List<Deity>
        {
            new Deity
            {
                Name = "Khoras",
                Domain = "War",
                Alignment = "Lawful",
                Description = "God of War"
            },
            new Deity
            {
                Name = "Lunara",
                Domain = "Moon",
                Alignment = "Neutral",
                Description = "Goddess of the Moon"
            }
        };

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(deities);

        // Act
        var result = InvokePrivateMethod("OnListDeities", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("War", result.StatusMessage);
        Assert.Contains("Lunara", result.StatusMessage);
        Assert.Contains("Moon", result.StatusMessage);
        Assert.Contains("God of War", result.StatusMessage);
        Assert.Contains("Goddess of the Moon", result.StatusMessage);
    }

    #endregion

    #region OnDeityInfo Tests

    [Fact]
    public void OnDeityInfo_WithNonExistentDeity_ReturnsError()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object, "NonExistent");

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(Enumerable.Empty<Deity>());

        // Act
        var result = InvokePrivateMethod("OnDeityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("NonExistent", result.StatusMessage);
        Assert.Contains("not found", result.StatusMessage);
    }

    [Fact]
    public void OnDeityInfo_WithValidDeity_ShowsDetailedInformation()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object, "Khoras");

        var deity = new Deity
        {
            Name = "Khoras",
            Domain = "War",
            Alignment = "Lawful",
            Description = "God of War and Honor",
            Playstyle = "Aggressive combat focused",
            Type = DeityType.Khoras,
            Relationships = new Dictionary<DeityType, string>
            {
                { DeityType.Lunara, "Ally" },
                { DeityType.Morthos, "Enemy" }
            }
        };

        var lunaraDeity = new Deity { Name = "Lunara", Type = DeityType.Lunara };
        var morthosDeity = new Deity { Name = "Morthos", Type = DeityType.Morthos };

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(new[] { deity });
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Lunara)).Returns(lunaraDeity);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Morthos)).Returns(morthosDeity);

        // Act
        var result = InvokePrivateMethod("OnDeityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("War", result.StatusMessage);
        Assert.Contains("Lawful", result.StatusMessage);
        Assert.Contains("God of War and Honor", result.StatusMessage);
        Assert.Contains("Aggressive combat focused", result.StatusMessage);
        Assert.Contains("Ally", result.StatusMessage);
        Assert.Contains("Lunara", result.StatusMessage);
        Assert.Contains("Enemy", result.StatusMessage);
        Assert.Contains("Morthos", result.StatusMessage);
    }

    [Fact]
    public void OnDeityInfo_WithCaseInsensitiveName_FindsDeity()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object, "khoras");

        var deity = new Deity
        {
            Name = "Khoras",
            Domain = "War",
            Alignment = "Lawful",
            Description = "God of War"
        };

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(new[] { deity });

        // Act
        var result = InvokePrivateMethod("OnDeityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras", result.StatusMessage);
    }

    [Fact]
    public void OnDeityInfo_WithNoRelationships_DoesNotShowRelationshipsSection()
    {
        // Arrange
        var args = TestFixtures.CreateCommandArgs(TestFixtures.CreateMockServerPlayer("test-player").Object, "Khoras");

        var deity = new Deity
        {
            Name = "Khoras",
            Domain = "War",
            Alignment = "Lawful",
            Description = "God of War",
            Relationships = new Dictionary<DeityType, string>()
        };

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(new[] { deity });

        // Act
        var result = InvokePrivateMethod("OnDeityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.DoesNotContain("Relationships:", result.StatusMessage);
    }

    #endregion

    #region OnSelectDeity Tests

    [Fact]
    public void OnSelectDeity_WithNonExistentDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "NonExistent");

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(Enumerable.Empty<Deity>());

        // Act
        var result = InvokePrivateMethod("OnSelectDeity", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("NonExistent", result.StatusMessage);
        Assert.Contains("not found", result.StatusMessage);
    }

    [Fact]
    public void OnSelectDeity_WhenAlreadyPledgedToSameDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Khoras");

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(new[] { deity });
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSelectDeity", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("already pledged to Khoras", result.StatusMessage);
    }

    [Fact]
    public void OnSelectDeity_WhenPledgedToDifferentDeity_ReturnsErrorAboutReligionSystem()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Khoras");

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var currentDeity = new Deity { Name = "Lunara", Type = DeityType.Lunara };
        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Lunara };

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(new[] { deity, currentDeity });
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Lunara)).Returns(currentDeity);
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSelectDeity", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("already pledged to Lunara", result.StatusMessage);
        Assert.Contains("religion system", result.StatusMessage);
    }

    [Fact]
    public void OnSelectDeity_WithNoCurrentDeity_InformsAboutReligionSystem()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "Khoras");

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };

        _mockDeityRegistry.Setup(m => m.GetAllDeities()).Returns(new[] { deity });
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnSelectDeity", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("pledge to Khoras", result.StatusMessage);
        Assert.Contains("join or create a religion", result.StatusMessage);
    }

    #endregion

    #region OnDeityStatus Tests

    [Fact]
    public void OnDeityStatus_WithNoDeity_ReturnsInfoMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnDeityStatus", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnDeityStatus_WithActiveDeity_ShowsFullStatus()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 150,
            FavorRank = FavorRank.Disciple,
            KillCount = 5,
            TotalFavorEarned = 1000,
            LastReligionSwitch = DateTime.UtcNow.AddDays(-30)
        };

        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns(deity);

        // Act
        var result = InvokePrivateMethod("OnDeityStatus", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("150", result.StatusMessage);
        Assert.Contains("Disciple", result.StatusMessage);
        Assert.Contains("5", result.StatusMessage);
        Assert.Contains("1000", result.StatusMessage);
        Assert.Contains("Days Served:", result.StatusMessage);
    }

    [Fact]
    public void OnDeityStatus_WithoutLastReligionSwitch_DoesNotShowDaysServed()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 150,
            FavorRank = FavorRank.Disciple,
            KillCount = 5,
            TotalFavorEarned = 1000,
            LastReligionSwitch = null
        };

        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns(deity);

        // Act
        var result = InvokePrivateMethod("OnDeityStatus", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.DoesNotContain("Days Served:", result.StatusMessage);
    }

    [Fact]
    public void OnDeityStatus_WithNullDeity_ShowsDeityType()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 150,
            FavorRank = FavorRank.Disciple,
            KillCount = 5,
            TotalFavorEarned = 1000
        };

        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns((Deity?)null);

        // Act
        var result = InvokePrivateMethod("OnDeityStatus", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Khoras", result.StatusMessage);
    }

    #endregion

    #region OnCheckFavor Tests

    [Fact]
    public void OnCheckFavor_WithNoDeity_ReturnsInfoMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnCheckFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnCheckFavor_WithActiveDeity_ShowsFavorAndRank()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var deity = new Deity { Name = "Khoras", Type = DeityType.Khoras };
        var religionData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            Favor = 250,
            FavorRank = FavorRank.Zealot
        };

        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockDeityRegistry.Setup(m => m.GetDeity(DeityType.Khoras)).Returns(deity);

        // Act
        var result = InvokePrivateMethod("OnCheckFavor", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("250 favor", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("Zealot", result.StatusMessage);
    }

    #endregion

    #region Helper Methods

    private TextCommandResult InvokePrivateMethod(string methodName, TextCommandCallingArgs args)
    {
        var method = typeof(DeityCommands).GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (TextCommandResult)method!.Invoke(_commands, new object[] { args })!;
    }

    #endregion
}
