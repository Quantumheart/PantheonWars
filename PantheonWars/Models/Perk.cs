using System.Collections.Generic;
using PantheonWars.Models.Enum;
using Vintagestory.GameContent;

namespace PantheonWars.Models;

/// <summary>
///     Represents a passive perk that can be unlocked by players or religions
/// </summary>
public class Perk : Trait
{
    /// <summary>
    ///     Creates a new perk with the specified ID, name, and deity
    /// </summary>
    public Perk(string perkId, string name, DeityType deity)
    {
        PerkId = perkId;
        Name = name;
        Deity = deity;
    }

    /// <summary>
    ///     Parameterless constructor for serialization
    /// </summary>
    public Perk()
    {
    }

    /// <summary>
    ///     Unique identifier for this perk (e.g., "khoras_warriors_resolve")
    /// </summary>
    public string PerkId { get; set; } = string.Empty;

    /// <summary>
    ///     Display name of the perk
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Detailed description of what the perk does
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Which deity this perk belongs to
    /// </summary>
    public DeityType Deity { get; set; }

    /// <summary>
    ///     Type of perk (Player or Religion)
    /// </summary>
    public PerkKind Kind { get; set; }

    /// <summary>
    ///     Category for organization (Combat, Defense, etc.)
    /// </summary>
    public PerkCategory Category { get; set; }

    /// <summary>
    ///     Required player favor rank to unlock (for Player perks)
    /// </summary>
    public int RequiredFavorRank { get; set; }

    /// <summary>
    ///     Required religion prestige rank to unlock (for Religion perks)
    /// </summary>
    public int RequiredPrestigeRank { get; set; }

    /// <summary>
    ///     List of prerequisite perk IDs that must be unlocked first
    /// </summary>
    public List<string> PrerequisitePerks { get; set; } = new();

    /// <summary>
    ///     Dictionary of stat modifiers this perk provides
    ///     Key: stat name (e.g., "walkspeed", "meleeDamageMultiplier")
    ///     Value: modifier value (additive)
    /// </summary>
    public Dictionary<string, float> StatModifiers { get; set; } = new();

    /// <summary>
    ///     List of special effect identifiers for complex perk behaviors
    /// </summary>
    public List<string> SpecialEffects { get; set; } = new();
}