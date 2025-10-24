using PantheonWars.Models;
using ProtoBuf;

namespace PantheonWars.Network;

[ProtoContract]
public class PlayerReligionDataPacket
{
    
    [ProtoMember(1)]
    public string ReligionName { get; set; } = string.Empty;
    
    [ProtoMember(2)]
    public string Deity { get; set; } = string.Empty;
    
    [ProtoMember(3)]
    public int Favor { get; set; }
    [ProtoMember(4)]
    public string FavorRank { get; set; }
    [ProtoMember(5)]
    public int Prestige { get; set; }
    [ProtoMember(6)]
    public string PrestigeRank { get; set; }

    public PlayerReligionDataPacket()
    {
    }

    public PlayerReligionDataPacket(
        string religionName,
        string deity,
        int favor,
        string favorRank,
        int prestige,
        string prestigeRank)
    {
        ReligionName = religionName;
        Deity = deity;
        Favor = favor;
        FavorRank = favorRank;
        Prestige = prestige;
        PrestigeRank = prestigeRank;
        
    }
}