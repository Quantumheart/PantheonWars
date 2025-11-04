using System.Diagnostics.CodeAnalysis;
using PantheonWars.GUI;
using PantheonWars.Models.Enum;

namespace PantheonWars.Tests.Systems
{
    [ExcludeFromCodeCoverage]
    public class PerkDialogManagerTests
    {
        [Fact]
        public void TestPropertyInitialization()
        {
            var manager = new PerkDialogManager(null);
            Assert.Null(manager.CurrentReligionUID);
            Assert.Equal(DeityType.None, manager.CurrentDeity);
            Assert.Null(manager.CurrentReligionName);
            Assert.Null(manager.SelectedPerkId);
            Assert.Null(manager.HoveringPerkId);
            Assert.Equal(0f, manager.PlayerTreeScrollX);
            Assert.Equal(0f, manager.PlayerTreeScrollY);
            Assert.Equal(0f, manager.ReligionTreeScrollX);
            Assert.Equal(0f, manager.ReligionTreeScrollY);
            Assert.False(manager.IsDataLoaded);
        }

        [Fact]
        public void TestInitializeMethod()
        {
            var manager = new PerkDialogManager(null);
            manager.Initialize("religion123", DeityType.Khoras, "God of Warriors");
            Assert.Equal("religion123", manager.CurrentReligionUID);
            Assert.Equal(DeityType.Khoras, manager.CurrentDeity);
            Assert.Equal("God of Warriors", manager.CurrentReligionName);
            Assert.True(manager.IsDataLoaded);
            Assert.Null(manager.SelectedPerkId);
            Assert.Null(manager.HoveringPerkId);
            Assert.Equal(0f, manager.PlayerTreeScrollX);
            Assert.Equal(0f, manager.PlayerTreeScrollY);
            Assert.Equal(0f, manager.ReligionTreeScrollX);
            Assert.Equal(0f, manager.ReligionTreeScrollY);
        }

        [Fact]
        public void TestResetMethod()
        {
            var manager = new PerkDialogManager(null);
            manager.Initialize("religion123", DeityType.Khoras, "God of Warriors");
            manager.Reset();
            Assert.Null(manager.CurrentReligionUID);
            Assert.Equal(DeityType.None, manager.CurrentDeity);
            Assert.Null(manager.CurrentReligionName);
            Assert.Null(manager.SelectedPerkId);
            Assert.Null(manager.HoveringPerkId);
            Assert.Equal(0f, manager.PlayerTreeScrollX);
            Assert.Equal(0f, manager.PlayerTreeScrollY);
            Assert.Equal(0f, manager.ReligionTreeScrollX);
            Assert.Equal(0f, manager.ReligionTreeScrollY);
            Assert.False(manager.IsDataLoaded);
        }

        [Fact]
        public void TestSelectPerk()
        {
            var manager = new PerkDialogManager(null);
            manager.SelectPerk("perk456");
            Assert.Equal("perk456", manager.SelectedPerkId);
        }

        [Fact]
        public void TestClearSelection()
        {
            var manager = new PerkDialogManager(null);
            manager.SelectPerk("perk456");
            manager.ClearSelection();
            Assert.Null(manager.SelectedPerkId);
        }

        [Fact]
        public void TestHasReligion()
        {
            var manager = new PerkDialogManager(null);
            Assert.False(manager.HasReligion());

            manager.Initialize("religion123", DeityType.Khoras, "God of Warriors");
            Assert.True(manager.HasReligion());

            manager.Reset();
            Assert.False(manager.HasReligion());
        }
    }
}
