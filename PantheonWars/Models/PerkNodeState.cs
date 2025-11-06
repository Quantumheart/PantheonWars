namespace PantheonWars.Models;

/// <summary>
///     Represents the visual/UI state of a perk node in the perk tree
/// </summary>
public class PerkNodeState
{
    public PerkNodeState(Perk perk)
    {
        Perk = perk;
        VisualState = PerkNodeVisualState.Locked;
        IsUnlocked = false;
        CanUnlock = false;
    }

    /// <summary>
    ///     Reference to the perk this state represents
    /// </summary>
    public Perk Perk { get; set; }

    /// <summary>
    ///     Current visual state of the node
    /// </summary>
    public PerkNodeVisualState VisualState { get; set; }

    /// <summary>
    ///     Whether the perk is unlocked
    /// </summary>
    public bool IsUnlocked { get; set; }

    /// <summary>
    ///     Whether the perk can be unlocked (prerequisites met, rank sufficient)
    /// </summary>
    public bool CanUnlock { get; set; }

    /// <summary>
    ///     X position in the tree layout (calculated by PerkTreeLayout)
    /// </summary>
    public float PositionX { get; set; }

    /// <summary>
    ///     Y position in the tree layout (calculated by PerkTreeLayout)
    /// </summary>
    public float PositionY { get; set; }

    /// <summary>
    ///     Width of the node (for hit detection)
    /// </summary>
    public float Width { get; set; } = 64f;

    /// <summary>
    ///     Height of the node (for hit detection)
    /// </summary>
    public float Height { get; set; } = 64f;

    /// <summary>
    ///     Current glow animation alpha (0-1) for unlockable perks
    ///     Animated via lerp in Phase 5
    /// </summary>
    public float GlowAlpha { get; set; } = 0f;

    /// <summary>
    ///     Whether the node is currently being hovered
    /// </summary>
    public bool IsHovered { get; set; }

    /// <summary>
    ///     Whether the node is currently selected
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    ///     Tier/level of this perk in the tree (for vertical layout)
    ///     Tier 1 = top, Tier 4 = bottom
    /// </summary>
    public int Tier { get; set; }

    /// <summary>
    ///     Update visual state based on unlock status and unlock eligibility
    /// </summary>
    public void UpdateVisualState()
    {
        if (IsUnlocked)
            VisualState = PerkNodeVisualState.Unlocked;
        else if (CanUnlock)
            VisualState = PerkNodeVisualState.Unlockable;
        else
            VisualState = PerkNodeVisualState.Locked;
    }
}

/// <summary>
///     Visual states for perk nodes
/// </summary>
public enum PerkNodeVisualState
{
    /// <summary>
    ///     Perk is locked (greyed out, prerequisites not met)
    /// </summary>
    Locked,

    /// <summary>
    ///     Perk can be unlocked (green glow, player meets requirements)
    /// </summary>
    Unlockable,

    /// <summary>
    ///     Perk is already unlocked (gold/active color)
    /// </summary>
    Unlocked
}