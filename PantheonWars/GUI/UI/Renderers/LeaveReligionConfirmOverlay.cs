using System;
using System.Numerics;
using ImGuiNET;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Confirmation overlay for leaving a religion
/// </summary>
internal static class LeaveReligionConfirmOverlay
{
    // Color constants
    private static readonly Vector4 ColorGold = new(0.996f, 0.682f, 0.204f, 1.0f);
    private static readonly Vector4 ColorWhite = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Vector4 ColorGrey = new(0.573f, 0.502f, 0.416f, 1.0f);
    private static readonly Vector4 ColorDarkBrown = new(0.24f, 0.18f, 0.13f, 1.0f);
    private static readonly Vector4 ColorLightBrown = new(0.35f, 0.26f, 0.19f, 1.0f);
    private static readonly Vector4 ColorBackground = new(0.16f, 0.12f, 0.09f, 0.95f);
    private static readonly Vector4 ColorRed = new(0.9f, 0.2f, 0.2f, 1.0f);

    /// <summary>
    ///     Draw the leave religion confirmation overlay
    /// </summary>
    /// <param name="api">Client API</param>
    /// <param name="windowWidth">Parent window width</param>
    /// <param name="windowHeight">Parent window height</param>
    /// <param name="religionName">Name of the religion being left</param>
    /// <param name="onCancel">Callback when cancel clicked</param>
    /// <param name="onConfirm">Callback when confirm clicked</param>
    /// <returns>True if overlay should remain open</returns>
    public static bool Draw(
        ICoreClientAPI api,
        int windowWidth,
        int windowHeight,
        string religionName,
        Action onCancel,
        Action onConfirm)
    {
        const float overlayWidth = 450f;
        const float overlayHeight = 280f; // Increased from 250f to prevent button/text overlap
        const float padding = 20f;

        var windowPos = ImGui.GetWindowPos();
        var overlayX = windowPos.X + (windowWidth - overlayWidth) / 2;
        var overlayY = windowPos.Y + (windowHeight - overlayHeight) / 2;

        var drawList = ImGui.GetForegroundDrawList();

        // Draw semi-transparent background
        var bgStart = windowPos;
        var bgEnd = new Vector2(windowPos.X + windowWidth, windowPos.Y + windowHeight);
        var bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.8f));
        drawList.AddRectFilled(bgStart, bgEnd, bgColor);

        // Draw main panel
        var panelStart = new Vector2(overlayX, overlayY);
        var panelEnd = new Vector2(overlayX + overlayWidth, overlayY + overlayHeight);
        var panelColor = ImGui.ColorConvertFloat4ToU32(ColorBackground);
        drawList.AddRectFilled(panelStart, panelEnd, panelColor, 8f);

        // Draw border (red to indicate warning)
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorRed);
        drawList.AddRect(panelStart, panelEnd, borderColor, 8f, ImDrawFlags.None, 3f);

        var currentY = overlayY + padding;

        // === HEADER ===
        var headerText = "Leave Religion?";
        var headerSize = ImGui.CalcTextSize(headerText);
        var headerPos = new Vector2(overlayX + (overlayWidth - headerSize.X) / 2, currentY);
        var headerColor = ImGui.ColorConvertFloat4ToU32(ColorRed);
        drawList.AddText(ImGui.GetFont(), 20f, headerPos, headerColor, headerText);

        currentY += headerSize.Y + padding * 1.5f;

        // === WARNING MESSAGE ===
        var warningLines = new[]
        {
            $"Are you sure you want to leave '{religionName}'?",
            "",
            "You will lose:",
            "  - Access to all deity perks",
            "  - Your rank and position in the religion",
            "  - Connection with fellow members",
            "",
            "This action cannot be easily undone."
        };

        foreach (var line in warningLines)
        {
            if (string.IsNullOrEmpty(line))
            {
                currentY += 8f;
                continue;
            }

            var lineSize = ImGui.CalcTextSize(line);
            var linePos = new Vector2(overlayX + padding, currentY);
            var lineColor = line.StartsWith("  -")
                ? ImGui.ColorConvertFloat4ToU32(ColorGrey)
                : ImGui.ColorConvertFloat4ToU32(ColorWhite);
            drawList.AddText(ImGui.GetFont(), 13f, linePos, lineColor, line);
            currentY += lineSize.Y + 4f;
        }

        // === BUTTONS ===
        const float buttonWidth = 120f;
        const float buttonHeight = 36f;
        const float buttonSpacing = 20f;
        var buttonY = overlayY + overlayHeight - padding - buttonHeight;
        var buttonsStartX = overlayX + (overlayWidth - buttonWidth * 2 - buttonSpacing) / 2;

        // Cancel button
        var cancelButtonX = buttonsStartX;
        if (DrawButton(drawList, "Cancel", cancelButtonX, buttonY, buttonWidth, buttonHeight, true))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            onCancel.Invoke();
            return false;
        }

        // Leave button (warning colored)
        var leaveButtonX = buttonsStartX + buttonWidth + buttonSpacing;
        if (DrawButton(drawList, "Leave", leaveButtonX, buttonY, buttonWidth, buttonHeight, false, ColorRed))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            onConfirm.Invoke();
            return false;
        }

        return true; // Keep overlay open
    }

    /// <summary>
    ///     Draw button
    /// </summary>
    private static bool DrawButton(ImDrawListPtr drawList, string text, float x, float y, float width, float height, bool isPrimary, Vector4? customColor = null)
    {
        var buttonStart = new Vector2(x, y);
        var buttonEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;

        // Determine background color
        Vector4 bgColor;
        var baseColor = customColor ?? (isPrimary ? ColorGold : ColorDarkBrown);

        if (isHovering && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            bgColor = baseColor * 0.7f;
        }
        else if (isHovering)
        {
            bgColor = baseColor * 0.9f;
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        else
        {
            bgColor = customColor.HasValue ? baseColor * 0.8f : (isPrimary ? ColorGold * 0.7f : ColorDarkBrown);
        }

        // Draw background
        var bgColorU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
        drawList.AddRectFilled(buttonStart, buttonEnd, bgColorU32, 4f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(customColor ?? (isPrimary ? ColorGold : ColorGrey) * 0.7f);
        drawList.AddRect(buttonStart, buttonEnd, borderColor, 4f, ImDrawFlags.None, 1.5f);

        // Draw text (centered)
        var textSize = ImGui.CalcTextSize(text);
        var textPos = new Vector2(
            x + (width - textSize.X) / 2,
            y + (height - textSize.Y) / 2
        );
        var textColor = ImGui.ColorConvertFloat4ToU32(isPrimary ? ColorDarkBrown : ColorWhite);
        drawList.AddText(textPos, textColor, text);

        return isHovering && ImGui.IsMouseReleased(ImGuiMouseButton.Left);
    }
}
