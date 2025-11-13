using System.Collections.Generic;
using Moq;
using PantheonWars.Commands;
using PantheonWars.Constants;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Xunit;

namespace PantheonWars.Tests.Commands;

/// <summary>
///     Tests for BlessingCommands class
/// </summary>
public class BlessingCommandsTests
{
    private readonly Mock<IBlessingEffectSystem> _mockBlessingEffectSystem;
    private readonly Mock<IBlessingRegistry> _mockBlessingRegistry;
    private readonly Mock<IPlayerReligionDataManager> _mockPlayerDataManager;
    private readonly Mock<IReligionManager> _mockReligionManager;
    private readonly Mock<ICoreServerAPI> _mockSapi;
    private readonly BlessingCommands _commands;

    public BlessingCommandsTests()
    {
        _mockSapi = TestFixtures.CreateMockServerAPI();
        _mockBlessingRegistry = new Mock<IBlessingRegistry>();
        _mockPlayerDataManager = new Mock<IPlayerReligionDataManager>();
        _mockReligionManager = new Mock<IReligionManager>();
        _mockBlessingEffectSystem = new Mock<IBlessingEffectSystem>();

        _commands = new BlessingCommands(
            _mockSapi.Object,
            _mockBlessingRegistry.Object,
            _mockPlayerDataManager.Object,
            _mockReligionManager.Object,
            _mockBlessingEffectSystem.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var commands = new BlessingCommands(
            _mockSapi.Object,
            _mockBlessingRegistry.Object,
            _mockPlayerDataManager.Object,
            _mockReligionManager.Object,
            _mockBlessingEffectSystem.Object
        );

        // Assert
        Assert.NotNull(commands);
    }

    [Fact]
    public void Constructor_WithNullSapi_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BlessingCommands(
            null,
            _mockBlessingRegistry.Object,
            _mockPlayerDataManager.Object,
            _mockReligionManager.Object,
            _mockBlessingEffectSystem.Object
        ));
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
        mockChatCommands.Verify(c => c.Create(BlessingCommandConstants.CommandName), Times.Once);

