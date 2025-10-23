using System;
using System.Collections.Generic;
using PantheonWars.Data;
using Vintagestory.API.Server;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Manages ability cooldowns for all players
    /// </summary>
    public class AbilityCooldownManager
    {
        private const string DATA_KEY = "pantheonwars_abilitycooldowns";
        private readonly ICoreServerAPI _sapi;
        private readonly Dictionary<string, PlayerAbilityData> _playerAbilityData = new();

        public AbilityCooldownManager(ICoreServerAPI sapi)
        {
            _sapi = sapi;
        }

        /// <summary>
        /// Initializes the cooldown manager
        /// </summary>
        public void Initialize()
        {
            _sapi.Logger.Notification("[PantheonWars] Initializing Ability Cooldown Manager...");

            // Register event handlers
            _sapi.Event.PlayerJoin += OnPlayerJoin;
            _sapi.Event.PlayerDisconnect += OnPlayerDisconnect;
            _sapi.Event.SaveGameLoaded += OnSaveGameLoaded;
            _sapi.Event.GameWorldSave += OnGameWorldSave;

            _sapi.Logger.Notification("[PantheonWars] Ability Cooldown Manager initialized");
        }

        /// <summary>
        /// Gets or creates ability data for a player
        /// </summary>
        public PlayerAbilityData GetOrCreateAbilityData(string playerUID)
        {
            if (!_playerAbilityData.TryGetValue(playerUID, out var data))
            {
                data = new PlayerAbilityData(playerUID);
                _playerAbilityData[playerUID] = data;
            }
            return data;
        }

        /// <summary>
        /// Gets or creates ability data for a player
        /// </summary>
        public PlayerAbilityData GetOrCreateAbilityData(IServerPlayer player)
        {
            return GetOrCreateAbilityData(player.PlayerUID);
        }

        /// <summary>
        /// Checks if an ability is on cooldown for a player
        /// </summary>
        public bool IsOnCooldown(string playerUID, string abilityId, float cooldownSeconds)
        {
            var data = GetOrCreateAbilityData(playerUID);
            return data.IsOnCooldown(abilityId, cooldownSeconds);
        }

        /// <summary>
        /// Gets remaining cooldown for an ability
        /// </summary>
        public float GetRemainingCooldown(string playerUID, string abilityId, float cooldownSeconds)
        {
            var data = GetOrCreateAbilityData(playerUID);
            return data.GetRemainingCooldown(abilityId, cooldownSeconds);
        }

        /// <summary>
        /// Starts cooldown for an ability
        /// </summary>
        public void StartCooldown(string playerUID, string abilityId)
        {
            var data = GetOrCreateAbilityData(playerUID);
            data.StartCooldown(abilityId);
        }

        /// <summary>
        /// Clears cooldown for a specific ability (admin command)
        /// </summary>
        public void ClearCooldown(string playerUID, string abilityId)
        {
            var data = GetOrCreateAbilityData(playerUID);
            data.ClearCooldown(abilityId);
        }

        /// <summary>
        /// Clears all cooldowns for a player (admin command)
        /// </summary>
        public void ClearAllCooldowns(string playerUID)
        {
            var data = GetOrCreateAbilityData(playerUID);
            data.ClearAllCooldowns();
        }

        #region Event Handlers

        private void OnPlayerJoin(IServerPlayer player)
        {
            LoadPlayerData(player.PlayerUID);
        }

        private void OnPlayerDisconnect(IServerPlayer player)
        {
            SavePlayerData(player.PlayerUID);
        }

        private void OnSaveGameLoaded()
        {
            // Player data will be loaded individually as players join
        }

        private void OnGameWorldSave()
        {
            SaveAllPlayerData();
        }

        #endregion

        #region Persistence

        private void LoadPlayerData(string playerUID)
        {
            try
            {
                byte[]? data = _sapi.WorldManager.SaveGame.GetData($"{DATA_KEY}_{playerUID}");
                if (data != null)
                {
                    var abilityData = SerializerUtil.Deserialize<PlayerAbilityData>(data);
                    if (abilityData != null)
                    {
                        // Clean up old cooldowns on load
                        abilityData.CleanupExpiredCooldowns();
                        _playerAbilityData[playerUID] = abilityData;
                        _sapi.Logger.Debug($"[PantheonWars] Loaded ability cooldowns for player {playerUID}");
                    }
                }
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Failed to load ability cooldowns for player {playerUID}: {ex.Message}");
            }
        }

        private void SavePlayerData(string playerUID)
        {
            try
            {
                if (_playerAbilityData.TryGetValue(playerUID, out var abilityData))
                {
                    // Clean up before saving
                    abilityData.CleanupExpiredCooldowns();

                    byte[] data = SerializerUtil.Serialize(abilityData);
                    _sapi.WorldManager.SaveGame.StoreData($"{DATA_KEY}_{playerUID}", data);
                    _sapi.Logger.Debug($"[PantheonWars] Saved ability cooldowns for player {playerUID}");
                }
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Failed to save ability cooldowns for player {playerUID}: {ex.Message}");
            }
        }

        private void SaveAllPlayerData()
        {
            _sapi.Logger.Notification("[PantheonWars] Saving all ability cooldowns...");
            foreach (var playerUID in _playerAbilityData.Keys)
            {
                SavePlayerData(playerUID);
            }
            _sapi.Logger.Notification($"[PantheonWars] Saved ability cooldowns for {_playerAbilityData.Count} players");
        }

        #endregion
    }
}
