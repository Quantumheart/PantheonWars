using PantheonWars.Commands;
using PantheonWars.GUI;
using PantheonWars.Models;
using PantheonWars.Network;
using PantheonWars.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;

namespace PantheonWars
{
    public class PantheonWarsModSystem : ModSystem
    {
        public const string NETWORK_CHANNEL = "pantheonwars";

        // Server-side systems
        private ICoreServerAPI? _sapi;
        private DeityRegistry? _deityRegistry;
        private PlayerDataManager? _playerDataManager;
        private FavorSystem? _favorSystem;
        private DeityCommands? _deityCommands;

        // Client-side systems
        private ICoreClientAPI? _capi;
        private FavorHudElement? _favorHud;
        private DeityRegistry? _clientDeityRegistry;

        public string ModName => "Pantheon Wars";

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Logger.Notification("[PantheonWars] Mod loaded!");

            // Register network channel
            api.Network.RegisterChannel(NETWORK_CHANNEL)
                .RegisterMessageType<PlayerDataPacket>();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            _sapi = api;
            api.Logger.Notification("[PantheonWars] Initializing server-side systems...");

            // Initialize deity registry
            _deityRegistry = new DeityRegistry(api);
            _deityRegistry.Initialize();

            // Initialize player data manager
            _playerDataManager = new PlayerDataManager(api);
            _playerDataManager.Initialize();

            // Initialize favor system
            _favorSystem = new FavorSystem(api, _playerDataManager, _deityRegistry);
            _favorSystem.Initialize();

            // Register commands
            _deityCommands = new DeityCommands(api, _deityRegistry, _playerDataManager);
            _deityCommands.RegisterCommands();

            // Setup network handlers
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

            // Register deity selection dialog opener command
            api.Input.RegisterHotKey("opendeityselection", "Open Deity Selection", GlKeys.P, HotkeyType.GUIOrOtherControls);
            api.Input.SetHotKeyHandler("opendeityselection", OpenDeitySelectionDialog);

            api.Logger.Notification("[PantheonWars] Client-side initialization complete");
        }

        #region Server Networking

        private void SetupServerNetworking(ICoreServerAPI api)
        {
            var channel = api.Network.GetChannel(NETWORK_CHANNEL);

            // Handle deity selection from client (future implementation)
            // channel.SetMessageHandler<DeitySelectionPacket>(OnClientDeitySelection);
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            // Send initial player data to client
            SendPlayerDataToClient(player);
        }

        private void SendPlayerDataToClient(IServerPlayer player)
        {
            if (_playerDataManager == null || _deityRegistry == null || _sapi == null) return;

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);
            var deity = _deityRegistry.GetDeity(playerData.DeityType);
            string deityName = deity?.Name ?? "None";

            var packet = new PlayerDataPacket(
                playerData.DeityType,
                playerData.DivineFavor,
                playerData.DevotionRank,
                deityName
            );

            var channel = _sapi.Network.GetChannel(NETWORK_CHANNEL);
            channel.SendPacket(packet, player);
        }

        #endregion

        #region Client Networking

        private void SetupClientNetworking(ICoreClientAPI api)
        {
            var channel = api.Network.GetChannel(NETWORK_CHANNEL);
            channel.SetMessageHandler<PlayerDataPacket>(OnServerPlayerDataUpdate);
        }

        private void OnServerPlayerDataUpdate(PlayerDataPacket packet)
        {
            // Update HUD with server data
            if (_favorHud != null)
            {
                _favorHud.UpdateDisplay(
                    packet.DeityName,
                    packet.DivineFavor,
                    packet.GetDevotionRank().ToString()
                );
            }
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

        #endregion

        public override void Dispose()
        {
            base.Dispose();

            // Cleanup
            _favorHud?.Dispose();
        }
    }
}
