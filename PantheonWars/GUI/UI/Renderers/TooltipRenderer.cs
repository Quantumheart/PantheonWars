using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Renders rich tooltips when hovering over perk nodes
///     Displays perk details, requirements, stats, and unlock status
/// </summary>
internal static class TooltipRenderer
{
    // Color constants
    private static readonly Vector4 ColorGold = new(0.996f, 0.682f, 0.204f, 1.0f); // #feae34
    private static readonly Vector4 ColorWhite = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Vector4 ColorGrey = new(0.573f, 0.502f, 0.416f, 1.0f); // #92806a
    private static readonly Vector4 ColorGreen = new(0.478f, 0.776f, 0.184f, 1.0f); // #7ac62f
    private static readonly Vector4 ColorRed = new(0.8f, 0.2f, 0.2f, 1.0f);
    private static readonly Vector4 ColorDarkBrown = new(0.16f, 0.12f, 0.09f, 0.95f); // #2a1f16
    private static readonly Vector4 ColorBorderGold = new(0.996f, 0.682f, 0.204f, 0.6f);

    private const float TooltipMaxWidth = 320f;
    private const float TooltipPadding = 12f;
    private const float LineSpacing = 4f;
    private const float SectionSpacing = 8f;

    /// <summary>
    ///     Draw a tooltip for a perk node when hovering
    /// </summary>
    /// <param name="tooltipData">Tooltip data to display</param>
    /// <param name="mouseX">Mouse X position (screen space)</param>
    /// <param name="mouseY">Mouse Y position (screen space)</param>
    /// <param name="windowWidth">Window width for edge detection</param>
    /// <param name="windowHeight">Window height for edge detection</param>
    public static void Draw(
        PerkTooltipData tooltipData,
        float mouseX,
        float mouseY,
        float windowWidth,
        float windowHeight)
    {
        if (tooltipData == null) return;

        // Use foreground draw list to ensure tooltip renders on top of everything
        var drawList = ImGui.GetForegroundDrawList();

        // Calculate tooltip content
        var lines = BuildTooltipLines(tooltipData);

        // Calculate tooltip dimensions
        var contentHeight = CalculateTooltipHeight(lines);
        var tooltipWidth = TooltipMaxWidth;
        var tooltipHeight = contentHeight + (TooltipPadding * 2);

        // Get window position to work in screen space
        var windowPos = ImGui.GetWindowPos();

        // Position tooltip (offset from mouse, check screen edges)
        var offsetX = 16f; // Offset from cursor
        var offsetY = 16f;

        var tooltipX = mouseX + offsetX;
        var tooltipY = mouseY + offsetY;

        // Check right edge (relative to window)
        if (tooltipX - windowPos.X + tooltipWidth > windowWidth)
            tooltipX = mouseX - tooltipWidth - offsetX; // Show on left side

        // Check bottom edge (relative to window)
        if (tooltipY - windowPos.Y + tooltipHeight > windowHeight)
            tooltipY = mouseY - tooltipHeight - offsetY; // Show above cursor

        // Ensure doesn't go off left edge
        if (tooltipX < windowPos.X)
            tooltipX = windowPos.X + 4f; // Small padding from left edge

        // Ensure doesn't go off top edge
        if (tooltipY < windowPos.Y)
            tooltipY = windowPos.Y + 4f; // Small padding from top edge

        // Draw tooltip background
        var bgStart = new Vector2(tooltipX, tooltipY);
        var bgEnd = new Vector2(tooltipX + tooltipWidth, tooltipY + tooltipHeight);
        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown);
        drawList.AddRectFilled(bgStart, bgEnd, bgColor, 4f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorBorderGold);
        drawList.AddRect(bgStart, bgEnd, borderColor, 4f, ImDrawFlags.None, 2f);

