using System;
using System.Collections.Generic;
using System.Linq;
using PantheonWars.Models;
using Vintagestory.API.Common;
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

        // Track applied modifiers per player for cleanup
        private readonly Dictionary<string, HashSet<string>> _appliedModifiers = new();

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
            _playerReligionDataManager.OnPlayerLeavesReligion += OnPlayerLeavesReligion;

            _sapi.Logger.Notification("[PantheonWars] Perk Effect System initialized");
        }

        private void OnPlayerLeavesReligion(string playerUid)
        {
            RefreshPlayerPerks(playerUid);
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
                if (perk != null && perk.Kind == PerkKind.Player)
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
                if (perk != null && perk.Kind == PerkKind.Religion)
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
        /// Applies perks to a player using Vintage Story's Stats API
        /// Based on XSkills implementation pattern
        /// </summary>
        public void ApplyPerksToPlayer(IServerPlayer player)
        {
            if (player?.Entity == null)
            {
                _sapi.Logger.Warning($"[PantheonWars] Cannot apply perks - player entity is null");
                return;
            }

            EntityAgent agent = player.Entity as EntityAgent;
            if (agent?.Stats == null)
            {
                _sapi.Logger.Warning($"[PantheonWars] Cannot apply perks - player has no Stats");
                return;
            }

            // Get combined modifiers (player perks + religion perks)
            var modifiers = GetCombinedStatModifiers(player.PlayerUID);

            // Remove old modifiers first
            RemovePerksFromPlayer(player);

            // Apply new modifiers
            int appliedCount = 0;
            var appliedSet = new HashSet<string>();

            foreach (var modifier in modifiers)
            {
                // Stat names now come directly from VintageStoryStats constants
                string statName = modifier.Key;

                // Use namespaced modifier ID to avoid conflicts
                string modifierId = $"perk-{player.PlayerUID}";
                float value = modifier.Value;

                try
                {
                    // Apply stat modifier (false = deferred update, per XSkills pattern)
                    agent.Stats.Set(statName, modifierId, value, false);
                    appliedSet.Add(statName);
                    appliedCount++;

                    _sapi.Logger.Debug($"[PantheonWars] Applied {statName}: {modifierId} = {value:F3}");
                }
                catch (KeyNotFoundException ex)
                {
                    _sapi.Logger.Warning($"[PantheonWars] Stat '{statName}' not found for player {player.PlayerName}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _sapi.Logger.Error($"[PantheonWars] Error applying stat '{statName}' to player {player.PlayerName}: {ex}");
                }
            }

            // Track applied modifiers for cleanup later
            _appliedModifiers[player.PlayerUID] = appliedSet;

            if (appliedCount > 0)
            {
                _sapi.Logger.Notification($"[PantheonWars] Applied {appliedCount} perk modifiers to player {player.PlayerName}");
            }
        }

        /// <summary>
        /// Removes all perk modifiers from a player
        /// </summary>
        private void RemovePerksFromPlayer(IServerPlayer player)
        {
            if (player?.Entity == null) return;

            EntityAgent agent = player.Entity as EntityAgent;
            if (agent?.Stats == null) return;

            // Get previously applied modifiers
            if (!_appliedModifiers.TryGetValue(player.PlayerUID, out var appliedSet))
            {
                return; // No modifiers to remove
            }

            string modifierId = $"perk-{player.PlayerUID}";
            int removedCount = 0;

            foreach (var statName in appliedSet)
            {
                try
                {
                    agent.Stats.Remove(statName, modifierId);
                    removedCount++;
                }
                catch (Exception ex)
                {
                    _sapi.Logger.Debug($"[PantheonWars] Could not remove modifier '{statName}' from player {player.PlayerName}: {ex.Message}");
                }
            }

            if (removedCount > 0)
            {
                _sapi.Logger.Debug($"[PantheonWars] Removed {removedCount} old perk modifiers from player {player.PlayerName}");
            }

            appliedSet.Clear();
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
                "meleeWeaponsDamage" => "Melee Damage",
                "rangedWeaponsDamage" => "Ranged Damage",
                "meleeWeaponsSpeed" => "Attack Speed",
                "meleeWeaponArmor" => "Armor",
                "maxhealthExtraPoints" => "Max Health",
                "walkspeed" => "Walk Speed",
                "healingeffectivness" => "Health Regen",
                _ => statKey
            };
        }
    }
}
