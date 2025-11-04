using System;
using System.Collections.Generic;
using System.Linq;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using Vintagestory.API.Server;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Manages religion prestige progression and religion-wide perks
    /// </summary>
    public class ReligionPrestigeManager
    {
        private readonly ICoreServerAPI _sapi;
        private readonly ReligionManager _religionManager;
        private PerkRegistry? _perkRegistry;
        private PerkEffectSystem? _perkEffectSystem;

        // Prestige rank thresholds
        private const int FLEDGLING_THRESHOLD = 0;
        private const int ESTABLISHED_THRESHOLD = 500;
        private const int RENOWNED_THRESHOLD = 2000;
        private const int LEGENDARY_THRESHOLD = 5000;
        private const int MYTHIC_THRESHOLD = 10000;

        public ReligionPrestigeManager(ICoreServerAPI sapi, ReligionManager religionManager)
        {
            _sapi = sapi;
            _religionManager = religionManager;
        }

        /// <summary>
        /// Sets the perk registry and effect system (called after they're initialized)
        /// </summary>
        public void SetPerkSystems(PerkRegistry perkRegistry, PerkEffectSystem perkEffectSystem)
        {
            _perkRegistry = perkRegistry;
            _perkEffectSystem = perkEffectSystem;
        }

        /// <summary>
        /// Initializes the religion prestige manager
        /// </summary>
        public void Initialize()
        {
            _sapi.Logger.Notification("[PantheonWars] Initializing Religion Prestige Manager...");
            _sapi.Logger.Notification("[PantheonWars] Religion Prestige Manager initialized");
        }

        /// <summary>
        /// Adds prestige to a religion and updates rank if needed
        /// </summary>
        public void AddPrestige(string religionUID, int amount, string reason = "")
        {
            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null)
            {
                _sapi.Logger.Error($"[PantheonWars] Cannot add prestige to non-existent religion: {religionUID}");
                return;
            }

            var oldRank = religion.PrestigeRank;

            // Add prestige
            religion.Prestige += amount;
            religion.TotalPrestige += amount;

            if (!string.IsNullOrEmpty(reason))
            {
                _sapi.Logger.Debug($"[PantheonWars] Religion {religion.ReligionName} gained {amount} prestige: {reason}");
            }

            // Update rank
            UpdatePrestigeRank(religionUID);

            // Check if rank changed
            if (religion.PrestigeRank > oldRank)
            {
                SendReligionRankUpNotification(religionUID, religion.PrestigeRank);
            }
        }

        /// <summary>
        /// Updates prestige rank based on total prestige earned
        /// </summary>
        public void UpdatePrestigeRank(string religionUID)
        {
            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null)
            {
                _sapi.Logger.Error($"[PantheonWars] Cannot update prestige rank for non-existent religion: {religionUID}");
                return;
            }

            var oldRank = religion.PrestigeRank;
            var newRank = CalculatePrestigeRank(religion.TotalPrestige);

            if (newRank != oldRank)
            {
                religion.PrestigeRank = newRank;
                _sapi.Logger.Notification($"[PantheonWars] Religion {religion.ReligionName} rank changed: {oldRank} -> {newRank}");

                // Check for new perk unlocks
                CheckForNewPerkUnlocks(religionUID, newRank);
            }
        }

        /// <summary>
        /// Calculates prestige rank based on total prestige
        /// </summary>
        private PrestigeRank CalculatePrestigeRank(int totalPrestige)
        {
            if (totalPrestige >= MYTHIC_THRESHOLD) return PrestigeRank.Mythic;
            if (totalPrestige >= LEGENDARY_THRESHOLD) return PrestigeRank.Legendary;
            if (totalPrestige >= RENOWNED_THRESHOLD) return PrestigeRank.Renowned;
            if (totalPrestige >= ESTABLISHED_THRESHOLD) return PrestigeRank.Established;
            return PrestigeRank.Fledgling;
        }

        /// <summary>
        /// Checks for new perk unlocks when religion ranks up
        /// </summary>
        private void CheckForNewPerkUnlocks(string religionUID, PrestigeRank newRank)
        {
            if (_perkRegistry == null)
            {
                _sapi.Logger.Debug($"[PantheonWars] Perk registry not yet initialized, skipping perk unlock check");
                return;
            }

            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null)
            {
                return;
            }

            // Get all religion perks for this deity
            var allReligionPerks = _perkRegistry.GetPerksForDeity(religion.Deity, PerkKind.Religion);

            // Find perks that are now unlockable at the new rank
            var newlyUnlockablePerks = new List<Perk>();

            foreach (var perk in allReligionPerks)
            {
                // Check if this perk requires the new rank (or lower)
                if ((int)perk.RequiredPrestigeRank > (int)newRank)
                {
                    continue; // Not yet available
                }

                // Check if already unlocked
                if (religion.UnlockedPerks.TryGetValue(perk.PerkId, out bool unlocked) && unlocked)
                {
                    continue; // Already unlocked
                }

                // Check if all prerequisites are met
                bool allPrereqsMet = true;
                foreach (var prereqId in perk.PrerequisitePerks)
                {
                    if (!religion.UnlockedPerks.TryGetValue(prereqId, out bool prereqUnlocked) || !prereqUnlocked)
                    {
                        allPrereqsMet = false;
                        break;
                    }
                }

                if (allPrereqsMet)
                {
                    newlyUnlockablePerks.Add(perk);
                }
            }

            // Notify religion members about newly unlockable perks
            if (newlyUnlockablePerks.Count > 0)
            {
                NotifyNewPerksAvailable(religionUID, newlyUnlockablePerks);
            }
            else
            {
                _sapi.Logger.Debug($"[PantheonWars] No new perks available for religion {religion.ReligionName} at rank {newRank}");
            }
        }

        /// <summary>
        /// Notifies all religion members about newly available perks
        /// </summary>
        private void NotifyNewPerksAvailable(string religionUID, List<Perk> newPerks)
        {
            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null) return;

            string perkNames = string.Join(", ", newPerks.Select(p => p.Name));
            string message = $"New perks available for '{religion.ReligionName}': {perkNames}. Use /perks religion to view and /perks unlock to unlock them.";

            // Notify all members
            foreach (var memberUID in religion.MemberUIDs)
            {
                var player = _sapi.World.PlayerByUid(memberUID) as IServerPlayer;
                if (player != null)
                {
                    player.SendMessage(
                        Vintagestory.API.Config.GlobalConstants.GeneralChatGroup,
                        message,
                        Vintagestory.API.Common.EnumChatType.Notification
                    );
                }
            }

            _sapi.Logger.Notification($"[PantheonWars] Religion {religion.ReligionName} has {newPerks.Count} new perks available: {perkNames}");
        }

        /// <summary>
        /// Sends rank-up notification to all religion members
        /// </summary>
        private void SendReligionRankUpNotification(string religionUID, PrestigeRank newRank)
        {
            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null) return;

            string message = $"Your religion '{religion.ReligionName}' has ascended to {newRank} rank!";

            // Notify all members
            foreach (var memberUID in religion.MemberUIDs)
            {
                var player = _sapi.World.PlayerByUid(memberUID) as IServerPlayer;
                if (player != null)
                {
                    player.SendMessage(
                        Vintagestory.API.Config.GlobalConstants.GeneralChatGroup,
                        message,
                        Vintagestory.API.Common.EnumChatType.Notification
                    );
                }
            }

            _sapi.Logger.Notification($"[PantheonWars] Religion {religion.ReligionName} reached {newRank} rank!");
        }

        /// <summary>
        /// Unlocks a religion perk if requirements are met
        /// </summary>
        public bool UnlockReligionPerk(string religionUID, string perkId)
        {
            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null)
            {
                _sapi.Logger.Error($"[PantheonWars] Cannot unlock perk for non-existent religion: {religionUID}");
                return false;
            }

            // Check if already unlocked
            if (religion.UnlockedPerks.TryGetValue(perkId, out bool unlocked) && unlocked)
            {
                return false;
            }

            // Unlock the perk
            religion.UnlockedPerks[perkId] = true;
            _sapi.Logger.Notification($"[PantheonWars] Religion {religion.ReligionName} unlocked perk: {perkId}");

            // Trigger perk effect refresh for all members
            TriggerPerkEffectRefresh(religionUID);

            return true;
        }

        /// <summary>
        /// Triggers perk effect refresh for all members
        /// </summary>
        private void TriggerPerkEffectRefresh(string religionUID)
        {
            if (_perkEffectSystem == null)
            {
                _sapi.Logger.Debug($"[PantheonWars] Perk effect system not yet initialized, skipping perk refresh");
                return;
            }

            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null) return;

            _sapi.Logger.Debug($"[PantheonWars] Triggering perk effect refresh for religion {religion.ReligionName}");

            // Refresh perk effects for all members
            _perkEffectSystem.RefreshReligionPerks(religionUID);
        }

        /// <summary>
        /// Gets all active (unlocked) religion perks
        /// </summary>
        public List<string> GetActiveReligionPerks(string religionUID)
        {
            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null)
            {
                return new List<string>();
            }

            var activePerks = new List<string>();
            foreach (var kvp in religion.UnlockedPerks)
            {
                if (kvp.Value) // If unlocked
                {
                    activePerks.Add(kvp.Key);
                }
            }

            return activePerks;
        }

        /// <summary>
        /// Gets prestige progress information for display
        /// </summary>
        public (int current, int nextThreshold, PrestigeRank nextRank) GetPrestigeProgress(string religionUID)
        {
            var religion = _religionManager.GetReligion(religionUID);
            if (religion == null)
            {
                return (0, 0, PrestigeRank.Fledgling);
            }

            int nextThreshold = religion.PrestigeRank switch
            {
                PrestigeRank.Fledgling => ESTABLISHED_THRESHOLD,
                PrestigeRank.Established => RENOWNED_THRESHOLD,
                PrestigeRank.Renowned => LEGENDARY_THRESHOLD,
                PrestigeRank.Legendary => MYTHIC_THRESHOLD,
                PrestigeRank.Mythic => MYTHIC_THRESHOLD, // Max rank
                _ => ESTABLISHED_THRESHOLD
            };

            PrestigeRank nextRank = religion.PrestigeRank switch
            {
                PrestigeRank.Fledgling => PrestigeRank.Established,
                PrestigeRank.Established => PrestigeRank.Renowned,
                PrestigeRank.Renowned => PrestigeRank.Legendary,
                PrestigeRank.Legendary => PrestigeRank.Mythic,
                PrestigeRank.Mythic => PrestigeRank.Mythic, // Max rank
                _ => PrestigeRank.Established
            };

            return (religion.TotalPrestige, nextThreshold, nextRank);
        }
    }
}
