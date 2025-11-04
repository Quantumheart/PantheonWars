using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using PantheonWars.Models;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Renders tooltips when hovering over perk nodes
///     Shows detailed perk information including requirements, stats, and unlock status
/// </summary>
internal static class TooltipRenderer
{
    private const float TooltipWidth = 300f;
    private const float TooltipPadding = 12f;
    private const float LineHeight = 18f;
    private const float TitleSize = 16f;
    private const float BodySize = 13f;
    private const float SmallSize = 11f;

    // Color constants
    private static readonly Vector4 ColorBg = new(0.1f, 0.08f, 0.06f, 0.95f); // Dark brown, mostly opaque
    private static readonly Vector4 ColorBorder = new(0.996f, 0.682f, 0.204f, 1.0f); // #feae34 gold
    private static readonly Vector4 ColorTitle = new(0.996f, 0.682f, 0.204f, 1.0f); // #feae34 gold
    private static readonly Vector4 ColorWhite = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Vector4 ColorGrey = new(0.573f, 0.502f, 0.416f, 1.0f); // #92806a
    private static readonly Vector4 ColorGreen = new(0.478f, 0.776f, 0.184f, 1.0f); // #7ac62f lime
    private static readonly Vector4 ColorRed = new(0.749f, 0.400f, 0.247f, 1.0f); // #bf663f
    private static readonly Vector4 ColorYellow = new(0.9f, 0.8f, 0.3f, 1.0f);

    /// <summary>
    ///     Draw a tooltip for a hovered perk node
    /// </summary>
    /// <param name="manager">Perk dialog state manager</param>
    /// <param name="api">Client API</param>
    /// <param name="perkState">The perk node being hovered</param>
    /// <param name="mouseX">Mouse X position</param>
    /// <param name="mouseY">Mouse Y position</param>
    /// <param name="screenWidth">Screen width (for positioning)</param>
    /// <param name="screenHeight">Screen height (for positioning)</param>
    public static void Draw(
        PerkDialogManager manager,
        ICoreClientAPI api,
        PerkNodeState perkState,
        float mouseX, float mouseY,
        float screenWidth, float screenHeight)
    {
        var perk = perkState.Perk;

        // Calculate tooltip content and height
        var contentLines = BuildTooltipContent(manager, perkState);
        var tooltipHeight = CalculateTooltipHeight(contentLines);

        // Position tooltip near mouse, but keep it on screen
        var tooltipX = mouseX + 15f;
        var tooltipY = mouseY + 15f;

        // Adjust if tooltip goes off right edge
        if (tooltipX + TooltipWidth > screenWidth)
            tooltipX = mouseX - TooltipWidth - 15f;

        // Adjust if tooltip goes off bottom edge
        if (tooltipY + tooltipHeight > screenHeight)
            tooltipY = screenHeight - tooltipHeight - 10f;

        // Ensure minimum position
        if (tooltipX < 10f) tooltipX = 10f;
        if (tooltipY < 10f) tooltipY = 10f;

        // Draw tooltip
        DrawTooltipBox(tooltipX, tooltipY, TooltipWidth, tooltipHeight);
        DrawTooltipContent(contentLines, tooltipX, tooltipY);
    }

    /// <summary>
    ///     Build the content lines for the tooltip
    /// </summary>
    private static List<TooltipLine> BuildTooltipContent(PerkDialogManager manager, PerkNodeState perkState)
    {
        var lines = new List<TooltipLine>();
        var perk = perkState.Perk;

        // Title (perk name)
        lines.Add(new TooltipLine(perk.Name, TitleSize, ColorTitle, true));

        // Status badge
        var (statusText, statusColor) = perkState.VisualState switch
        {
            PerkNodeVisualState.Unlocked => ("[UNLOCKED]", ColorGreen),
            PerkNodeVisualState.Unlockable => ("[AVAILABLE TO UNLOCK]", ColorYellow),
            _ => ("[LOCKED]", ColorRed)
        };
        lines.Add(new TooltipLine(statusText, SmallSize, statusColor, false));

        // Metadata (category, kind, tier)
        var metaText = $"{perk.Category} | {perk.Kind} Perk | Tier {perkState.Tier}";
        lines.Add(new TooltipLine(metaText, SmallSize, ColorGrey, false));

        // Spacer
        lines.Add(new TooltipLine("", 6f, ColorWhite, false));

        // Description
        lines.Add(new TooltipLine(perk.Description, BodySize, ColorWhite, false));

        // Spacer
        lines.Add(new TooltipLine("", 8f, ColorWhite, false));

        // Requirements (if not unlocked)
        if (!perkState.IsUnlocked)
        {
            lines.Add(new TooltipLine("Requirements:", BodySize, ColorTitle, true));

            // Rank requirement
            var rankReq = perk.Kind == Models.Enum.PerkKind.Player
                ? $"• Favor Rank: {GetRankName(perk.RequiredFavorRank, true)}"
                : $"• Prestige Rank: {GetRankName(perk.RequiredPrestigeRank, false)}";
            lines.Add(new TooltipLine(rankReq, BodySize, ColorWhite, false));

            // Prerequisites
            if (perk.PrerequisitePerks != null && perk.PrerequisitePerks.Count > 0)
            {
                foreach (var prereqId in perk.PrerequisitePerks)
                {
                    var prereqState = manager.GetPerkState(prereqId);
                    var prereqName = prereqState?.Perk.Name ?? prereqId;
                    var prereqMet = prereqState?.IsUnlocked ?? false;
                    var prereqColor = prereqMet ? ColorGreen : ColorRed;
                    var prereqPrefix = prereqMet ? "✓" : "✗";

                    lines.Add(new TooltipLine($"  {prereqPrefix} Unlock: {prereqName}", BodySize, prereqColor, false));
                }
            }

            // Spacer
            lines.Add(new TooltipLine("", 8f, ColorWhite, false));
        }

        // Effects (stat modifiers)
        if (perk.StatModifiers.Count > 0)
        {
            lines.Add(new TooltipLine("Effects:", BodySize, ColorTitle, true));

            foreach (var stat in perk.StatModifiers)
            {
                var statText = FormatStatModifier(stat.Key, stat.Value);
                lines.Add(new TooltipLine($"• {statText}", BodySize, ColorGreen, false));
            }

            // Spacer
            lines.Add(new TooltipLine("", 8f, ColorWhite, false));
        }

        // Special effects
        if (perk.SpecialEffects.Count > 0)
        {
            lines.Add(new TooltipLine("Special Effects:", BodySize, ColorTitle, true));

            foreach (var effect in perk.SpecialEffects)
            {
                lines.Add(new TooltipLine($"• {effect}", BodySize, ColorYellow, false));
            }
        }

        return lines;
    }

