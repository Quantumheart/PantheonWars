using System;
using System.Numerics;
using ImGuiNET;
using PantheonWars.Models.Enum;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Overlay for creating a new religion
///     Displays as modal form on top of browser
/// </summary>
internal static class CreateReligionOverlay
{
    // Color constants (matching perk dialog design)
    private static readonly Vector4 ColorGold = new(0.996f, 0.682f, 0.204f, 1.0f);
    private static readonly Vector4 ColorWhite = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Vector4 ColorGrey = new(0.573f, 0.502f, 0.416f, 1.0f);
    private static readonly Vector4 ColorDarkBrown = new(0.24f, 0.18f, 0.13f, 1.0f);
    private static readonly Vector4 ColorLightBrown = new(0.35f, 0.26f, 0.19f, 1.0f);
    private static readonly Vector4 ColorBackground = new(0.16f, 0.12f, 0.09f, 0.95f);

    // State
    private static string _religionName = "";
    private static int _selectedDeityIndex = 0; // Khoras
    private static bool _isPublic = true;
    private static string? _errorMessage;
    private static bool _dropdownOpen = false;

    private static readonly string[] DeityNames =
    {
        "Khoras", "Lysa", "Morthen", "Aethra", "Umbros", "Tharos", "Gaia", "Vex"
    };

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
        var bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.8f));
        drawList.AddRectFilled(bgStart, bgEnd, bgColor);

        // Draw main panel
        var panelStart = new Vector2(overlayX, overlayY);
        var panelEnd = new Vector2(overlayX + overlayWidth, overlayY + overlayHeight);
        var panelColor = ImGui.ColorConvertFloat4ToU32(ColorBackground);
        drawList.AddRectFilled(panelStart, panelEnd, panelColor, 8f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorGold * 0.7f);
        drawList.AddRect(panelStart, panelEnd, borderColor, 8f, ImDrawFlags.None, 2f);

        var currentY = overlayY + padding;

        // === HEADER ===
        var headerText = "Create New Religion";
        var headerSize = ImGui.CalcTextSize(headerText);
        var headerPos = new Vector2(overlayX + padding, currentY);
        var headerColor = ImGui.ColorConvertFloat4ToU32(ColorGold);
        drawList.AddText(ImGui.GetFont(), 20f, headerPos, headerColor, headerText);

        // Close button (X)
        const float closeButtonSize = 24f;
        var closeButtonX = overlayX + overlayWidth - padding - closeButtonSize;
        var closeButtonY = currentY;
        if (DrawCloseButton(drawList, closeButtonX, closeButtonY, closeButtonSize))
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

        _religionName = DrawTextInput(drawList, _religionName, overlayX + padding, currentY, fieldWidth, 32f, "Enter religion name...");
        currentY += 40f;

        // Deity Selection
        DrawLabel(drawList, "Deity:", overlayX + padding, currentY);
        currentY += 25f;

        var dropdownY = currentY;
        _selectedDeityIndex = DrawDeityDropdown(drawList, api, overlayX + padding, currentY, fieldWidth, 36f);
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
        if (DrawButton(drawList, "Create", createButtonX, buttonY, buttonWidth, buttonHeight, true, canCreate))
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

                var deityName = DeityNames[_selectedDeityIndex];
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
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorWhite);
        drawList.AddText(ImGui.GetFont(), 14f, new Vector2(x, y), textColor, text);
    }

    /// <summary>
    ///     Draw info text (smaller, grey)
    /// </summary>
    private static void DrawInfoText(ImDrawListPtr drawList, string text, float x, float y, float width)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorGrey);

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
    ///     Draw text input field
    /// </summary>
    private static string DrawTextInput(ImDrawListPtr drawList, string currentValue, float x, float y, float width, float height, string placeholder)
    {
        var inputStart = new Vector2(x, y);
        var inputEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;

        // Draw input background
        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.7f);
        drawList.AddRectFilled(inputStart, inputEnd, bgColor, 4f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorGrey * 0.5f);
        drawList.AddRectFilled(inputStart, inputEnd, bgColor, 4f);
        drawList.AddRect(inputStart, inputEnd, borderColor, 4f, ImDrawFlags.None, 1f);

        // Create invisible button for click detection
        ImGui.SetCursorScreenPos(inputStart);
        ImGui.InvisibleButton("##textinput", new Vector2(width, height));
        bool isActive = ImGui.IsItemActive() || ImGui.IsItemFocused();
        bool wasClicked = ImGui.IsItemClicked();

        if (isActive || isHovering)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.TextInput);
        }

        // Make the input "focused" if clicked
        if (wasClicked)
        {
            ImGui.SetKeyboardFocusHere(-1); // Focus the invisible button
        }

        // When active, capture keyboard to prevent game input
        if (isActive)
        {
            var io = ImGui.GetIO();
            // Tell ImGui we want keyboard input (this sets WantCaptureKeyboard)
            io.WantCaptureKeyboard = true;

            // Handle backspace
            if (ImGui.IsKeyPressed(ImGuiKey.Backspace) && !string.IsNullOrEmpty(currentValue))
            {
                currentValue = currentValue.Substring(0, currentValue.Length - 1);
            }

            // Handle delete
            if (ImGui.IsKeyPressed(ImGuiKey.Delete))
            {
                // For simplicity, we don't support cursor position, so delete does nothing
            }

            // Process character input
            if (io.InputQueueCharacters.Size > 0)
            {
                for (int i = 0; i < io.InputQueueCharacters.Size; i++)
                {
                    ushort c = io.InputQueueCharacters[i];
                    // Only accept printable ASCII characters and ensure we don't exceed max length
                    if (c >= 32 && c < 127 && currentValue.Length < 32)
                    {
                        currentValue += (char)c;
                    }
                }
            }
        }

        // Draw text or placeholder
        var displayText = string.IsNullOrEmpty(currentValue) ? placeholder : currentValue;
        var textColor = string.IsNullOrEmpty(currentValue)
            ? ImGui.ColorConvertFloat4ToU32(ColorGrey * 0.7f)
            : ImGui.ColorConvertFloat4ToU32(ColorWhite);

        var textPos = new Vector2(x + 8f, y + (height - 16f) / 2);
        drawList.AddText(textPos, textColor, displayText);

        // Draw blinking cursor if active
        if (isActive && ((int)(ImGui.GetTime() * 2) % 2 == 0))
        {
            var textWidth = string.IsNullOrEmpty(currentValue) ? 0f : ImGui.CalcTextSize(currentValue).X;
            var cursorX = x + 8f + textWidth;
            var cursorTop = new Vector2(cursorX, y + 6f);
            var cursorBottom = new Vector2(cursorX, y + height - 6f);
            drawList.AddLine(cursorTop, cursorBottom, ImGui.ColorConvertFloat4ToU32(ColorWhite), 2f);
        }

        return currentValue;
    }

    /// <summary>
    ///     Draw deity dropdown button (without menu)
    /// </summary>
    private static int DrawDeityDropdown(ImDrawListPtr drawList, ICoreClientAPI api, float x, float y, float width, float height)
    {
        var dropdownStart = new Vector2(x, y);
        var dropdownEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;

        // Draw dropdown background
        var bgColor = isHovering
            ? ImGui.ColorConvertFloat4ToU32(ColorLightBrown * 0.7f)
            : ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.7f);
        drawList.AddRectFilled(dropdownStart, dropdownEnd, bgColor, 4f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorGrey * 0.5f);
        drawList.AddRect(dropdownStart, dropdownEnd, borderColor, 4f, ImDrawFlags.None, 1f);

        if (isHovering)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        // Draw selected deity
        var selectedDeity = DeityNames[_selectedDeityIndex];
        var deityText = $"{selectedDeity} - {GetDeityTitle(selectedDeity)}";
        var textPos = new Vector2(x + 12f, y + (height - 16f) / 2);
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorWhite);
        drawList.AddText(textPos, textColor, deityText);

        // Draw dropdown arrow
        var arrowX = x + width - 20f;
        var arrowY = y + height / 2;
        var arrowColor = ImGui.ColorConvertFloat4ToU32(ColorGrey);
        if (_dropdownOpen)
        {
            // Arrow pointing up when open
            drawList.AddTriangleFilled(
                new Vector2(arrowX, arrowY - 4f),
                new Vector2(arrowX - 4f, arrowY + 2f),
                new Vector2(arrowX + 4f, arrowY + 2f),
                arrowColor
            );
        }
        else
        {
            // Arrow pointing down when closed
            drawList.AddTriangleFilled(
                new Vector2(arrowX - 4f, arrowY - 2f),
                new Vector2(arrowX + 4f, arrowY - 2f),
                new Vector2(arrowX, arrowY + 4f),
                arrowColor
            );
        }

        // Handle click to toggle dropdown
        if (isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            _dropdownOpen = !_dropdownOpen;
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.3f);
        }

        return _selectedDeityIndex;
    }

    /// <summary>
    ///     Draw deity dropdown menu (handles interaction and returns if click was consumed)
    /// </summary>
    private static bool DrawDeityDropdownMenu(ImDrawListPtr drawList, ICoreClientAPI api, float x, float y, float width, float height)
    {
        var mousePos = ImGui.GetMousePos();
        const float itemHeight = 40f;
        var menuHeight = DeityNames.Length * itemHeight;
        var menuStart = new Vector2(x, y + height + 2f);
        var menuEnd = new Vector2(x + width, y + height + 2f + menuHeight);

        bool clickConsumed = false;

        // Check if mouse is over the menu area
        bool isMouseOverMenu = mousePos.X >= menuStart.X && mousePos.X <= menuEnd.X &&
                              mousePos.Y >= menuStart.Y && mousePos.Y <= menuEnd.Y;

        // Handle item clicks
        for (int i = 0; i < DeityNames.Length; i++)
        {
            var itemY = y + height + 2f + i * itemHeight;
            var isItemHovering = mousePos.X >= x && mousePos.X <= x + width &&
                                mousePos.Y >= itemY && mousePos.Y <= itemY + itemHeight;

            // Handle item click
            if (isItemHovering)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    _selectedDeityIndex = i;
                    _dropdownOpen = false;
                    api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                        api.World.Player.Entity, null, false, 8f, 0.3f);
                    clickConsumed = true;
                }
            }
        }

        // Close dropdown if clicked outside
        var isHoveringButton = mousePos.X >= x && mousePos.X <= x + width &&
                              mousePos.Y >= y && mousePos.Y <= y + height;
        if (!clickConsumed && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !isHoveringButton)
        {
            if (!isMouseOverMenu)
            {
                _dropdownOpen = false;
            }
            else
            {
                // Clicked in menu but not on an item - consume the click
                clickConsumed = true;
            }
        }

        return clickConsumed || isMouseOverMenu;
    }

    /// <summary>
    ///     Draw deity dropdown menu visual only (no interaction)
    /// </summary>
    private static void DrawDeityDropdownMenuVisual(ImDrawListPtr drawList, float x, float y, float width, float height)
    {
        var mousePos = ImGui.GetMousePos();
        const float itemHeight = 40f;
        var menuHeight = DeityNames.Length * itemHeight;
        var menuStart = new Vector2(x, y + height + 2f);
        var menuEnd = new Vector2(x + width, y + height + 2f + menuHeight);

        // Draw menu background
        var menuBgColor = ImGui.ColorConvertFloat4ToU32(ColorBackground);
        drawList.AddRectFilled(menuStart, menuEnd, menuBgColor, 4f);

        // Draw menu border
        var menuBorderColor = ImGui.ColorConvertFloat4ToU32(ColorGold * 0.7f);
        drawList.AddRect(menuStart, menuEnd, menuBorderColor, 4f, ImDrawFlags.None, 2f);

        // Draw each deity option
        for (int i = 0; i < DeityNames.Length; i++)
        {
            var itemY = y + height + 2f + i * itemHeight;
            var itemStart = new Vector2(x, itemY);
            var itemEnd = new Vector2(x + width, itemY + itemHeight);

            var isItemHovering = mousePos.X >= x && mousePos.X <= x + width &&
                                mousePos.Y >= itemY && mousePos.Y <= itemY + itemHeight;

            // Draw item background if hovering or selected
            if (isItemHovering || i == _selectedDeityIndex)
            {
                var itemBgColor = isItemHovering
                    ? ImGui.ColorConvertFloat4ToU32(ColorLightBrown * 0.6f)
                    : ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.8f);
                drawList.AddRectFilled(itemStart, itemEnd, itemBgColor);
            }

            // Draw deity text
            var deity = DeityNames[i];
            var itemText = $"{deity} - {GetDeityTitle(deity)}";
            var itemTextPos = new Vector2(x + 12f, itemY + (itemHeight - 16f) / 2);
            var itemTextColor = i == _selectedDeityIndex
                ? ImGui.ColorConvertFloat4ToU32(ColorGold)
                : ImGui.ColorConvertFloat4ToU32(ColorWhite);
            drawList.AddText(itemTextPos, itemTextColor, itemText);
        }
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
            ? ImGui.ColorConvertFloat4ToU32(ColorLightBrown * 0.7f)
            : ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.7f);
        drawList.AddRectFilled(checkboxStart, checkboxEnd, bgColor, 3f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(isChecked ? ColorGold : ColorGrey * 0.5f);
        drawList.AddRect(checkboxStart, checkboxEnd, borderColor, 3f, ImDrawFlags.None, 1.5f);

        if (isHovering)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        // Draw checkmark if checked
        if (isChecked)
        {
            var checkColor = ImGui.ColorConvertFloat4ToU32(ColorGold);
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
        var labelColor = ImGui.ColorConvertFloat4ToU32(ColorWhite);
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

    /// <summary>
    ///     Draw close button
    /// </summary>
    private static bool DrawCloseButton(ImDrawListPtr drawList, float x, float y, float size)
    {
        var buttonStart = new Vector2(x, y);
        var buttonEnd = new Vector2(x + size, y + size);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + size &&
                        mousePos.Y >= y && mousePos.Y <= y + size;

        // Draw button background
        var bgColor = isHovering ? ColorLightBrown : ColorDarkBrown;
        var bgColorU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
        drawList.AddRectFilled(buttonStart, buttonEnd, bgColorU32, 4f);

        // Draw X
        var xColor = ImGui.ColorConvertFloat4ToU32(isHovering ? ColorWhite : ColorGrey);
        var xPadding = size * 0.25f;
        drawList.AddLine(new Vector2(x + xPadding, y + xPadding),
            new Vector2(x + size - xPadding, y + size - xPadding), xColor, 2f);
        drawList.AddLine(new Vector2(x + size - xPadding, y + xPadding),
            new Vector2(x + xPadding, y + size - xPadding), xColor, 2f);

        if (isHovering)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left);
    }

    /// <summary>
    ///     Draw button
    /// </summary>
    private static bool DrawButton(ImDrawListPtr drawList, string text, float x, float y, float width, float height, bool isPrimary, bool enabled = true)
    {
        var buttonStart = new Vector2(x, y);
        var buttonEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = enabled && mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;

        // Determine background color
        Vector4 bgColor;
        if (!enabled)
        {
            bgColor = ColorDarkBrown * 0.5f;
        }
        else if (isHovering && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            bgColor = (isPrimary ? ColorGold : ColorDarkBrown) * 0.7f;
        }
        else if (isHovering)
        {
            bgColor = (isPrimary ? ColorGold : ColorLightBrown) * 0.9f;
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        else
        {
            bgColor = isPrimary ? ColorGold * 0.7f : ColorDarkBrown;
        }

        // Draw background
        var bgColorU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
        drawList.AddRectFilled(buttonStart, buttonEnd, bgColorU32, 4f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(enabled ? ColorGold * 0.7f : ColorGrey * 0.3f);
        drawList.AddRect(buttonStart, buttonEnd, borderColor, 4f, ImDrawFlags.None, 1.5f);

        // Draw text (centered)
        var textSize = ImGui.CalcTextSize(text);
        var textPos = new Vector2(
            x + (width - textSize.X) / 2,
            y + (height - textSize.Y) / 2
        );
        var textColor = ImGui.ColorConvertFloat4ToU32(enabled ? (isPrimary ? ColorDarkBrown : ColorWhite) : ColorGrey * 0.7f);
        drawList.AddText(textPos, textColor, text);

        return enabled && isHovering && ImGui.IsMouseReleased(ImGuiMouseButton.Left);
    }

    /// <summary>
    ///     Get title for deity
    /// </summary>
    private static string GetDeityTitle(string deity)
    {
        return deity switch
        {
            "Khoras" => "God of War",
            "Lysa" => "Goddess of the Hunt",
            "Morthen" => "God of Death",
            "Aethra" => "Goddess of Light",
            "Umbros" => "God of Shadows",
            "Tharos" => "God of Storms",
            "Gaia" => "Goddess of Earth",
            "Vex" => "God of Madness",
            _ => "Unknown Deity"
        };
    }
}
