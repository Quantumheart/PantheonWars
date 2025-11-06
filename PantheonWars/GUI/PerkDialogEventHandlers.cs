using System;
using System.Linq;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using PantheonWars.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace PantheonWars.GUI;

/// <summary>
///     Event handlers for PerkDialog - extracted from main class for maintainability
/// </summary>
public partial class PerkDialog
{
    /// <summary>
    ///     Periodically check if player religion data is available
    /// </summary>
    private void OnCheckDataAvailability(float dt)
    {
        if (_isReady) return;

        // Request perk data from server
        if (_pantheonWarsSystem != null)
        {
            _pantheonWarsSystem.RequestPerkData();
            // Don't set _isReady yet - wait for server response in OnPerkDataReceived
            _capi!.Event.UnregisterGameTickListener(_checkDataId);
        }
    }

    /// <summary>
    ///     Handle perk data received from server
    /// </summary>
    private void OnPerkDataReceived(PerkDataResponsePacket packet)
    {
        _capi!.Logger.Debug($"[PantheonWars] Processing perk data: HasReligion={packet.HasReligion}");

        if (!packet.HasReligion)
        {
            _capi.Logger.Debug("[PantheonWars] Player has no religion - showing 'No Religion' state");
            _manager!.Reset();
            _isReady = true; // Set ready so dialog can open to show "No Religion" state

            // If dialog should be open, open it now
            if (!_isOpen && _imguiModSystem != null)
            {
                _isOpen = true;
                _imguiModSystem.Show();
                _capi.Logger.Debug("[PantheonWars] Perk Dialog opened with 'No Religion' state");
            }

            return;
        }

        // Parse deity type from string
        if (!Enum.TryParse<DeityType>(packet.Deity, out var deityType))
        {
            _capi.Logger.Error($"[PantheonWars] Invalid deity type: {packet.Deity}");
            return;
        }

        // Initialize manager with real data
        _manager!.Initialize(packet.ReligionUID, deityType, packet.ReligionName, packet.FavorRank,
            packet.PrestigeRank);

        // Convert packet perks to Perk objects
        var playerPerks = packet.PlayerPerks.Select(p => new Perk(p.PerkId, p.Name, deityType)
        {
            Kind = PerkKind.Player,
            Category = (PerkCategory)p.Category,
            Description = p.Description,
            RequiredFavorRank = p.RequiredFavorRank,
            RequiredPrestigeRank = p.RequiredPrestigeRank,
            PrerequisitePerks = p.PrerequisitePerks,
            StatModifiers = p.StatModifiers
        }).ToList();

        var religionPerks = packet.ReligionPerks.Select(p => new Perk(p.PerkId, p.Name, deityType)
        {
            Kind = PerkKind.Religion,
            Category = (PerkCategory)p.Category,
            Description = p.Description,
            RequiredFavorRank = p.RequiredFavorRank,
            RequiredPrestigeRank = p.RequiredPrestigeRank,
            PrerequisitePerks = p.PrerequisitePerks,
            StatModifiers = p.StatModifiers
        }).ToList();

        // Load perk states into manager
        _manager.LoadPerkStates(playerPerks, religionPerks);

        // Mark unlocked perks
        foreach (var perkId in packet.UnlockedPlayerPerks) _manager.SetPerkUnlocked(perkId, true);

        foreach (var perkId in packet.UnlockedReligionPerks) _manager.SetPerkUnlocked(perkId, true);

        // Refresh states to update can-unlock status
        _manager.RefreshAllPerkStates();

        _isReady = true;
        _capi.Logger.Notification(
            $"[PantheonWars] Loaded {playerPerks.Count} player perks and {religionPerks.Count} religion perks for {packet.Deity}");

        // Request player religion info to get founder status (needed for Manage Religion button)
        _pantheonWarsSystem?.RequestPlayerReligionInfo();

        // If dialog should be open, open it now that data is ready
        if (!_isOpen && _imguiModSystem != null)
        {
            _isOpen = true;
            _imguiModSystem.Show();
            _capi.Logger.Debug("[PantheonWars] Perk Dialog opened after data load");
        }
    }

    /// <summary>
    ///     Handle religion state change (religion disbanded, kicked, etc.)
    /// </summary>
    private void OnReligionStateChanged(ReligionStateChangedPacket packet)
    {
        _capi!.Logger.Notification($"[PantheonWars] Religion state changed: {packet.Reason}");

        // Show notification to user
        _capi.ShowChatMessage(packet.Reason);

        // Close any open overlays
        _overlayCoordinator!.CloseAllOverlays();

        // Reset perk dialog state to "No Religion" mode
        _manager!.Reset();
        _isReady = true; // Keep dialog ready so it doesn't close

        // Request fresh data from server (will show "No Religion" state)
        _pantheonWarsSystem?.RequestPerkData();
    }

    /// <summary>
    ///     Keybind handler - toggle dialog open/close
    /// </summary>
    private bool OnToggleDialog(KeyCombination keyCombination)
    {
        if (_isOpen)
            Close();
        else
            Open();
        return true;
    }

