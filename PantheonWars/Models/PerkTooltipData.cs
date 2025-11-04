using System.Collections.Generic;
using PantheonWars.Models.Enum;

namespace PantheonWars.Models
{
    /// <summary>
    /// Contains formatted data for displaying perk tooltips on hover
    /// </summary>
    public class PerkTooltipData
    {
        /// <summary>
        /// Perk name (title of tooltip)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Perk description (main text)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category of the perk (Combat, Defense, etc.)
        /// </summary>
        public PerkCategory Category { get; set; }

        /// <summary>
        /// Perk kind (Player or Religion)
        /// </summary>
        public PerkKind Kind { get; set; }

        /// <summary>
        /// Tier/level in the tree (1-4)
        /// </summary>
        public int Tier { get; set; }

        /// <summary>
        /// Required favor rank to unlock (for Player perks)
        /// </summary>
        public string RequiredFavorRank { get; set; } = string.Empty;

        /// <summary>
        /// Required prestige rank to unlock (for Religion perks)
        /// </summary>
        public string RequiredPrestigeRank { get; set; } = string.Empty;

        /// <summary>
        /// List of prerequisite perk names that must be unlocked first
        /// </summary>
        public List<string> PrerequisiteNames { get; set; } = new();

        /// <summary>
        /// Formatted stat modifiers (e.g., "+10% melee damage", "+5 max health")
        /// </summary>
        public List<string> FormattedStats { get; set; } = new();

        /// <summary>
        /// Special effect descriptions
        /// </summary>
        public List<string> SpecialEffectDescriptions { get; set; } = new();

        /// <summary>
        /// Whether the perk is unlocked
        /// </summary>
        public bool IsUnlocked { get; set; }

        /// <summary>
        /// Whether the perk can be unlocked
        /// </summary>
        public bool CanUnlock { get; set; }

        /// <summary>
        /// Reason why perk cannot be unlocked (if applicable)
        /// e.g., "Requires Initiate rank" or "Unlock 'Warrior's Resolve' first"
        /// </summary>
        public string UnlockBlockReason { get; set; } = string.Empty;

        /// <summary>
        /// Create tooltip data from a Perk and PerkNodeState
        /// </summary>
        public static PerkTooltipData FromPerkAndState(Perk perk, PerkNodeState state,
            Dictionary<string, Perk>? perkRegistry = null)
        {
            var tooltip = new PerkTooltipData
            {
                Name = perk.Name,
                Description = perk.Description,
                Category = perk.Category,
                Kind = perk.Kind,
                Tier = state.Tier,
                IsUnlocked = state.IsUnlocked,
                CanUnlock = state.CanUnlock
            };

            // Add requirement text based on perk kind
            if (perk.Kind == PerkKind.Player)
            {
                tooltip.RequiredFavorRank = GetFavorRankName(perk.RequiredFavorRank);
            }
            else if (perk.Kind == PerkKind.Religion)
            {
                tooltip.RequiredPrestigeRank = GetPrestigeRankName(perk.RequiredPrestigeRank);
            }

            // Add prerequisite names
            if (perkRegistry != null && perk.PrerequisitePerks.Count > 0)
            {
                foreach (var prereqId in perk.PrerequisitePerks)
                {
                    if (perkRegistry.TryGetValue(prereqId, out var prereqPerk))
                    {
                        tooltip.PrerequisiteNames.Add(prereqPerk.Name);
                    }
                }
            }

            // Format stat modifiers
            foreach (var stat in perk.StatModifiers)
            {
                tooltip.FormattedStats.Add(FormatStatModifier(stat.Key, stat.Value));
            }

            // Add special effects
            tooltip.SpecialEffectDescriptions.AddRange(perk.SpecialEffects);

            // Determine unlock block reason
            if (!state.IsUnlocked && !state.CanUnlock)
            {
                if (perk.PrerequisitePerks.Count > 0 && perkRegistry != null)
                {
                    // Check which prerequisites are missing
                    foreach (var prereqId in perk.PrerequisitePerks)
                    {
                        if (perkRegistry.TryGetValue(prereqId, out var prereqPerk))
                        {
                            // This is simplified - in Phase 6 we'll check actual unlock status
                            tooltip.UnlockBlockReason = $"Requires '{prereqPerk.Name}'";
                            break;
                        }
                    }
                }
                else if (perk.Kind == PerkKind.Player)
                {
                    tooltip.UnlockBlockReason = $"Requires {tooltip.RequiredFavorRank} rank";
                }
                else
                {
                    tooltip.UnlockBlockReason = $"Requires religion {tooltip.RequiredPrestigeRank} rank";
                }
            }

            return tooltip;
        }

        /// <summary>
        /// Format a stat modifier for display
        /// </summary>
        private static string FormatStatModifier(string statName, float value)
        {
            // Handle percentage-based stats
            var percentageStats = new[] { "walkspeed", "meleeDamage", "rangedDamage", "maxhealthExtraMultiplier" };
            bool isPercentage = false;
            foreach (var percentStat in percentageStats)
            {
                if (statName.Contains(percentStat))
                {
                    isPercentage = true;
                    break;
                }
            }

            string displayName = FormatStatName(statName);
            string sign = value >= 0 ? "+" : "";

            if (isPercentage)
            {
                return $"{sign}{value * 100:0.#}% {displayName}";
            }
            else
            {
                return $"{sign}{value:0.#} {displayName}";
            }
        }

        /// <summary>
        /// Convert stat name to readable format
        /// </summary>
        private static string FormatStatName(string statName)
        {
            return statName switch
            {
                "walkspeed" => "movement speed",
                "meleeDamage" => "melee damage",
                "rangedDamage" => "ranged damage",
                "maxhealthExtraMultiplier" => "max health",
                "healingeffectivness" => "healing effectiveness",
                "rangedWeaponsSpeed" => "ranged attack speed",
                _ => statName
            };
        }

        /// <summary>
        /// Get favor rank name from rank number
        /// </summary>
        private static string GetFavorRankName(int rank)
        {
            return rank switch
            {
                0 => "Initiate",
                1 => "Devoted",
                2 => "Zealot",
                3 => "Champion",
                4 => "Exalted",
                _ => $"Rank {rank}"
            };
        }

        /// <summary>
        /// Get prestige rank name from rank number
        /// </summary>
        private static string GetPrestigeRankName(int rank)
        {
            return rank switch
            {
                0 => "Fledgling",
                1 => "Established",
                2 => "Renowned",
                3 => "Legendary",
                4 => "Mythic",
                _ => $"Rank {rank}"
            };
        }
    }
}
