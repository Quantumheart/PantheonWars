namespace PantheonWars.Constants;

/// <summary>
///     System-wide constants for modifier IDs, log prefixes, and cache keys.
///     Use these constants instead of hardcoded strings to maintain consistency.
/// </summary>
public static class SystemConstants
{
    #region Modifier IDs

    /// <summary>
    ///     Format string for perk modifier IDs. Use with string.Format(ModifierIdFormat, playerUID)
    /// </summary>
    public const string ModifierIdFormat = "perk-{0}";

    #endregion

    #region Log Message Prefixes

    /// <summary>
    ///     Standard log prefix for all PantheonWars mod messages
    /// </summary>
    public const string LogPrefix = "[PantheonWars]";

    #endregion

    #region Debug Messages

    /// <summary>
    ///     Debug message format for applying stat. Use with string.Format(DebugAppliedStat, statName, modifierId, value)
    /// </summary>
    public const string DebugAppliedStatFormat = "Applied {0}: {1} = {2:F3}";

    #endregion

    #region Miscellaneous

    /// <summary>
    ///     Default message when no modifiers are active
    /// </summary>
    public const string NoActiveModifiers = "No active modifiers";

    #endregion

    #region Cache Keys

    /// <summary>
    ///     Prefix for player modifier cache keys
    /// </summary>
    public const string PlayerModifierCachePrefix = "player-modifier-";

    /// <summary>
    ///     Prefix for religion modifier cache keys
    /// </summary>
    public const string ReligionModifierCachePrefix = "religion-modifier-";

    #endregion

    #region Stat Display Names

    /// <summary>
    ///     Display name for melee damage stat
    /// </summary>
    public const string StatDisplayMeleeDamage = "Melee Damage";

    /// <summary>
    ///     Display name for ranged damage stat
    /// </summary>
    public const string StatDisplayRangedDamage = "Ranged Damage";

    /// <summary>
    ///     Display name for attack speed stat
    /// </summary>
    public const string StatDisplayAttackSpeed = "Attack Speed";

    /// <summary>
    ///     Display name for armor stat
    /// </summary>
    public const string StatDisplayArmor = "Armor";

    /// <summary>
    ///     Display name for max health stat
    /// </summary>
    public const string StatDisplayMaxHealth = "Max Health";

    /// <summary>
    ///     Display name for walk speed stat
    /// </summary>
    public const string StatDisplayWalkSpeed = "Walk Speed";

    /// <summary>
    ///     Display name for health regeneration stat
    /// </summary>
    public const string StatDisplayHealthRegen = "Health Regen";

    #endregion

    #region Error Messages

    /// <summary>
    ///     Error message when player entity is null
    /// </summary>
    public const string ErrorPlayerEntityNull = "Cannot apply perks - player entity is null";

    /// <summary>
    ///     Error message when player has no Stats
    /// </summary>
    public const string ErrorPlayerStatsNull = "Cannot apply perks - player has no Stats";

    /// <summary>
    ///     Error message format for stat not found. Use with string.Format(ErrorStatNotFound, statName, playerName)
    /// </summary>
    public const string ErrorStatNotFoundFormat = "Stat '{0}' not found for player {1}";

    /// <summary>
    ///     Error message format for stat application error. Use with string.Format(ErrorApplyingStat, statName, playerName)
    /// </summary>
    public const string ErrorApplyingStatFormat = "Error applying stat '{0}' to player {1}";

    /// <summary>
    ///     Warning message format for removing modifier. Use with string.Format(ErrorRemovingModifier, statName, playerName)
    /// </summary>
    public const string ErrorRemovingModifierFormat = "Could not remove modifier '{0}' from player {1}";

    #endregion

    #region Success Messages

    /// <summary>
    ///     Success message format for applying modifiers. Use with string.Format(SuccessAppliedModifiers, count, playerName)
    /// </summary>
    public const string SuccessAppliedModifiersFormat = "Applied {0} perk modifiers to player {1}";

    /// <summary>
    ///     Success message format for removing modifiers. Use with string.Format(SuccessRemovedModifiers, count, playerName)
    /// </summary>
    public const string SuccessRemovedModifiersFormat = "Removed {0} old perk modifiers from player {1}";

    /// <summary>
    ///     Success message format for refreshing perks. Use with string.Format(SuccessRefreshedPerks, playerUID)
    /// </summary>
    public const string SuccessRefreshedPerksFormat = "Refreshed perks for player {0}";

    /// <summary>
    ///     Success message format for refreshing religion perks. Use with string.Format(SuccessRefreshedReligionPerks,
    ///     religionName, memberCount)
    /// </summary>
    public const string SuccessRefreshedReligionPerksFormat = "Refreshed perks for religion {0} ({1} members)";

    /// <summary>
    ///     Success message format for health update. Use with string.Format(SuccessHealthUpdate, playerName, beforeHealth,
    ///     afterHealth, baseHealth, statValue)
    /// </summary>
    public const string SuccessHealthUpdateFormat = "{0} max health: {1:F1} → {2:F1} (Base: {3:F1}, Stat: {4:F3})";

    #endregion

    #region Info Messages

    /// <summary>
    ///     Info message for initializing perk effect system
    /// </summary>
    public const string InfoInitializingPerkSystem = "Initializing Perk Effect System...";

    /// <summary>
    ///     Info message for perk effect system initialized
    /// </summary>
    public const string InfoPerkSystemInitialized = "Perk Effect System initialized";

    /// <summary>
    ///     Info message for clearing caches
    /// </summary>
    public const string InfoClearedCaches = "Cleared all perk modifier caches";

    #endregion
}