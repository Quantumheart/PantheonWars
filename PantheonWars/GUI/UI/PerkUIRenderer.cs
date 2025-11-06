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
    /// <param name="deltaTime">Time elapsed since last frame (for animations)</param>
    /// <param name="onUnlockClicked">Callback when unlock button clicked</param>
    /// <param name="onCloseClicked">Callback when close button clicked</param>
    /// <param name="onChangeReligionClicked">Callback when Change Religion button clicked</param>
    /// <param name="onManageReligionClicked">Callback when Manage Religion button clicked</param>
    /// <param name="onLeaveReligionClicked">Callback when Leave Religion button clicked</param>
    public static void Draw(
        PerkDialogManager manager,
        ICoreClientAPI api,
        int windowWidth,
        int windowHeight,
        float deltaTime,
        Action? onUnlockClicked,
        Action? onCloseClicked,
        Action? onChangeReligionClicked = null,
        Action? onManageReligionClicked = null,
        Action? onLeaveReligionClicked = null)
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
            windowPos.X + currentX, windowPos.Y + currentY, contentWidth,
            onChangeReligionClicked,
            onManageReligionClicked,
            onLeaveReligionClicked
        );
        currentY += usedHeight + padding;

        // === 2. PERK TREE (Split View: Player Left, Religion Right) ===
        var treeHeight = windowHeight - currentY - infoPanelHeight - padding * 2;

        PerkTreeRenderer.Draw(
            manager, api,
            windowPos.X + currentX, windowPos.Y + currentY, contentWidth, treeHeight,
            deltaTime,
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

        // === 5. TOOLTIPS (Rendered Last, On Top of Everything) ===
        if (!string.IsNullOrEmpty(hoveringPerkId))
        {
            var hoveringState = manager.GetPerkState(hoveringPerkId);
            if (hoveringState != null)
            {
                // Get all perks for registry (needed for prerequisite names)
                var allPerks = new System.Collections.Generic.Dictionary<string, Models.Perk>();
                foreach (var state in manager.PlayerPerkStates.Values)
                    if (!allPerks.ContainsKey(state.Perk.PerkId))
                        allPerks[state.Perk.PerkId] = state.Perk;
                foreach (var state in manager.ReligionPerkStates.Values)
                    if (!allPerks.ContainsKey(state.Perk.PerkId))
                        allPerks[state.Perk.PerkId] = state.Perk;

                // Build tooltip data
                var tooltipData = Models.PerkTooltipData.FromPerkAndState(
                    hoveringState.Perk,
                    hoveringState,
                    allPerks
                );

                // Get mouse position
                var mousePos = ImGui.GetMousePos();

                // Render tooltip
                TooltipRenderer.Draw(
                    tooltipData,
                    mousePos.X,
                    mousePos.Y,
                    windowWidth,
                    windowHeight
                );
            }
        }
    }
}