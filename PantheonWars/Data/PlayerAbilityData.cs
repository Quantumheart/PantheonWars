using System;
using System.Collections.Generic;

namespace PantheonWars.Data;

/// <summary>
///     Stores player-specific ability data for cooldown tracking
/// </summary>
public class PlayerAbilityData
{
    public PlayerAbilityData()
    {
    }

    public PlayerAbilityData(string playerUID)
    {
        PlayerUID = playerUID;
    }

    /// <summary>
    ///     Player's unique identifier
    /// </summary>
    public string PlayerUID { get; set; } = string.Empty;

    /// <summary>
    ///     Tracks when each ability was last used (AbilityId -> UTC timestamp)
    /// </summary>
    public Dictionary<string, DateTime> AbilityCooldowns { get; set; } = new();

    /// <summary>
    ///     Checks if an ability is currently on cooldown
    /// </summary>
    /// <param name="abilityId">The ability ID to check</param>
    /// <param name="cooldownSeconds">The ability's cooldown duration in seconds</param>
    /// <returns>True if ability is on cooldown, false if ready to use</returns>
    public bool IsOnCooldown(string abilityId, float cooldownSeconds)
    {
        if (!AbilityCooldowns.TryGetValue(abilityId, out var lastUsed)) return false; // Never used, not on cooldown

        var elapsed = (DateTime.UtcNow - lastUsed).TotalSeconds;
        return elapsed < cooldownSeconds;
    }

    /// <summary>
    ///     Gets the remaining cooldown time in seconds
    /// </summary>
    /// <param name="abilityId">The ability ID to check</param>
    /// <param name="cooldownSeconds">The ability's cooldown duration in seconds</param>
    /// <returns>Remaining cooldown in seconds, or 0 if ready</returns>
    public float GetRemainingCooldown(string abilityId, float cooldownSeconds)
    {
        if (!AbilityCooldowns.TryGetValue(abilityId, out var lastUsed)) return 0f;

        var elapsed = (DateTime.UtcNow - lastUsed).TotalSeconds;
        var remaining = cooldownSeconds - elapsed;
        return remaining > 0 ? (float)remaining : 0f;
    }

    /// <summary>
    ///     Starts the cooldown timer for an ability
    /// </summary>
    /// <param name="abilityId">The ability ID</param>
    public void StartCooldown(string abilityId)
    {
        AbilityCooldowns[abilityId] = DateTime.UtcNow;
    }

    /// <summary>
    ///     Clears cooldown for a specific ability (admin/debug purposes)
    /// </summary>
    /// <param name="abilityId">The ability ID</param>
    public void ClearCooldown(string abilityId)
    {
        AbilityCooldowns.Remove(abilityId);
    }

    /// <summary>
    ///     Clears all cooldowns (admin/debug purposes)
    /// </summary>
    public void ClearAllCooldowns()
    {
        AbilityCooldowns.Clear();
    }

    /// <summary>
    ///     Removes expired cooldowns to prevent memory bloat
    /// </summary>
    /// <param name="maxCooldownSeconds">Maximum cooldown duration to consider</param>
    public void CleanupExpiredCooldowns(float maxCooldownSeconds = 300f)
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-maxCooldownSeconds);
        var toRemove = new List<string>();

        foreach (var kvp in AbilityCooldowns)
            if (kvp.Value < cutoff)
                toRemove.Add(kvp.Key);

        foreach (var abilityId in toRemove) AbilityCooldowns.Remove(abilityId);
    }
}