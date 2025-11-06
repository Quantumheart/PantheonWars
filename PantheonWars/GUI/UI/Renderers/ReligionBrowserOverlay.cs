using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using PantheonWars.GUI.UI.Components.Buttons;
using PantheonWars.GUI.UI.Components.Lists;
using PantheonWars.GUI.UI.Utilities;
using PantheonWars.Network;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Overlay for browsing and joining religions
///     Displays as modal panel on top of Perk Dialog
/// </summary>
internal static class ReligionBrowserOverlay
{

    // State
    private static string _selectedDeityFilter = "All";
    private static string? _selectedReligionUID;
    private static float _scrollY;
    private static List<ReligionListResponsePacket.ReligionInfo> _religions = new();
    private static bool _isLoading = true;

    /// <summary>
    ///     Initialize/reset overlay state
    /// </summary>
    public static void Initialize()
    {
        _selectedDeityFilter = "All";
        _selectedReligionUID = null;
        _scrollY = 0f;
        _religions.Clear();
        _isLoading = true;
    }

    /// <summary>
    ///     Update religion list from server response
    /// </summary>
    public static void UpdateReligionList(List<ReligionListResponsePacket.ReligionInfo> religions)
    {
        _religions = religions;
        _isLoading = false;
    }

    /// <summary>
    ///     Draw the religion browser overlay
    /// </summary>
    /// <param name="api">Client API</param>
    /// <param name="windowWidth">Parent window width</param>
    /// <param name="windowHeight">Parent window height</param>
    /// <param name="onClose">Callback when close button clicked</param>
    /// <param name="onJoinReligion">Callback when join button clicked (religionUID)</param>
    /// <param name="onRequestRefresh">Callback when refresh requested (deityFilter)</param>
    /// <param name="onCreateReligion">Callback when create religion clicked</param>
    /// <param name="userHasReligion">Whether the user already has a religion</param>
    /// <returns>True if overlay should remain open</returns>
    public static bool Draw(
        ICoreClientAPI api,
        int windowWidth,
        int windowHeight,
        Action onClose,
        Action<string> onJoinReligion,
        Action<string> onRequestRefresh,
        Action onCreateReligion,
        bool userHasReligion)
    {
        const float overlayWidth = 800f;
        const float overlayHeight = 600f;
        const float padding = 16f;

        var windowPos = ImGui.GetWindowPos();
        var overlayX = windowPos.X + (windowWidth - overlayWidth) / 2;
        var overlayY = windowPos.Y + (windowHeight - overlayHeight) / 2;

        var drawList = ImGui.GetForegroundDrawList();

        // Draw semi-transparent background overlay
        var bgStart = windowPos;
        var bgEnd = new Vector2(windowPos.X + windowWidth, windowPos.Y + windowHeight);
        var bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.7f));
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
        var headerText = "Browse Religions";
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

        currentY += headerSize.Y + padding * 2;

        // === DEITY FILTER TABS ===
        var deityFilters = new[] { "All", "Khoras", "Lysa", "Morthen", "Aethra", "Umbros", "Tharos", "Gaia", "Vex" };
        const float tabHeight = 32f;
        const float tabSpacing = 4f;
        var tabWidth = (overlayWidth - padding * 2 - tabSpacing * (deityFilters.Length - 1)) / deityFilters.Length;

        var tabX = overlayX + padding;
        for (int i = 0; i < deityFilters.Length; i++)
        {
            var filter = deityFilters[i];
            var isSelected = _selectedDeityFilter == filter;

            if (DrawTab(drawList, filter, tabX, currentY, tabWidth, tabHeight, isSelected))
            {
                _selectedDeityFilter = filter;
                _selectedReligionUID = null;
                _scrollY = 0f;
                _isLoading = true;

                // Request refresh with new filter
                var filterString = filter == "All" ? "" : filter;
                onRequestRefresh.Invoke(filterString);

                api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                    api.World.Player.Entity, null, false, 8f, 0.5f);
            }

            tabX += tabWidth + tabSpacing;
        }

        currentY += tabHeight + padding;

        // === RELIGION LIST ===
        var listHeight = overlayHeight - (currentY - overlayY) - padding * 2 - 40f; // 40f for join button
        DrawReligionList(drawList, api, overlayX + padding, currentY, overlayWidth - padding * 2, listHeight);

        currentY += listHeight + padding;

        // === ACTION BUTTONS ===
        const float buttonWidth = 180f;
        const float buttonHeight = 36f;
        const float buttonSpacing = 12f;
        var buttonY = currentY;
        var canJoin = !string.IsNullOrEmpty(_selectedReligionUID);

        // Only show Create button if user doesn't have a religion
        if (!userHasReligion)
        {
            // Show both Create and Join buttons
            var totalButtonWidth = buttonWidth * 2 + buttonSpacing;
            var buttonsStartX = overlayX + (overlayWidth - totalButtonWidth) / 2;

            // Create Religion button
            var createButtonX = buttonsStartX;
            if (ButtonRenderer.DrawButton(drawList, "Create Religion", createButtonX, buttonY, buttonWidth, buttonHeight, isPrimary: true, enabled: true))
            {
                api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                    api.World.Player.Entity, null, false, 8f, 0.5f);
                onCreateReligion.Invoke();
            }

            // Join Religion button
            var joinButtonX = buttonsStartX + buttonWidth + buttonSpacing;
            if (ButtonRenderer.DrawButton(drawList, canJoin ? "Join Religion" : "Select a religion", joinButtonX, buttonY, buttonWidth, buttonHeight, isPrimary: false, enabled: canJoin))
            {
                if (canJoin)
                {
                    api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                        api.World.Player.Entity, null, false, 8f, 0.5f);
                    onJoinReligion.Invoke(_selectedReligionUID!);
                    return false; // Close overlay after join
                }
                else
                {
                    api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/error"),
                        api.World.Player.Entity, null, false, 8f, 0.3f);
                }
            }
        }
        else
        {
            // User has religion - only show centered Join button (for switching religions)
            var joinButtonX = overlayX + (overlayWidth - buttonWidth) / 2;
            if (ButtonRenderer.DrawButton(drawList, canJoin ? "Join Religion" : "Select a religion", joinButtonX, buttonY, buttonWidth, buttonHeight, isPrimary: false, enabled: canJoin))
            {
                if (canJoin)
                {
                    api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                        api.World.Player.Entity, null, false, 8f, 0.5f);
                    onJoinReligion.Invoke(_selectedReligionUID!);
                    return false; // Close overlay after join
                }
                else
                {
                    api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/error"),
                        api.World.Player.Entity, null, false, 8f, 0.3f);
                }
            }
        }

        return true; // Keep overlay open
    }

    /// <summary>
    ///     Draw the religion list
    /// </summary>
    private static void DrawReligionList(ImDrawListPtr drawList, ICoreClientAPI api, float x, float y, float width, float height)
    {
        const float itemHeight = 80f;
        const float itemSpacing = 8f;
        const float scrollbarWidth = 16f;

        // Draw list background
        var listStart = new Vector2(x, y);
        var listEnd = new Vector2(x + width, y + height);
        var listBgColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.DarkBrown * 0.5f);
        drawList.AddRectFilled(listStart, listEnd, listBgColor, 4f);

        // Loading state
        if (_isLoading)
        {
            var loadingText = "Loading religions...";
            var loadingSize = ImGui.CalcTextSize(loadingText);
            var loadingPos = new Vector2(
                x + (width - loadingSize.X) / 2,
                y + (height - loadingSize.Y) / 2
            );
            var loadingColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);
            drawList.AddText(loadingPos, loadingColor, loadingText);
            return;
        }

        // No religions state
        if (_religions.Count == 0)
        {
            var noReligionText = "No religions found";
            var noReligionSize = ImGui.CalcTextSize(noReligionText);
            var noReligionPos = new Vector2(
                x + (width - noReligionSize.X) / 2,
                y + (height - noReligionSize.Y) / 2
            );
            var noReligionColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);
            drawList.AddText(noReligionPos, noReligionColor, noReligionText);
            return;
        }

        // Calculate scroll limits
        var contentHeight = _religions.Count * (itemHeight + itemSpacing);
        var maxScroll = Math.Max(0f, contentHeight - height);

        // Handle mouse wheel scrolling
        var mousePos = ImGui.GetMousePos();
        var isMouseOver = mousePos.X >= x && mousePos.X <= x + width &&
                          mousePos.Y >= y && mousePos.Y <= y + height;
        if (isMouseOver)
        {
            var wheel = ImGui.GetIO().MouseWheel;
            if (wheel != 0)
            {
                _scrollY = Math.Clamp(_scrollY - wheel * 40f, 0f, maxScroll);
            }
        }

        // Clip to list bounds
        drawList.PushClipRect(listStart, listEnd, true);

        // Draw religion items
        var itemY = y - _scrollY;
        for (int i = 0; i < _religions.Count; i++)
        {
            var religion = _religions[i];

            // Skip if not visible
            if (itemY + itemHeight < y || itemY > y + height)
            {
                itemY += itemHeight + itemSpacing;
                continue;
            }

            DrawReligionItem(drawList, api, religion, x, itemY, width - scrollbarWidth, itemHeight);
            itemY += itemHeight + itemSpacing;
        }

        drawList.PopClipRect();

        // Draw scrollbar if needed
        if (contentHeight > height)
        {
            Scrollbar.Draw(drawList, x + width - scrollbarWidth, y, scrollbarWidth, height, _scrollY, maxScroll);
        }
    }

    /// <summary>
    ///     Draw a single religion item
    /// </summary>
    private static void DrawReligionItem(ImDrawListPtr drawList, ICoreClientAPI api,
        ReligionListResponsePacket.ReligionInfo religion, float x, float y, float width, float height)
    {
        const float padding = 12f;

        var itemStart = new Vector2(x, y);
        var itemEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;
        var isSelected = _selectedReligionUID == religion.ReligionUID;

        // Determine background color
        Vector4 bgColor;
        if (isSelected)
        {
            bgColor = ColorPalette.Gold * 0.3f;
        }
        else if (isHovering)
        {
            bgColor = ColorPalette.LightBrown * 0.7f;
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        else
        {
            bgColor = ColorPalette.DarkBrown;
        }

        // Draw background
        var bgColorU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
        drawList.AddRectFilled(itemStart, itemEnd, bgColorU32, 4f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(isSelected ? ColorPalette.Gold : ColorPalette.Grey * 0.5f);
        drawList.AddRect(itemStart, itemEnd, borderColor, 4f, ImDrawFlags.None, isSelected ? 2f : 1f);

        // Handle click
        if (isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            _selectedReligionUID = religion.ReligionUID;
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
        }

        // Draw deity icon (placeholder circle)
        const float iconSize = 48f;
        var iconCenter = new Vector2(x + padding + iconSize / 2, y + height / 2);
        var deityColor = DeityHelper.GetDeityColor(religion.Deity);
        var iconColorU32 = ImGui.ColorConvertFloat4ToU32(deityColor);
        drawList.AddCircleFilled(iconCenter, iconSize / 2, iconColorU32, 16);

        // Draw religion name
        var namePos = new Vector2(x + padding * 2 + iconSize, y + padding);
        var nameColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Gold);
        drawList.AddText(ImGui.GetFont(), 16f, namePos, nameColor, religion.ReligionName);

        // Draw deity name
        var deityText = $"{religion.Deity} - {DeityHelper.GetDeityTitle(religion.Deity)}";
        var deityPos = new Vector2(x + padding * 2 + iconSize, y + padding + 22f);
        var deityColorU32 = ImGui.ColorConvertFloat4ToU32(ColorPalette.White);
        drawList.AddText(ImGui.GetFont(), 13f, deityPos, deityColorU32, deityText);

        // Draw member count and prestige
        var infoText = $"{religion.MemberCount} members • {religion.PrestigeRank} Prestige • {(religion.IsPublic ? "Public" : "Private")}";
        var infoPos = new Vector2(x + padding * 2 + iconSize, y + padding + 42f);
        var infoColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);
        drawList.AddText(ImGui.GetFont(), 12f, infoPos, infoColor, infoText);
    }

    /// <summary>
    ///     Draw scrollbar
    /// </summary>
    private static void DrawScrollbar(ImDrawListPtr drawList, float x, float y, float width, float height, float scrollY, float maxScroll)
    {
        // Draw track
        var trackStart = new Vector2(x, y);
        var trackEnd = new Vector2(x + width, y + height);
        var trackColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.DarkBrown * 0.7f);
        drawList.AddRectFilled(trackStart, trackEnd, trackColor, 4f);

        // Draw thumb
        var thumbHeight = Math.Max(20f, height * (height / (height + maxScroll)));
        var thumbY = y + (scrollY / maxScroll) * (height - thumbHeight);
        var thumbStart = new Vector2(x + 2f, thumbY);
        var thumbEnd = new Vector2(x + width - 2f, thumbY + thumbHeight);
        var thumbColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);
        drawList.AddRectFilled(thumbStart, thumbEnd, thumbColor, 4f);
    }

    /// <summary>
    ///     Draw tab button (unique to this overlay)
    /// </summary>
    private static bool DrawTab(ImDrawListPtr drawList, string text, float x, float y, float width, float height, bool isSelected)
    {
        var tabStart = new Vector2(x, y);
        var tabEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;

        // Determine background color
        Vector4 bgColor;
        if (isSelected)
        {
            bgColor = ColorPalette.Gold * 0.4f;
        }
        else if (isHovering)
        {
            bgColor = ColorPalette.LightBrown;
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        else
        {
            bgColor = ColorPalette.DarkBrown;
        }

        // Draw background
        var bgColorU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
        drawList.AddRectFilled(tabStart, tabEnd, bgColorU32, 4f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(isSelected ? ColorPalette.Gold : ColorPalette.Grey * 0.5f);
        drawList.AddRect(tabStart, tabEnd, borderColor, 4f, ImDrawFlags.None, isSelected ? 2f : 1f);

        // Draw text (centered)
        var textSize = ImGui.CalcTextSize(text);
        var textPos = new Vector2(
            x + (width - textSize.X) / 2,
            y + (height - textSize.Y) / 2
        );
        var textColor = ImGui.ColorConvertFloat4ToU32(isSelected ? ColorPalette.White : ColorPalette.Grey);
        drawList.AddText(textPos, textColor, text);

        return !isSelected && isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left);
    }
}
