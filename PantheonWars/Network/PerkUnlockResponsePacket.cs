using ProtoBuf;

namespace PantheonWars.Network;

/// <summary>
///     Server responds to perk unlock request
/// </summary>
[ProtoContract]
public class PerkUnlockResponsePacket
{
    public PerkUnlockResponsePacket()
    {
    }

    public PerkUnlockResponsePacket(bool success, string message, string perkId)
    {
        Success = success;
        Message = message;
        PerkId = perkId;
    }

    [ProtoMember(1)] public bool Success { get; set; }

    [ProtoMember(2)] public string Message { get; set; } = string.Empty;

    [ProtoMember(3)] public string PerkId { get; set; } = string.Empty;
}
