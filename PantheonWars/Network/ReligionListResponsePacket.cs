using ProtoBuf;
using System.Collections.Generic;

namespace PantheonWars.Network;

/// <summary>
/// Server sends list of available religions to client
/// </summary>
[ProtoContract]
public class ReligionListResponsePacket
{
    [ProtoContract]
    public class ReligionInfo
    {
        [ProtoMember(1)]
        public string ReligionUID { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string ReligionName { get; set; } = string.Empty;

        [ProtoMember(3)]
        public string Deity { get; set; } = string.Empty;

        [ProtoMember(4)]
        public int MemberCount { get; set; }

        [ProtoMember(5)]
        public int Prestige { get; set; }

        [ProtoMember(6)]
        public string PrestigeRank { get; set; } = string.Empty;

        [ProtoMember(7)]
        public bool IsPublic { get; set; }

        [ProtoMember(8)]
        public string FounderUID { get; set; } = string.Empty;

        [ProtoMember(9)]
        public string Description { get; set; } = string.Empty;
    }

    [ProtoMember(1)]
    public List<ReligionInfo> Religions { get; set; } = new List<ReligionInfo>();

    public ReligionListResponsePacket()
    {
    }

    public ReligionListResponsePacket(List<ReligionInfo> religions)
    {
        Religions = religions;
    }
}
