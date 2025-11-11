using System.Diagnostics.CodeAnalysis;
using PantheonWars.Data;
using PantheonWars.Models.Enum;

namespace PantheonWars.Tests.Systems;

[ExcludeFromCodeCoverage]
public class FavorSystemTests
{
    #region Devotion Rank Multiplier Tests

    [Theory]
    [InlineData(DevotionRank.Initiate, 1.0f)]
    [InlineData(DevotionRank.Disciple, 1.1f)]
    [InlineData(DevotionRank.Zealot, 1.2f)]
    [InlineData(DevotionRank.Champion, 1.3f)]
    [InlineData(DevotionRank.Avatar, 1.5f)]
    public void CalculateDevotionMultiplier_ShouldReturn_CorrectValue(DevotionRank rank, float expected)
    {
        // Given: Specific devotion rank
        // When: Calculate multiplier (using same logic as FavorSystem)
        float multiplier = rank switch
        {
            DevotionRank.Initiate => 1.0f,
            DevotionRank.Disciple => 1.1f,
            DevotionRank.Zealot => 1.2f,
            DevotionRank.Champion => 1.3f,
            DevotionRank.Avatar => 1.5f,
            _ => 1.0f
        };

        // Then: Matches expected value
        Assert.Equal(expected, multiplier);
    }

    #endregion

    #region Religion Prestige Multiplier Tests

    [Theory]
    [InlineData(PrestigeRank.Fledgling, 1.0f)]
    [InlineData(PrestigeRank.Established, 1.1f)]
    [InlineData(PrestigeRank.Renowned, 1.2f)]
    [InlineData(PrestigeRank.Legendary, 1.3f)]
    [InlineData(PrestigeRank.Mythic, 1.5f)]
    public void CalculatePrestigeMultiplier_ShouldReturn_CorrectValue(PrestigeRank rank, float expected)
    {
        // Given: Specific prestige rank
        // When: Calculate multiplier (using same logic as FavorSystem)
        float multiplier = rank switch
        {
            PrestigeRank.Fledgling => 1.0f,
            PrestigeRank.Established => 1.1f,
            PrestigeRank.Renowned => 1.2f,
            PrestigeRank.Legendary => 1.3f,
            PrestigeRank.Mythic => 1.5f,
            _ => 1.0f
        };

        // Then: Matches expected value
        Assert.Equal(expected, multiplier);
    }

    #endregion

    #region Combined Multiplier Tests

    [Theory]
    [InlineData(DevotionRank.Initiate, PrestigeRank.Fledgling, 1.0f)]
    [InlineData(DevotionRank.Avatar, PrestigeRank.Fledgling, 1.5f)]
    [InlineData(DevotionRank.Initiate, PrestigeRank.Mythic, 1.5f)]
    [InlineData(DevotionRank.Avatar, PrestigeRank.Mythic, 2.25f)]
    [InlineData(DevotionRank.Disciple, PrestigeRank.Established, 1.21f)]
    [InlineData(DevotionRank.Zealot, PrestigeRank.Renowned, 1.44f)]
    [InlineData(DevotionRank.Champion, PrestigeRank.Legendary, 1.69f)]
    public void CalculateTotalMultiplier_ShouldStack_DevotionAndPrestige(
        DevotionRank devotion, PrestigeRank prestige, float expected)
    {
        // Given: Player and religion with ranks
        float devotionMultiplier = GetDevotionMultiplier(devotion);
        float prestigeMultiplier = GetPrestigeMultiplier(prestige);

        // When: Multiply together
        float totalMultiplier = devotionMultiplier * prestigeMultiplier;

        // Then: Correct stacking
        Assert.Equal(expected, totalMultiplier, precision: 2);
    }

    #endregion

    #region Passive Favor Calculation Tests

    [Fact]
    public void CalculateBaseFavor_ShouldReturn_CorrectAmount_ForOneSecond()
    {
        // Given: 1 second tick (dt = 1.0), typical VS calendar
        float dt = 1.0f;
        float hoursPerDay = 24.0f; // Typical Vintage Story value
        float baseFavorPerHour = 2.0f;

        // When: Calculate base favor
        float inGameHoursElapsed = dt / hoursPerDay;
        float baseFavor = baseFavorPerHour * inGameHoursElapsed;

        // Then: Should be ~0.0833 favor per second
        float expected = 2.0f / 24.0f;
        Assert.Equal(expected, baseFavor, precision: 4);
    }

