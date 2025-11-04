using System;
using System.Linq;
using PantheonWars.Commands;
using PantheonWars.GUI;
using PantheonWars.Models.Enum;
using PantheonWars.Network;
using PantheonWars.Systems;
using PantheonWars.Systems.BuffSystem;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars;

public class PantheonWarsSystem : ModSystem
{
    public const string NETWORK_CHANNEL = "pantheonwars";
    private AbilityCommands? _abilityCommands;
    private AbilityRegistry? _abilityRegistry;
    private AbilitySystem? _abilitySystem;
    private BuffManager? _buffManager;

    // Client-side systems
    private ICoreClientAPI? _capi;
    private IClientNetworkChannel? _clientChannel;
    private DeityRegistry? _clientDeityRegistry;
    private AbilityCooldownManager? _cooldownManager;
    private CreateReligionDialog? _createReligionDialog;
    private DeityCommands? _deityCommands;
    private DeityRegistry? _deityRegistry;
    private FavorCommands? _favorCommands;
    private FavorHudElement? _favorHud;
    private FavorSystem? _favorSystem;
    private PerkCommands? _perkCommands;
    private PerkEffectSystem? _perkEffectSystem;
    private PerkRegistry? _perkRegistry;
    private PlayerDataManager? _playerDataManager;
    private PlayerReligionDataManager? _playerReligionDataManager;
    private PvPManager? _pvpManager;
    private ReligionCommands? _religionCommands;
    private ReligionManagementDialog? _religionDialog;
    private ReligionManager? _religionManager;
    private ReligionPrestigeManager? _religionPrestigeManager;

    // Server-side systems
    private ICoreServerAPI? _sapi;
    private IServerNetworkChannel? _serverChannel;

    public string ModName => "pantheonwars";

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.Logger.Notification("[PantheonWars] Mod loaded!");

        // Register network channel and message types
        api.Network.RegisterChannel(NETWORK_CHANNEL)
            .RegisterMessageType<PlayerReligionDataPacket>()
            .RegisterMessageType<ReligionListRequestPacket>()
            .RegisterMessageType<ReligionListResponsePacket>()
            .RegisterMessageType<PlayerReligionInfoRequestPacket>()
            .RegisterMessageType<PlayerReligionInfoResponsePacket>()
            .RegisterMessageType<ReligionActionRequestPacket>()
            .RegisterMessageType<ReligionActionResponsePacket>()
            .RegisterMessageType<CreateReligionRequestPacket>()
            .RegisterMessageType<CreateReligionResponsePacket>()
            .RegisterMessageType<EditDescriptionRequestPacket>()
            .RegisterMessageType<EditDescriptionResponsePacket>();
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        _sapi = api;
        api.Logger.Notification("[PantheonWars] Initializing server-side systems...");

        // Register entity behaviors
        api.RegisterEntityBehaviorClass("PantheonWarsBuffTracker", typeof(EntityBehaviorBuffTracker));

        // Initialize deity registry
        _deityRegistry = new DeityRegistry(api);
        _deityRegistry.Initialize();

        // Initialize ability registry
        _abilityRegistry = new AbilityRegistry(api);
        _abilityRegistry.Initialize();

        // Initialize player data manager
        _playerDataManager = new PlayerDataManager(api);
        _playerDataManager.Initialize();

        // Initialize ability cooldown manager
        _cooldownManager = new AbilityCooldownManager(api);
        _cooldownManager.Initialize();

        // Initialize buff manager
        _buffManager = new BuffManager(api);

        // Initialize favor system
        _favorSystem = new FavorSystem(api, _playerDataManager, _deityRegistry);
        _favorSystem.Initialize();

        // Initialize ability system (pass buff manager to it)
        _abilitySystem = new AbilitySystem(api, _abilityRegistry, _playerDataManager, _cooldownManager, _buffManager);
        _abilitySystem.Initialize();

