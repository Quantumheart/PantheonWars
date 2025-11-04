using System.Diagnostics.CodeAnalysis;
using PantheonWars.Data;

namespace PantheonWars.Tests.Data
{
    [ExcludeFromCodeCoverage]
    public class PlayerAbilityDataTests
    {
        [Fact]
        public void IsOnCooldown_ReturnsFalse_WhenAbilityNotInCooldowns()
        {
            var data = new PlayerAbilityData();
            var result = data.IsOnCooldown("testAbility", 10f);
            Assert.False(result);
        }

        [Fact]
        public void IsOnCooldown_ReturnsTrue_WhenAbilityIsOnCooldown()
        {
            var data = new PlayerAbilityData();
            data.StartCooldown("testAbility");
            var result = data.IsOnCooldown("testAbility", 10f);
            Assert.True(result);
        }

        [Fact]
        public void IsOnCooldown_ReturnsFalse_WhenCooldownExpired()
        {
            var data = new PlayerAbilityData();
            data.StartCooldown("testAbility");
            System.Threading.Thread.Sleep(1000); // Simulate time passing
            var result = data.IsOnCooldown("testAbility", 1f);
            Assert.False(result);
        }

        [Fact]
        public void GetRemainingCooldown_ReturnsZero_WhenAbilityNotInCooldowns()
        {
            var data = new PlayerAbilityData();
            var result = data.GetRemainingCooldown("testAbility", 10f);
            Assert.Equal(0f, result);
        }

        [Fact]
        public void GetRemainingCooldown_ReturnsCorrectValue_WhenOnCooldown()
        {
            var data = new PlayerAbilityData();
            data.StartCooldown("testAbility");
            var result = data.GetRemainingCooldown("testAbility", 10f);
            Assert.InRange(result, 9f, 10f);
        }

        [Fact]
        public void GetRemainingCooldown_ReturnsZero_WhenCooldownExpired()
        {
            var data = new PlayerAbilityData();
            data.StartCooldown("testAbility");
            Thread.Sleep(1000); // Simulate time passing
            var result = data.GetRemainingCooldown("testAbility", 1f);
            Assert.Equal(0f, result);
        }

        [Fact]
        public void StartCooldown_AddsAbilityToCooldowns()
        {
            var data = new PlayerAbilityData();
            data.StartCooldown("testAbility");
            Assert.Contains("testAbility", data.AbilityCooldowns.Keys);
        }

        [Fact]
        public void ClearCooldown_RemovesAbilityFromCooldowns()
        {
            var data = new PlayerAbilityData();
            data.StartCooldown("testAbility");
            data.ClearCooldown("testAbility");
            Assert.DoesNotContain("testAbility", data.AbilityCooldowns.Keys);
        }

        [Fact]
        public void ClearAllCooldowns_ClearsAllCooldowns()
        {
            var data = new PlayerAbilityData();
            data.StartCooldown("testAbility1");
            data.StartCooldown("testAbility2");
            data.ClearAllCooldowns();
            Assert.Empty(data.AbilityCooldowns);
        }

        [Fact]
        public void CleanupExpiredCooldowns_RemovesOldCooldowns()
        {
            var data = new PlayerAbilityData();
            data.StartCooldown("testAbility1");
            data.StartCooldown("testAbility2");
            data.CleanupExpiredCooldowns(0f);
            Assert.Empty(data.AbilityCooldowns);
        }
    }
}
