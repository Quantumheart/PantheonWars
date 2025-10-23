using PantheonWars.Models;
using ProtoBuf;

namespace PantheonWars.Network
{
    /// <summary>
    /// Network packet for syncing player deity data from server to client
    /// </summary>
    [ProtoContract]
    public class PlayerDataPacket
    {
        [ProtoMember(1)]
        public int DeityTypeId { get; set; }

        [ProtoMember(2)]
        public int DivineFavor { get; set; }

        [ProtoMember(3)]
        public int DevotionRankId { get; set; }

        [ProtoMember(4)]
        public string DeityName { get; set; } = string.Empty;

        public PlayerDataPacket()
        {
        }

        public PlayerDataPacket(DeityType deityType, int favor, DevotionRank rank, string deityName)
        {
            DeityTypeId = (int)deityType;
            DivineFavor = favor;
            DevotionRankId = (int)rank;
            DeityName = deityName;
        }

        public DeityType GetDeityType()
        {
            return (DeityType)DeityTypeId;
        }

        public DevotionRank GetDevotionRank()
        {
            return (DevotionRank)DevotionRankId;
        }
    }
}