        // Verify notification was logged
        var mockLogger = Mock.Get(_mockSapi.Object.Logger);
        mockLogger.Verify(l => l.Notification(LogMessageConstants.LogBlessingCommandsRegistered), Times.Once);
    }

    #endregion

    #region OnBlessingsList Tests

    [Fact]
    public void OnBlessingsList_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokeInternalMethod("OnBlessingsList", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains(ErrorMessageConstants.ErrorMustJoinReligion, result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsList_WithValidDeity_ListsPlayerAndReligionBlessings()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            ReligionUID = "religion-1"
        };
        playerData.UnlockBlessing("blessing1");

        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true)
        {
            ReligionUID = "religion-1"
        };
        religion.UnlockedBlessings["blessing2"] = true;

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-1")).Returns(religion);

        var playerBlessing = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Player Blessing",
            Deity = DeityType.Khoras,
            Kind = BlessingKind.Player,
            RequiredFavorRank = (int)FavorRank.Initiate,
            Description = "A player blessing"
        };

        var religionBlessing = new Blessing
        {
            BlessingId = "blessing2",
            Name = "Religion Blessing",
            Deity = DeityType.Khoras,
            Kind = BlessingKind.Religion,
            RequiredPrestigeRank = (int)PrestigeRank.Emerging,
            Description = "A religion blessing"
        };

        _mockBlessingRegistry.Setup(m => m.GetBlessingsForDeity(DeityType.Khoras, BlessingKind.Player))
            .Returns(new List<Blessing> { playerBlessing });
        _mockBlessingRegistry.Setup(m => m.GetBlessingsForDeity(DeityType.Khoras, BlessingKind.Religion))
            .Returns(new List<Blessing> { religionBlessing });

        // Act
        var result = InvokeInternalMethod("OnBlessingsList", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Player Blessing", result.StatusMessage);
        Assert.Contains("Religion Blessing", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
    }

    #endregion

    #region OnBlessingsPlayer Tests

    [Fact]
    public void OnBlessingsPlayer_WithNoActiveBlessings_ReturnsInfoMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        _mockBlessingEffectSystem.Setup(m => m.GetActiveBlessings("test-player"))
            .Returns((new List<Blessing>(), new List<Blessing>()));

        // Act
        var result = InvokeInternalMethod("OnBlessingsPlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains(InfoMessageConstants.InfoNoPlayerBlessings, result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsPlayer_WithActiveBlessings_ListsBlessingsWithEffects()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var blessing = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Test Blessing",
            Category = "Combat",
            Description = "A test blessing",
            StatModifiers = new Dictionary<string, float>
            {
                { "walkspeed", 0.1f }
            }
        };

        _mockBlessingEffectSystem.Setup(m => m.GetActiveBlessings("test-player"))
            .Returns((new List<Blessing> { blessing }, new List<Blessing>()));

        // Act
        var result = InvokeInternalMethod("OnBlessingsPlayer", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Test Blessing", result.StatusMessage);
        Assert.Contains("Combat", result.StatusMessage);
        Assert.Contains("walkspeed", result.StatusMessage);
    }

    #endregion

    #region OnBlessingsReligion Tests

    [Fact]
    public void OnBlessingsReligion_WithNoReligion_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = null };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokeInternalMethod("OnBlessingsReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains(ErrorMessageConstants.ErrorNoReligion, result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsReligion_WithNoReligionBlessings_ReturnsInfoMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerData = new PlayerReligionData { ReligionUID = "religion-1" };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);
        _mockReligionManager.Setup(m => m.GetReligion("religion-1")).Returns(religion);

        _mockBlessingEffectSystem.Setup(m => m.GetActiveBlessings("test-player"))
            .Returns((new List<Blessing>(), new List<Blessing>()));

        // Act
        var result = InvokeInternalMethod("OnBlessingsReligion", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains(InfoMessageConstants.InfoNoReligionBlessings, result.StatusMessage);
    }

    #endregion

    #region OnBlessingsInfo Tests

    [Fact]
    public void OnBlessingsInfo_WithEmptyBlessingId_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "");

        // Act
        var result = InvokeInternalMethod("OnBlessingsInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains(UsageMessageConstants.UsageBlessingsInfo, result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsInfo_WithNonExistentBlessing_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "nonexistent");

        _mockBlessingRegistry.Setup(m => m.GetBlessing("nonexistent")).Returns((Blessing?)null);

        // Act
        var result = InvokeInternalMethod("OnBlessingsInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("nonexistent", result.StatusMessage);
        Assert.Contains(ErrorMessageConstants.ErrorBlessingNotFound.Split('{')[0].Trim(), result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsInfo_WithValidBlessing_ShowsDetailedInformation()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "blessing1");

        var blessing = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Test Blessing",
            Deity = DeityType.Khoras,
            Kind = BlessingKind.Player,
            Category = "Combat",
            Description = "A detailed test blessing",
            RequiredFavorRank = (int)FavorRank.Disciple,
            PrerequisiteBlessings = new List<string> { "prereq1" },
            StatModifiers = new Dictionary<string, float> { { "walkspeed", 0.1f } },
            SpecialEffects = new List<string> { "Special Effect 1" }
        };

        var prereqBlessing = new Blessing
        {
            BlessingId = "prereq1",
            Name = "Prerequisite Blessing"
        };

        _mockBlessingRegistry.Setup(m => m.GetBlessing("blessing1")).Returns(blessing);
        _mockBlessingRegistry.Setup(m => m.GetBlessing("prereq1")).Returns(prereqBlessing);

        // Act
        var result = InvokeInternalMethod("OnBlessingsInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Test Blessing", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("Prerequisite Blessing", result.StatusMessage);
        Assert.Contains("walkspeed", result.StatusMessage);
        Assert.Contains("Special Effect 1", result.StatusMessage);
    }

    #endregion

    #region OnBlessingsTree Tests

    [Fact]
    public void OnBlessingsTree_WithNoDeity_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "player");

        var playerData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        // Act
        var result = InvokeInternalMethod("OnBlessingsTree", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains(ErrorMessageConstants.ErrorMustJoinReligionForTree, result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsTree_WithPlayerType_ShowsPlayerBlessingTree()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "player");

        var playerData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            ReligionUID = "religion-1"
        };
        playerData.UnlockBlessing("blessing1");

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);
        _mockReligionManager.Setup(m => m.GetReligion("religion-1")).Returns(religion);

        var blessing1 = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Initiate Blessing",
            RequiredFavorRank = (int)FavorRank.Initiate,
            PrerequisiteBlessings = new List<string>()
        };

        var blessing2 = new Blessing
        {
            BlessingId = "blessing2",
            Name = "Disciple Blessing",
            RequiredFavorRank = (int)FavorRank.Disciple,
            PrerequisiteBlessings = new List<string> { "blessing1" }
        };

        _mockBlessingRegistry.Setup(m => m.GetBlessingsForDeity(DeityType.Khoras, BlessingKind.Player))
            .Returns(new List<Blessing> { blessing1, blessing2 });
        _mockBlessingRegistry.Setup(m => m.GetBlessing("blessing1")).Returns(blessing1);

        // Act
        var result = InvokeInternalMethod("OnBlessingsTree", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Initiate Blessing", result.StatusMessage);
        Assert.Contains("Disciple Blessing", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsTree_WithReligionType_ShowsReligionBlessingTree()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "religion");

        var playerData = new PlayerReligionData
        {
            ActiveDeity = DeityType.Khoras,
            ReligionUID = "religion-1"
        };

        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);

        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);
        religion.UnlockedBlessings["blessing1"] = true;

        _mockReligionManager.Setup(m => m.GetReligion("religion-1")).Returns(religion);

        var blessing = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Religion Blessing",
            RequiredPrestigeRank = (int)PrestigeRank.Emerging,
            PrerequisiteBlessings = new List<string>()
        };

        _mockBlessingRegistry.Setup(m => m.GetBlessingsForDeity(DeityType.Khoras, BlessingKind.Religion))
            .Returns(new List<Blessing> { blessing });

        // Act
        var result = InvokeInternalMethod("OnBlessingsTree", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Religion Blessing", result.StatusMessage);
        Assert.Contains("Religion", result.StatusMessage);
    }

    #endregion

    #region OnBlessingsUnlock Tests

    [Fact]
    public void OnBlessingsUnlock_WithEmptyBlessingId_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "");

        // Act
        var result = InvokeInternalMethod("OnBlessingsUnlock", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains(UsageMessageConstants.UsageBlessingsUnlock, result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsUnlock_WhenCannotUnlock_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "blessing1");

        var blessing = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Test Blessing",
            Kind = BlessingKind.Player
        };

        var playerData = new PlayerReligionData
        {
            ReligionUID = "religion-1",
            ActiveDeity = DeityType.Khoras
        };

        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);

        _mockBlessingRegistry.Setup(m => m.GetBlessing("blessing1")).Returns(blessing);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-1")).Returns(religion);
        _mockBlessingRegistry.Setup(m => m.CanUnlockBlessing(playerData, religion, blessing))
            .Returns((false, "Insufficient favor"));

        // Act
        var result = InvokeInternalMethod("OnBlessingsUnlock", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("Insufficient favor", result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsUnlock_PlayerBlessing_UnlocksAndRefreshes()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "blessing1");

        var blessing = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Test Blessing",
            Kind = BlessingKind.Player
        };

        var playerData = new PlayerReligionData
        {
            ReligionUID = "religion-1",
            ActiveDeity = DeityType.Khoras
        };

        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);

        _mockBlessingRegistry.Setup(m => m.GetBlessing("blessing1")).Returns(blessing);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-1")).Returns(religion);
        _mockBlessingRegistry.Setup(m => m.CanUnlockBlessing(playerData, religion, blessing))
            .Returns((true, ""));
        _mockPlayerDataManager.Setup(m => m.UnlockPlayerBlessing("test-player", "blessing1"))
            .Returns(true);

        // Act
        var result = InvokeInternalMethod("OnBlessingsUnlock", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Test Blessing", result.StatusMessage);
        _mockBlessingEffectSystem.Verify(m => m.RefreshPlayerBlessings("test-player"), Times.Once);
    }

    [Fact]
    public void OnBlessingsUnlock_ReligionBlessing_AsNonFounder_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "blessing1");

        var blessing = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Test Blessing",
            Kind = BlessingKind.Religion
        };

        var playerData = new PlayerReligionData
        {
            ReligionUID = "religion-1",
            ActiveDeity = DeityType.Khoras
        };

        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true);

        _mockBlessingRegistry.Setup(m => m.GetBlessing("blessing1")).Returns(blessing);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-1")).Returns(religion);
        _mockBlessingRegistry.Setup(m => m.CanUnlockBlessing(playerData, religion, blessing))
            .Returns((true, ""));

        // Act
        var result = InvokeInternalMethod("OnBlessingsUnlock", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains(ErrorMessageConstants.ErrorOnlyFounderCanUnlock, result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsUnlock_ReligionBlessing_AsFounder_UnlocksAndNotifiesMembers()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("founder-uid");
        var mockMember = TestFixtures.CreateMockServerPlayer("member-uid");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "blessing1");

        var blessing = new Blessing
        {
            BlessingId = "blessing1",
            Name = "Test Blessing",
            Kind = BlessingKind.Religion
        };

        var playerData = new PlayerReligionData
        {
            ReligionUID = "religion-1",
            ActiveDeity = DeityType.Khoras
        };

        var religion = new ReligionData("Test Religion", DeityType.Khoras, "founder-uid", true)
        {
            ReligionUID = "religion-1"
        };
        religion.MemberUIDs.Add("founder-uid");
        religion.MemberUIDs.Add("member-uid");

        _mockBlessingRegistry.Setup(m => m.GetBlessing("blessing1")).Returns(blessing);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData("founder-uid")).Returns(playerData);
        _mockReligionManager.Setup(m => m.GetReligion("religion-1")).Returns(religion);
        _mockBlessingRegistry.Setup(m => m.CanUnlockBlessing(playerData, religion, blessing))
            .Returns((true, ""));

        var mockWorld = Mock.Get(_mockSapi.Object.World);
        mockWorld.Setup(w => w.PlayerByUid("founder-uid")).Returns(mockPlayer.Object);
        mockWorld.Setup(w => w.PlayerByUid("member-uid")).Returns(mockMember.Object);

        // Act
        var result = InvokeInternalMethod("OnBlessingsUnlock", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Test Blessing", result.StatusMessage);
        Assert.True(religion.UnlockedBlessings["blessing1"]);
        _mockBlessingEffectSystem.Verify(m => m.RefreshReligionBlessings("religion-1"), Times.Once);
        mockMember.Verify(p => p.SendMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<EnumChatType>()), Times.Once);
    }

    #endregion

    #region OnBlessingsActive Tests

    [Fact]
    public void OnBlessingsActive_WithNoBlessings_ShowsNone()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        _mockBlessingEffectSystem.Setup(m => m.GetActiveBlessings("test-player"))
            .Returns((new List<Blessing>(), new List<Blessing>()));
        _mockBlessingEffectSystem.Setup(m => m.GetCombinedStatModifiers("test-player"))
            .Returns(new Dictionary<string, float>());

        // Act
        var result = InvokeInternalMethod("OnBlessingsActive", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("0", result.StatusMessage);
    }

    [Fact]
    public void OnBlessingsActive_WithActiveBlessings_ShowsCombinedModifiers()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var playerBlessing = new Blessing { Name = "Player Blessing" };
        var religionBlessing = new Blessing { Name = "Religion Blessing" };

        var modifiers = new Dictionary<string, float> { { "walkspeed", 0.15f } };

        _mockBlessingEffectSystem.Setup(m => m.GetActiveBlessings("test-player"))
            .Returns((new List<Blessing> { playerBlessing }, new List<Blessing> { religionBlessing }));
        _mockBlessingEffectSystem.Setup(m => m.GetCombinedStatModifiers("test-player"))
            .Returns(modifiers);
        _mockBlessingEffectSystem.Setup(m => m.FormatStatModifiers(modifiers))
            .Returns("walkspeed: +15%");

        // Act
        var result = InvokeInternalMethod("OnBlessingsActive", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Player Blessing", result.StatusMessage);
        Assert.Contains("Religion Blessing", result.StatusMessage);
        Assert.Contains("walkspeed", result.StatusMessage);
    }

    #endregion

    #region Helper Methods

    private TextCommandResult InvokeInternalMethod(string methodName, TextCommandCallingArgs args)
    {
        var method = typeof(BlessingCommands).GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (TextCommandResult)method!.Invoke(_commands, new object[] { args })!;
    }

    #endregion
}
