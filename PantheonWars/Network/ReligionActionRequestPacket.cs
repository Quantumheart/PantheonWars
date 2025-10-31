using ProtoBuf;

namespace PantheonWars.Network;

/// <summary>
/// Client requests an action on a religion (join, leave, kick, invite, etc.)
/// </summary>
[ProtoContract]
public class ReligionActionRequestPacket
{
    [ProtoMember(1)]
    public string Action { get; set; } = string.Empty; // "join", "leave", "kick", "invite"

    [ProtoMember(2)]
    public string ReligionUID { get; set; } = string.Empty;

    [ProtoMember(3)]
    public string TargetPlayerUID { get; set; } = string.Empty; // For kick/invite actions

    public ReligionActionRequestPacket()
    {
    }

    public ReligionActionRequestPacket(string action, string religionUID = "", string targetPlayerUID = "")
    {
        Action = action;
        ReligionUID = religionUID;
        TargetPlayerUID = targetPlayerUID;
    }
}
