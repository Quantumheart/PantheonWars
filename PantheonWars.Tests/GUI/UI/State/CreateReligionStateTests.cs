using System.Diagnostics.CodeAnalysis;
using PantheonWars.GUI.UI.State;
using Xunit;

namespace PantheonWars.Tests.GUI.UI.State;

/// <summary>
///     Unit tests for CreateReligionState
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateReligionStateTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Act
        var state = new CreateReligionState();

        // Assert
        Assert.Equal("", state.ReligionName);
        Assert.Equal(0, state.SelectedDeityIndex);
        Assert.True(state.IsPublic);
        Assert.Null(state.ErrorMessage);
        Assert.False(state.DropdownOpen);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new CreateReligionState();

        // Act
        state.ReligionName = "Test Religion";
        state.SelectedDeityIndex = 3;
        state.IsPublic = false;
        state.ErrorMessage = "Test error";
        state.DropdownOpen = true;

        // Assert
        Assert.Equal("Test Religion", state.ReligionName);
        Assert.Equal(3, state.SelectedDeityIndex);
        Assert.False(state.IsPublic);
        Assert.Equal("Test error", state.ErrorMessage);
        Assert.True(state.DropdownOpen);
    }

    [Fact]
    public void ErrorMessage_CanBeSetToNull()
    {
        // Arrange
        var state = new CreateReligionState
        {
            ErrorMessage = "Error"
        };

        // Act
        state.ErrorMessage = null;

        // Assert
        Assert.Null(state.ErrorMessage);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ResetsAllPropertiesToDefaults()
    {
        // Arrange
        var state = new CreateReligionState
        {
            ReligionName = "Test",
            SelectedDeityIndex = 5,
            IsPublic = false,
            ErrorMessage = "Error",
            DropdownOpen = true
        };

        // Act
        state.Reset();

        // Assert
        Assert.Equal("", state.ReligionName);
        Assert.Equal(0, state.SelectedDeityIndex);
        Assert.True(state.IsPublic);
        Assert.Null(state.ErrorMessage);
        Assert.False(state.DropdownOpen);
    }

    [Fact]
    public void Reset_CanBeCalledMultipleTimes()
    {
        // Arrange
        var state = new CreateReligionState();
        state.ReligionName = "Test";
        state.SelectedDeityIndex = 2;

        // Act
        state.Reset();
        state.Reset();

        // Assert
        Assert.Equal("", state.ReligionName);
        Assert.Equal(0, state.SelectedDeityIndex);
    }

    #endregion
}
