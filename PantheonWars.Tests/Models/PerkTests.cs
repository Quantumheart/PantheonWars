using System.Diagnostics.CodeAnalysis;
using PantheonWars.Models;
using PantheonWars.Models.Enum;

namespace PantheonWars.Tests.Models
{
    [ExcludeFromCodeCoverage]
    public class PerkTests
    {
        [Fact]
        public void TestParameterizedConstructor()
        {
            var perk = new Perk("khoras_warriors_resolve", "Warrior's Resolve", DeityType.Khoras);
            Assert.Equal("khoras_warriors_resolve", perk.PerkId);
            Assert.Equal("Warrior's Resolve", perk.Name);
            Assert.Equal(DeityType.Khoras, perk.Deity);
            Assert.Equal(PerkKind.Player, perk.Kind);
            Assert.Equal(PerkCategory.Combat, perk.Category);
            Assert.Equal(0, perk.RequiredFavorRank);
            Assert.Equal(0, perk.RequiredPrestigeRank);
            Assert.Empty(perk.PrerequisitePerks);
            Assert.Empty(perk.StatModifiers);
            Assert.Empty(perk.SpecialEffects);
        }

        [Fact]
        public void TestParameterlessConstructor()
        {
            var perk = new Perk();
            Assert.Empty(perk.PerkId);
            Assert.Empty(perk.Name);
            Assert.Equal(DeityType.None, perk.Deity);
            Assert.Equal(0, perk.RequiredFavorRank);
            Assert.Equal(0, perk.RequiredPrestigeRank);
            Assert.Empty(perk.PrerequisitePerks);
            Assert.Empty(perk.StatModifiers);
            Assert.Empty(perk.SpecialEffects);
        }

        [Fact]
        public void TestCollectionProperties()
        {
            var perk = new Perk("test-perk", "Test Perk", DeityType.None);
            perk.StatModifiers["walkspeed"] = 0.2f;
            perk.SpecialEffects.Add("effect1");
            Assert.Equal(0.2f, perk.StatModifiers["walkspeed"]);
            Assert.Contains("effect1", perk.SpecialEffects);
        }

        [Fact]
        public void TestEnumValues()
        {
            var perk = new Perk("test-perk", "Test Perk", DeityType.None);
            Assert.Equal(PerkKind.Player, perk.Kind);
            Assert.Equal(PerkCategory.Combat, perk.Category);
            Assert.Equal(DeityType.None, perk.Deity);
        }

        [Fact]
        public void TestPropertyValidation()
        {
            var perk = new Perk("test-perk", "Test Perk", DeityType.None);
            Assert.False(string.IsNullOrEmpty(perk.PerkId));
            Assert.False(string.IsNullOrEmpty(perk.Name));
            Assert.Equal(DeityType.None, perk.Deity);
        }
    }
}
