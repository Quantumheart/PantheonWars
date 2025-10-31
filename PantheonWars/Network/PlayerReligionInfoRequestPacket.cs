using ProtoBuf;

namespace PantheonWars.Network;

/// <summary>
/// Client requests their own religion information (for "My Religion" tab)
/// </summary>
[ProtoContract]
public class PlayerReligionInfoRequestPacket
{
    public PlayerReligionInfoRequestPacket()
    {
    }
}
