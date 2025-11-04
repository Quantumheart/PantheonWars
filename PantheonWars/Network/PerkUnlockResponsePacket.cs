using ProtoBuf;

namespace PantheonWars.Network;

/// <summary>
///     Server responds to a perk unlock request
/// </summary>
[ProtoContract]
public class PerkUnlockResponsePacket
{
    /// <summary>
    ///     Whether the unlock was successful
    /// </summary>
    [ProtoMember(1)]
    public bool Success { get; set; }

    /// <summary>
    ///     Error message if unlock failed
    /// </summary>
    [ProtoMember(2)]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    ///     ID of the perk that was unlocked (for verification)
    /// </summary>
    [ProtoMember(3)]
    public string PerkId { get; set; } = string.Empty;

    /// <summary>
    ///     Name of the perk that was unlocked (for display)
    /// </summary>
    [ProtoMember(4)]
    public string PerkName { get; set; } = string.Empty;
}
