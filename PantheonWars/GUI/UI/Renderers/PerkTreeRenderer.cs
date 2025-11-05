using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using PantheonWars.Models;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Renders the split-panel perk tree view
///     Left panel: Player perks (50% width)
///     Right panel: Religion perks (50% width)
///     Both panels are independently scrollable
/// </summary>
internal static class PerkTreeRenderer
{
    // Color constants
    private static readonly Vector4 ColorDivider = new(0.573f, 0.502f, 0.416f, 1.0f); // #92806a
    private static readonly Vector4 ColorLabel = new(0.996f, 0.682f, 0.204f, 1.0f); // #feae34 gold

    /// <summary>
    ///     Draw the split-panel perk tree
    /// </summary>
    /// <param name="manager">Perk dialog state manager</param>
    /// <param name="api">Client API</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="width">Total available width</param>
    /// <param name="height">Total available height</param>
    /// <param name="deltaTime">Time elapsed since last frame (for animations)</param>
    /// <param name="hoveringPerkId">Output: ID of currently hovered perk</param>
    /// <returns>Height used by this renderer</returns>
    public static float Draw(
        PerkDialogManager manager,
        ICoreClientAPI api,
        float x, float y, float width, float height,
        float deltaTime,
        ref string? hoveringPerkId)
    {
        const float labelHeight = 30f;
        const float dividerWidth = 2f;

        // Calculate panel dimensions
        var panelWidth = (width - dividerWidth) / 2f;
        var treeAreaHeight = height - labelHeight;

        var drawList = ImGui.GetWindowDrawList();

        // === LEFT PANEL: Player Perks ===
        var leftX = x;
        var leftY = y;

        // Draw label
        DrawPanelLabel(drawList, "Player Perks", leftX, leftY, panelWidth);

        // Draw tree area (use local variables for scroll offsets)
        var treeY = leftY + labelHeight;
        var playerScrollX = manager.PlayerTreeScrollX;
        var playerScrollY = manager.PlayerTreeScrollY;
        DrawTreePanel(
            manager, api, drawList,
            leftX, treeY, panelWidth, treeAreaHeight,
            manager.PlayerPerkStates,
            deltaTime,
            ref playerScrollX,
            ref playerScrollY,
            ref hoveringPerkId
        );
        manager.PlayerTreeScrollX = playerScrollX;
        manager.PlayerTreeScrollY = playerScrollY;

        // === CENTER DIVIDER ===
        var dividerX = x + panelWidth;
        var dividerTop = new Vector2(dividerX, y);
        var dividerBottom = new Vector2(dividerX, y + height);
        var dividerColor = ImGui.ColorConvertFloat4ToU32(ColorDivider);
        drawList.AddLine(dividerTop, dividerBottom, dividerColor, dividerWidth);

        // === RIGHT PANEL: Religion Perks ===
        var rightX = dividerX + dividerWidth;
        var rightY = y;

        // Draw label
        DrawPanelLabel(drawList, "Religion Perks", rightX, rightY, panelWidth);

        // Draw tree area (use local variables for scroll offsets)
        var religionScrollX = manager.ReligionTreeScrollX;
        var religionScrollY = manager.ReligionTreeScrollY;
        DrawTreePanel(
            manager, api, drawList,
            rightX, treeY, panelWidth, treeAreaHeight,
            manager.ReligionPerkStates,
            deltaTime,
            ref religionScrollX,
            ref religionScrollY,
            ref hoveringPerkId
        );
        manager.ReligionTreeScrollX = religionScrollX;
        manager.ReligionTreeScrollY = religionScrollY;

        return height;
    }