        // Initialize religion systems (Phase 3.1 & 3.2)
        _religionManager = new ReligionManager(api);
        _religionManager.Initialize();

        _playerReligionDataManager = new PlayerReligionDataManager(api, _religionManager);
        _playerReligionDataManager.Initialize();

        _religionPrestigeManager = new ReligionPrestigeManager(api, _religionManager);
        _religionPrestigeManager.Initialize();

        _pvpManager = new PvPManager(api, _playerReligionDataManager, _religionManager, _religionPrestigeManager,
            _deityRegistry);
        _pvpManager.Initialize();

        // Initialize perk systems (Phase 3.3)
        _perkRegistry = new PerkRegistry(api);
        _perkRegistry.Initialize();

        _perkEffectSystem = new PerkEffectSystem(api, _perkRegistry, _playerReligionDataManager, _religionManager);
        _perkEffectSystem.Initialize();

        // Connect perk systems to religion prestige manager
        _religionPrestigeManager.SetPerkSystems(_perkRegistry, _perkEffectSystem);

        // Register commands
        _deityCommands = new DeityCommands(api, _deityRegistry, _playerDataManager);
        _deityCommands.RegisterCommands();

        _abilityCommands = new AbilityCommands(api, _abilitySystem, _playerDataManager);
        _abilityCommands.RegisterCommands();

        _favorCommands = new FavorCommands(api, _deityRegistry, _playerDataManager);
        _favorCommands.RegisterCommands();

        _religionCommands = new ReligionCommands(api, _religionManager, _playerReligionDataManager);
        _religionCommands.RegisterCommands();

        _perkCommands = new PerkCommands(api, _perkRegistry, _playerReligionDataManager, _religionManager,
            _perkEffectSystem);
        _perkCommands.RegisterCommands();

        // Setup network channel and handlers
        _serverChannel = api.Network.GetChannel(NETWORK_CHANNEL);
        _serverChannel.SetMessageHandler<PlayerReligionDataPacket>(OnServerMessageReceived);
        SetupServerNetworking(api);

        // Hook player join to send initial data
        api.Event.PlayerJoin += OnPlayerJoin;

        api.Logger.Notification("[PantheonWars] Server-side initialization complete");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        _capi = api;
        api.Logger.Notification("[PantheonWars] Initializing client-side systems...");

        // Initialize client-side deity registry (for GUI purposes)
        _clientDeityRegistry = new DeityRegistry(api);
        _clientDeityRegistry.Initialize();

        // Setup network handlers
        SetupClientNetworking(api);

        // Initialize HUD
        _favorHud = new FavorHudElement(api);
        _favorHud.TryOpen();

        // Initialize religion management dialog
        _religionDialog = new ReligionManagementDialog(api, _clientChannel!);

        // Initialize create religion dialog
        _createReligionDialog = new CreateReligionDialog(api, _clientChannel!);

        // Register deity selection dialog opener command
        api.Input.RegisterHotKey("opendeityselection", "Open Deity Selection", GlKeys.K, HotkeyType.GUIOrOtherControls);
        api.Input.SetHotKeyHandler("opendeityselection", OpenDeitySelectionDialog);

        // Register religion dialog opener command
        api.Input.RegisterHotKey("openreligionmanagement", "Open Religion Management", GlKeys.R,
            HotkeyType.GUIOrOtherControls);
        api.Input.SetHotKeyHandler("openreligionmanagement", OpenReligionManagementDialog);

