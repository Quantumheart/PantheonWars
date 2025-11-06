using System;
using System.Collections.Generic;
using PantheonWars.Models.Enum;
using ProtoBuf;

namespace PantheonWars.Data;

/// <summary>
///     Stores religion-specific data for persistence
/// </summary>
[ProtoContract]
public class ReligionData
{
    /// <summary>
    ///     Creates a new religion with the specified parameters
    /// </summary>
    public ReligionData(string religionUID, string religionName, DeityType deity, string founderUID)
    {
        ReligionUID = religionUID;
        ReligionName = religionName;
        Deity = deity;
        FounderUID = founderUID;
        MemberUIDs = new List<string> { founderUID }; // Founder is first member
        CreationDate = DateTime.UtcNow;
    }

    /// <summary>
    ///     Parameterless constructor for serialization
    /// </summary>
    public ReligionData()
    {
    }

    /// <summary>
    ///     Unique identifier for this religion
    /// </summary>
    [ProtoMember(1)]
    public string ReligionUID { get; set; } = string.Empty;

    /// <summary>
    ///     Display name of the religion (e.g., "Knights of Khoras")
    /// </summary>
    [ProtoMember(2)]
    public string ReligionName { get; set; } = string.Empty;

    /// <summary>
    ///     The deity this religion serves (permanent, cannot be changed)
    /// </summary>
    [ProtoMember(3)]
    public DeityType Deity { get; set; } = DeityType.None;

    /// <summary>
    ///     Player UID of the religion founder
    /// </summary>
    [ProtoMember(4)]
    public string FounderUID { get; set; } = string.Empty;

    /// <summary>
    ///     Ordered list of member player UIDs (founder is always first)
    /// </summary>
    [ProtoMember(5)]
    public List<string> MemberUIDs { get; set; } = new();

    /// <summary>
    ///     Current prestige rank of the religion
    /// </summary>
    [ProtoMember(6)]
    public PrestigeRank PrestigeRank { get; set; } = PrestigeRank.Fledgling;

    /// <summary>
    ///     Current prestige points
    /// </summary>
    [ProtoMember(7)]
    public int Prestige { get; set; }

    /// <summary>
    ///     Total prestige earned (lifetime stat, used for ranking)
    /// </summary>
    [ProtoMember(8)]
    public int TotalPrestige { get; set; }

    /// <summary>
    ///     When the religion was created
    /// </summary>
    [ProtoMember(9)]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Dictionary of unlocked religion perks
    ///     Key: perk ID, Value: unlock status (true if unlocked)
    /// </summary>
    [ProtoMember(10)]
    public Dictionary<string, bool> UnlockedPerks { get; set; } = new();

    /// <summary>
    ///     Whether this is a public religion (anyone can join) or private (invite-only)
    /// </summary>
    [ProtoMember(11)]
    public bool IsPublic { get; set; } = true;

    /// <summary>
    ///     Religion description or manifesto set by the founder
    /// </summary>
    [ProtoMember(12)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Adds a member to the religion
    /// </summary>
    public void AddMember(string playerUID)
    {
        if (!MemberUIDs.Contains(playerUID)) MemberUIDs.Add(playerUID);
    }

    /// <summary>
    ///     Removes a member from the religion
    /// </summary>
    public bool RemoveMember(string playerUID)
    {
        return MemberUIDs.Remove(playerUID);
    }

    /// <summary>
    ///     Checks if a player is a member of this religion
    /// </summary>
    public bool IsMember(string playerUID)
    {
        return MemberUIDs.Contains(playerUID);
    }

    /// <summary>
    ///     Checks if a player is the founder
    /// </summary>
    public bool IsFounder(string playerUID)
    {
        return FounderUID == playerUID;
    }

    /// <summary>
    ///     Gets the member count
    /// </summary>
    public int GetMemberCount()
    {
        return MemberUIDs.Count;
    }

    /// <summary>
    ///     Updates the prestige rank based on total prestige earned
    /// </summary>
    public void UpdatePrestigeRank()
    {
        PrestigeRank = TotalPrestige switch
        {
            >= 10000 => PrestigeRank.Mythic,
            >= 5000 => PrestigeRank.Legendary,
            >= 2000 => PrestigeRank.Renowned,
            >= 500 => PrestigeRank.Established,
            _ => PrestigeRank.Fledgling
        };
    }

    /// <summary>
    ///     Adds prestige and updates statistics
    /// </summary>
    public void AddPrestige(int amount)
    {
        if (amount > 0)
        {
            Prestige += amount;
            TotalPrestige += amount;
            UpdatePrestigeRank();
        }
    }

    /// <summary>
    ///     Unlocks a perk for this religion
    /// </summary>
    public void UnlockPerk(string perkId)
    {
        UnlockedPerks[perkId] = true;
    }

    /// <summary>
    ///     Checks if a perk is unlocked
    /// </summary>
    public bool IsPerkUnlocked(string perkId)
    {
        return UnlockedPerks.TryGetValue(perkId, out var unlocked) && unlocked;
    }
}