    /// <summary>
    ///     Handle unlock button click
    /// </summary>
    private void OnUnlockButtonClicked()
    {
        var selectedState = _manager!.GetSelectedPerkState();
        if (selectedState == null || !selectedState.CanUnlock || selectedState.IsUnlocked) return;

        // Client-side validation before sending request
        if (string.IsNullOrEmpty(selectedState.Perk.PerkId))
        {
            _capi!.ShowChatMessage("Error: Invalid perk ID");
            return;
        }

        // Send unlock request to server
        _capi!.Logger.Debug($"[PantheonWars] Sending unlock request for: {selectedState.Perk.Name}");
        _pantheonWarsSystem?.RequestPerkUnlock(selectedState.Perk.PerkId);
    }

    /// <summary>
    ///     Handle close button click
    /// </summary>
    private void OnCloseButtonClicked()
    {
        Close();
    }

    /// <summary>
    ///     Handle Change Religion button click
    /// </summary>
    private void OnChangeReligionClicked()
    {
        _capi!.Logger.Debug("[PantheonWars] Opening religion browser");
        _overlayCoordinator!.ShowReligionBrowser();

        // Initialize overlay and request religion list
        UI.Renderers.ReligionBrowserOverlay.Initialize();
        _pantheonWarsSystem?.RequestReligionList("");
    }

    /// <summary>
    ///     Handle Manage Religion button click (for leaders)
    /// </summary>
    private void OnManageReligionClicked()
    {
        _capi!.Logger.Debug("[PantheonWars] Manage Religion clicked");
        _overlayCoordinator!.ShowReligionManagement();
        UI.Renderers.ReligionManagementOverlay.Initialize();

        // Request religion info from server
        _pantheonWarsSystem?.RequestPlayerReligionInfo();
    }

    /// <summary>
    ///     Handle religion list received from server
    /// </summary>
    private void OnReligionListReceived(ReligionListResponsePacket packet)
    {
        _capi!.Logger.Debug($"[PantheonWars] Received {packet.Religions.Count} religions from server");
        UI.Renderers.ReligionBrowserOverlay.UpdateReligionList(packet.Religions);
    }

    /// <summary>
    ///     Handle religion action completed (join, leave, etc.)
    /// </summary>
    private void OnReligionActionCompleted(ReligionActionResponsePacket packet)
    {
        _capi!.Logger.Debug($"[PantheonWars] Religion action '{packet.Action}' completed: {packet.Message}");

        if (packet.Success)
        {
            _capi.ShowChatMessage(packet.Message);

            // Close any open overlays
            _overlayCoordinator!.CloseReligionBrowser();
            _overlayCoordinator.CloseLeaveConfirmation();

            // If leaving religion, reset perk dialog state immediately
            if (packet.Action == "leave")
            {
                _capi.Logger.Debug("[PantheonWars] Resetting perk dialog after leaving religion");
                _manager!.Reset();
            }

            // Request fresh perk data (religion may have changed)
            _pantheonWarsSystem?.RequestPerkData();
        }
        else
        {
            _capi.ShowChatMessage($"Error: {packet.Message}");

            // Play error sound
            _capi.World.PlaySoundAt(new AssetLocation("pantheonwars:sounds/error"),
                _capi.World.Player.Entity, null, false, 8f, 0.5f);
        }
    }

    /// <summary>
    ///     Handle join religion request
    /// </summary>
    private void OnJoinReligionClicked(string religionUID)
    {
        _capi!.Logger.Debug($"[PantheonWars] Requesting to join religion: {religionUID}");
        _pantheonWarsSystem?.RequestReligionAction("join", religionUID);
    }

    /// <summary>
    ///     Handle religion list refresh request
    /// </summary>
    private void OnRefreshReligionList(string deityFilter)
    {
        _capi!.Logger.Debug($"[PantheonWars] Refreshing religion list with filter: {deityFilter}");
        _pantheonWarsSystem?.RequestReligionList(deityFilter);
    }

    /// <summary>
    ///     Handle Leave Religion button click
    /// </summary>
    private void OnLeaveReligionClicked()
    {
        _capi!.Logger.Debug("[PantheonWars] Leave Religion clicked");

        if (_manager!.HasReligion())
        {
            // Show confirmation dialog
            _overlayCoordinator!.ShowLeaveConfirmation();
        }
        else
        {
            _capi.ShowChatMessage("You are not in a religion");
            _capi.World.PlaySoundAt(new AssetLocation("pantheonwars:sounds/error"),
                _capi.World.Player.Entity, null, false, 8f, 0.3f);
        }
    }

    /// <summary>
    ///     Handle leave religion confirmation
    /// </summary>
    private void OnLeaveReligionConfirmed()
    {
        _capi!.Logger.Debug("[PantheonWars] Leave religion confirmed");
        _pantheonWarsSystem?.RequestReligionAction("leave", _manager!.CurrentReligionUID ?? "");
        _overlayCoordinator!.CloseLeaveConfirmation();
    }

