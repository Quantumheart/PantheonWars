using System;
using ImGuiNET;
using PantheonWars.GUI.UI.Renderers;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI;

/// <summary>
///     Central coordinator that orchestrates all perk UI renderers
///     Follows XSkillsGilded pattern - calls renderers in correct order with proper layout
/// </summary>
internal static class PerkUIRenderer
{
    /// <summary>
    ///     Draw the complete perk UI
    /// </summary>
    /// <param name="manager">Perk dialog state manager</param>
    /// <param name="api">Client API</param>
    /// <param name="windowWidth">Total window width</param>
    /// <param name="windowHeight">Total window height</param>
    /// <param name="onUnlockClicked">Callback when unlock button clicked</param>
    /// <param name="onCloseClicked">Callback when close button clicked</param>
    public static void Draw(
        PerkDialogManager manager,
        ICoreClientAPI api,
        int windowWidth,
        int windowHeight,
        Action? onUnlockClicked,
        Action? onCloseClicked)
    {
        const float padding = 16f;
        const float infoPanelHeight = 200f;
        const float actionButtonHeight = 36f;
        const float actionButtonPadding = 16f;

        // Get window position for screen-space drawing
        var windowPos = ImGui.GetWindowPos();

        var currentX = padding;
        var currentY = padding;
        var contentWidth = windowWidth - padding * 2;

        // Track hovering state across renderers
        string? hoveringPerkId = null;

        // === 1. RELIGION HEADER (Top Banner) ===
        var usedHeight = ReligionHeaderRenderer.Draw(
            manager, api,
            windowPos.X + currentX, windowPos.Y + currentY, contentWidth
        );
        currentY += usedHeight + padding;

        // === 2. PERK TREE (Split View: Player Left, Religion Right) ===
        var treeHeight = windowHeight - currentY - infoPanelHeight - padding * 2;

        PerkTreeRenderer.Draw(
            manager, api,
            windowPos.X + currentX, windowPos.Y + currentY, contentWidth, treeHeight,
            ref hoveringPerkId
        );
        currentY += treeHeight + padding;

        // === 3. PERK INFO PANEL (Bottom Panel) ===
        PerkInfoRenderer.Draw(
            manager, api,
            windowPos.X + currentX, windowPos.Y + currentY, contentWidth, infoPanelHeight
        );

        // === 4. ACTION BUTTONS (Overlay Bottom-Right) ===
        var buttonY = windowHeight - actionButtonHeight - actionButtonPadding;
        var buttonX = windowWidth - actionButtonPadding;

        PerkActionsRenderer.Draw(
            manager, api,
            windowPos.X + buttonX, windowPos.Y + buttonY,
            onUnlockClicked,
            onCloseClicked
        );

        // Update manager's hovering state
        manager.HoveringPerkId = hoveringPerkId;

        // === 5. TOOLTIPS (Draw last, on top of everything) ===
        if (!string.IsNullOrEmpty(hoveringPerkId))
        {
            var hoveredState = manager.GetPerkState(hoveringPerkId);
            if (hoveredState != null)
            {
                var mousePos = ImGui.GetMousePos();
                var viewport = ImGui.GetMainViewport();
                TooltipRenderer.Draw(
                    manager, api,
                    hoveredState,
                    mousePos.X, mousePos.Y,
                    viewport.Size.X, viewport.Size.Y
                );
            }
        }
    }
}