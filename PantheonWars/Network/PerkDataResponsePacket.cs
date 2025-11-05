using System.Collections.Generic;
using ProtoBuf;

namespace PantheonWars.Network;

/// <summary>
///     Server sends perk data for the player's current deity
/// </summary>
[ProtoContract]
public class PerkDataResponsePacket
{
    [ProtoMember(1)] public bool HasReligion { get; set; }

    [ProtoMember(2)] public string ReligionUID { get; set; } = string.Empty;

    [ProtoMember(3)] public string ReligionName { get; set; } = string.Empty;

    [ProtoMember(4)] public string Deity { get; set; } = string.Empty;

    [ProtoMember(5)] public int FavorRank { get; set; }

    [ProtoMember(6)] public int PrestigeRank { get; set; }

    [ProtoMember(7)] public List<PerkInfo> PlayerPerks { get; set; } = new();

    [ProtoMember(8)] public List<PerkInfo> ReligionPerks { get; set; } = new();

    [ProtoMember(9)] public List<string> UnlockedPlayerPerks { get; set; } = new();

    [ProtoMember(10)] public List<string> UnlockedReligionPerks { get; set; } = new();

    /// <summary>
    ///     Basic perk information needed for UI display
    /// </summary>
    [ProtoContract]
    public class PerkInfo
    {
        [ProtoMember(1)] public string PerkId { get; set; } = string.Empty;

        [ProtoMember(2)] public string Name { get; set; } = string.Empty;

        [ProtoMember(3)] public string Description { get; set; } = string.Empty;

        [ProtoMember(4)] public int RequiredFavorRank { get; set; }

        [ProtoMember(5)] public int RequiredPrestigeRank { get; set; }

        [ProtoMember(6)] public List<string> PrerequisitePerks { get; set; } = new();

        [ProtoMember(7)] public int Category { get; set; } // PerkCategory as int

        [ProtoMember(8)] public Dictionary<string, float> StatModifiers { get; set; } = new();
    }
}