    /// <summary>
    ///     Handle leave religion cancelled
    /// </summary>
    private void OnLeaveReligionCancelled()
    {
        _capi!.Logger.Debug("[PantheonWars] Leave religion cancelled");
        _overlayCoordinator!.CloseLeaveConfirmation();
    }

    /// <summary>
    ///     Handle Create Religion button click
    /// </summary>
    private void OnCreateReligionClicked()
    {
        _capi!.Logger.Debug("[PantheonWars] Create Religion clicked");
        _overlayCoordinator!.ShowCreateReligion();
        UI.Renderers.CreateReligionOverlay.Initialize();
    }

    /// <summary>
    ///     Handle Create Religion form submission
    /// </summary>
    private void OnCreateReligionSubmit(string religionName, string deity, bool isPublic)
    {
        _capi!.Logger.Debug($"[PantheonWars] Creating religion: {religionName}, Deity: {deity}, Public: {isPublic}");
        _pantheonWarsSystem?.RequestCreateReligion(religionName, deity, isPublic);

        // Close create form and show browser to see the new religion
        _overlayCoordinator!.CloseCreateReligion();
        _overlayCoordinator.ShowReligionBrowser();

        // Request updated religion list to show the newly created religion
        _pantheonWarsSystem?.RequestReligionList("");
    }

    /// <summary>
    ///     Handle player religion info received from server
    /// </summary>
    private void OnPlayerReligionInfoReceived(PlayerReligionInfoResponsePacket packet)
    {
        _capi!.Logger.Debug($"[PantheonWars] Received player religion info: HasReligion={packet.HasReligion}, IsFounder={packet.IsFounder}");

        // Update manager with player's role (enables Manage Religion button for leaders)
        if (packet.HasReligion)
        {
            _manager!.PlayerRoleInReligion = packet.IsFounder ? "Leader" : "Member";
            _capi!.Logger.Debug($"[PantheonWars] Set PlayerRoleInReligion to: {_manager.PlayerRoleInReligion}");
        }
        else
        {
            _manager!.PlayerRoleInReligion = null;
            _capi!.Logger.Debug("[PantheonWars] Cleared PlayerRoleInReligion (no religion)");
        }

        UI.Renderers.ReligionManagementOverlay.UpdateReligionInfo(packet);
    }

    /// <summary>
    ///     Handle request for religion info refresh
    /// </summary>
    private void OnRequestReligionInfo()
    {
        _capi!.Logger.Debug("[PantheonWars] Requesting religion info refresh");
        _pantheonWarsSystem?.RequestPlayerReligionInfo();
    }

    /// <summary>
    ///     Handle Kick Member action
    /// </summary>
    private void OnKickMemberClicked(string memberUID)
    {
        _capi!.Logger.Debug($"[PantheonWars] Kicking member: {memberUID}");
        _pantheonWarsSystem?.RequestReligionAction("kick", _manager!.CurrentReligionUID ?? "", memberUID);
    }

    /// <summary>
    ///     Handle Invite Player action
    /// </summary>
    private void OnInvitePlayerClicked(string playerName)
    {
        _capi!.Logger.Debug($"[PantheonWars] Inviting player: {playerName}");
        // Note: The server expects playerUID, but we're sending playerName
        // The old system sends playerName in targetPlayerUID field
        _pantheonWarsSystem?.RequestReligionAction("invite", _manager!.CurrentReligionUID ?? "", playerName);
    }

    /// <summary>
    ///     Handle Edit Description action
    /// </summary>
    private void OnEditDescriptionClicked(string description)
    {
        _capi!.Logger.Debug("[PantheonWars] Editing religion description");
        _pantheonWarsSystem?.RequestEditDescription(_manager!.CurrentReligionUID ?? "", description);
    }

    /// <summary>
    ///     Handle Disband Religion action
    /// </summary>
    private void OnDisbandReligionClicked()
    {
        _capi!.Logger.Debug("[PantheonWars] Disbanding religion");
        _pantheonWarsSystem?.RequestReligionAction("disband", _manager!.CurrentReligionUID ?? "");

        // Close management overlay
        _overlayCoordinator!.CloseReligionManagement();
    }

    /// <summary>
    ///     Handle perk unlock response from server
    /// </summary>
    private void OnPerkUnlockedFromServer(string perkId, bool success)
    {
        if (!success)
        {
            _capi!.Logger.Debug($"[PantheonWars] Perk unlock failed: {perkId}");

            // Play error sound on failure
            _capi.World.PlaySoundAt(new AssetLocation("pantheonwars:sounds/error"),
                _capi.World.Player.Entity, null, false, 8f, 0.5f);

            return;
        }

        _capi!.Logger.Debug($"[PantheonWars] Perk unlocked from server: {perkId}");

        // Play unlock success sound
        _capi.World.PlaySoundAt(new AssetLocation("pantheonwars:sounds/unlock"),
            _capi.World.Player.Entity, null, false, 16f, 1.0f);

        // Update manager state
        _manager?.SetPerkUnlocked(perkId, true);

        // Refresh all perk states to update prerequisites and glow effects
        _manager?.RefreshAllPerkStates();
    }
}
