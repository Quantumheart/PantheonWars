using System;
using System.Numerics;
using ImGuiNET;
using PantheonWars.GUI.UI.Components.Buttons;
using PantheonWars.GUI.UI.Components.Inputs;
using PantheonWars.GUI.UI.Utilities;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Overlay for creating a new religion
///     Displays as modal form on top of browser
/// </summary>
internal static class CreateReligionOverlay
{
    // State
    private static string _religionName = "";
    private static int _selectedDeityIndex = 0; // Khoras
    private static bool _isPublic = true;
    private static string? _errorMessage;
    private static bool _dropdownOpen;

    /// <summary>
    ///     Initialize/reset overlay state
    /// </summary>
    public static void Initialize()
    {
        _religionName = "";
        _selectedDeityIndex = 0;
        _isPublic = true;
        _errorMessage = null;
        _dropdownOpen = false;
    }

    /// <summary>
    ///     Draw the create religion overlay
    /// </summary>
    /// <param name="api">Client API</param>
    /// <param name="windowWidth">Parent window width</param>
    /// <param name="windowHeight">Parent window height</param>
    /// <param name="onClose">Callback when close/cancel clicked</param>
    /// <param name="onCreate">Callback when create clicked (name, deity, isPublic)</param>
    /// <returns>True if overlay should remain open</returns>
    public static bool Draw(
        ICoreClientAPI api,
        int windowWidth,
        int windowHeight,
        Action onClose,
        Action<string, string, bool> onCreate)
    {
        const float overlayWidth = 500f;
        const float overlayHeight = 400f;
        const float padding = 20f;

        var windowPos = ImGui.GetWindowPos();
        var overlayX = windowPos.X + (windowWidth - overlayWidth) / 2;
        var overlayY = windowPos.Y + (windowHeight - overlayHeight) / 2;

        var drawList = ImGui.GetForegroundDrawList();

        // Draw semi-transparent background
        var bgStart = windowPos;
        var bgEnd = new Vector2(windowPos.X + windowWidth, windowPos.Y + windowHeight);
        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.BlackOverlay);
        drawList.AddRectFilled(bgStart, bgEnd, bgColor);

