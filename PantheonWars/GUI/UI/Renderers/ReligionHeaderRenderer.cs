using System.Numerics;
using ImGuiNET;
using PantheonWars.Models.Enum;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Renders the religion/deity header banner at the top of the perk dialog
///     Shows: Religion name, deity icon/name, favor/prestige ranks
/// </summary>
internal static class ReligionHeaderRenderer
{
    // Color constants (matching XSkillsGilded design language)
    private static readonly Vector4 ColorGold = new(0.996f, 0.682f, 0.204f, 1.0f); // #feae34
    private static readonly Vector4 ColorWhite = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Vector4 ColorGrey = new(0.573f, 0.502f, 0.416f, 1.0f); // #92806a
    private static readonly Vector4 ColorDarkBrown = new(0.24f, 0.18f, 0.13f, 1.0f); // #3d2e20

    /// <summary>
    ///     Draw the religion header banner
    /// </summary>
    /// <param name="manager">Perk dialog state manager</param>
    /// <param name="api">Client API</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="width">Available width</param>
    /// <returns>Height used by this renderer</returns>
    public static float Draw(
        PerkDialogManager manager,
        ICoreClientAPI api,
        float x, float y, float width)
    {
        const float headerHeight = 80f;
        const float padding = 16f;

        var drawList = ImGui.GetWindowDrawList();
        var startPos = new Vector2(x, y);
        var endPos = new Vector2(x + width, y + headerHeight);

        // Draw header background
        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown);
        drawList.AddRectFilled(startPos, endPos, bgColor, 4f); // Rounded corners

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorGold * 0.5f);
        drawList.AddRect(startPos, endPos, borderColor, 4f, ImDrawFlags.None, 2f);

        // Check if player has a religion
        if (!manager.HasReligion())
        {
            // Display "No Religion" message
            var noReligionText = "No Religion - Join or create one to unlock perks!";
            var textSize = ImGui.CalcTextSize(noReligionText);
            var textPos = new Vector2(
                x + (width - textSize.X) / 2,
                y + (headerHeight - textSize.Y) / 2
            );

            var textColor = ImGui.ColorConvertFloat4ToU32(ColorGrey);
            drawList.AddText(textPos, textColor, noReligionText);

            return headerHeight;
        }

        // Religion info available - draw detailed header
        var currentX = x + padding;
        var centerY = y + headerHeight / 2;

        // TODO: Phase 5 - Add deity icon (if textures available)
        // For now, use placeholder circle
        const float iconSize = 48f;
        var iconCenter = new Vector2(currentX + iconSize / 2, centerY);
        var iconColor = ImGui.ColorConvertFloat4ToU32(GetDeityColor(manager.CurrentDeity));
        drawList.AddCircleFilled(iconCenter, iconSize / 2, iconColor, 16);

        currentX += iconSize + padding;

        // Religion name and deity
        var religionName = manager.CurrentReligionName ?? "Unknown Religion";
        var deityName = GetDeityDisplayName(manager.CurrentDeity);
        var headerText = $"{religionName} - {deityName}";

        var headerTextPos = new Vector2(currentX, y + 16f);
        var headerTextColor = ImGui.ColorConvertFloat4ToU32(ColorGold);
        drawList.AddText(ImGui.GetFont(), 20f, headerTextPos, headerTextColor, headerText);

        // TODO: Phase 6 - Display actual favor/prestige from server data
        // For now, show placeholder
        var subText = "Favor: Initiate | Prestige: Fledgling";
        var subTextPos = new Vector2(currentX, y + 45f);
        var subTextColor = ImGui.ColorConvertFloat4ToU32(ColorWhite);
        drawList.AddText(ImGui.GetFont(), 14f, subTextPos, subTextColor, subText);

        return headerHeight;
    }

    /// <summary>
    ///     Get display name for a deity
    /// </summary>
    private static string GetDeityDisplayName(DeityType deity)
    {
        return deity switch
        {
            DeityType.Khoras => "Khoras - God of War",
            DeityType.Lysa => "Lysa - Goddess of the Hunt",
            DeityType.Morthen => "Morthen - God of Death",
            DeityType.Aethra => "Aethra - Goddess of Light",
            DeityType.Umbros => "Umbros - God of Shadows",
            DeityType.Tharos => "Tharos - God of Storms",
            DeityType.Gaia => "Gaia - Goddess of Earth",
            DeityType.Vex => "Vex - God of Madness",
            _ => "Unknown Deity"
        };
    }

    /// <summary>
    ///     Get color associated with a deity (for icon placeholder)
    /// </summary>
    private static Vector4 GetDeityColor(DeityType deity)
    {
        return deity switch
        {
            DeityType.Khoras => new Vector4(0.8f, 0.2f, 0.2f, 1.0f), // Red - War
            DeityType.Lysa => new Vector4(0.4f, 0.8f, 0.3f, 1.0f), // Green - Hunt
            DeityType.Morthen => new Vector4(0.3f, 0.1f, 0.4f, 1.0f), // Purple - Death
            DeityType.Aethra => new Vector4(0.9f, 0.9f, 0.6f, 1.0f), // Light yellow - Light
            DeityType.Umbros => new Vector4(0.2f, 0.2f, 0.3f, 1.0f), // Dark grey - Shadows
            DeityType.Tharos => new Vector4(0.3f, 0.6f, 0.9f, 1.0f), // Blue - Storms
            DeityType.Gaia => new Vector4(0.5f, 0.4f, 0.2f, 1.0f), // Brown - Earth
            DeityType.Vex => new Vector4(0.7f, 0.2f, 0.7f, 1.0f), // Magenta - Madness
            _ => new Vector4(0.5f, 0.5f, 0.5f, 1.0f) // Grey - Unknown
        };
    }
}