using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PantheonWars.GUI.UI.State;
using PantheonWars.Network;
using Xunit;

namespace PantheonWars.Tests.GUI.UI.State;

/// <summary>
///     Unit tests for ReligionBrowserState
/// </summary>
[ExcludeFromCodeCoverage]
public class ReligionBrowserStateTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Act
        var state = new ReligionBrowserState();

        // Assert
        Assert.Equal("All", state.SelectedDeityFilter);
        Assert.Null(state.SelectedReligionUID);
        Assert.Equal(0f, state.ScrollY);
        Assert.Empty(state.Religions);
        Assert.True(state.IsLoading);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new ReligionBrowserState();
        var religions = new List<ReligionListResponsePacket.ReligionInfo>
        {
            new() { ReligionUID = "rel1", ReligionName = "Religion 1" },
            new() { ReligionUID = "rel2", ReligionName = "Religion 2" }
        };

        // Act
        state.SelectedDeityFilter = "Khoras";
        state.SelectedReligionUID = "rel1";
        state.ScrollY = 150.5f;
        state.Religions = religions;
        state.IsLoading = false;

        // Assert
        Assert.Equal("Khoras", state.SelectedDeityFilter);
        Assert.Equal("rel1", state.SelectedReligionUID);
        Assert.Equal(150.5f, state.ScrollY);
        Assert.Equal(2, state.Religions.Count);
        Assert.False(state.IsLoading);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ResetsAllPropertiesToDefaults()
    {
        // Arrange
        var state = new ReligionBrowserState
        {
            SelectedDeityFilter = "Lysa",
            SelectedReligionUID = "rel1",
            ScrollY = 100f,
            Religions = new List<ReligionListResponsePacket.ReligionInfo>
            {
                new() { ReligionUID = "rel1", ReligionName = "Test" }
            },
            IsLoading = false
        };

        // Act
        state.Reset();

        // Assert
        Assert.Equal("All", state.SelectedDeityFilter);
        Assert.Null(state.SelectedReligionUID);
        Assert.Equal(0f, state.ScrollY);
        Assert.Empty(state.Religions);
        Assert.True(state.IsLoading);
    }

    [Fact]
    public void Reset_ClearsReligionsList()
    {
        // Arrange
        var state = new ReligionBrowserState();
        state.Religions.Add(new ReligionListResponsePacket.ReligionInfo { ReligionUID = "rel1" });
        state.Religions.Add(new ReligionListResponsePacket.ReligionInfo { ReligionUID = "rel2" });

        // Act
        state.Reset();

        // Assert
        Assert.Empty(state.Religions);
    }

    #endregion

    #region UpdateReligionList Tests

    [Fact]
    public void UpdateReligionList_UpdatesReligionsAndStopsLoading()
    {
        // Arrange
        var state = new ReligionBrowserState();
        var religions = new List<ReligionListResponsePacket.ReligionInfo>
        {
            new() { ReligionUID = "rel1", ReligionName = "Religion 1" },
            new() { ReligionUID = "rel2", ReligionName = "Religion 2" }
        };

        // Act
        state.UpdateReligionList(religions);

        // Assert
        Assert.Equal(2, state.Religions.Count);
        Assert.False(state.IsLoading);
    }

    [Fact]
    public void UpdateReligionList_ReplacesExistingReligions()
    {
        // Arrange
        var state = new ReligionBrowserState();
        state.Religions = new List<ReligionListResponsePacket.ReligionInfo>
        {
            new() { ReligionUID = "old1", ReligionName = "Old 1" }
        };

        var newReligions = new List<ReligionListResponsePacket.ReligionInfo>
        {
            new() { ReligionUID = "new1", ReligionName = "New 1" },
            new() { ReligionUID = "new2", ReligionName = "New 2" }
        };

        // Act
        state.UpdateReligionList(newReligions);

        // Assert
        Assert.Equal(2, state.Religions.Count);
        Assert.Equal("new1", state.Religions[0].ReligionUID);
        Assert.Equal("new2", state.Religions[1].ReligionUID);
    }

    [Fact]
    public void UpdateReligionList_WithEmptyList_ClearsReligions()
    {
        // Arrange
        var state = new ReligionBrowserState();
        state.Religions = new List<ReligionListResponsePacket.ReligionInfo>
        {
            new() { ReligionUID = "rel1", ReligionName = "Test" }
        };

        // Act
        state.UpdateReligionList(new List<ReligionListResponsePacket.ReligionInfo>());

        // Assert
        Assert.Empty(state.Religions);
        Assert.False(state.IsLoading);
    }

    #endregion
}
