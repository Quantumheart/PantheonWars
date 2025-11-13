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
///     Tests for AbilityCommands class
/// </summary>
public class AbilityCommandsTests
{
    private readonly Mock<AbilitySystem> _mockAbilitySystem;
    private readonly Mock<PlayerDataManager> _mockPlayerDataManager;
    private readonly Mock<IPlayerReligionDataManager> _mockReligionDataManager;
    private readonly Mock<ICoreServerAPI> _mockSapi;
    private readonly AbilityCommands _commands;

    public AbilityCommandsTests()
    {
        _mockSapi = TestFixtures.CreateMockServerAPI();
        _mockAbilitySystem = new Mock<AbilitySystem>(_mockSapi.Object);
        _mockPlayerDataManager = new Mock<PlayerDataManager>(_mockSapi.Object);
        _mockReligionDataManager = new Mock<IPlayerReligionDataManager>();

        _commands = new AbilityCommands(
            _mockSapi.Object,
            _mockAbilitySystem.Object,
            _mockPlayerDataManager.Object,
            _mockReligionDataManager.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var commands = new AbilityCommands(
            _mockSapi.Object,
            _mockAbilitySystem.Object,
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
        mockChatCommands.Verify(c => c.Create("ability"), Times.Once);

        // Verify notification was logged
        var mockLogger = Mock.Get(_mockSapi.Object.Logger);
        mockLogger.Verify(l => l.Notification("[PantheonWars] Ability commands registered"), Times.Once);
    }

    #endregion

    #region OnListAbilities Tests

    [Fact]
    public void OnListAbilities_WithNoDeity_ReturnsInfoMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnListAbilities", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnListAbilities_WithNoAbilities_ReturnsInfoMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockAbilitySystem.Setup(m => m.GetPlayerAbilities(mockPlayer.Object))
            .Returns(Enumerable.Empty<Ability>());

        // Act
        var result = InvokePrivateMethod("OnListAbilities", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("No abilities available", result.StatusMessage);
    }

    [Fact]
    public void OnListAbilities_WithAbilities_ListsAbilitiesWithCooldowns()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        var ability1 = new Ability
        {
            Id = "smite",
            Name = "Smite",
            Description = "Strike an enemy",
            FavorCost = 10,
            MinimumRank = DevotionRank.Initiate
        };

        var ability2 = new Ability
        {
            Id = "heal",
            Name = "Divine Heal",
            Description = "Heal yourself",
            FavorCost = 15,
            MinimumRank = DevotionRank.Disciple
        };

        _mockAbilitySystem.Setup(m => m.GetPlayerAbilities(mockPlayer.Object))
            .Returns(new[] { ability1, ability2 });
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "smite")).Returns(0);
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "heal")).Returns(30.5);

        // Act
        var result = InvokePrivateMethod("OnListAbilities", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Smite", result.StatusMessage);
        Assert.Contains("Divine Heal", result.StatusMessage);
        Assert.Contains("[READY]", result.StatusMessage);
        Assert.Contains("[CD: 30.5s]", result.StatusMessage);
        Assert.Contains("10 favor", result.StatusMessage);
        Assert.Contains("15 favor", result.StatusMessage);
    }

    [Fact]
    public void OnListAbilities_WithRankRequirement_ShowsRankInfo()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        var ability = new Ability
        {
            Id = "ultimate",
            Name = "Ultimate Ability",
            Description = "Powerful ability",
            FavorCost = 50,
            MinimumRank = DevotionRank.Champion
        };

        _mockAbilitySystem.Setup(m => m.GetPlayerAbilities(mockPlayer.Object))
            .Returns(new[] { ability });
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "ultimate")).Returns(0);

        // Act
        var result = InvokePrivateMethod("OnListAbilities", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Champion", result.StatusMessage);
    }

    #endregion

    #region OnAbilityInfo Tests

    [Fact]
    public void OnAbilityInfo_WithNonExistentAbility_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "nonexistent");

        _mockAbilitySystem.Setup(m => m.GetAbility("nonexistent")).Returns((Ability?)null);

        // Act
        var result = InvokePrivateMethod("OnAbilityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        Assert.Contains("nonexistent", result.StatusMessage);
        Assert.Contains("not found", result.StatusMessage);
    }

    [Fact]
    public void OnAbilityInfo_WithValidAbility_ShowsDetailedInfo()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "smite");

        var playerData = new PlayerData { DivineFavor = 20, DevotionRank = DevotionRank.Initiate };
        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };

        var ability = new Ability
        {
            Id = "smite",
            Name = "Smite",
            Type = AbilityType.Offensive,
            Deity = DeityType.Khoras,
            Description = "Strike an enemy",
            FavorCost = 10,
            CooldownSeconds = 30,
            MinimumRank = DevotionRank.Initiate
        };

        _mockAbilitySystem.Setup(m => m.GetAbility("smite")).Returns(ability);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData(mockPlayer.Object)).Returns(playerData);
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "smite")).Returns(0);

        // Act
        var result = InvokePrivateMethod("OnAbilityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Smite", result.StatusMessage);
        Assert.Contains("Offensive", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
        Assert.Contains("10", result.StatusMessage);
        Assert.Contains("30 seconds", result.StatusMessage);
        Assert.Contains("[READY]", result.StatusMessage);
    }

    [Fact]
    public void OnAbilityInfo_WithWrongDeity_ShowsCannotUse()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "smite");

        var playerData = new PlayerData();
        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Lunara };

        var ability = new Ability
        {
            Id = "smite",
            Name = "Smite",
            Deity = DeityType.Khoras,
            FavorCost = 10,
            CooldownSeconds = 30,
            MinimumRank = DevotionRank.Initiate
        };

        _mockAbilitySystem.Setup(m => m.GetAbility("smite")).Returns(ability);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData(mockPlayer.Object)).Returns(playerData);
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnAbilityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("cannot use this ability", result.StatusMessage);
        Assert.Contains("Khoras", result.StatusMessage);
    }

    [Fact]
    public void OnAbilityInfo_WithOnCooldown_ShowsRemainingTime()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "smite");

        var playerData = new PlayerData { DivineFavor = 20, DevotionRank = DevotionRank.Initiate };
        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };

        var ability = new Ability
        {
            Id = "smite",
            Name = "Smite",
            Deity = DeityType.Khoras,
            FavorCost = 10,
            CooldownSeconds = 30,
            MinimumRank = DevotionRank.Initiate
        };

        _mockAbilitySystem.Setup(m => m.GetAbility("smite")).Returns(ability);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData(mockPlayer.Object)).Returns(playerData);
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "smite")).Returns(15.3);

        // Act
        var result = InvokePrivateMethod("OnAbilityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("On cooldown", result.StatusMessage);
        Assert.Contains("15.3", result.StatusMessage);
    }

    [Fact]
    public void OnAbilityInfo_WithInsufficientFavor_ShowsWarning()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "smite");

        var playerData = new PlayerData { DivineFavor = 5, DevotionRank = DevotionRank.Initiate };
        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };

        var ability = new Ability
        {
            Id = "smite",
            Name = "Smite",
            Deity = DeityType.Khoras,
            FavorCost = 10,
            CooldownSeconds = 30,
            MinimumRank = DevotionRank.Initiate
        };

        _mockAbilitySystem.Setup(m => m.GetAbility("smite")).Returns(ability);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData(mockPlayer.Object)).Returns(playerData);
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "smite")).Returns(0);

        // Act
        var result = InvokePrivateMethod("OnAbilityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Insufficient favor", result.StatusMessage);
        Assert.Contains("need 10", result.StatusMessage);
        Assert.Contains("have 5", result.StatusMessage);
    }

    [Fact]
    public void OnAbilityInfo_WithRankTooLow_ShowsWarning()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "smite");

        var playerData = new PlayerData { DivineFavor = 20, DevotionRank = DevotionRank.Initiate };
        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };

        var ability = new Ability
        {
            Id = "smite",
            Name = "Smite",
            Deity = DeityType.Khoras,
            FavorCost = 10,
            CooldownSeconds = 30,
            MinimumRank = DevotionRank.Champion
        };

        _mockAbilitySystem.Setup(m => m.GetAbility("smite")).Returns(ability);
        _mockPlayerDataManager.Setup(m => m.GetOrCreatePlayerData(mockPlayer.Object)).Returns(playerData);
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "smite")).Returns(0);

        // Act
        var result = InvokePrivateMethod("OnAbilityInfo", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Rank too low", result.StatusMessage);
        Assert.Contains("Champion", result.StatusMessage);
        Assert.Contains("Initiate", result.StatusMessage);
    }

    #endregion

    #region OnUseAbility Tests

    [Fact]
    public void OnUseAbility_WithSuccessfulExecution_ReturnsSuccess()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "smite");

        _mockAbilitySystem.Setup(m => m.ExecuteAbility(mockPlayer.Object, "smite")).Returns(true);

        // Act
        var result = InvokePrivateMethod("OnUseAbility", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        _mockAbilitySystem.Verify(m => m.ExecuteAbility(mockPlayer.Object, "smite"), Times.Once);
    }

    [Fact]
    public void OnUseAbility_WithFailedExecution_ReturnsError()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "smite");

        _mockAbilitySystem.Setup(m => m.ExecuteAbility(mockPlayer.Object, "smite")).Returns(false);

        // Act
        var result = InvokePrivateMethod("OnUseAbility", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
    }

    [Fact]
    public void OnUseAbility_WithEmptyAbilityId_CallsExecuteWithEmpty()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object, "");

        _mockAbilitySystem.Setup(m => m.ExecuteAbility(mockPlayer.Object, "")).Returns(false);

        // Act
        var result = InvokePrivateMethod("OnUseAbility", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Error, result.Status);
        _mockAbilitySystem.Verify(m => m.ExecuteAbility(mockPlayer.Object, ""), Times.Once);
    }

    #endregion

    #region OnShowCooldowns Tests

    [Fact]
    public void OnShowCooldowns_WithNoDeity_ReturnsInfoMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.None };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        // Act
        var result = InvokePrivateMethod("OnShowCooldowns", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("not in a religion", result.StatusMessage);
    }

    [Fact]
    public void OnShowCooldowns_WithNoAbilities_ReturnsInfoMessage()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);
        _mockAbilitySystem.Setup(m => m.GetPlayerAbilities(mockPlayer.Object))
            .Returns(Enumerable.Empty<Ability>());

        // Act
        var result = InvokePrivateMethod("OnShowCooldowns", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("No abilities available", result.StatusMessage);
    }

    [Fact]
    public void OnShowCooldowns_WithMixedCooldowns_ShowsAllAbilities()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        var ability1 = new Ability { Id = "smite", Name = "Smite" };
        var ability2 = new Ability { Id = "heal", Name = "Heal" };
        var ability3 = new Ability { Id = "buff", Name = "Buff" };

        _mockAbilitySystem.Setup(m => m.GetPlayerAbilities(mockPlayer.Object))
            .Returns(new[] { ability1, ability2, ability3 });
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "smite")).Returns(0);
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "heal")).Returns(15.5);
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "buff")).Returns(0);

        // Act
        var result = InvokePrivateMethod("OnShowCooldowns", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("Smite: READY", result.StatusMessage);
        Assert.Contains("Heal: 15.5 seconds", result.StatusMessage);
        Assert.Contains("Buff: READY", result.StatusMessage);
        Assert.Contains("2 ready", result.StatusMessage);
        Assert.Contains("1 on cooldown", result.StatusMessage);
    }

    [Fact]
    public void OnShowCooldowns_WithAllReady_ShowsAllReady()
    {
        // Arrange
        var mockPlayer = TestFixtures.CreateMockServerPlayer("test-player");
        var args = TestFixtures.CreateCommandArgs(mockPlayer.Object);

        var religionData = new PlayerReligionData { ActiveDeity = DeityType.Khoras };
        _mockReligionDataManager.Setup(m => m.GetOrCreatePlayerData("test-player")).Returns(religionData);

        var ability1 = new Ability { Id = "smite", Name = "Smite" };
        var ability2 = new Ability { Id = "heal", Name = "Heal" };

        _mockAbilitySystem.Setup(m => m.GetPlayerAbilities(mockPlayer.Object))
            .Returns(new[] { ability1, ability2 });
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "smite")).Returns(0);
        _mockAbilitySystem.Setup(m => m.GetAbilityCooldown(mockPlayer.Object, "heal")).Returns(0);

        // Act
        var result = InvokePrivateMethod("OnShowCooldowns", args);

        // Assert
        Assert.Equal(EnumCommandStatus.Success, result.Status);
        Assert.Contains("2 ready", result.StatusMessage);
        Assert.Contains("0 on cooldown", result.StatusMessage);
    }

    #endregion

    #region Helper Methods

    private TextCommandResult InvokePrivateMethod(string methodName, TextCommandCallingArgs args)
    {
        var method = typeof(AbilityCommands).GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (TextCommandResult)method!.Invoke(_commands, new object[] { args })!;
    }

    #endregion
}
