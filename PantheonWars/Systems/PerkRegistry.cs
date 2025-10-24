using System;
using System.Collections.Generic;
using System.Linq;
using PantheonWars.Data;
using PantheonWars.Models;
using Vintagestory.API.Common;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Registry for all perks in the game (Phase 3.3)
    /// </summary>
    public class PerkRegistry
    {
        private readonly ICoreAPI _api;
        private readonly Dictionary<string, Perk> _perks = new();

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

            // Register sample perks for testing (Khoras)
            RegisterSamplePerks();

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
            return _perks.TryGetValue(perkId, out var perk) ? perk : null;
        }

        /// <summary>
        /// Gets all perks for a specific deity and type
        /// </summary>
        public List<Perk> GetPerksForDeity(DeityType deity, PerkType? type = null)
        {
            var query = _perks.Values.Where(p => p.Deity == deity);

            if (type.HasValue)
            {
                query = query.Where(p => p.Type == type.Value);
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
            Perk perk)
        {
            // Check if perk exists
            if (perk == null)
            {
                return (false, "Perk not found");
            }

            // Check perk type and corresponding requirements
            if (perk.Type == PerkType.Player)
            {
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

        /// <summary>
        /// Registers sample perks for testing (Khoras - War deity)
        /// </summary>
        private void RegisterSamplePerks()
        {
            // Khoras Player Perks - Tier 1 (Initiate)
            RegisterPerk(new Perk("khoras_warriors_resolve", "Warrior's Resolve", DeityType.Khoras)
            {
                Type = PerkType.Player,
                Category = PerkCategory.Combat,
                Description = "Your devotion to war strengthens your strikes. +5% melee damage.",
                RequiredFavorRank = (int)FavorRank.Initiate,
                StatModifiers = new Dictionary<string, float>
                {
                    { "meleeDamageMultiplier", 0.05f }
                }
            });

            RegisterPerk(new Perk("khoras_battle_endurance", "Battle Endurance", DeityType.Khoras)
            {
                Type = PerkType.Player,
                Category = PerkCategory.Defense,
                Description = "Your body adapts to the rigors of combat. +10% max health.",
                RequiredFavorRank = (int)FavorRank.Initiate,
                StatModifiers = new Dictionary<string, float>
                {
                    { "maxHealthMultiplier", 0.10f }
                }
            });

            // Khoras Player Perks - Tier 2 (Disciple)
            RegisterPerk(new Perk("khoras_bloodlust", "Bloodlust", DeityType.Khoras)
            {
                Type = PerkType.Player,
                Category = PerkCategory.Combat,
                Description = "Each strike fuels your rage. +10% melee damage. Requires Warrior's Resolve.",
                RequiredFavorRank = (int)FavorRank.Disciple,
                PrerequisitePerks = new List<string> { "khoras_warriors_resolve" },
                StatModifiers = new Dictionary<string, float>
                {
                    { "meleeDamageMultiplier", 0.10f }
                }
            });

            // Khoras Religion Perks - Tier 1 (Fledgling)
            RegisterPerk(new Perk("khoras_congregation_strength", "Congregation's Strength", DeityType.Khoras)
            {
                Type = PerkType.Religion,
                Category = PerkCategory.Combat,
                Description = "All members of your religion gain +3% melee damage.",
                RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                StatModifiers = new Dictionary<string, float>
                {
                    { "meleeDamageMultiplier", 0.03f }
                }
            });

            RegisterPerk(new Perk("khoras_war_banner", "War Banner", DeityType.Khoras)
            {
                Type = PerkType.Religion,
                Category = PerkCategory.Defense,
                Description = "Your religion's banner inspires courage. All members gain +5% max health.",
                RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                StatModifiers = new Dictionary<string, float>
                {
                    { "maxHealthMultiplier", 0.05f }
                }
            });

            // Khoras Religion Perks - Tier 2 (Established)
            RegisterPerk(new Perk("khoras_legion_tactics", "Legion Tactics", DeityType.Khoras)
            {
                Type = PerkType.Religion,
                Category = PerkCategory.Combat,
                Description = "Your religion masters combat tactics. All members gain +5% melee damage. Requires Congregation's Strength.",
                RequiredPrestigeRank = (int)PrestigeRank.Established,
                PrerequisitePerks = new List<string> { "khoras_congregation_strength" },
                StatModifiers = new Dictionary<string, float>
                {
                    { "meleeDamageMultiplier", 0.05f }
                }
            });
        }
    }
}
