using ProtoBuf;

namespace PantheonWars.Network;

/// <summary>
///     Client requests to unlock a perk
/// </summary>
[ProtoContract]
public class PerkUnlockRequestPacket
{
    public PerkUnlockRequestPacket()
    {
    }

    public PerkUnlockRequestPacket(string perkId)
    {
        PerkId = perkId;
    }

    [ProtoMember(1)] public string PerkId { get; set; } = string.Empty;
}