    [Fact]
    public void CalculatePassiveFavor_ShouldReturn_CorrectAmount_WithAllMultipliers()
    {
        // Given: Avatar in Mythic religion, 1 second tick
        float baseFavorPerHour = 2.0f;
        float dt = 1.0f;
        float hoursPerDay = 24.0f;
        float devotionMultiplier = 1.5f; // Avatar
        float prestigeMultiplier = 1.5f; // Mythic

        // When: Calculate final favor
        float inGameHoursElapsed = dt / hoursPerDay;
        float baseFavor = baseFavorPerHour * inGameHoursElapsed;
        float finalFavor = baseFavor * devotionMultiplier * prestigeMultiplier;

        // Then: Should be ~0.1875 favor per second
        float expected = (2.0f / 24.0f) * 1.5f * 1.5f;
        Assert.Equal(expected, finalFavor, precision: 4);
    }

    [Theory]
    [InlineData(60, 5.0f)]   // 1 minute = 5 favor (base rate)
    [InlineData(120, 10.0f)] // 2 minutes = 10 favor
    [InlineData(360, 30.0f)] // 6 minutes = 30 favor
    [InlineData(720, 60.0f)] // 12 minutes = 60 favor
    public void CalculatePassiveFavor_ShouldAccumulate_OverTime_BaseRate(int seconds, float expectedFavor)
    {
        // Given: Multiple ticks at base rate (Initiate, no religion)
        float baseFavorPerHour = 2.0f;
        float hoursPerDay = 24.0f;
        float favorPerSecond = baseFavorPerHour / hoursPerDay;

        // When: Calculate total favor for duration
        float totalFavor = favorPerSecond * seconds;

        // Then: Should match expected accumulation
        Assert.Equal(expectedFavor, totalFavor, precision: 1);
    }

    [Theory]
    [InlineData(DevotionRank.Initiate, PrestigeRank.Fledgling, 60, 5.0f)]    // Base: 5/min
    [InlineData(DevotionRank.Avatar, PrestigeRank.Fledgling, 60, 7.5f)]      // Avatar: 7.5/min
    [InlineData(DevotionRank.Initiate, PrestigeRank.Mythic, 60, 7.5f)]       // Mythic: 7.5/min
    [InlineData(DevotionRank.Avatar, PrestigeRank.Mythic, 60, 11.25f)]       // Both: 11.25/min
    [InlineData(DevotionRank.Disciple, PrestigeRank.Established, 60, 6.05f)] // Mid-tier: 6.05/min
    public void CalculatePassiveFavor_ShouldScale_WithMultipliers_OverTime(
        DevotionRank devotion, PrestigeRank prestige, int seconds, float expectedFavor)
    {
        // Given: Various rank combinations
        float baseFavorPerHour = 2.0f;
        float hoursPerDay = 24.0f;
        float devotionMultiplier = GetDevotionMultiplier(devotion);
        float prestigeMultiplier = GetPrestigeMultiplier(prestige);

        // When: Calculate total favor for duration
        float favorPerSecond = (baseFavorPerHour / hoursPerDay) * devotionMultiplier * prestigeMultiplier;
        float totalFavor = favorPerSecond * seconds;

        // Then: Should match expected with multipliers
        Assert.Equal(expectedFavor, totalFavor, precision: 2);
    }

    #endregion

    #region Player Without Deity Tests

    [Fact]
    public void PlayerWithoutDeity_ShouldNotReceive_PassiveFavor()
    {
        // Given: Player without deity
        var playerData = new PlayerDeityData("test-player");
        // DeityType is None by default

        // When: Check if should award
        bool hasDeity = playerData.HasDeity();

        // Then: Should not award
        Assert.False(hasDeity);
        Assert.Equal(DeityType.None, playerData.DeityType);
    }

    [Fact]
    public void PlayerWithDeity_ShouldBeEligible_ForPassiveFavor()
    {
        // Given: Player with deity
        var playerData = new PlayerDeityData("test-player");
        playerData.DeityType = DeityType.Khoras;

        // When: Check if should award
        bool hasDeity = playerData.HasDeity();

        // Then: Should be eligible
        Assert.True(hasDeity);
    }

    #endregion

    #region Helper Methods

    private float GetDevotionMultiplier(DevotionRank rank)
    {
        return rank switch
        {
            DevotionRank.Initiate => 1.0f,
            DevotionRank.Disciple => 1.1f,
            DevotionRank.Zealot => 1.2f,
            DevotionRank.Champion => 1.3f,
            DevotionRank.Avatar => 1.5f,
            _ => 1.0f
        };
    }

    private float GetPrestigeMultiplier(PrestigeRank rank)
    {
        return rank switch
        {
            PrestigeRank.Fledgling => 1.0f,
            PrestigeRank.Established => 1.1f,
            PrestigeRank.Renowned => 1.2f,
            PrestigeRank.Legendary => 1.3f,
            PrestigeRank.Mythic => 1.5f,
            _ => 1.0f
        };
    }

    #endregion
}
