using System.Diagnostics.CodeAnalysis;
using PantheonWars.Data;
using PantheonWars.Models;

namespace PantheonWars.Tests.Data;

[ExcludeFromCodeCoverage]
public class ReligionDataTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Parameterless_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var religion = new ReligionData();

        // Assert
        Assert.Empty(religion.ReligionUID);
        Assert.Empty(religion.ReligionName);
        Assert.Equal(DeityType.None, religion.Deity);
        Assert.Empty(religion.FounderUID);
        Assert.Empty(religion.MemberUIDs);
        Assert.Equal(PrestigeRank.Fledgling, religion.PrestigeRank);
        Assert.Equal(0, religion.Prestige);
        Assert.Equal(0, religion.TotalPrestige);
        Assert.Empty(religion.UnlockedPerks);
        Assert.True(religion.IsPublic);
        Assert.Empty(religion.Description);
    }

    [Fact]
    public void Constructor_WithParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var religionUID = "test-religion-uid";
        var religionName = "Knights of Khoras";
        var deity = DeityType.Khoras;
        var founderUID = "founder-123";

        // Act
        var religion = new ReligionData(religionUID, religionName, deity, founderUID);

        // Assert
        Assert.Equal(religionUID, religion.ReligionUID);
        Assert.Equal(religionName, religion.ReligionName);
        Assert.Equal(deity, religion.Deity);
        Assert.Equal(founderUID, religion.FounderUID);
        Assert.Single(religion.MemberUIDs);
        Assert.Contains(founderUID, religion.MemberUIDs);
        Assert.Equal(PrestigeRank.Fledgling, religion.PrestigeRank);
        Assert.Equal(0, religion.Prestige);
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetCreationDate()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.InRange(religion.CreationDate, beforeCreation, afterCreation);
    }

    #endregion

    #region Member Management Tests

    [Fact]
    public void AddMember_NewMember_ShouldAddToMemberList()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var newMemberUID = "member-123";

        // Act
        religion.AddMember(newMemberUID);

        // Assert
        Assert.Equal(2, religion.MemberUIDs.Count);
        Assert.Contains(newMemberUID, religion.MemberUIDs);
    }

    [Fact]
    public void AddMember_ExistingMember_ShouldNotDuplicate()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var memberUID = "member-123";
        religion.AddMember(memberUID);

        // Act
        religion.AddMember(memberUID); // Try to add again

        // Assert
        Assert.Equal(2, religion.MemberUIDs.Count);
        Assert.Single(religion.MemberUIDs, m => m == memberUID);
    }

    [Fact]
    public void RemoveMember_ExistingMember_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var memberUID = "member-123";
        religion.AddMember(memberUID);

        // Act
        bool result = religion.RemoveMember(memberUID);

        // Assert
        Assert.True(result);
        Assert.Single(religion.MemberUIDs);
        Assert.DoesNotContain(memberUID, religion.MemberUIDs);
    }

    [Fact]
    public void RemoveMember_NonExistingMember_ShouldReturnFalse()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act
        bool result = religion.RemoveMember("non-existing-member");

        // Assert
        Assert.False(result);
        Assert.Single(religion.MemberUIDs);
    }

    [Fact]
    public void IsMember_ExistingMember_ShouldReturnTrue()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var memberUID = "member-123";
        religion.AddMember(memberUID);

        // Act
        bool result = religion.IsMember(memberUID);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsMember_NonExistingMember_ShouldReturnFalse()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act
        bool result = religion.IsMember("non-existing-member");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsMember_Founder_ShouldReturnTrue()
    {
        // Arrange
        var founderUID = "founder-123";
        var religion = new ReligionData("uid", "name", DeityType.Khoras, founderUID);

        // Act
        bool result = religion.IsMember(founderUID);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetMemberCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        religion.AddMember("member-1");
        religion.AddMember("member-2");
        religion.AddMember("member-3");

        // Act
        int count = religion.GetMemberCount();

        // Assert
        Assert.Equal(4, count); // Founder + 3 members
    }

    #endregion

    #region Founder Tests

    [Fact]
    public void IsFounder_Founder_ShouldReturnTrue()
    {
        // Arrange
        var founderUID = "founder-123";
        var religion = new ReligionData("uid", "name", DeityType.Khoras, founderUID);

        // Act
        bool result = religion.IsFounder(founderUID);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsFounder_RegularMember_ShouldReturnFalse()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var memberUID = "member-123";
        religion.AddMember(memberUID);

        // Act
        bool result = religion.IsFounder(memberUID);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsFounder_NonMember_ShouldReturnFalse()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act
        bool result = religion.IsFounder("random-player");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Prestige Tests

    [Fact]
    public void AddPrestige_PositiveAmount_ShouldIncreasePrestige()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act
        religion.AddPrestige(100);

        // Assert
        Assert.Equal(100, religion.Prestige);
        Assert.Equal(100, religion.TotalPrestige);
    }

    [Fact]
    public void AddPrestige_MultipleAdditions_ShouldAccumulate()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act
        religion.AddPrestige(100);
        religion.AddPrestige(200);
        religion.AddPrestige(150);

        // Assert
        Assert.Equal(450, religion.Prestige);
        Assert.Equal(450, religion.TotalPrestige);
    }

    [Fact]
    public void AddPrestige_ZeroAmount_ShouldNotChange()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        religion.AddPrestige(100);

        // Act
        religion.AddPrestige(0);

        // Assert
        Assert.Equal(100, religion.Prestige);
        Assert.Equal(100, religion.TotalPrestige);
    }

    [Fact]
    public void AddPrestige_NegativeAmount_ShouldNotChange()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        religion.AddPrestige(100);

        // Act
        religion.AddPrestige(-50);

        // Assert
        Assert.Equal(100, religion.Prestige);
        Assert.Equal(100, religion.TotalPrestige);
    }

    [Theory]
    [InlineData(0, PrestigeRank.Fledgling)]
    [InlineData(100, PrestigeRank.Fledgling)]
    [InlineData(499, PrestigeRank.Fledgling)]
    [InlineData(500, PrestigeRank.Established)]
    [InlineData(1000, PrestigeRank.Established)]
    [InlineData(1999, PrestigeRank.Established)]
    [InlineData(2000, PrestigeRank.Renowned)]
    [InlineData(3000, PrestigeRank.Renowned)]
    [InlineData(4999, PrestigeRank.Renowned)]
    [InlineData(5000, PrestigeRank.Legendary)]
    [InlineData(7500, PrestigeRank.Legendary)]
    [InlineData(9999, PrestigeRank.Legendary)]
    [InlineData(10000, PrestigeRank.Mythic)]
    [InlineData(15000, PrestigeRank.Mythic)]
    [InlineData(99999, PrestigeRank.Mythic)]
    public void UpdatePrestigeRank_ShouldSetCorrectRankBasedOnTotalPrestige(int totalPrestige, PrestigeRank expectedRank)
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        religion.AddPrestige(totalPrestige);

        // Act
        religion.UpdatePrestigeRank();

        // Assert
        Assert.Equal(expectedRank, religion.PrestigeRank);
    }

    [Fact]
    public void AddPrestige_ShouldAutomaticallyUpdateRank()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act
        religion.AddPrestige(2500);

        // Assert
        Assert.Equal(PrestigeRank.Renowned, religion.PrestigeRank);
    }

    #endregion

    #region Perk Tests

    [Fact]
    public void UnlockPerk_NewPerk_ShouldAddToUnlockedPerks()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var perkId = "test-perk-1";

        // Act
        religion.UnlockPerk(perkId);

        // Assert
        Assert.Single(religion.UnlockedPerks);
        Assert.True(religion.UnlockedPerks[perkId]);
    }

    [Fact]
    public void UnlockPerk_MultipleDifferentPerks_ShouldAddAll()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act
        religion.UnlockPerk("perk-1");
        religion.UnlockPerk("perk-2");
        religion.UnlockPerk("perk-3");

        // Assert
        Assert.Equal(3, religion.UnlockedPerks.Count);
        Assert.True(religion.UnlockedPerks["perk-1"]);
        Assert.True(religion.UnlockedPerks["perk-2"]);
        Assert.True(religion.UnlockedPerks["perk-3"]);
    }

    [Fact]
    public void UnlockPerk_SamePerkTwice_ShouldRemainUnlocked()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var perkId = "test-perk";

        // Act
        religion.UnlockPerk(perkId);
        religion.UnlockPerk(perkId); // Unlock again

        // Assert
        Assert.Single(religion.UnlockedPerks);
        Assert.True(religion.UnlockedPerks[perkId]);
    }

    [Fact]
    public void IsPerkUnlocked_UnlockedPerk_ShouldReturnTrue()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var perkId = "test-perk";
        religion.UnlockPerk(perkId);

        // Act
        bool result = religion.IsPerkUnlocked(perkId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsPerkUnlocked_LockedPerk_ShouldReturnFalse()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act
        bool result = religion.IsPerkUnlocked("non-existent-perk");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsPerkUnlocked_PerkSetToFalse_ShouldReturnFalse()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");
        var perkId = "test-perk";
        religion.UnlockedPerks[perkId] = false;

        // Act
        bool result = religion.IsPerkUnlocked(perkId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CompleteWorkflow_CreateReligionAddMembersAndPerks_ShouldWork()
    {
        // Arrange
        var founderUID = "founder-123";
        var religion = new ReligionData("religion-1", "Divine Order", DeityType.Khoras, founderUID);

        // Act - Add members
        religion.AddMember("member-1");
        religion.AddMember("member-2");
        religion.AddMember("member-3");

        // Act - Gain prestige
        religion.AddPrestige(600); // Should reach Established rank

        // Act - Unlock perks
        religion.UnlockPerk("perk-1");
        religion.UnlockPerk("perk-2");

        // Assert - Verify everything
        Assert.Equal(4, religion.GetMemberCount());
        Assert.Equal(PrestigeRank.Established, religion.PrestigeRank);
        Assert.Equal(2, religion.UnlockedPerks.Count);
        Assert.True(religion.IsFounder(founderUID));
        Assert.True(religion.IsMember("member-1"));
        Assert.True(religion.IsPerkUnlocked("perk-1"));
    }

    [Fact]
    public void PrestigeProgression_FromFledglingToMythic_ShouldUpdateRanks()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act & Assert - Progress through all ranks
        Assert.Equal(PrestigeRank.Fledgling, religion.PrestigeRank);

        religion.AddPrestige(500);
        Assert.Equal(PrestigeRank.Established, religion.PrestigeRank);

        religion.AddPrestige(1500); // Total: 2000
        Assert.Equal(PrestigeRank.Renowned, religion.PrestigeRank);

        religion.AddPrestige(3000); // Total: 5000
        Assert.Equal(PrestigeRank.Legendary, religion.PrestigeRank);

        religion.AddPrestige(5000); // Total: 10000
        Assert.Equal(PrestigeRank.Mythic, religion.PrestigeRank);

        Assert.Equal(10000, religion.TotalPrestige);
    }

    [Fact]
    public void MemberManagement_AddRemoveMultiple_ShouldMaintainCorrectState()
    {
        // Arrange
        var religion = new ReligionData("uid", "name", DeityType.Khoras, "founder");

        // Act - Add multiple members
        religion.AddMember("member-1");
        religion.AddMember("member-2");
        religion.AddMember("member-3");
        religion.AddMember("member-4");
        Assert.Equal(5, religion.GetMemberCount());

        // Act - Remove some members
        religion.RemoveMember("member-2");
        Assert.Equal(4, religion.GetMemberCount());
        Assert.False(religion.IsMember("member-2"));

        religion.RemoveMember("member-4");
        Assert.Equal(3, religion.GetMemberCount());

        // Assert - Verify remaining members
        Assert.True(religion.IsMember("founder"));
        Assert.True(religion.IsMember("member-1"));
        Assert.True(religion.IsMember("member-3"));
        Assert.False(religion.IsMember("member-2"));
        Assert.False(religion.IsMember("member-4"));
    }

    #endregion
}
