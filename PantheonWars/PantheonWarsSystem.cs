using PantheonWars.Commands;
using PantheonWars.GUI;
using PantheonWars.Models;
using PantheonWars.Network;
using PantheonWars.Systems;
using PantheonWars.Systems.BuffSystem;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;

namespace PantheonWars
{
    public class PantheonWarsSystem : ModSystem
    {
        public const string NETWORK_CHANNEL = "pantheonwars";

        // Server-side systems
        private ICoreServerAPI _sapi;
        private DeityRegistry _deityRegistry;
        private AbilityRegistry _abilityRegistry;
        private PlayerDataManager _playerDataManager;
        private AbilityCooldownManager _cooldownManager;
        private FavorSystem _favorSystem;
        private AbilitySystem _abilitySystem;
        private BuffManager _buffManager;
        private ReligionManager _religionManager;
        private PlayerReligionDataManager _playerReligionDataManager;
        private DeityCommands _deityCommands;
        private AbilityCommands _abilityCommands;
        private FavorCommands _favorCommands;
        private ReligionCommands _religionCommands;

        // Client-side systems
        private ICoreClientAPI _capi;
        private FavorHudElement _favorHud;
        private DeityRegistry _clientDeityRegistry;
        private IServerNetworkChannel _serverChannel;
        private IClientNetworkChannel _clientChannel;

        public string ModName => "pantheonwars";

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Logger.Notification("[PantheonWars] Mod loaded!");

            // Register network channel and message types
            api.Network.RegisterChannel(NETWORK_CHANNEL)
                .RegisterMessageType<PlayerDataPacket>();
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

            // Initialize religion systems (Phase 3.1)
            _religionManager = new ReligionManager(api);
            _religionManager.Initialize();

            _playerReligionDataManager = new PlayerReligionDataManager(api, _religionManager);
            _playerReligionDataManager.Initialize();

            // Register commands
            _deityCommands = new DeityCommands(api, _deityRegistry, _playerDataManager);
            _deityCommands.RegisterCommands();

            _abilityCommands = new AbilityCommands(api, _abilitySystem, _playerDataManager);
            _abilityCommands.RegisterCommands();

            _favorCommands = new FavorCommands(api, _deityRegistry, _playerDataManager);
            _favorCommands.RegisterCommands();

            _religionCommands = new ReligionCommands(api, _religionManager, _playerReligionDataManager);
            _religionCommands.RegisterCommands();

            // Setup network channel and handlers
            _serverChannel = api.Network.GetChannel(NETWORK_CHANNEL);
            _serverChannel.SetMessageHandler<PlayerDataPacket>(OnServerMessageReceived);
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
            api.Input.RegisterHotKey("opendeityselection", "Open Deity Selection", GlKeys.K, HotkeyType.GUIOrOtherControls);
            api.Input.SetHotKeyHandler("opendeityselection", OpenDeitySelectionDialog);

            api.Logger.Notification("[PantheonWars] Client-side initialization complete");
        }

        #region Server Networking

        private void SetupServerNetworking(ICoreServerAPI api)
        {
            // Channel already registered and handlers set in StartServerSide
            // Add any additional server-side packet handlers here if needed
        }

        private void OnServerMessageReceived(IServerPlayer fromPlayer, PlayerDataPacket packet)
        {
            // Handle any client-to-server messages here
            // Currently not used, but necessary for channel setup
            // Future implementation: Handle deity selection from client dialog
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            // Send initial player data to client
            SendPlayerDataToClient(player);
        }

        private void SendPlayerDataToClient(IServerPlayer player)
        {
            if (_playerDataManager == null || _deityRegistry == null || _serverChannel == null) return;

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);
            var deity = _deityRegistry.GetDeity(playerData.DeityType);
            string deityName = deity?.Name ?? "None";

            var packet = new PlayerDataPacket(
                playerData.DeityType,
                playerData.DivineFavor,
                playerData.DevotionRank,
                deityName
            );

            _serverChannel.SendPacket(packet, player);
        }

        #endregion

        #region Client Networking

        private void SetupClientNetworking(ICoreClientAPI api)
        {
            _clientChannel = api.Network.GetChannel(NETWORK_CHANNEL);
            _clientChannel.SetMessageHandler<PlayerDataPacket>(OnServerPlayerDataUpdate);
            _clientChannel.RegisterMessageType(typeof(PlayerDataPacket));
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
