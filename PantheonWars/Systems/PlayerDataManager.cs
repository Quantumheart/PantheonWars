using System;
using System.Collections.Generic;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Manages player deity data persistence and runtime access
    /// </summary>
    public class PlayerDataManager
    {
        private const string DATA_KEY = "pantheonwars_playerdata";
        private readonly ICoreServerAPI _sapi;
        private readonly Dictionary<string, PlayerDeityData> _playerData = new();

        public PlayerDataManager(ICoreServerAPI sapi)
        {
            _sapi = sapi;
        }

        /// <summary>
        /// Initializes the player data manager
        /// </summary>
        public void Initialize()
        {
            _sapi.Logger.Notification("[PantheonWars] Initializing Player Data Manager...");

            // Register event handlers
            _sapi.Event.PlayerJoin += OnPlayerJoin;
            _sapi.Event.PlayerDisconnect += OnPlayerDisconnect;
            _sapi.Event.SaveGameLoaded += OnSaveGameLoaded;
            _sapi.Event.GameWorldSave += OnGameWorldSave;

            _sapi.Logger.Notification("[PantheonWars] Player Data Manager initialized");
        }

        /// <summary>
        /// Gets player data, creating new data if it doesn't exist
        /// </summary>
        public PlayerDeityData GetOrCreatePlayerData(string playerUID)
        {
            if (!_playerData.TryGetValue(playerUID, out var data))
            {
                data = new PlayerDeityData(playerUID);
                _playerData[playerUID] = data;
                _sapi.Logger.Debug($"[PantheonWars] Created new player data for {playerUID}");
            }
            return data;
        }

        /// <summary>
        /// Gets player data by IServerPlayer
        /// </summary>
        public PlayerDeityData GetOrCreatePlayerData(IServerPlayer player)
        {
            return GetOrCreatePlayerData(player.PlayerUID);
        }

        /// <summary>
        /// Checks if player has pledged to a deity
        /// </summary>
        public bool HasDeity(string playerUID)
        {
            var data = GetOrCreatePlayerData(playerUID);
            return data.HasDeity();
        }

        /// <summary>
        /// Sets player's deity (initial pledge or switch)
        /// </summary>
        public void SetDeity(string playerUID, DeityType deityType)
        {
            var data = GetOrCreatePlayerData(playerUID);
            var previousDeity = data.DeityType;

            data.DeityType = deityType;

            if (previousDeity == DeityType.None)
            {
                // First time pledging
                data.PledgeDate = DateTime.UtcNow;
                _sapi.Logger.Notification($"[PantheonWars] Player {playerUID} pledged to {deityType}");
            }
            else
            {
                // Switching deities
                data.LastDeitySwitch = DateTime.UtcNow;
                // Reset stats when switching (can be adjusted)
                data.DivineFavor = 0;
                data.DevotionRank = DevotionRank.Initiate;
                data.KillCount = 0;
                _sapi.Logger.Notification($"[PantheonWars] Player {playerUID} switched from {previousDeity} to {deityType}");
            }
        }

        /// <summary>
        /// Adds favor to a player
        /// </summary>
        public void AddFavor(string playerUID, int amount, string reason = "")
        {
            var data = GetOrCreatePlayerData(playerUID);
            data.AddFavor(amount);

            if (!string.IsNullOrEmpty(reason))
            {
                _sapi.Logger.Debug($"[PantheonWars] Player {playerUID} gained {amount} favor: {reason}");
            }
        }

        /// <summary>
        /// Removes favor from a player
        /// </summary>
        public bool RemoveFavor(string playerUID, int amount, string reason = "")
        {
            var data = GetOrCreatePlayerData(playerUID);
            bool success = data.RemoveFavor(amount);

            if (success && !string.IsNullOrEmpty(reason))
            {
                _sapi.Logger.Debug($"[PantheonWars] Player {playerUID} spent {amount} favor: {reason}");
            }

            return success;
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
            LoadAllPlayerData();
        }

        private void OnGameWorldSave()
        {
            SaveAllPlayerData();
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Loads player data from world storage
        /// </summary>
        private void LoadPlayerData(string playerUID)
        {
            try
            {
                byte[]? data = _sapi.WorldManager.SaveGame.GetData($"{DATA_KEY}_{playerUID}");
                if (data != null)
                {
                    var playerData = SerializerUtil.Deserialize<PlayerDeityData>(data);
                    if (playerData != null)
                    {
                        _playerData[playerUID] = playerData;
                        _sapi.Logger.Debug($"[PantheonWars] Loaded data for player {playerUID}");
                    }
                }
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Failed to load data for player {playerUID}: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves player data to world storage
        /// </summary>
        private void SavePlayerData(string playerUID)
        {
            try
            {
                if (_playerData.TryGetValue(playerUID, out var playerData))
                {
                    byte[] data = SerializerUtil.Serialize(playerData);
                    _sapi.WorldManager.SaveGame.StoreData($"{DATA_KEY}_{playerUID}", data);
                    _sapi.Logger.Debug($"[PantheonWars] Saved data for player {playerUID}");
                }
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Failed to save data for player {playerUID}: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads all player data (called on server start)
        /// </summary>
        private void LoadAllPlayerData()
        {
            _sapi.Logger.Notification("[PantheonWars] Loading all player data...");
            // Player data will be loaded individually as players join
            // This method is here for future batch loading if needed
        }

        /// <summary>
        /// Saves all player data (called on server save)
        /// </summary>
        private void SaveAllPlayerData()
        {
            _sapi.Logger.Notification("[PantheonWars] Saving all player data...");
            foreach (var playerUID in _playerData.Keys)
            {
                SavePlayerData(playerUID);
            }
            _sapi.Logger.Notification($"[PantheonWars] Saved data for {_playerData.Count} players");
        }

        #endregion
    }
}
