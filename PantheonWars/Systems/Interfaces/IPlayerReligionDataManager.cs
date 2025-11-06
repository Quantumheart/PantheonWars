using System;
using System.Collections.Generic;
using PantheonWars.Data;

namespace PantheonWars.Systems.Interfaces;

public interface IPlayerReligionDataManager
{
    event PlayerReligionDataManager.PlayerReligionDataChangedDelegate OnPlayerLeavesReligion;

    /// <summary>
    ///     Initializes the player religion data manager
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Gets or creates player data
    /// </summary>
    PlayerReligionData GetOrCreatePlayerData(string playerUID);

    /// <summary>
    ///     Adds favor to a player
    /// </summary>
    void AddFavor(string playerUID, int amount, string reason = "");

    /// <summary>
    ///     Updates favor rank for a player
    /// </summary>
    void UpdateFavorRank(string playerUID);

    /// <summary>
    ///     Unlocks a player perk
    /// </summary>
    bool UnlockPlayerPerk(string playerUID, string perkId);

    /// <summary>
    ///     Gets active player perks (to be expanded in Phase 3.3)
    /// </summary>
    List<string> GetActivePlayerPerks(string playerUID);

    /// <summary>
    ///     Joins a player to a religion
    /// </summary>
    void JoinReligion(string playerUID, string religionUID);

    /// <summary>
    ///     Removes a player from their current religion
    /// </summary>
    void LeaveReligion(string playerUID);

    /// <summary>
    ///     Checks if a player can switch religions (cooldown check)
    /// </summary>
    bool CanSwitchReligion(string playerUID);

    /// <summary>
    ///     Gets remaining cooldown time for religion switching
    /// </summary>
    TimeSpan? GetSwitchCooldownRemaining(string playerUID);

    /// <summary>
    ///     Applies switching penalty when changing religions
    /// </summary>
    void HandleReligionSwitch(string playerUID);
}