using System;
using System.Collections.Generic;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Manages player-religion relationships and player progression
    /// </summary>
    public class PlayerReligionDataManager : IPlayerReligionDataManager
    {
        private const string DATA_KEY = "pantheonwars_playerreligiondata";
        private const int RELIGION_SWITCH_COOLDOWN_DAYS = 7;

        private readonly ICoreServerAPI _sapi;
        private readonly ReligionManager _religionManager;
        private readonly Dictionary<string, PlayerReligionData> _playerData = new();
        public delegate void PlayerReligionDataChangedDelegate(IServerPlayer player, string religionUID);
        public event PlayerReligionDataChangedDelegate OnPlayerLeavesReligion = null!;

        // ReSharper disable once ConvertToPrimaryConstructor
        public PlayerReligionDataManager(ICoreServerAPI sapi, ReligionManager religionManager)
        {
            _sapi = sapi;
            _religionManager = religionManager;
        }

        /// <summary>
        /// Initializes the player religion data manager
        /// </summary>
        public void Initialize()
        {
            _sapi.Logger.Notification("[PantheonWars] Initializing Player Religion Data Manager...");

            // Register event handlers
            _sapi.Event.PlayerJoin += OnPlayerJoin;
            _sapi.Event.PlayerDisconnect += OnPlayerDisconnect;
            _sapi.Event.SaveGameLoaded += OnSaveGameLoaded;
            _sapi.Event.GameWorldSave += OnGameWorldSave;

            _sapi.Logger.Notification("[PantheonWars] Player Religion Data Manager initialized");
        }

        /// <summary>
        /// Gets or creates player data
        /// </summary>
        public PlayerReligionData GetOrCreatePlayerData(string playerUID)
        {
            if (!_playerData.TryGetValue(playerUID, out var data))
            {
                data = new PlayerReligionData(playerUID);
                _playerData[playerUID] = data;
                _sapi.Logger.Debug($"[PantheonWars] Created new player religion data for {playerUID}");
            }
            return data;
        }

        /// <summary>
        /// Adds favor to a player
        /// </summary>
        public void AddFavor(string playerUID, int amount, string reason = "")
        {
            var data = GetOrCreatePlayerData(playerUID);
            var oldRank = data.FavorRank;

            data.AddFavor(amount);

            if (!string.IsNullOrEmpty(reason))
            {
                _sapi.Logger.Debug($"[PantheonWars] Player {playerUID} gained {amount} favor: {reason}");
            }

            // Check for rank up
            if (data.FavorRank > oldRank)
            {
                SendRankUpNotification(playerUID, data.FavorRank);
            }
        }

        /// <summary>
        /// Updates favor rank for a player
        /// </summary>
        public void UpdateFavorRank(string playerUID)
        {
            var data = GetOrCreatePlayerData(playerUID);
            var oldRank = data.FavorRank;

            data.UpdateFavorRank();

            // Check for rank change
            if (data.FavorRank != oldRank)
            {
                _sapi.Logger.Notification($"[PantheonWars] Player {playerUID} rank changed: {oldRank} -> {data.FavorRank}");

                if (data.FavorRank > oldRank)
                {
                    SendRankUpNotification(playerUID, data.FavorRank);
                }
            }
        }

        /// <summary>
        /// Sends rank-up notification to player
        /// </summary>
        private void SendRankUpNotification(string playerUID, FavorRank newRank)
        {
            var player = _sapi.World.PlayerByUid(playerUID) as IServerPlayer;
            if (player != null)
            {
                player.SendMessage(
                    Vintagestory.API.Config.GlobalConstants.GeneralChatGroup,
                    $"You have ascended to {newRank} rank!",
                    Vintagestory.API.Common.EnumChatType.Notification
                );
            }
        }

        /// <summary>
        /// Unlocks a player perk
        /// </summary>
        public bool UnlockPlayerPerk(string playerUID, string perkId)
        {
            var data = GetOrCreatePlayerData(playerUID);

            // Check if already unlocked
            if (data.IsPerkUnlocked(perkId))
            {
                return false;
            }

            // Unlock the perk
            data.UnlockPerk(perkId);
            _sapi.Logger.Notification($"[PantheonWars] Player {playerUID} unlocked perk: {perkId}");

            return true;
        }

        /// <summary>
        /// Gets active player perks (to be expanded in Phase 3.3)
        /// </summary>
        public List<string> GetActivePlayerPerks(string playerUID)
        {
            var data = GetOrCreatePlayerData(playerUID);
            var unlockedPerks = new List<string>();

            foreach (var kvp in data.UnlockedPerks)
            {
                if (kvp.Value) // If unlocked
                {
                    unlockedPerks.Add(kvp.Key);
                }
            }

            return unlockedPerks;
        }

        /// <summary>
        /// Joins a player to a religion
        /// </summary>
        public void JoinReligion(string playerUID, string religionUID)
        {
            var data = GetOrCreatePlayerData(playerUID);

            // Check if player is already in a religion
            if (data.HasReligion())
            {
                // Leave current religion first
                LeaveReligion(playerUID);
            }

            // Get religion to set active deity
            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null)
            {
                _sapi.Logger.Error($"[PantheonWars] Cannot join non-existent religion: {religionUID}");
                return;
            }

            // Set religion and deity
            data.ReligionUID = religionUID;
            data.ActiveDeity = religion.Deity;
            data.LastReligionSwitch = DateTime.UtcNow;

            // Add player to religion
            _religionManager.AddMember(religionUID, playerUID);

            _sapi.Logger.Notification($"[PantheonWars] Player {playerUID} joined religion {religion.ReligionName}");
        }

        /// <summary>
        /// Removes a player from their current religion
        /// </summary>
        public void LeaveReligion(string playerUID)
        {
            var data = GetOrCreatePlayerData(playerUID);

            if (!data.HasReligion())
            {
                return;
            }

            string religionUID = data.ReligionUID!;
            
            HandleReligionSwitch(playerUID);
            // Remove from religion
            _religionManager.RemoveMember(religionUID, playerUID);

            OnPlayerLeavesReligion.Invoke((_sapi.World.PlayerByUid(playerUID) as IServerPlayer)!, religionUID);
            // Clear player data
            data.ReligionUID = null;
            data.ActiveDeity = DeityType.None;

            _sapi.Logger.Notification($"[PantheonWars] Player {playerUID} left religion");
        }

        /// <summary>
        /// Checks if a player can switch religions (cooldown check)
        /// </summary>
        public bool CanSwitchReligion(string playerUID)
        {
            var data = GetOrCreatePlayerData(playerUID);

            // First-time joining is always allowed
            if (data.LastReligionSwitch == null)
            {
                return true;
            }

            // Check cooldown
            var cooldownEnd = data.LastReligionSwitch.Value.AddDays(RELIGION_SWITCH_COOLDOWN_DAYS);
            return DateTime.UtcNow >= cooldownEnd;
        }

        /// <summary>
        /// Gets remaining cooldown time for religion switching
        /// </summary>
        public TimeSpan? GetSwitchCooldownRemaining(string playerUID)
        {
            var data = GetOrCreatePlayerData(playerUID);

            if (data.LastReligionSwitch == null)
            {
                return null;
            }

            var cooldownEnd = data.LastReligionSwitch.Value.AddDays(RELIGION_SWITCH_COOLDOWN_DAYS);
            var remaining = cooldownEnd - DateTime.UtcNow;

            return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// Applies switching penalty when changing religions
        /// </summary>
        public void HandleReligionSwitch(string playerUID)
        {
            var data = GetOrCreatePlayerData(playerUID);

            _sapi.Logger.Notification($"[PantheonWars] Applying religion switch penalty to player {playerUID}");

            // Apply penalty (reset favor and perks)
            data.ApplySwitchPenalty();
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
                    var playerData = SerializerUtil.Deserialize<PlayerReligionData>(data);
                    if (playerData != null)
                    {
                        _playerData[playerUID] = playerData;
                        _sapi.Logger.Debug($"[PantheonWars] Loaded religion data for player {playerUID}");
                    }
                }
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Failed to load religion data for player {playerUID}: {ex.Message}");
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
                    _sapi.Logger.Debug($"[PantheonWars] Saved religion data for player {playerUID}");
                }
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Failed to save religion data for player {playerUID}: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads all player data (called on server start)
        /// </summary>
        private void LoadAllPlayerData()
        {
            _sapi.Logger.Notification("[PantheonWars] Loading all player religion data...");
            // Player data will be loaded individually as players join
            // This method is here for future batch loading if needed
        }

        /// <summary>
        /// Saves all player data (called on server save)
        /// </summary>
        private void SaveAllPlayerData()
        {
            _sapi.Logger.Notification("[PantheonWars] Saving all player religion data...");
            foreach (var playerUID in _playerData.Keys)
            {
                SavePlayerData(playerUID);
            }
            _sapi.Logger.Notification($"[PantheonWars] Saved religion data for {_playerData.Count} players");
        }

        #endregion
    }
}
