using System.Diagnostics.CodeAnalysis;
using PantheonWars.GUI.UI.State;
using PantheonWars.Network;
using Xunit;

namespace PantheonWars.Tests.GUI.UI.State;

/// <summary>
///     Unit tests for ReligionManagementState
/// </summary>
[ExcludeFromCodeCoverage]
public class ReligionManagementStateTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Act
        var state = new ReligionManagementState();

        // Assert
        Assert.Null(state.ReligionInfo);
        Assert.Equal("", state.InvitePlayerName);
        Assert.Equal("", state.Description);
        Assert.Equal(0f, state.MemberScrollY);
        Assert.Null(state.ErrorMessage);
        Assert.False(state.ShowDisbandConfirm);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new ReligionManagementState();
        var religionInfo = new PlayerReligionInfoResponsePacket
        {
            ReligionName = "Test Religion",
            Description = "Test Description"
        };

        // Act
        state.ReligionInfo = religionInfo;
        state.InvitePlayerName = "PlayerName";
        state.Description = "New Description";
        state.MemberScrollY = 75.5f;
        state.ErrorMessage = "Error message";
        state.ShowDisbandConfirm = true;

        // Assert
        Assert.NotNull(state.ReligionInfo);
        Assert.Equal("Test Religion", state.ReligionInfo.ReligionName);
        Assert.Equal("PlayerName", state.InvitePlayerName);
        Assert.Equal("New Description", state.Description);
        Assert.Equal(75.5f, state.MemberScrollY);
        Assert.Equal("Error message", state.ErrorMessage);
        Assert.True(state.ShowDisbandConfirm);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ResetsAllPropertiesToDefaults()
    {
        // Arrange
        var state = new ReligionManagementState
        {
            ReligionInfo = new PlayerReligionInfoResponsePacket(),
            InvitePlayerName = "Player",
            Description = "Desc",
            MemberScrollY = 100f,
            ErrorMessage = "Error",
            ShowDisbandConfirm = true
        };

        // Act
        state.Reset();

        // Assert
        Assert.Null(state.ReligionInfo);
        Assert.Equal("", state.InvitePlayerName);
        Assert.Equal("", state.Description);
        Assert.Equal(0f, state.MemberScrollY);
        Assert.Null(state.ErrorMessage);
        Assert.False(state.ShowDisbandConfirm);
    }

    [Fact]
    public void Reset_CanBeCalledMultipleTimes()
    {
        // Arrange
        var state = new ReligionManagementState();
        state.Description = "Test";
        state.ShowDisbandConfirm = true;

        // Act
        state.Reset();
        state.Reset();

        // Assert
        Assert.Equal("", state.Description);
        Assert.False(state.ShowDisbandConfirm);
    }

    #endregion

    #region UpdateReligionInfo Tests

    [Fact]
    public void UpdateReligionInfo_SetsReligionInfoAndDescription()
    {
        // Arrange
        var state = new ReligionManagementState();
        var packet = new PlayerReligionInfoResponsePacket
        {
            ReligionName = "Test Religion",
            Description = "Test Description"
        };

        // Act
        state.UpdateReligionInfo(packet);

        // Assert
        Assert.NotNull(state.ReligionInfo);
        Assert.Equal("Test Religion", state.ReligionInfo.ReligionName);
        Assert.Equal("Test Description", state.Description);
    }

    [Fact]
    public void UpdateReligionInfo_WithNullDescription_SetsEmptyString()
    {
        // Arrange
        var state = new ReligionManagementState();
        var packet = new PlayerReligionInfoResponsePacket
        {
            ReligionName = "Test Religion",
            Description = null!
        };

        // Act
        state.UpdateReligionInfo(packet);

        // Assert
        Assert.NotNull(state.ReligionInfo);
        Assert.Equal("", state.Description);
    }

    [Fact]
    public void UpdateReligionInfo_ReplacesExistingInfo()
    {
        // Arrange
        var state = new ReligionManagementState();
        state.ReligionInfo = new PlayerReligionInfoResponsePacket
        {
            ReligionName = "Old Religion",
            Description = "Old Description"
        };
        state.Description = "Old Description";

        var newPacket = new PlayerReligionInfoResponsePacket
        {
            ReligionName = "New Religion",
            Description = "New Description"
        };

        // Act
        state.UpdateReligionInfo(newPacket);

        // Assert
        Assert.Equal("New Religion", state.ReligionInfo.ReligionName);
        Assert.Equal("New Description", state.Description);
    }

    #endregion
}