    /// <summary>
    ///     Calculate total tooltip height based on content lines
    /// </summary>
    private static float CalculateTooltipHeight(List<TooltipLine> lines)
    {
        var totalHeight = TooltipPadding * 2;

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.Text))
            {
                // Spacer
                totalHeight += line.FontSize;
            }
            else
            {
                // Calculate wrapped text height
                var wrappedLines = WrapText(line.Text, line.FontSize, TooltipWidth - TooltipPadding * 2);
                totalHeight += wrappedLines.Count * (line.FontSize + 4f);
            }
        }

        return totalHeight;
    }

    /// <summary>
    ///     Draw the tooltip background box
    /// </summary>
    private static void DrawTooltipBox(float x, float y, float width, float height)
    {
        var drawList = ImGui.GetForegroundDrawList();
        var start = new Vector2(x, y);
        var end = new Vector2(x + width, y + height);

        // Background
        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorBg);
        drawList.AddRectFilled(start, end, bgColor, 4f);

        // Border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorBorder);
        drawList.AddRect(start, end, borderColor, 4f, ImDrawFlags.None, 2f);
    }

    /// <summary>
    ///     Draw tooltip content lines
    /// </summary>
    private static void DrawTooltipContent(List<TooltipLine> lines, float x, float y)
    {
        var drawList = ImGui.GetForegroundDrawList();
        var currentY = y + TooltipPadding;
        var contentWidth = TooltipWidth - TooltipPadding * 2;

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.Text))
            {
                // Spacer
                currentY += line.FontSize;
                continue;
            }

            // Word wrap if needed
            var wrappedLines = WrapText(line.Text, line.FontSize, contentWidth);

            foreach (var wrappedLine in wrappedLines)
            {
                var textPos = new Vector2(x + TooltipPadding, currentY);
                var textColor = ImGui.ColorConvertFloat4ToU32(line.Color);

                if (line.Bold)
                {
                    // Simulate bold by drawing text twice with slight offset
                    drawList.AddText(ImGui.GetFont(), line.FontSize, textPos, textColor, wrappedLine);
                    drawList.AddText(ImGui.GetFont(), line.FontSize, new Vector2(textPos.X + 0.5f, textPos.Y),
                        textColor, wrappedLine);
                }
                else
                {
                    drawList.AddText(ImGui.GetFont(), line.FontSize, textPos, textColor, wrappedLine);
                }

                currentY += line.FontSize + 4f;
            }
        }
    }

    /// <summary>
    ///     Wrap text to fit within max width
    /// </summary>
    private static List<string> WrapText(string text, float fontSize, float maxWidth)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;

            // Approximate text width (ImGui.CalcTextSize requires pushed font)
            var approxWidth = testLine.Length * (fontSize * 0.5f);

            if (approxWidth > maxWidth && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
            lines.Add(currentLine);

        return lines;
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
            "healingeffectivness" => "Healing Effectiveness",
            "rangedWeaponsSpeed" => "Ranged Attack Speed",
            _ => statName
        };

        var sign = value >= 0 ? "+" : "";

        if (isPercentage)
            return $"{sign}{value * 100:0.#}% {displayName}";

        return $"{sign}{value:0.#} {displayName}";
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

    /// <summary>
    ///     Helper class to represent a line in the tooltip
    /// </summary>
    private class TooltipLine
    {
        public TooltipLine(string text, float fontSize, Vector4 color, bool bold)
        {
            Text = text;
            FontSize = fontSize;
            Color = color;
            Bold = bold;
        }

        public string Text { get; }
        public float FontSize { get; }
        public Vector4 Color { get; }
        public bool Bold { get; }
    }
}