        // Render content
        var currentY = tooltipY + TooltipPadding;
        foreach (var line in lines)
        {
            var textPos = new Vector2(tooltipX + TooltipPadding, currentY);
            var textColor = ImGui.ColorConvertFloat4ToU32(line.Color);

            if (line.IsBold)
                drawList.AddText(ImGui.GetFont(), line.FontSize, textPos, textColor, line.Text);
            else
                drawList.AddText(ImGui.GetFont(), line.FontSize, textPos, textColor, line.Text);

            currentY += line.Height + line.SpacingAfter;
        }
    }

    /// <summary>
    ///     Build formatted lines for tooltip content
    /// </summary>
    private static List<TooltipLine> BuildTooltipLines(PerkTooltipData data)
    {
        var lines = new List<TooltipLine>();

        // Title (perk name) - Gold, larger font
        lines.Add(new TooltipLine
        {
            Text = data.Name,
            Color = ColorGold,
            FontSize = 18f,
            IsBold = true,
            SpacingAfter = SectionSpacing
        });

        // Category and Tier
        var categoryText = $"{data.Category} • Tier {data.Tier}";
        if (data.Kind == PerkKind.Religion)
            categoryText += " (Religion)";

        lines.Add(new TooltipLine
        {
            Text = categoryText,
            Color = ColorGrey,
            FontSize = 12f,
            SpacingAfter = SectionSpacing
        });

        // Description
        if (!string.IsNullOrEmpty(data.Description))
        {
            lines.Add(new TooltipLine
            {
                Text = data.Description,
                Color = ColorWhite,
                FontSize = 14f,
                SpacingAfter = SectionSpacing
            });
        }

        // Stat modifiers
        if (data.FormattedStats.Count > 0)
        {
            foreach (var stat in data.FormattedStats)
            {
                lines.Add(new TooltipLine
                {
                    Text = stat,
                    Color = ColorGreen,
                    FontSize = 13f,
                    SpacingAfter = LineSpacing
                });
            }

            // Add spacing after stats section
            if (lines.Count > 0)
                lines[lines.Count - 1].SpacingAfter = SectionSpacing;
        }

        // Special effects
        if (data.SpecialEffectDescriptions.Count > 0)
        {
            foreach (var effect in data.SpecialEffectDescriptions)
            {
                lines.Add(new TooltipLine
                {
                    Text = "• " + effect,
                    Color = ColorWhite,
                    FontSize = 13f,
                    SpacingAfter = LineSpacing
                });
            }

            // Add spacing after effects section
            if (lines.Count > 0)
                lines[lines.Count - 1].SpacingAfter = SectionSpacing;
        }

        // Requirements section
        var hasRequirements = false;

        // Rank requirement
        if (!string.IsNullOrEmpty(data.RequiredFavorRank))
        {
            // Green if unlocked, white if can unlock, red if locked
            var rankColor = data.IsUnlocked ? ColorGreen : (data.CanUnlock ? ColorWhite : ColorRed);
            lines.Add(new TooltipLine
            {
                Text = $"Requires: {data.RequiredFavorRank} (Favor)",
                Color = rankColor,
                FontSize = 13f,
                SpacingAfter = LineSpacing
            });
            hasRequirements = true;
        }
        else if (!string.IsNullOrEmpty(data.RequiredPrestigeRank))
        {
            // Green if unlocked, white if can unlock, red if locked
            var rankColor = data.IsUnlocked ? ColorGreen : (data.CanUnlock ? ColorWhite : ColorRed);
            lines.Add(new TooltipLine
            {
                Text = $"Requires: {data.RequiredPrestigeRank} (Prestige)",
                Color = rankColor,
                FontSize = 13f,
                SpacingAfter = LineSpacing
            });
            hasRequirements = true;
        }

        // Prerequisites
        if (data.PrerequisiteNames.Count > 0)
        {
            foreach (var prereq in data.PrerequisiteNames)
            {
                // Green if unlocked, white if can unlock, red if locked
                var prereqColor = data.IsUnlocked ? ColorGreen : (data.CanUnlock ? ColorWhite : ColorRed);
                lines.Add(new TooltipLine
                {
                    Text = $"Requires: {prereq}",
                    Color = prereqColor,
                    FontSize = 13f,
                    SpacingAfter = LineSpacing
                });
            }

            hasRequirements = true;
        }

        // Add spacing after requirements
        if (hasRequirements && lines.Count > 0)
            lines[lines.Count - 1].SpacingAfter = SectionSpacing;

        // Unlock status
        if (data.IsUnlocked)
        {
            lines.Add(new TooltipLine
            {
                Text = "[UNLOCKED]",
                Color = ColorGreen,
                FontSize = 14f,
                IsBold = true,
                SpacingAfter = 0
            });
        }
        else if (data.CanUnlock)
        {
            lines.Add(new TooltipLine
            {
                Text = "Click to unlock",
                Color = ColorGreen,
                FontSize = 13f,
                SpacingAfter = 0
            });
        }
        else
        {
            // Show lock reason if available, otherwise show generic locked message
            var lockMessage = !string.IsNullOrEmpty(data.UnlockBlockReason)
                ? data.UnlockBlockReason
                : "LOCKED";

            lines.Add(new TooltipLine
            {
                Text = lockMessage,
                Color = ColorRed,
                FontSize = 13f,
                SpacingAfter = 0
            });
        }

        return lines;
    }

    /// <summary>
    ///     Calculate total height needed for tooltip content
    /// </summary>
    private static float CalculateTooltipHeight(List<TooltipLine> lines)
    {
        var totalHeight = 0f;
        foreach (var line in lines) totalHeight += line.Height + line.SpacingAfter;

        return totalHeight;
    }

    /// <summary>
    ///     Represents a single line in a tooltip
    /// </summary>
    private class TooltipLine
    {
        public string Text { get; set; } = string.Empty;
        public Vector4 Color { get; set; } = new(1, 1, 1, 1);
        public float FontSize { get; set; } = 14f;
        public bool IsBold { get; set; }
        public float SpacingAfter { get; set; }

        public float Height => FontSize + 4f; // Font size + small buffer
    }
}
