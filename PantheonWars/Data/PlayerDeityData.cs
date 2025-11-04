using System;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using ProtoBuf;

namespace PantheonWars.Data
{
    /// <summary>
    /// Stores player-specific deity data for persistence
    /// </summary>
    [ProtoContract]
    public class PlayerDeityData
    {
        /// <summary>
        /// Player's unique identifier
        /// </summary>
        [ProtoMember(1)]
        public string PlayerUID { get; set; } = string.Empty;

        /// <summary>
        /// Currently pledged deity
        /// </summary>
        [ProtoMember(2)]
        public DeityType DeityType { get; set; } = DeityType.None;

        /// <summary>
        /// Current divine favor amount
        /// </summary>
        [ProtoMember(3)]
        public int DivineFavor { get; set; } = 0;

        /// <summary>
        /// Current devotion rank
        /// </summary>
        [ProtoMember(4)]
        public DevotionRank DevotionRank { get; set; } = DevotionRank.Initiate;

        /// <summary>
        /// Total favor earned (lifetime stat)
        /// </summary>
        [ProtoMember(5)]
        public int TotalFavorEarned { get; set; } = 0;

        /// <summary>
        /// Total kills while pledged to current deity
        /// </summary>
        [ProtoMember(6)]
        public int KillCount { get; set; } = 0;

        /// <summary>
        /// Timestamp when player first pledged to current deity
        /// </summary>
        [ProtoMember(7)]
        public DateTime PledgeDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Last time player switched deities (for cooldown tracking)
        /// </summary>
        [ProtoMember(8)]
        public DateTime? LastDeitySwitch { get; set; } = null;

        /// <summary>
        /// Version number for data migration
        /// </summary>
        [ProtoMember(9)]
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
