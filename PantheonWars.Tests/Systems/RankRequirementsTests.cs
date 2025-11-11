using PantheonWars.Systems;
using Xunit;

namespace PantheonWars.Tests.Systems;

public class RankRequirementsTests
{
    [Theory]
    [InlineData(0, 100)]  // Initiate → Devoted
    [InlineData(1, 250)]  // Devoted → Zealot
    [InlineData(2, 500)]  // Zealot → Champion
    [InlineData(3, 1000)] // Champion → Exalted
    [InlineData(4, 0)]    // Max rank
    public void GetRequiredFavorForNextRank_ReturnsCorrectValue(int currentRank, int expectedFavor)
    {
        // Act
        var required = RankRequirements.GetRequiredFavorForNextRank(currentRank);

        // Assert
        Assert.Equal(expectedFavor, required);
    }

    [Theory]
    [InlineData(0, 500)]   // Fledgling → Established
    [InlineData(1, 1500)]  // Established → Renowned
    [InlineData(2, 3500)]  // Renowned → Legendary
    [InlineData(3, 7500)]  // Legendary → Mythic
    [InlineData(4, 0)]     // Max rank
    public void GetRequiredPrestigeForNextRank_ReturnsCorrectValue(int currentRank, int expectedPrestige)
    {
        // Act
        var required = RankRequirements.GetRequiredPrestigeForNextRank(currentRank);

        // Assert
        Assert.Equal(expectedPrestige, required);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(10)]
    public void GetRequiredFavorForNextRank_WithInvalidRank_ReturnsZero(int invalidRank)
    {
        // Act
        var required = RankRequirements.GetRequiredFavorForNextRank(invalidRank);

        // Assert
        Assert.Equal(0, required);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(10)]
    public void GetRequiredPrestigeForNextRank_WithInvalidRank_ReturnsZero(int invalidRank)
    {
        // Act
        var required = RankRequirements.GetRequiredPrestigeForNextRank(invalidRank);

        // Assert
        Assert.Equal(0, required);
    }

    [Theory]
    [InlineData(0, "Initiate")]
    [InlineData(1, "Devoted")]
    [InlineData(2, "Zealot")]
    [InlineData(3, "Champion")]
    [InlineData(4, "Exalted")]
    public void GetFavorRankName_ReturnsCorrectName(int rank, string expectedName)
    {
        // Act
        var name = RankRequirements.GetFavorRankName(rank);

        // Assert
        Assert.Equal(expectedName, name);
    }

    [Theory]
    [InlineData(0, "Fledgling")]
    [InlineData(1, "Established")]
    [InlineData(2, "Renowned")]
    [InlineData(3, "Legendary")]
    [InlineData(4, "Mythic")]
    public void GetPrestigeRankName_ReturnsCorrectName(int rank, string expectedName)
    {
        // Act
        var name = RankRequirements.GetPrestigeRankName(rank);

        // Assert
        Assert.Equal(expectedName, name);
    }
}
