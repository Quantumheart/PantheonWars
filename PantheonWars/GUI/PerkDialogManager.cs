using System;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using Vintagestory.API.Client;

namespace PantheonWars.GUI
{
    /// <summary>
    /// Manages state for the Perk Dialog UI
    /// </summary>
    public class PerkDialogManager
    {
        private readonly ICoreClientAPI _capi;

        // Religion and deity state
        public string? CurrentReligionUID { get; set; }
        public DeityType CurrentDeity { get; set; } = DeityType.None;
        public string? CurrentReligionName { get; set; }

        // Perk selection state
        public string? SelectedPerkId { get; set; }
        public string? HoveringPerkId { get; set; }

        // Scroll state
        public float PlayerTreeScrollX { get; set; } = 0f;
        public float PlayerTreeScrollY { get; set; } = 0f;
        public float ReligionTreeScrollX { get; set; } = 0f;
        public float ReligionTreeScrollY { get; set; } = 0f;

        // Data loaded flags
        public bool IsDataLoaded { get; set; } = false;

        public PerkDialogManager(ICoreClientAPI capi)
        {
            _capi = capi;
        }

        /// <summary>
        /// Initialize dialog state from player's current religion data
        /// </summary>
        public void Initialize(string? religionUID, DeityType deity, string? religionName)
        {
            CurrentReligionUID = religionUID;
            CurrentDeity = deity;
            CurrentReligionName = religionName;
            IsDataLoaded = true;

            // Reset selection and scroll
            SelectedPerkId = null;
            HoveringPerkId = null;
            PlayerTreeScrollX = 0f;
            PlayerTreeScrollY = 0f;
            ReligionTreeScrollX = 0f;
            ReligionTreeScrollY = 0f;
        }

        /// <summary>
        /// Reset all state
        /// </summary>
        public void Reset()
        {
            CurrentReligionUID = null;
            CurrentDeity = DeityType.None;
            CurrentReligionName = null;
            SelectedPerkId = null;
            HoveringPerkId = null;
            PlayerTreeScrollX = 0f;
            PlayerTreeScrollY = 0f;
            ReligionTreeScrollX = 0f;
            ReligionTreeScrollY = 0f;
            IsDataLoaded = false;
        }

        /// <summary>
        /// Select a perk (for displaying details)
        /// </summary>
        public void SelectPerk(string perkId)
        {
            SelectedPerkId = perkId;
        }

        /// <summary>
        /// Clear perk selection
        /// </summary>
        public void ClearSelection()
        {
            SelectedPerkId = null;
        }

        /// <summary>
        /// Check if player has a religion
        /// </summary>
        public bool HasReligion()
        {
            return !string.IsNullOrEmpty(CurrentReligionUID) && CurrentDeity != DeityType.None;
        }
    }
}