        // Draw main panel
        var panelStart = new Vector2(overlayX, overlayY);
        var panelEnd = new Vector2(overlayX + overlayWidth, overlayY + overlayHeight);
        var panelColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Background);
        drawList.AddRectFilled(panelStart, panelEnd, panelColor, 8f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Gold * 0.7f);
        drawList.AddRect(panelStart, panelEnd, borderColor, 8f, ImDrawFlags.None, 2f);

        var currentY = overlayY + padding;

        // === HEADER ===
        var headerText = "Create New Religion";
        var headerSize = ImGui.CalcTextSize(headerText);
        var headerPos = new Vector2(overlayX + padding, currentY);
        var headerColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Gold);
        drawList.AddText(ImGui.GetFont(), 20f, headerPos, headerColor, headerText);

        // Close button (X)
        const float closeButtonSize = 24f;
        var closeButtonX = overlayX + overlayWidth - padding - closeButtonSize;
        var closeButtonY = currentY;
        if (ButtonRenderer.DrawCloseButton(drawList, closeButtonX, closeButtonY, closeButtonSize))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            onClose.Invoke();
            return false;
        }

        currentY += headerSize.Y + padding * 1.5f;

        // === FORM FIELDS ===
        var fieldWidth = overlayWidth - padding * 2;

        // Religion Name
        DrawLabel(drawList, "Religion Name:", overlayX + padding, currentY);
        currentY += 25f;

        _religionName = TextInput.Draw(drawList, "##religionname", _religionName, overlayX + padding, currentY, fieldWidth, 32f, "Enter religion name...", 32);
        currentY += 40f;

        // Deity Selection
        DrawLabel(drawList, "Deity:", overlayX + padding, currentY);
        currentY += 25f;

        var dropdownY = currentY;
        DrawDeityDropdown(drawList, api, overlayX + padding, currentY, fieldWidth, 36f);
        currentY += 45f;

        // Public/Private Toggle
        _isPublic = DrawCheckbox(drawList, api, "Public (anyone can join)", overlayX + padding, currentY, _isPublic);
        currentY += 35f;

        // Info text
        var infoText = _isPublic
            ? "Public religions appear in the browser and anyone can join."
            : "Private religions require an invitation from the founder.";
        DrawInfoText(drawList, infoText, overlayX + padding, currentY, fieldWidth);
        currentY += 50f;

        // Error message
        if (!string.IsNullOrEmpty(_errorMessage))
        {
            DrawErrorText(drawList, _errorMessage, overlayX + padding, currentY, fieldWidth);
            currentY += 30f;
        }

        // Process dropdown menu FIRST (before buttons) to consume clicks
        bool dropdownConsumedClick = false;
        if (_dropdownOpen)
        {
            dropdownConsumedClick = DrawDeityDropdownMenu(drawList, api, overlayX + padding, dropdownY, fieldWidth, 36f);
        }

        // === CREATE BUTTON (centered) ===
        const float buttonWidth = 120f;
        const float buttonHeight = 36f;
        var buttonY = overlayY + overlayHeight - padding - buttonHeight;
        var createButtonX = overlayX + (overlayWidth - buttonWidth) / 2; // Center the button

        var canCreate = !string.IsNullOrWhiteSpace(_religionName) && _religionName.Length >= 3;

        // Draw Create button
        if (ButtonRenderer.DrawButton(drawList, "Create", createButtonX, buttonY, buttonWidth, buttonHeight, isPrimary: true, enabled: canCreate))
        {
            if (canCreate)
            {
                // Validate name length
                if (_religionName.Length > 32)
                {
                    _errorMessage = "Name must be 32 characters or less";
                    api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/error"),
                        api.World.Player.Entity, null, false, 8f, 0.3f);
                    return true;
                }

                api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                    api.World.Player.Entity, null, false, 8f, 0.5f);

                var deityName = DeityHelper.DeityNames[_selectedDeityIndex];
                onCreate.Invoke(_religionName, deityName, _isPublic);
                return false; // Close overlay after create
            }
            else
            {
                _errorMessage = "Religion name must be at least 3 characters";
                api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/error"),
                    api.World.Player.Entity, null, false, 8f, 0.3f);
            }
        }

        // Redraw dropdown menu AFTER buttons for proper z-order (visual only, interaction already handled)
        if (_dropdownOpen)
        {
            DrawDeityDropdownMenuVisual(drawList, overlayX + padding, dropdownY, fieldWidth, 36f);
        }

        return true; // Keep overlay open
    }

    /// <summary>
    ///     Draw label text
    /// </summary>
    private static void DrawLabel(ImDrawListPtr drawList, string text, float x, float y)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.White);
        drawList.AddText(ImGui.GetFont(), 14f, new Vector2(x, y), textColor, text);
    }

    /// <summary>
    ///     Draw info text (smaller, grey)
    /// </summary>
    private static void DrawInfoText(ImDrawListPtr drawList, string text, float x, float y, float width)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);

        // Simple word wrap (basic implementation)
        var words = text.Split(' ');
        var currentLine = "";
        var lineY = y;

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            var testSize = ImGui.CalcTextSize(testLine);

            if (testSize.X > width && !string.IsNullOrEmpty(currentLine))
            {
                drawList.AddText(ImGui.GetFont(), 12f, new Vector2(x, lineY), textColor, currentLine);
                lineY += 18f;
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            drawList.AddText(ImGui.GetFont(), 12f, new Vector2(x, lineY), textColor, currentLine);
        }
    }

    /// <summary>
    ///     Draw error text (red)
    /// </summary>
    private static void DrawErrorText(ImDrawListPtr drawList, string text, float x, float y, float width)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.3f, 0.3f, 1f));
        drawList.AddText(ImGui.GetFont(), 13f, new Vector2(x, y), textColor, text);
    }


    /// <summary>
    ///     Draw deity dropdown button (without menu)
    /// </summary>
    private static void DrawDeityDropdown(ImDrawListPtr drawList, ICoreClientAPI api, float x, float y, float width, float height)
    {
        // Create display text for selected deity
        var selectedDeity = DeityHelper.DeityNames[_selectedDeityIndex];
        var deityText = DeityHelper.GetDeityDisplayText(selectedDeity);

        // Draw dropdown button
        if (Dropdown.DrawButton(drawList, x, y, width, height, deityText, _dropdownOpen))
        {
            _dropdownOpen = !_dropdownOpen;
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.3f);
        }
    }

    /// <summary>
    ///     Draw deity dropdown menu (handles interaction and returns if click was consumed)
    /// </summary>
    private static bool DrawDeityDropdownMenu(ImDrawListPtr drawList, ICoreClientAPI api, float x, float y, float width, float height)
    {
        // Create display texts for all deities
        var deityDisplayTexts = new string[DeityHelper.DeityNames.Length];
        for (int i = 0; i < DeityHelper.DeityNames.Length; i++)
        {
            deityDisplayTexts[i] = DeityHelper.GetDeityDisplayText(DeityHelper.DeityNames[i]);
        }

        // Handle interaction
        var (newIndex, shouldClose, clickConsumed) = Dropdown.DrawMenuAndHandleInteraction(
            drawList, api, x, y, width, height, deityDisplayTexts, _selectedDeityIndex);

        _selectedDeityIndex = newIndex;
        if (shouldClose)
        {
            _dropdownOpen = false;
        }

        return clickConsumed;
    }

    /// <summary>
    ///     Draw deity dropdown menu visual only (no interaction)
    /// </summary>
    private static void DrawDeityDropdownMenuVisual(ImDrawListPtr drawList, float x, float y, float width, float height)
    {
        // Create display texts for all deities
        var deityDisplayTexts = new string[DeityHelper.DeityNames.Length];
        for (int i = 0; i < DeityHelper.DeityNames.Length; i++)
        {
            deityDisplayTexts[i] = DeityHelper.GetDeityDisplayText(DeityHelper.DeityNames[i]);
        }

        // Draw menu visual
        Dropdown.DrawMenuVisual(drawList, x, y, width, height, deityDisplayTexts, _selectedDeityIndex);
    }

    /// <summary>
    ///     Draw checkbox
    /// </summary>
    private static bool DrawCheckbox(ImDrawListPtr drawList, ICoreClientAPI api, string label, float x, float y, bool isChecked)
    {
        const float checkboxSize = 20f;
        const float labelPadding = 8f;

        var checkboxStart = new Vector2(x, y);
        var checkboxEnd = new Vector2(x + checkboxSize, y + checkboxSize);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + checkboxSize &&
                        mousePos.Y >= y && mousePos.Y <= y + checkboxSize;

        // Draw checkbox background
        var bgColor = isHovering
            ? ImGui.ColorConvertFloat4ToU32(ColorPalette.LightBrown * 0.7f)
            : ImGui.ColorConvertFloat4ToU32(ColorPalette.DarkBrown * 0.7f);
        drawList.AddRectFilled(checkboxStart, checkboxEnd, bgColor, 3f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(isChecked ? ColorPalette.Gold : ColorPalette.Grey * 0.5f);
        drawList.AddRect(checkboxStart, checkboxEnd, borderColor, 3f, ImDrawFlags.None, 1.5f);

        if (isHovering)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        // Draw checkmark if checked
        if (isChecked)
        {
            var checkColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Gold);
            drawList.AddLine(
                new Vector2(x + 4f, y + checkboxSize / 2),
                new Vector2(x + checkboxSize / 2 - 1f, y + checkboxSize - 5f),
                checkColor, 2f
            );
            drawList.AddLine(
                new Vector2(x + checkboxSize / 2 - 1f, y + checkboxSize - 5f),
                new Vector2(x + checkboxSize - 4f, y + 4f),
                checkColor, 2f
            );
        }

        // Draw label
        var labelPos = new Vector2(x + checkboxSize + labelPadding, y + (checkboxSize - 14f) / 2);
        var labelColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.White);
        drawList.AddText(labelPos, labelColor, label);

        // Handle click
        if (isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.3f);
            return !isChecked;
        }

        return isChecked;
    }

}
