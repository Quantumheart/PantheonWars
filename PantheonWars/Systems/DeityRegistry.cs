using System;
using System.Collections.Generic;
using System.Linq;
using PantheonWars.Models;
using Vintagestory.API.Common;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Central registry for managing all deities in the game
    /// </summary>
    public class DeityRegistry
    {
        private readonly Dictionary<DeityType, Deity> _deities = new();
        private readonly ICoreAPI _api;

        public DeityRegistry(ICoreAPI api)
        {
            _api = api;
        }

        /// <summary>
        /// Initializes the registry with all deities
        /// </summary>
        public void Initialize()
        {
            _api.Logger.Notification("[PantheonWars] Initializing Deity Registry...");

            // Register deities (Phase 1: Only Khoras and Lysa)
            RegisterDeity(CreateKhoras());
            RegisterDeity(CreateLysa());

            // Future deities will be added in Phase 3
            // RegisterDeity(CreateMorthen());
            // RegisterDeity(CreateAethra());
            // RegisterDeity(CreateUmbros());
            // RegisterDeity(CreateTharos());
            // RegisterDeity(CreateGaia());
            // RegisterDeity(CreateVex());

            _api.Logger.Notification($"[PantheonWars] Registered {_deities.Count} deities");
        }

        /// <summary>
        /// Registers a deity in the registry
        /// </summary>
        private void RegisterDeity(Deity deity)
        {
            if (_deities.ContainsKey(deity.Type))
            {
                _api.Logger.Warning($"[PantheonWars] Deity {deity.Name} already registered, skipping");
                return;
            }

            _deities[deity.Type] = deity;
            _api.Logger.Debug($"[PantheonWars] Registered deity: {deity.Name} ({deity.Domain})");
        }

        /// <summary>
        /// Gets a deity by type
        /// </summary>
        public Deity? GetDeity(DeityType type)
        {
            return _deities.TryGetValue(type, out var deity) ? deity : null;
        }

        /// <summary>
        /// Gets all registered deities
        /// </summary>
        public IEnumerable<Deity> GetAllDeities()
        {
            return _deities.Values;
        }

        /// <summary>
        /// Checks if a deity exists
        /// </summary>
        public bool HasDeity(DeityType type)
        {
            return _deities.ContainsKey(type);
        }

        /// <summary>
        /// Gets the relationship between two deities
        /// </summary>
        public DeityRelationshipType GetRelationship(DeityType deity1, DeityType deity2)
        {
            if (deity1 == deity2) return DeityRelationshipType.Neutral;

            var deity = GetDeity(deity1);
            if (deity == null) return DeityRelationshipType.Neutral;

            return deity.Relationships.TryGetValue(deity2, out var relationship)
                ? relationship
                : DeityRelationshipType.Neutral;
        }

        /// <summary>
        /// Gets the favor multiplier based on deity relationship
        /// Allied: 0.5x favor, Rival: 2x favor, Neutral: 1x favor
        /// </summary>
        public float GetFavorMultiplier(DeityType attackerDeity, DeityType victimDeity)
        {
            var relationship = GetRelationship(attackerDeity, victimDeity);
            return relationship switch
            {
                DeityRelationshipType.Allied => 0.5f,
                DeityRelationshipType.Rival => 2.0f,
                _ => 1.0f
            };
        }

        #region Deity Definitions

        private Deity CreateKhoras()
        {
            return new Deity(DeityType.Khoras, "Khoras", "War")
            {
                Description = "The God of War, Khoras embodies martial prowess and strategic combat. " +
                             "Followers gain powerful offensive abilities and excel in direct confrontation.",
                Alignment = DeityAlignment.Lawful,
                PrimaryColor = "#8B0000",      // Dark Red
                SecondaryColor = "#FFD700",    // Gold
                Playstyle = "Aggressive melee combat with high damage abilities and tactical buffs",
                Relationships = new Dictionary<DeityType, DeityRelationshipType>
                {
                    { DeityType.Lysa, DeityRelationshipType.Allied },
                    { DeityType.Morthen, DeityRelationshipType.Rival }
                },
                AbilityIds = new List<string>
                {
                    // To be implemented in Task 6
                    "khoras_warbanner",
                    "khoras_battlecry",
                    "khoras_blade_storm",
                    "khoras_last_stand"
                }
            };
        }

        private Deity CreateLysa()
        {
            return new Deity(DeityType.Lysa, "Lysa", "Hunt")
            {
                Description = "The Goddess of the Hunt, Lysa rewards patience, precision, and tracking. " +
                             "Followers gain mobility and ranged combat advantages.",
                Alignment = DeityAlignment.Neutral,
                PrimaryColor = "#228B22",      // Forest Green
                SecondaryColor = "#8B4513",    // Saddle Brown
                Playstyle = "Mobile ranged combat with tracking abilities and tactical positioning",
                Relationships = new Dictionary<DeityType, DeityRelationshipType>
                {
                    { DeityType.Khoras, DeityRelationshipType.Allied },
                    { DeityType.Umbros, DeityRelationshipType.Rival }
                },
                AbilityIds = new List<string>
                {
                    // To be implemented in Task 6
                    "lysa_hunters_mark",
                    "lysa_swift_feet",
                    "lysa_arrow_rain",
                    "lysa_predator_instinct"
                }
            };
        }

        // Future deity definitions (Phase 3)
        /*
        private Deity CreateMorthen() { ... }
        private Deity CreateAethra() { ... }
        private Deity CreateUmbros() { ... }
        private Deity CreateTharos() { ... }
        private Deity CreateGaia() { ... }
        private Deity CreateVex() { ... }
        */

        #endregion
    }
}
