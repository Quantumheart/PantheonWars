using System.Collections.Generic;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using Vintagestory.API.Client;

namespace PantheonWars.GUI;

/// <summary>
///     Manages state for the Perk Dialog UI
/// </summary>
public class PerkDialogManager
{
    private readonly ICoreClientAPI _capi;

    public PerkDialogManager(ICoreClientAPI capi)
    {
        _capi = capi;
    }

    // Religion and deity state
    public string? CurrentReligionUID { get; set; }
    public DeityType CurrentDeity { get; set; } = DeityType.None;
    public string? CurrentReligionName { get; set; }

    // Perk selection state
    public string? SelectedPerkId { get; set; }
    public string? HoveringPerkId { get; set; }

    // Scroll state
    public float PlayerTreeScrollX { get; set; }
    public float PlayerTreeScrollY { get; set; }
    public float ReligionTreeScrollX { get; set; }
    public float ReligionTreeScrollY { get; set; }

    // Data loaded flags
    public bool IsDataLoaded { get; set; }

    // Perk node states (Phase 2)
    public Dictionary<string, PerkNodeState> PlayerPerkStates { get; } = new();
    public Dictionary<string, PerkNodeState> ReligionPerkStates { get; } = new();

    /// <summary>
    ///     Initialize dialog state from player's current religion data
    /// </summary>
    public void Initialize(string? religionUID, DeityType deity, string? religionName)
    {
        CurrentReligionUID = religionUID;
        CurrentDeity = deity;
        CurrentReligionName = religionName;
        IsDataLoaded = true;

        // Reset selection and scroll
        SelectedPerkId = null;
        HoveringPerkId = null;
        PlayerTreeScrollX = 0f;
        PlayerTreeScrollY = 0f;
        ReligionTreeScrollX = 0f;
        ReligionTreeScrollY = 0f;
    }

    /// <summary>
    ///     Reset all state
    /// </summary>
    public void Reset()
    {
        CurrentReligionUID = null;
        CurrentDeity = DeityType.None;
        CurrentReligionName = null;
        SelectedPerkId = null;
        HoveringPerkId = null;
        PlayerTreeScrollX = 0f;
        PlayerTreeScrollY = 0f;
        ReligionTreeScrollX = 0f;
        ReligionTreeScrollY = 0f;
        IsDataLoaded = false;
    }

    /// <summary>
    ///     Select a perk (for displaying details)
    /// </summary>
    public void SelectPerk(string perkId)
    {
        SelectedPerkId = perkId;
    }

    /// <summary>
    ///     Clear perk selection
    /// </summary>
    public void ClearSelection()
    {
        SelectedPerkId = null;
    }

    /// <summary>
    ///     Check if player has a religion
    /// </summary>
    public bool HasReligion()
    {
        return !string.IsNullOrEmpty(CurrentReligionUID) && CurrentDeity != DeityType.None;
    }

    /// <summary>
    ///     Load perk states for player and religion perks
    ///     Called in Phase 6 when connected to PerkRegistry
    /// </summary>
    public void LoadPerkStates(List<Perk> playerPerks, List<Perk> religionPerks)
    {
        PlayerPerkStates.Clear();
        ReligionPerkStates.Clear();

        foreach (var perk in playerPerks)
        {
            var state = new PerkNodeState(perk);
            PlayerPerkStates[perk.PerkId] = state;
        }

        foreach (var perk in religionPerks)
        {
            var state = new PerkNodeState(perk);
            ReligionPerkStates[perk.PerkId] = state;
        }
    }

    /// <summary>
    ///     Get perk node state by ID
    /// </summary>
    public PerkNodeState? GetPerkState(string perkId)
    {
        if (PlayerPerkStates.TryGetValue(perkId, out var playerState)) return playerState;

        if (ReligionPerkStates.TryGetValue(perkId, out var religionState)) return religionState;

        return null;
    }

    /// <summary>
    ///     Get selected perk's state (if any)
    /// </summary>
    public PerkNodeState? GetSelectedPerkState()
    {
        if (string.IsNullOrEmpty(SelectedPerkId)) return null;

        return GetPerkState(SelectedPerkId);
    }

    /// <summary>
    ///     Update unlock status for a perk
    /// </summary>
    public void SetPerkUnlocked(string perkId, bool unlocked)
    {
        var state = GetPerkState(perkId);
        if (state != null)
        {
            state.IsUnlocked = unlocked;
            state.UpdateVisualState();
        }
    }

    /// <summary>
    ///     Update all perk states based on current unlock status and requirements
    ///     Called after data refresh in Phase 6
    /// </summary>
    public void RefreshAllPerkStates()
    {
        foreach (var state in PlayerPerkStates.Values) state.UpdateVisualState();

        foreach (var state in ReligionPerkStates.Values) state.UpdateVisualState();
    }
}