        api.Logger.Notification("[PantheonWars] Client-side initialization complete");
    }

    public override void Dispose()
    {
        base.Dispose();

        // Cleanup
        _favorHud?.Dispose();
        _religionDialog?.Dispose();
        _createReligionDialog?.Dispose();
    }

    #region Server Networking

    private void SetupServerNetworking(ICoreServerAPI api)
    {
        // Register handlers for religion dialog packets
        _serverChannel!.SetMessageHandler<ReligionListRequestPacket>(OnReligionListRequest);
        _serverChannel.SetMessageHandler<PlayerReligionInfoRequestPacket>(OnPlayerReligionInfoRequest);
        _serverChannel.SetMessageHandler<ReligionActionRequestPacket>(OnReligionActionRequest);
        _serverChannel.SetMessageHandler<CreateReligionRequestPacket>(OnCreateReligionRequest);
        _serverChannel.SetMessageHandler<EditDescriptionRequestPacket>(OnEditDescriptionRequest);
    }

    private void OnServerMessageReceived(IServerPlayer fromPlayer, PlayerReligionDataPacket packet)
    {
        // Handle any client-to-server messages here
        // Currently not used, but necessary for channel setup
        // Future implementation: Handle deity selection from client dialog
    }

    private void OnReligionListRequest(IServerPlayer fromPlayer, ReligionListRequestPacket packet)
    {
        var religions = string.IsNullOrEmpty(packet.FilterDeity)
            ? _religionManager!.GetAllReligions()
            : _religionManager!.GetReligionsByDeity(
                Enum.TryParse<DeityType>(packet.FilterDeity, out var deity) ? deity : DeityType.None);

        var religionInfoList = religions.Select(r => new ReligionListResponsePacket.ReligionInfo
        {
            ReligionUID = r.ReligionUID,
            ReligionName = r.ReligionName,
            Deity = r.Deity.ToString(),
            MemberCount = r.MemberUIDs.Count,
            Prestige = r.Prestige,
            PrestigeRank = r.PrestigeRank.ToString(),
            IsPublic = r.IsPublic,
            FounderUID = r.FounderUID,
            Description = r.Description
        }).ToList();

        var response = new ReligionListResponsePacket(religionInfoList);
        _serverChannel!.SendPacket(response, fromPlayer);
    }

    private void OnPlayerReligionInfoRequest(IServerPlayer fromPlayer, PlayerReligionInfoRequestPacket packet)
    {
        var religion = _religionManager!.GetPlayerReligion(fromPlayer.PlayerUID);
        var response = new PlayerReligionInfoResponsePacket();

        if (religion != null)
        {
            response.HasReligion = true;
            response.ReligionUID = religion.ReligionUID;
            response.ReligionName = religion.ReligionName;
            response.Deity = religion.Deity.ToString();
            response.FounderUID = religion.FounderUID;
            response.Prestige = religion.Prestige;
            response.PrestigeRank = religion.PrestigeRank.ToString();
            response.IsPublic = religion.IsPublic;
            response.Description = religion.Description;
            response.IsFounder = religion.FounderUID == fromPlayer.PlayerUID;

            // Build member list with player names and favor ranks
            foreach (var memberUID in religion.MemberUIDs)
            {
                var memberPlayerData = _playerReligionDataManager!.GetOrCreatePlayerData(memberUID);
                var memberPlayer = _sapi!.World.PlayerByUid(memberUID);
                var memberName = memberPlayer?.PlayerName ?? memberUID;

                response.Members.Add(new PlayerReligionInfoResponsePacket.MemberInfo
                {
                    PlayerUID = memberUID,
                    PlayerName = memberName,
                    FavorRank = memberPlayerData.FavorRank.ToString(),
                    Favor = memberPlayerData.Favor,
                    IsFounder = memberUID == religion.FounderUID
                });
            }
        }
        else
        {
            response.HasReligion = false;
        }

        _serverChannel!.SendPacket(response, fromPlayer);
    }

    private void OnReligionActionRequest(IServerPlayer fromPlayer, ReligionActionRequestPacket packet)
    {
        string message;
        var success = false;

        try
        {
            switch (packet.Action.ToLower())
            {
                case "join":
                    if (_religionManager!.CanJoinReligion(packet.ReligionUID, fromPlayer.PlayerUID))
                    {
                        _playerReligionDataManager!.JoinReligion(fromPlayer.PlayerUID, packet.ReligionUID);
                        var religion = _religionManager.GetReligion(packet.ReligionUID);
                        message = $"Successfully joined {religion?.ReligionName ?? "religion"}!";
                        success = true;
                        SendPlayerDataToClient(fromPlayer); // Refresh player's HUD
                    }
                    else
                    {
                        message =
                            "Cannot join this religion. Check if you already have a religion or if it's invite-only.";
                    }

                    break;

                case "leave":
                    var currentReligion = _religionManager!.GetPlayerReligion(fromPlayer.PlayerUID);
                    if (currentReligion != null)
                    {
                        _playerReligionDataManager!.LeaveReligion(fromPlayer.PlayerUID);
                        message = $"Left {currentReligion.ReligionName}.";
                        success = true;
                        SendPlayerDataToClient(fromPlayer); // Refresh player's HUD
                    }
                    else
                    {
                        message = "You are not in a religion.";
                    }

                    break;

                case "kick":
                    var religionForKick = _religionManager!.GetPlayerReligion(fromPlayer.PlayerUID);
                    if (religionForKick != null && religionForKick.FounderUID == fromPlayer.PlayerUID)
                    {
                        if (packet.TargetPlayerUID != fromPlayer.PlayerUID)
                        {
                            _playerReligionDataManager!.LeaveReligion(packet.TargetPlayerUID);
                            message = "Kicked player from religion.";
                            success = true;

                            // Notify kicked player if online
                            var kickedPlayer = _sapi!.World.PlayerByUid(packet.TargetPlayerUID) as IServerPlayer;
                            if (kickedPlayer != null)
                            {
                                kickedPlayer.SendMessage(0,
                                    $"You have been kicked from {religionForKick.ReligionName}.",
                                    EnumChatType.Notification);
                                SendPlayerDataToClient(kickedPlayer); // Refresh kicked player's HUD
                            }
                        }
                        else
                        {
                            message = "You cannot kick yourself.";
                        }
                    }
                    else
                    {
                        message = "Only the founder can kick members.";
                    }

                    break;

                case "invite":
                    var religionForInvite = _religionManager!.GetPlayerReligion(fromPlayer.PlayerUID);
                    if (religionForInvite != null)
                    {
                        _religionManager.InvitePlayer(religionForInvite.ReligionUID, packet.TargetPlayerUID,
                            fromPlayer.PlayerUID);
                        message = "Invitation sent!";
                        success = true;
                    }
                    else
                    {
                        message = "You are not in a religion.";
                    }

                    break;
                case "disband":
                    var religionForDisband = _religionManager!.GetPlayerReligion(fromPlayer.PlayerUID);
                    if (religionForDisband != null && religionForDisband.FounderUID == fromPlayer.PlayerUID)
                    {
                        var religionName = religionForDisband.ReligionName;

                        // Remove all members
                        var members =
                            religionForDisband.MemberUIDs.ToList(); // Copy to avoid modification during iteration
                        foreach (var memberUID in members)
                        {
                            _playerReligionDataManager!.LeaveReligion(memberUID);

                            // Notify member if online
                            var memberPlayer = _sapi!.World.PlayerByUid(memberUID) as IServerPlayer;
                            if (memberPlayer != null && memberUID != fromPlayer.PlayerUID)
                                memberPlayer.SendMessage(
                                    GlobalConstants.GeneralChatGroup,
                                    $"{religionName} has been disbanded by its founder",
                                    EnumChatType.Notification
                                );
                        }

                        // Delete the religion
                        _religionManager.DeleteReligion(religionForDisband.ReligionUID, fromPlayer.PlayerUID);

                        message = $"Successfully disbanded {religionForDisband.ReligionName ?? "religion"}!";
                    }
                    else
                    {
                        message = "Only the founder can kick members.";
                    }

                    break;

                default:
                    message = $"Unknown action: {packet.Action}";
                    break;
            }
        }
        catch (Exception ex)
        {
            message = $"Error: {ex.Message}";
            _sapi!.Logger.Error($"[PantheonWars] Religion action error: {ex}");
        }

        var response = new ReligionActionResponsePacket(success, message, packet.Action);
        _serverChannel!.SendPacket(response, fromPlayer);
    }

    private void OnCreateReligionRequest(IServerPlayer fromPlayer, CreateReligionRequestPacket packet)
    {
        string message;
        var success = false;
        var religionUID = "";

        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(packet.ReligionName))
            {
                message = "Religion name cannot be empty.";
            }
            else if (packet.ReligionName.Length < 3)
            {
                message = "Religion name must be at least 3 characters.";
            }
            else if (packet.ReligionName.Length > 32)
            {
                message = "Religion name must be 32 characters or less.";
            }
            else if (_religionManager!.GetReligionByName(packet.ReligionName) != null)
            {
                message = "A religion with that name already exists.";
            }
            else if (_religionManager.HasReligion(fromPlayer.PlayerUID))
            {
                message = "You are already in a religion. Leave your current religion first.";
            }
            else if (!Enum.TryParse<DeityType>(packet.Deity, out var deity) || deity == DeityType.None)
            {
                message = "Invalid deity selected.";
            }
            else
            {
                // Create the religion
                var newReligion = _religionManager.CreateReligion(
                    packet.ReligionName,
                    deity,
                    fromPlayer.PlayerUID,
                    packet.IsPublic
                );

                // Auto-join the founder
                _playerReligionDataManager!.JoinReligion(fromPlayer.PlayerUID, newReligion.ReligionUID);

                religionUID = newReligion.ReligionUID;
                message = $"Successfully created {packet.ReligionName}!";
                success = true;

                // Refresh player's HUD
                SendPlayerDataToClient(fromPlayer);
            }
        }
        catch (Exception ex)
        {
            message = $"Error creating religion: {ex.Message}";
            _sapi!.Logger.Error($"[PantheonWars] Religion creation error: {ex}");
        }

        var response = new CreateReligionResponsePacket(success, message, religionUID);
        _serverChannel!.SendPacket(response, fromPlayer);
    }

    private void OnEditDescriptionRequest(IServerPlayer fromPlayer, EditDescriptionRequestPacket packet)
    {
        string message;
        var success = false;

        try
        {
            var religion = _religionManager!.GetReligion(packet.ReligionUID);

            if (religion == null)
            {
                message = "Religion not found.";
            }
            else if (religion.FounderUID != fromPlayer.PlayerUID)
            {
                message = "Only the founder can edit the description.";
            }
            else if (packet.Description.Length > 200)
            {
                message = "Description must be 200 characters or less.";
            }
            else
            {
                // Update description
                religion.Description = packet.Description;
                message = "Description updated successfully!";
                success = true;
            }
        }
        catch (Exception ex)
        {
            message = $"Error updating description: {ex.Message}";
            _sapi!.Logger.Error($"[PantheonWars] Description edit error: {ex}");
        }

        var response = new EditDescriptionResponsePacket(success, message);
        _serverChannel!.SendPacket(response, fromPlayer);
    }

    private void OnPlayerJoin(IServerPlayer player)
    {
        // Send initial player data to client
        SendPlayerDataToClient(player);
    }

    private void SendPlayerDataToClient(IServerPlayer player)
    {
        if (_playerDataManager == null || _deityRegistry == null || _serverChannel == null) return;

        var playerReligionData = _playerReligionDataManager!.GetOrCreatePlayerData(player.PlayerUID);
        var religionData = _religionManager!.GetPlayerReligion(player.PlayerUID);
        var deity = _deityRegistry.GetDeity(playerReligionData.ActiveDeity);
        var deityName = deity?.Name ?? "None";

        if (religionData != null)
        {
            var packet = new PlayerReligionDataPacket(
                religionData.ReligionName, deityName, playerReligionData.Favor,
                playerReligionData.FavorRank.ToString(), religionData.Prestige, religionData.PrestigeRank.ToString()
            );

            _serverChannel.SendPacket(packet, player);
        }
    }

    #endregion

    #region Client Networking

    private void SetupClientNetworking(ICoreClientAPI api)
    {
        _clientChannel = api.Network.GetChannel(NETWORK_CHANNEL);
        _clientChannel.SetMessageHandler<PlayerReligionDataPacket>(OnServerPlayerDataUpdate);
        _clientChannel.SetMessageHandler<ReligionListResponsePacket>(OnReligionListResponse);
        _clientChannel.SetMessageHandler<PlayerReligionInfoResponsePacket>(OnPlayerReligionInfoResponse);
        _clientChannel.SetMessageHandler<ReligionActionResponsePacket>(OnReligionActionResponse);
        _clientChannel.SetMessageHandler<CreateReligionResponsePacket>(OnCreateReligionResponse);
        _clientChannel.SetMessageHandler<EditDescriptionResponsePacket>(OnEditDescriptionResponse);
        _clientChannel.RegisterMessageType(typeof(PlayerReligionDataPacket));
    }

    private void OnServerPlayerDataUpdate(PlayerReligionDataPacket packet)
    {
        // Update HUD with server data
        if (_favorHud != null)
            _favorHud.UpdateReligionDisplay(
                packet.ReligionName, // Religion name not sent yet
                packet.Deity,
                packet.Favor,
                packet.FavorRank,
                packet.Prestige,
                packet.PrestigeRank
            );
    }

    private bool OpenDeitySelectionDialog(KeyCombination key)
    {
        if (_capi == null || _clientDeityRegistry == null) return false;

        var dialog = new DeitySelectionDialog(_capi, _clientDeityRegistry, OnDeitySelectedInDialog);
        dialog.TryOpen();
        return true;
    }

    private void OnDeitySelectedInDialog(DeityType selectedDeity)
    {
        if (_capi == null) return;

        // For now, tell player to use command
        // In future, send network packet to server
        _capi.ShowChatMessage($"Selected deity: {selectedDeity}. Use /deity select {selectedDeity} to confirm.");
    }

    private bool OpenReligionManagementDialog(KeyCombination key)
    {
        if (_capi == null || _religionDialog == null) return false;

        _religionDialog.TryOpen();
        return true;
    }

    private void OnReligionListResponse(ReligionListResponsePacket packet)
    {
        _religionDialog?.OnReligionListResponse(packet);
    }

    private void OnPlayerReligionInfoResponse(PlayerReligionInfoResponsePacket packet)
    {
        _religionDialog?.OnPlayerReligionInfoResponse(packet);
    }

    private void OnReligionActionResponse(ReligionActionResponsePacket packet)
    {
        _religionDialog?.OnActionResponse(packet);
    }

    private void OnCreateReligionResponse(CreateReligionResponsePacket packet)
    {
        if (packet.Success)
        {
            _capi?.ShowChatMessage(packet.Message);
            // Refresh religion dialog data
            if (_religionDialog != null && _religionDialog.IsOpened())
            {
                _religionDialog.TryClose();
                _religionDialog.TryOpen(); // Reopen to refresh
            }
        }
        else
        {
            _capi?.ShowChatMessage($"Error: {packet.Message}");
        }
    }

    private void OnEditDescriptionResponse(EditDescriptionResponsePacket packet)
    {
        if (packet.Success)
        {
            _capi?.ShowChatMessage(packet.Message);
            // Refresh religion dialog to show updated description
            if (_religionDialog != null && _religionDialog.IsOpened())
            {
                _religionDialog.TryClose();
                _religionDialog.TryOpen(); // Reopen to refresh
            }
        }
        else
        {
            _capi?.ShowChatMessage($"Error: {packet.Message}");
        }
    }

    #endregion
}