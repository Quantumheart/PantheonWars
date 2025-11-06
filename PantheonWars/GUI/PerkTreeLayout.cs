using System;
using System.Collections.Generic;
using System.Linq;
using PantheonWars.Models;

namespace PantheonWars.GUI;

/// <summary>
///     Calculates positions for perk nodes in the tree layout
///     Uses tier-based vertical arrangement with horizontal spacing
/// </summary>
public static class PerkTreeLayout
{
    // Layout constants
    private const float NodeWidth = 64f;
    private const float NodeHeight = 64f;
    private const float VerticalSpacing = 120f; // Space between tiers
    private const float HorizontalSpacing = 100f; // Space between nodes in same tier
    private const float TopPadding = 40f;
    private const float LeftPadding = 40f;

    /// <summary>
    ///     Calculate layout for a collection of perk node states
    ///     Arranges nodes by tier (1-4) vertically, spread horizontally within each tier
    /// </summary>
    public static void CalculateLayout(Dictionary<string, PerkNodeState> perkStates, float containerWidth)
    {
        if (perkStates.Count == 0) return;

        // Determine tiers for each perk based on prerequisites
        AssignTiers(perkStates);

        // Group nodes by tier
        var nodesByTier = perkStates.Values
            .GroupBy(node => node.Tier)
            .OrderBy(group => group.Key)
            .ToList();

        var currentY = TopPadding;

        foreach (var tierGroup in nodesByTier)
        {
            var nodesInTier = tierGroup.ToList();
            var nodeCount = nodesInTier.Count;

            // Calculate horizontal layout for this tier
            var totalWidth = nodeCount * NodeWidth + (nodeCount - 1) * HorizontalSpacing;
            var startX = LeftPadding + (containerWidth - totalWidth - LeftPadding * 2) / 2;

            // If total width exceeds container, use left alignment with smaller spacing
            if (totalWidth > containerWidth - LeftPadding * 2)
            {
                startX = LeftPadding;
                var availableWidth = containerWidth - LeftPadding * 2 - nodeCount * NodeWidth;
                var adjustedSpacing = nodeCount > 1 ? availableWidth / (nodeCount - 1) : 0;

                // Position nodes with adjusted spacing
                for (var i = 0; i < nodeCount; i++)
                {
                    var node = nodesInTier[i];
                    node.PositionX = startX + i * (NodeWidth + adjustedSpacing);
                    node.PositionY = currentY;
                    node.Width = NodeWidth;
                    node.Height = NodeHeight;
                }
            }
            else
            {
                // Position nodes with standard spacing
                for (var i = 0; i < nodeCount; i++)
                {
                    var node = nodesInTier[i];
                    node.PositionX = startX + i * (NodeWidth + HorizontalSpacing);
                    node.PositionY = currentY;
                    node.Width = NodeWidth;
                    node.Height = NodeHeight;
                }
            }

            currentY += NodeHeight + VerticalSpacing;
        }
    }

    /// <summary>
    ///     Assign tier levels to perks based on their prerequisite chains
    ///     Tier 1 = no prerequisites, Tier 2+ = based on deepest prerequisite + 1
    /// </summary>
    private static void AssignTiers(Dictionary<string, PerkNodeState> perkStates)
    {
        // Build a lookup map
        var stateMap = new Dictionary<string, PerkNodeState>();
        foreach (var kvp in perkStates)
        {
            stateMap[kvp.Key] = kvp.Value;
            kvp.Value.Tier = 0; // Initialize
        }

        // Calculate tier for each node recursively
        foreach (var state in perkStates.Values) CalculateTier(state, stateMap, new HashSet<string>());
    }

    /// <summary>
    ///     Recursively calculate tier for a perk node
    /// </summary>
    private static int CalculateTier(PerkNodeState node, Dictionary<string, PerkNodeState> stateMap,
        HashSet<string> visiting)
    {
        // If already calculated, return cached value
        if (node.Tier > 0) return node.Tier;

        // Check for circular dependencies
        if (visiting.Contains(node.Perk.PerkId))
        {
            // Circular dependency detected, assign tier 1 to break cycle
            node.Tier = 1;
            return 1;
        }

        visiting.Add(node.Perk.PerkId);

        // If no prerequisites, this is tier 1
        if (node.Perk.PrerequisitePerks == null || node.Perk.PrerequisitePerks.Count == 0)
        {
            node.Tier = 1;
            visiting.Remove(node.Perk.PerkId);
            return 1;
        }

        // Find the maximum tier among prerequisites
        var maxPrereqTier = 0;
        foreach (var prereqId in node.Perk.PrerequisitePerks)
            if (stateMap.TryGetValue(prereqId, out var prereqState))
            {
                var prereqTier = CalculateTier(prereqState, stateMap, visiting);
                maxPrereqTier = Math.Max(maxPrereqTier, prereqTier);
            }

        // This node's tier is one more than the highest prerequisite tier
        node.Tier = maxPrereqTier + 1;
        visiting.Remove(node.Perk.PerkId);
        return node.Tier;
    }

    /// <summary>
    ///     Get total height needed for the tree layout
    /// </summary>
    public static float GetTotalHeight(Dictionary<string, PerkNodeState> perkStates)
    {
        if (perkStates.Count == 0) return 0;

        var maxTier = perkStates.Values.Max(node => node.Tier);
        return TopPadding + maxTier * (NodeHeight + VerticalSpacing);
    }

    /// <summary>
    ///     Get total width needed for the tree layout
    /// </summary>
    public static float GetTotalWidth(Dictionary<string, PerkNodeState> perkStates)
    {
        if (perkStates.Count == 0) return 0;

        // Find tier with most nodes
        var maxNodesInTier = perkStates.Values
            .GroupBy(node => node.Tier)
            .Max(group => group.Count());

        return LeftPadding * 2 + maxNodesInTier * NodeWidth + (maxNodesInTier - 1) * HorizontalSpacing;
    }

    /// <summary>
    ///     Check if a point is inside a node's bounds (for click detection)
    /// </summary>
    public static bool IsPointInNode(PerkNodeState node, float x, float y)
    {
        return x >= node.PositionX &&
               x <= node.PositionX + node.Width &&
               y >= node.PositionY &&
               y <= node.PositionY + node.Height;
    }

    /// <summary>
    ///     Find which node (if any) contains the given point
    /// </summary>
    public static PerkNodeState? FindNodeAtPoint(Dictionary<string, PerkNodeState> perkStates, float x, float y)
    {
        foreach (var state in perkStates.Values)
            if (IsPointInNode(state, x, y))
                return state;

        return null;
    }
}