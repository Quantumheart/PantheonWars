using System;
using System.Collections.Generic;
using System.Linq;
using PantheonWars.Data;
using PantheonWars.Models;
using Vintagestory.API.Server;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Manages perk effects and stat modifiers (Phase 3.3)
    /// </summary>
    public class PerkEffectSystem
    {
        private readonly ICoreServerAPI _sapi;
        private readonly PerkRegistry _perkRegistry;
        private readonly PlayerReligionDataManager _playerReligionDataManager;
        private readonly ReligionManager _religionManager;

        // Cache for stat modifiers to reduce computation
        private readonly Dictionary<string, Dictionary<string, float>> _playerModifierCache = new();
        private readonly Dictionary<string, Dictionary<string, float>> _religionModifierCache = new();

        public PerkEffectSystem(
            ICoreServerAPI sapi,
            PerkRegistry perkRegistry,
            PlayerReligionDataManager playerReligionDataManager,
            ReligionManager religionManager)
        {
            _sapi = sapi;
            _perkRegistry = perkRegistry;
            _playerReligionDataManager = playerReligionDataManager;
            _religionManager = religionManager;
        }

        /// <summary>
        /// Initializes the perk effect system
        /// </summary>
        public void Initialize()
        {
            _sapi.Logger.Notification("[PantheonWars] Initializing Perk Effect System...");

            // Register event handlers
            _sapi.Event.PlayerJoin += OnPlayerJoin;

            _sapi.Logger.Notification("[PantheonWars] Perk Effect System initialized");
        }

        /// <summary>
        /// Handles player join to apply perks
        /// </summary>
        private void OnPlayerJoin(IServerPlayer player)
        {
            RefreshPlayerPerks(player.PlayerUID);
        }

        /// <summary>
        /// Gets stat modifiers from player's unlocked perks
        /// </summary>
        public Dictionary<string, float> GetPlayerStatModifiers(string playerUID)
        {
            // Check cache first
            if (_playerModifierCache.TryGetValue(playerUID, out var cachedModifiers))
            {
                return new Dictionary<string, float>(cachedModifiers);
            }

            var modifiers = new Dictionary<string, float>();
            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(playerUID);

            // Get all unlocked player perks
            var unlockedPerkIds = playerData.UnlockedPerks
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            // Combine stat modifiers from all perks
            foreach (var perkId in unlockedPerkIds)
            {
                var perk = _perkRegistry.GetPerk(perkId);
                if (perk != null && perk.Type == PerkType.Player)
                {
                    CombineModifiers(modifiers, perk.StatModifiers);
                }
            }

            // Cache the result
            _playerModifierCache[playerUID] = new Dictionary<string, float>(modifiers);

            return modifiers;
        }

        /// <summary>
        /// Gets stat modifiers from religion's unlocked perks
        /// </summary>
        public Dictionary<string, float> GetReligionStatModifiers(string religionUID)
        {
            // Check cache first
            if (_religionModifierCache.TryGetValue(religionUID, out var cachedModifiers))
            {
                return new Dictionary<string, float>(cachedModifiers);
            }

            var modifiers = new Dictionary<string, float>();
            var religion = _religionManager.GetReligion(religionUID);

            if (religion == null)
            {
                return modifiers;
            }

            // Get all unlocked religion perks
            var unlockedPerkIds = religion.UnlockedPerks
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            // Combine stat modifiers from all perks
            foreach (var perkId in unlockedPerkIds)
            {
                var perk = _perkRegistry.GetPerk(perkId);
                if (perk != null && perk.Type == PerkType.Religion)
                {
                    CombineModifiers(modifiers, perk.StatModifiers);
                }
            }

            // Cache the result
            _religionModifierCache[religionUID] = new Dictionary<string, float>(modifiers);

            return modifiers;
        }

        /// <summary>
        /// Gets combined stat modifiers for a player (player perks + religion perks)
        /// </summary>
        public Dictionary<string, float> GetCombinedStatModifiers(string playerUID)
        {
            var combined = new Dictionary<string, float>();

            // Get player modifiers
            var playerModifiers = GetPlayerStatModifiers(playerUID);
            CombineModifiers(combined, playerModifiers);

            // Get religion modifiers
            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(playerUID);
            if (playerData.ReligionUID != null)
            {
                var religionModifiers = GetReligionStatModifiers(playerData.ReligionUID);
                CombineModifiers(combined, religionModifiers);
            }

            return combined;
        }

        /// <summary>
        /// Applies perks to a player (placeholder for future implementation)
        /// </summary>
        public void ApplyPerksToPlayer(IServerPlayer player)
        {
            var modifiers = GetCombinedStatModifiers(player.PlayerUID);

            // Log the modifiers for now
            if (modifiers.Count > 0)
            {
                _sapi.Logger.Debug($"[PantheonWars] Player {player.PlayerName} has {modifiers.Count} active stat modifiers:");
                foreach (var kvp in modifiers)
                {
                    _sapi.Logger.Debug($"  - {kvp.Key}: +{kvp.Value * 100}%");
                }
            }

            // TODO: Apply modifiers to player stats
            // This will require integration with Vintage Story's entity attribute system
            // or custom EntityBehavior implementation in a future phase
        }

        /// <summary>
        /// Refreshes all perk effects for a player
        /// </summary>
        public void RefreshPlayerPerks(string playerUID)
        {
            // Clear cached modifiers
            _playerModifierCache.Remove(playerUID);

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(playerUID);
            if (playerData.ReligionUID != null)
            {
                _religionModifierCache.Remove(playerData.ReligionUID);
            }

            // Recalculate and apply
            var player = _sapi.World.PlayerByUid(playerUID) as IServerPlayer;
            if (player != null)
            {
                ApplyPerksToPlayer(player);
            }

            _sapi.Logger.Debug($"[PantheonWars] Refreshed perks for player {playerUID}");
        }

        /// <summary>
        /// Refreshes perk effects for all members of a religion
        /// </summary>
        public void RefreshReligionPerks(string religionUID)
        {
            // Clear religion modifier cache
            _religionModifierCache.Remove(religionUID);

            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null)
            {
                return;
            }

            // Refresh all members
            foreach (var memberUID in religion.MemberUIDs)
            {
                RefreshPlayerPerks(memberUID);
            }

            _sapi.Logger.Debug($"[PantheonWars] Refreshed perks for religion {religion.ReligionName} ({religion.MemberUIDs.Count} members)");
        }

        /// <summary>
        /// Gets a summary of active perks for a player
        /// </summary>
        public (List<Perk> playerPerks, List<Perk> religionPerks) GetActivePerks(string playerUID)
        {
            var playerPerks = new List<Perk>();
            var religionPerks = new List<Perk>();

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(playerUID);

            // Get player perks
            var playerPerkIds = playerData.UnlockedPerks
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var perkId in playerPerkIds)
            {
                var perk = _perkRegistry.GetPerk(perkId);
                if (perk != null)
                {
                    playerPerks.Add(perk);
                }
            }

            // Get religion perks
            if (playerData.ReligionUID != null)
            {
                var religion = _religionManager.GetReligion(playerData.ReligionUID);
                if (religion != null)
                {
                    var religionPerkIds = religion.UnlockedPerks
                        .Where(kvp => kvp.Value)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var perkId in religionPerkIds)
                    {
                        var perk = _perkRegistry.GetPerk(perkId);
                        if (perk != null)
                        {
                            religionPerks.Add(perk);
                        }
                    }
                }
            }

            return (playerPerks, religionPerks);
        }

        /// <summary>
        /// Helper method to combine modifiers (additive)
        /// </summary>
        private void CombineModifiers(Dictionary<string, float> target, Dictionary<string, float> source)
        {
            foreach (var kvp in source)
            {
                if (target.ContainsKey(kvp.Key))
                {
                    target[kvp.Key] += kvp.Value;
                }
                else
                {
                    target[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Clears all modifier caches (useful for debugging/testing)
        /// </summary>
        public void ClearAllCaches()
        {
            _playerModifierCache.Clear();
            _religionModifierCache.Clear();
            _sapi.Logger.Notification("[PantheonWars] Cleared all perk modifier caches");
        }

        /// <summary>
        /// Gets a formatted string of all stat modifiers for display
        /// </summary>
        public string FormatStatModifiers(Dictionary<string, float> modifiers)
        {
            if (modifiers.Count == 0)
            {
                return "No active modifiers";
            }

            var lines = new List<string>();
            foreach (var kvp in modifiers.OrderBy(m => m.Key))
            {
                string statName = FormatStatName(kvp.Key);
                float percentage = kvp.Value * 100f;
                string sign = percentage >= 0 ? "+" : "";
                lines.Add($"  {statName}: {sign}{percentage:F1}%");
            }

            return string.Join("\n", lines);
        }

        /// <summary>
        /// Formats stat names for display
        /// </summary>
        private string FormatStatName(string statKey)
        {
            return statKey switch
            {
                "meleeDamageMultiplier" => "Melee Damage",
                "maxHealthMultiplier" => "Max Health",
                "walkSpeedMultiplier" => "Walk Speed",
                "rangedDamageMultiplier" => "Ranged Damage",
                "armorMultiplier" => "Armor",
                _ => statKey
            };
        }
    }
}
