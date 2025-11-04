using System.Numerics;
using ImGuiNET;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Renders the selected perk details panel at the bottom of the dialog
///     Shows: Name, description, requirements, stats, effects
/// </summary>
internal static class PerkInfoRenderer
{
    // Color constants
    private static readonly Vector4 ColorGold = new(0.996f, 0.682f, 0.204f, 1.0f); // #feae34
    private static readonly Vector4 ColorWhite = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Vector4 ColorGrey = new(0.573f, 0.502f, 0.416f, 1.0f); // #92806a
    private static readonly Vector4 ColorGreen = new(0.478f, 0.776f, 0.184f, 1.0f); // #7ac62f lime
    private static readonly Vector4 ColorRed = new(0.749f, 0.400f, 0.247f, 1.0f); // #bf663f
    private static readonly Vector4 ColorDarkBrown = new(0.16f, 0.12f, 0.09f, 0.9f); // #2a1f16

    /// <summary>
    ///     Draw the perk info panel
    /// </summary>
    /// <param name="manager">Perk dialog state manager</param>
    /// <param name="api">Client API</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="width">Available width</param>
    /// <param name="height">Available height</param>
    /// <returns>Height used by this renderer</returns>
    public static float Draw(
        PerkDialogManager manager,
        ICoreClientAPI api,
        float x, float y, float width, float height)
    {
        var drawList = ImGui.GetWindowDrawList();
        var startPos = new Vector2(x, y);
        var endPos = new Vector2(x + width, y + height);

        // Draw panel background
        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown);
        drawList.AddRectFilled(startPos, endPos, bgColor, 4f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorGold * 0.5f);
        drawList.AddRect(startPos, endPos, borderColor, 4f, ImDrawFlags.None, 2f);

        // Get selected perk state
        var selectedState = manager.GetSelectedPerkState();

        if (selectedState == null)
        {
            // No perk selected - show prompt
            var promptText = "Select a perk to view details";
            var textSize = ImGui.CalcTextSize(promptText);
            var textPos = new Vector2(
                x + (width - textSize.X) / 2,
                y + (height - textSize.Y) / 2
            );
            var textColor = ImGui.ColorConvertFloat4ToU32(ColorGrey);
            drawList.AddText(textPos, textColor, promptText);

            return height;
        }

        // Perk selected - draw detailed info
        const float padding = 16f;
        var currentY = y + padding;
        var contentWidth = width - padding * 2;

        // Perk name (title)
        var titleColor = selectedState.IsUnlocked ? ColorGold : ColorWhite;
        var titleColorU32 = ImGui.ColorConvertFloat4ToU32(titleColor);
        drawList.AddText(ImGui.GetFont(), 20f, new Vector2(x + padding, currentY), titleColorU32,
            selectedState.Perk.Name);
        currentY += 28f;

        // Status badge
        var statusText = selectedState.VisualState switch
        {
            PerkNodeVisualState.Unlocked => "[UNLOCKED]",
            PerkNodeVisualState.Unlockable => "[AVAILABLE]",
            _ => "[LOCKED]"
        };

        var statusColor = selectedState.VisualState switch
        {
            PerkNodeVisualState.Unlocked => ColorGold,
            PerkNodeVisualState.Unlockable => ColorGreen,
            _ => ColorRed
        };

        var statusColorU32 = ImGui.ColorConvertFloat4ToU32(statusColor);
        drawList.AddText(ImGui.GetFont(), 12f, new Vector2(x + padding, currentY), statusColorU32, statusText);
        currentY += 20f;

        // Category and kind
        var metaText = $"{selectedState.Perk.Category} | {selectedState.Perk.Kind} Perk | Tier {selectedState.Tier}";
        var metaColorU32 = ImGui.ColorConvertFloat4ToU32(ColorGrey);
        drawList.AddText(ImGui.GetFont(), 12f, new Vector2(x + padding, currentY), metaColorU32, metaText);
        currentY += 20f;

        // Separator line
        var lineY = currentY;
        var lineStart = new Vector2(x + padding, lineY);
        var lineEnd = new Vector2(x + width - padding, lineY);
        var lineColor = ImGui.ColorConvertFloat4ToU32(ColorGrey * 0.5f);
        drawList.AddLine(lineStart, lineEnd, lineColor, 1f);
        currentY += 8f;

        // Description (word-wrapped)
        var descriptionColorU32 = ImGui.ColorConvertFloat4ToU32(ColorWhite);
        DrawWrappedText(drawList, selectedState.Perk.Description,
            x + padding, currentY, contentWidth, descriptionColorU32, 14f);
        currentY += 40f; // Approximate space for description

