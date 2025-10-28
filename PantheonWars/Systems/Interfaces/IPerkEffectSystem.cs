using System.Collections.Generic;
using PantheonWars.Models;
using Vintagestory.API.Server;

namespace PantheonWars.Systems.Interfaces;

public interface IPerkEffectSystem
{
    /// <summary>
    /// Initializes the perk effect system
    /// </summary>
    void Initialize();

    /// <summary>
    /// Gets stat modifiers from player's unlocked perks
    /// </summary>
    Dictionary<string, float> GetPlayerStatModifiers(string playerUID);

    /// <summary>
    /// Gets stat modifiers from religion's unlocked perks
    /// </summary>
    Dictionary<string, float> GetReligionStatModifiers(string religionUID);

    /// <summary>
    /// Gets combined stat modifiers for a player (player perks + religion perks)
    /// </summary>
    Dictionary<string, float> GetCombinedStatModifiers(string playerUID);

    /// <summary>
    /// Applies perks to a player using Vintage Story's Stats API
    /// Based on XSkills implementation pattern
    /// </summary>
    void ApplyPerksToPlayer(IServerPlayer player);

    /// <summary>
    /// Refreshes all perk effects for a player
    /// </summary>
    void RefreshPlayerPerks(string playerUID);

    /// <summary>
    /// Refreshes perk effects for all members of a religion
    /// </summary>
    void RefreshReligionPerks(string religionUID);

    /// <summary>
    /// Gets a summary of active perks for a player
    /// </summary>
    (List<Perk> playerPerks, List<Perk> religionPerks) GetActivePerks(string playerUID);

    /// <summary>
    /// Clears all modifier caches (useful for debugging/testing)
    /// </summary>
    void ClearAllCaches();

    /// <summary>
    /// Gets a formatted string of all stat modifiers for display
    /// </summary>
    string FormatStatModifiers(Dictionary<string, float> modifiers);
}