using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using PantheonWars.GUI.UI.Components.Lists;
using PantheonWars.GUI.UI.Utilities;
using PantheonWars.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace PantheonWars.GUI.UI.Renderers.Components;

/// <summary>
///     Renders a scrollable religion list for religion browser
/// </summary>
public static class ReligionListRenderer
{
    /// <summary>
    ///     Draw religion list with scrolling and selection
    /// </summary>
    /// <param name="drawList">ImGui draw list</param>
    /// <param name="api">Client API</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="width">Width of list</param>
    /// <param name="height">Height of list</param>
    /// <param name="religions">List of religions to display</param>
    /// <param name="isLoading">Whether the list is loading</param>
    /// <param name="scrollY">Current scroll position (will be modified)</param>
    /// <param name="selectedReligionUID">Currently selected religion UID (will be modified)</param>
    /// <returns>Tuple of (updated scroll Y, updated selected UID)</returns>
    public static (float scrollY, string? selectedUID) Draw(
        ImDrawListPtr drawList,
        ICoreClientAPI api,
        float x,
        float y,
        float width,
        float height,
        List<ReligionListResponsePacket.ReligionInfo> religions,
        bool isLoading,
        float scrollY,
        string? selectedReligionUID)
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
        if (isLoading)
        {
            var loadingText = "Loading religions...";
            var loadingSize = ImGui.CalcTextSize(loadingText);
            var loadingPos = new Vector2(
                x + (width - loadingSize.X) / 2,
                y + (height - loadingSize.Y) / 2
            );
            var loadingColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);
            drawList.AddText(loadingPos, loadingColor, loadingText);
            return (scrollY, selectedReligionUID);
        }

        // No religions state
        if (religions.Count == 0)
        {
            var noReligionText = "No religions found";
            var noReligionSize = ImGui.CalcTextSize(noReligionText);
            var noReligionPos = new Vector2(
                x + (width - noReligionSize.X) / 2,
                y + (height - noReligionSize.Y) / 2
            );
            var noReligionColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);
            drawList.AddText(noReligionPos, noReligionColor, noReligionText);
            return (scrollY, selectedReligionUID);
        }

        // Calculate scroll limits
        var contentHeight = religions.Count * (itemHeight + itemSpacing);
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
                scrollY = Math.Clamp(scrollY - wheel * 40f, 0f, maxScroll);
            }
        }

        // Clip to list bounds
        drawList.PushClipRect(listStart, listEnd, true);

        // Draw religion items
        var itemY = y - scrollY;
        for (int i = 0; i < religions.Count; i++)
        {
            var religion = religions[i];

            // Skip if not visible
            if (itemY + itemHeight < y || itemY > y + height)
            {
                itemY += itemHeight + itemSpacing;
                continue;
            }

            var clickedUID = DrawReligionItem(drawList, api, religion, x, itemY, width - scrollbarWidth, itemHeight, selectedReligionUID);
            if (clickedUID != null)
            {
                selectedReligionUID = clickedUID;
            }
            itemY += itemHeight + itemSpacing;
        }

        drawList.PopClipRect();

        // Draw scrollbar if needed
        if (contentHeight > height)
        {
            Scrollbar.Draw(drawList, x + width - scrollbarWidth, y, scrollbarWidth, height, scrollY, maxScroll);
        }

        return (scrollY, selectedReligionUID);
    }

    /// <summary>
    ///     Draw a single religion item
    /// </summary>
    /// <returns>Religion UID if clicked, null otherwise</returns>
    private static string? DrawReligionItem(
        ImDrawListPtr drawList,
        ICoreClientAPI api,
        ReligionListResponsePacket.ReligionInfo religion,
        float x,
        float y,
        float width,
        float height,
        string? currentSelectedUID)
    {
        const float padding = 12f;

        var itemStart = new Vector2(x, y);
        var itemEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;
        var isSelected = currentSelectedUID == religion.ReligionUID;

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
        string? clickedUID = null;
        if (isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            clickedUID = religion.ReligionUID;
            api.World.PlaySoundAt(new AssetLocation("pantheonwars:sounds/click"),
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
        var infoText = $"{religion.MemberCount} members | {religion.PrestigeRank} Prestige | {(religion.IsPublic ? "Public" : "Private")}";
        var infoPos = new Vector2(x + padding * 2 + iconSize, y + padding + 42f);
        var infoColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);
        drawList.AddText(ImGui.GetFont(), 12f, infoPos, infoColor, infoText);

        return clickedUID;
    }
}
