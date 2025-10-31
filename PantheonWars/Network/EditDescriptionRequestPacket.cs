using ProtoBuf;

namespace PantheonWars.Network;

/// <summary>
/// Client requests to edit religion description
/// </summary>
[ProtoContract]
public class EditDescriptionRequestPacket
{
    [ProtoMember(1)]
    public string ReligionUID { get; set; } = string.Empty;

    [ProtoMember(2)]
    public string Description { get; set; } = string.Empty;

    public EditDescriptionRequestPacket()
    {
    }

    public EditDescriptionRequestPacket(string religionUID, string description)
    {
        ReligionUID = religionUID;
        Description = description;
    }
}
