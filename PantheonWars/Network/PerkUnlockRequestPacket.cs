using ProtoBuf;

namespace PantheonWars.Network;

/// <summary>
///     Client requests to unlock a perk
/// </summary>
[ProtoContract]
public class PerkUnlockRequestPacket
{
    /// <summary>
    ///     ID of the perk to unlock
    /// </summary>
    [ProtoMember(1)]
    public string PerkId { get; set; } = string.Empty;
}