    /// <summary>
    ///     Draw panel label header
    /// </summary>
    private static void DrawPanelLabel(ImDrawListPtr drawList, string label, float x, float y, float width)
    {
        const float labelHeight = 30f;

        // Draw background
        var bgStart = new Vector2(x, y);
        var bgEnd = new Vector2(x + width, y + labelHeight);
        var bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.16f, 0.12f, 0.09f, 0.8f));
        drawList.AddRectFilled(bgStart, bgEnd, bgColor);

        // Draw text (centered)
        var textSize = ImGui.CalcTextSize(label);
        var textPos = new Vector2(
            x + (width - textSize.X) / 2,
            y + (labelHeight - textSize.Y) / 2
        );
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorLabel);
        drawList.AddText(ImGui.GetFont(), 16f, textPos, textColor, label);
    }

    /// <summary>
    ///     Draw a single tree panel with scrolling
    /// </summary>
    private static void DrawTreePanel(
        PerkDialogManager manager,
        ICoreClientAPI api,
        ImDrawListPtr drawList,
        float x, float y, float width, float height,
        Dictionary<string, PerkNodeState> perkStates,
        float deltaTime,
        ref float scrollX, ref float scrollY,
        ref string? hoveringPerkId)
    {
        const float padding = 16f;

        // If no perks, show placeholder
        if (perkStates.Count == 0)
        {
            var emptyText = "No perks available";
            var textSize = ImGui.CalcTextSize(emptyText);
            var textPos = new Vector2(
                x + (width - textSize.X) / 2,
                y + (height - textSize.Y) / 2
            );
            var textColor = ImGui.ColorConvertFloat4ToU32(ColorDivider);
            drawList.AddText(textPos, textColor, emptyText);
            return;
        }

        // Calculate layout if not already done
        if (perkStates.Values.First().PositionX == 0 && perkStates.Values.First().PositionY == 0)
            PerkTreeLayout.CalculateLayout(perkStates, width - padding * 2);

        // Get total tree dimensions
        var totalHeight = PerkTreeLayout.GetTotalHeight(perkStates);
        var totalWidth = PerkTreeLayout.GetTotalWidth(perkStates);

        // Create scrollable area using ImGui child window
        var childId = perkStates.GetHashCode().ToString(); // Unique ID per panel
        ImGui.SetCursorScreenPos(new Vector2(x, y));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(padding, padding));
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.1f, 0.08f, 0.06f, 0.5f));

        // Enable scrolling with mouse wheel - border=true enables scrollbar
        ImGui.BeginChild(childId, new Vector2(width, height), true,
            ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        // Get mouse position (in screen space)
        var mousePos = ImGui.GetMousePos();
        var childWindowPos = ImGui.GetWindowPos(); // Position of child window in screen space

        // Get scroll position
        scrollX = ImGui.GetScrollX();
        scrollY = ImGui.GetScrollY();

        // Calculate drawing offset (accounts for scroll)
        var drawOffsetX = childWindowPos.X + padding - scrollX;
        var drawOffsetY = childWindowPos.Y + padding - scrollY;

        // Draw connection lines first (behind nodes)
        foreach (var state in perkStates.Values)
            if (state.Perk.PrerequisitePerks != null && state.Perk.PrerequisitePerks.Count > 0)
                foreach (var prereqId in state.Perk.PrerequisitePerks)
                    if (perkStates.TryGetValue(prereqId, out var prereqState))
                        PerkNodeRenderer.DrawConnectionLine(
                            prereqState, state,
                            drawOffsetX, drawOffsetY
                        );

        // Draw perk nodes
        foreach (var state in perkStates.Values)
        {
            var isSelected = manager.SelectedPerkId == state.Perk.PerkId;
            var isHovering = PerkNodeRenderer.DrawNode(
                state, api,
                drawOffsetX, drawOffsetY,
                mousePos.X, mousePos.Y,  // Pass screen-space mouse coordinates
                deltaTime,
                isSelected
            );

            // Handle hover
            if (isHovering)
            {
                hoveringPerkId = state.Perk.PerkId;
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

                // Handle click
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    api.Logger.Notification($"[PantheonWars] Clicked perk: {state.Perk.Name}");
                    manager.SelectPerk(state.Perk.PerkId);
                    api.Logger.Notification($"[PantheonWars] Selected perk ID: {manager.SelectedPerkId}");

                    // Play click sound
                    api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                        api.World.Player.Entity, null, false, 8f, 0.5f);
                }
            }
        }

        // Set child window content size for scrolling
        // We need to use Dummy to actually reserve the space since we're using DrawList
        ImGui.Dummy(new Vector2(totalWidth + padding * 2, totalHeight + padding * 2));

        ImGui.EndChild();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }
}