        // Requirements section
        if (!selectedState.IsUnlocked)
        {
            currentY += 8f;
            var reqTitleColorU32 = ImGui.ColorConvertFloat4ToU32(ColorGold);
            drawList.AddText(ImGui.GetFont(), 14f, new Vector2(x + padding, currentY), reqTitleColorU32,
                "Requirements:");
            currentY += 18f;

            // Rank requirement
            var rankReq = selectedState.Perk.Kind == PerkKind.Player
                ? $"• Favor Rank: {GetRankName(selectedState.Perk.RequiredFavorRank, true)}"
                : $"• Prestige Rank: {GetRankName(selectedState.Perk.RequiredPrestigeRank, false)}";

            drawList.AddText(ImGui.GetFont(), 12f, new Vector2(x + padding + 8, currentY), descriptionColorU32,
                rankReq);
            currentY += 16f;

            // Prerequisites
            if (selectedState.Perk.PrerequisitePerks != null && selectedState.Perk.PrerequisitePerks.Count > 0)
                foreach (var prereqId in selectedState.Perk.PrerequisitePerks)
                {
                    var prereqState = manager.GetPerkState(prereqId);
                    var prereqName = prereqState?.Perk.Name ?? prereqId;
                    var prereqText = $"• Unlock: {prereqName}";

                    var prereqColor = prereqState?.IsUnlocked ?? false ? ColorGreen : ColorRed;
                    var prereqColorU32 = ImGui.ColorConvertFloat4ToU32(prereqColor);

                    drawList.AddText(ImGui.GetFont(), 12f, new Vector2(x + padding + 8, currentY),
                        prereqColorU32, prereqText);
                    currentY += 16f;
                }
        }

        // Stats section (if space available)
        if (currentY < y + height - 60f && selectedState.Perk.StatModifiers.Count > 0)
        {
            currentY += 8f;
            var statsTitleColorU32 = ImGui.ColorConvertFloat4ToU32(ColorGold);
            drawList.AddText(ImGui.GetFont(), 14f, new Vector2(x + padding, currentY), statsTitleColorU32,
                "Effects:");
            currentY += 18f;

            foreach (var stat in selectedState.Perk.StatModifiers)
            {
                // Format stat display
                var statText = FormatStatModifier(stat.Key, stat.Value);
                var statColorU32 = ImGui.ColorConvertFloat4ToU32(ColorGreen);

                drawList.AddText(ImGui.GetFont(), 12f, new Vector2(x + padding + 8, currentY), statColorU32,
                    statText);
                currentY += 16f;

                // Prevent overflow
                if (currentY > y + height - 20f) break;
            }
        }

        return height;
    }

    /// <summary>
    ///     Draw text with word wrapping
    /// </summary>
    private static void DrawWrappedText(ImDrawListPtr drawList, string text,
        float x, float y, float maxWidth, uint color, float fontSize)
    {
        // Simple wrapping - split by words
        var words = text.Split(' ');
        var currentLine = "";
        var currentY = y;

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var testSize = ImGui.CalcTextSize(testLine);

            if (testSize.X > maxWidth && !string.IsNullOrEmpty(currentLine))
            {
                // Draw current line and start new one
                drawList.AddText(ImGui.GetFont(), fontSize, new Vector2(x, currentY), color, currentLine);
                currentY += fontSize + 4f;
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        // Draw last line
        if (!string.IsNullOrEmpty(currentLine))
            drawList.AddText(ImGui.GetFont(), fontSize, new Vector2(x, currentY), color, currentLine);
    }

    /// <summary>
    ///     Format stat modifier for display
    /// </summary>
    private static string FormatStatModifier(string statName, float value)
    {
        var percentageStats = new[] { "walkspeed", "meleeDamage", "rangedDamage", "maxhealthExtraMultiplier" };
        var isPercentage = false;

        foreach (var percentStat in percentageStats)
            if (statName.Contains(percentStat))
            {
                isPercentage = true;
                break;
            }

        var displayName = statName switch
        {
            "walkspeed" => "Movement Speed",
            "meleeDamage" => "Melee Damage",
            "rangedDamage" => "Ranged Damage",
            "maxhealthExtraMultiplier" => "Max Health",
            _ => statName
        };

        var sign = value >= 0 ? "+" : "";

        if (isPercentage) return $"• {sign}{value * 100:0.#}% {displayName}";

        return $"• {sign}{value:0.#} {displayName}";
    }

    /// <summary>
    ///     Get rank name from rank number
    /// </summary>
    private static string GetRankName(int rank, bool isFavorRank)
    {
        if (isFavorRank)
            return rank switch
            {
                0 => "Initiate",
                1 => "Devoted",
                2 => "Zealot",
                3 => "Champion",
                4 => "Exalted",
                _ => $"Rank {rank}"
            };

        return rank switch
        {
            0 => "Fledgling",
            1 => "Established",
            2 => "Renowned",
            3 => "Legendary",
            4 => "Mythic",
            _ => $"Rank {rank}"
        };
    }
}