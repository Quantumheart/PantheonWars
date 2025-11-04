using System.Collections.Generic;
using System.Linq;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Registry for all perks in the game (Phase 3.3)
    /// </summary>
    public class PerkRegistry : IPerkRegistry
    {
        private readonly ICoreAPI _api;
        private readonly Dictionary<string, Perk> _perks = new();

        // ReSharper disable once ConvertToPrimaryConstructor
        public PerkRegistry(ICoreAPI api)
        {
            _api = api;
        }

        /// <summary>
        /// Initializes the perk registry and registers all perks
        /// </summary>
        public void Initialize()
        {
            _api.Logger.Notification("[PantheonWars] Initializing Perk Registry...");

            // Register all perks from PerkDefinitions (Phase 3.4)
            var allPerks = PerkDefinitions.GetAllPerks();
            foreach (var perk in allPerks)
            {
                RegisterPerk(perk);
            }

            _api.Logger.Notification($"[PantheonWars] Perk Registry initialized with {_perks.Count} perks");
        }

        /// <summary>
        /// Registers a perk in the system
        /// </summary>
        public void RegisterPerk(Perk perk)
        {
            if (string.IsNullOrEmpty(perk.PerkId))
            {
                _api.Logger.Error("[PantheonWars] Cannot register perk with empty PerkId");
                return;
            }

            if (_perks.ContainsKey(perk.PerkId))
            {
                _api.Logger.Warning($"[PantheonWars] Perk {perk.PerkId} is already registered. Overwriting...");
            }

            _perks[perk.PerkId] = perk;
            _api.Logger.Debug($"[PantheonWars] Registered perk: {perk.PerkId} ({perk.Name})");
        }

        /// <summary>
        /// Gets a perk by its ID
        /// </summary>
        public Perk? GetPerk(string perkId)
        {
            return _perks.GetValueOrDefault(perkId);
        }

        /// <summary>
        /// Gets all perks for a specific deity and type
        /// </summary>
        public List<Perk> GetPerksForDeity(DeityType deity, PerkKind? type = null)
        {
            var query = _perks.Values.Where(p => p.Deity == deity);

            if (type.HasValue)
            {
                query = query.Where(p => p.Kind == type.Value);
            }

            return query.OrderBy(p => p.RequiredFavorRank)
                       .ThenBy(p => p.RequiredPrestigeRank)
                       .ToList();
        }

        /// <summary>
        /// Gets all perks in the registry
        /// </summary>
        public List<Perk> GetAllPerks()
        {
            return _perks.Values.ToList();
        }

        /// <summary>
        /// Checks if a perk can be unlocked by a player/religion
        /// </summary>
        public (bool canUnlock, string reason) CanUnlockPerk(
            PlayerReligionData playerData,
            ReligionData? religionData,
            Perk? perk)
        {
            // Check if perk exists
            if (perk == null)
            {
                return (false, "Perk not found");
            }

            // Check perk type and corresponding requirements
            if (perk.Kind == PerkKind.Player)
            {
                if (!playerData.HasReligion())
                {
                    return (false, "Not in a religion");
                }
                
                // Check if already unlocked
                if (playerData.IsPerkUnlocked(perk.PerkId))
                {
                    return (false, "Perk already unlocked");
                }

                // Check favor rank requirement
                if (playerData.FavorRank < (FavorRank)perk.RequiredFavorRank)
                {
                    FavorRank requiredRank = (FavorRank)perk.RequiredFavorRank;
                    return (false, $"Requires {requiredRank} favor rank (Current: {playerData.FavorRank})");
                }

                // Check deity matches
                if (playerData.ActiveDeity != perk.Deity)
                {
                    return (false, $"Requires deity: {perk.Deity} (Current: {playerData.ActiveDeity})");
                }

                // Check prerequisites
                foreach (var prereqId in perk.PrerequisitePerks)
                {
                    if (!playerData.IsPerkUnlocked(prereqId))
                    {
                        var prereqPerk = GetPerk(prereqId);
                        string prereqName = prereqPerk?.Name ?? prereqId;
                        return (false, $"Requires prerequisite perk: {prereqName}");
                    }
                }

                return (true, "Can unlock");
            }
            else // PerkType.Religion
            {
                // Check if player has a religion
                if (religionData == null)
                {
                    return (false, "Not in a religion");
                }

                // Check if already unlocked
                if (religionData.UnlockedPerks.TryGetValue(perk.PerkId, out bool unlocked) && unlocked)
                {
                    return (false, "Perk already unlocked");
                }

                // Check prestige rank requirement
                if (religionData.PrestigeRank < (PrestigeRank)perk.RequiredPrestigeRank)
                {
                    PrestigeRank requiredRank = (PrestigeRank)perk.RequiredPrestigeRank;
                    return (false, $"Religion requires {requiredRank} prestige rank (Current: {religionData.PrestigeRank})");
                }

                // Check deity matches
                if (religionData.Deity != perk.Deity)
                {
                    return (false, $"Religion deity mismatch (Perk: {perk.Deity}, Religion: {religionData.Deity})");
                }

                // Check prerequisites
                foreach (var prereqId in perk.PrerequisitePerks)
                {
                    if (!religionData.UnlockedPerks.TryGetValue(prereqId, out bool prereqUnlocked) || !prereqUnlocked)
                    {
                        var prereqPerk = GetPerk(prereqId);
                        string prereqName = prereqPerk?.Name ?? prereqId;
                        return (false, $"Requires prerequisite perk: {prereqName}");
                    }
                }

                return (true, "Can unlock");
            }
        }
    }
}
