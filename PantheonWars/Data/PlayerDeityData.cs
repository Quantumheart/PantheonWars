using System;
using PantheonWars.Models;

namespace PantheonWars.Data
{
    /// <summary>
    /// Stores player-specific deity data for persistence
    /// </summary>
    public class PlayerDeityData
    {
        /// <summary>
        /// Player's unique identifier
        /// </summary>
        public string PlayerUID { get; set; } = string.Empty;

        /// <summary>
        /// Currently pledged deity
        /// </summary>
        public DeityType DeityType { get; set; } = DeityType.None;

        /// <summary>
        /// Current divine favor amount
        /// </summary>
        public int DivineFavor { get; set; } = 0;

        /// <summary>
        /// Current devotion rank
        /// </summary>
        public DevotionRank DevotionRank { get; set; } = DevotionRank.Initiate;

        /// <summary>
        /// Total favor earned (lifetime stat)
        /// </summary>
        public int TotalFavorEarned { get; set; } = 0;

        /// <summary>
        /// Total kills while pledged to current deity
        /// </summary>
        public int KillCount { get; set; } = 0;

        /// <summary>
        /// Timestamp when player first pledged to current deity
        /// </summary>
        public DateTime PledgeDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Last time player switched deities (for cooldown tracking)
        /// </summary>
        public DateTime? LastDeitySwitch { get; set; } = null;

        /// <summary>
        /// Version number for data migration
        /// </summary>
        public int DataVersion { get; set; } = 1;

        public PlayerDeityData()
        {
        }

        public PlayerDeityData(string playerUID)
        {
            PlayerUID = playerUID;
            PledgeDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if player has pledged to a deity
        /// </summary>
        public bool HasDeity()
        {
            return DeityType != DeityType.None;
        }

        /// <summary>
        /// Updates the devotion rank based on total favor earned
        /// </summary>
        public void UpdateDevotionRank()
        {
            // Devotion rank thresholds
            DevotionRank = TotalFavorEarned switch
            {
                >= 10000 => DevotionRank.Avatar,
                >= 5000 => DevotionRank.Champion,
                >= 2000 => DevotionRank.Zealot,
                >= 500 => DevotionRank.Disciple,
                _ => DevotionRank.Initiate
            };
        }

        /// <summary>
        /// Adds favor and updates statistics
        /// </summary>
        public void AddFavor(int amount)
        {
            if (amount > 0)
            {
                DivineFavor += amount;
                TotalFavorEarned += amount;
                UpdateDevotionRank();
            }
        }

        /// <summary>
        /// Removes favor (for ability costs or penalties)
        /// </summary>
        public bool RemoveFavor(int amount)
        {
            if (DivineFavor >= amount)
            {
                DivineFavor -= amount;
                return true;
            }
            return false;
        }
    }
}
