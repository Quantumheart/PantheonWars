using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace PantheonWars.GUI
{
    /// <summary>
    /// HUD element that displays player's current deity, favor, and religion information (Phase 3.2)
    /// </summary>
    public class FavorHudElement : HudElement
    {
        private string _deityName = "None";
        private string _religionName = "None";
        private int _currentFavor = 0;
        private string _favorRank = "Initiate";
        private string _prestigeRank = "Fledgling";
        private int _currentPrestige = 0;

        public FavorHudElement(ICoreClientAPI capi) : base(capi)
        {
            // Position in top-right area, below other HUD elements
            var bounds = ElementStdBounds.Statbar(EnumDialogArea.RightTop, 100)
                .WithFixedOffset(0, 60);

            SetupDialog(bounds);
        }

        private void SetupDialog(ElementBounds dialogBounds)
        {
            // Expanded height to accommodate religion info
            var insetBounds = ElementBounds.Fixed(0, 0, 250, 130);

            SingleComposer = capi.Gui
                .CreateCompo("pantheonwars_favorhud", dialogBounds)
                .AddInset(insetBounds, 2)
                .AddStaticText("Religion: None", CairoFont.WhiteSmallText().WithFontSize(12),
                    ElementBounds.Fixed(10, 10, 230, 15), "religionText")
                .AddStaticText("Deity: None", CairoFont.WhiteSmallText().WithFontSize(11),
                    ElementBounds.Fixed(10, 30, 230, 15), "deityText")
                .AddStaticText("Player Favor: 0", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, 50, 230, 15), "favorText")
                .AddStaticText("Favor Rank: Initiate", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, 70, 230, 15), "favorRankText")
                .AddStaticText("Religion Prestige: 0", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, 90, 230, 15), "prestigeText")
                .AddStaticText("Prestige Rank: Fledgling", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, 110, 230, 15), "prestigeRankText")
                .Compose();
        }

        /// <summary>
        /// Updates the displayed information (legacy method for old system)
        /// </summary>
        public void UpdateDisplay(string deityName, int favor, string rank)
        {
            _deityName = deityName;
            _currentFavor = favor;
            _favorRank = rank;

            var deityText = SingleComposer.GetStaticText("deityText");
            var favorText = SingleComposer.GetStaticText("favorText");
            var favorRankText = SingleComposer.GetStaticText("favorRankText");

            if (deityText != null)
            {
                deityText.SetValue($"Deity: {_deityName}");
            }

            if (favorText != null)
            {
                favorText.SetValue($"Player Favor: {_currentFavor}");
            }

            if (favorRankText != null)
            {
                favorRankText.SetValue($"Favor Rank: {_favorRank}");
            }
        }

        /// <summary>
        /// Updates the displayed information with religion data (Phase 3.2)
        /// </summary>
        public void UpdateReligionDisplay(string religionName, string deityName, int favor, string favorRank, int prestige, string prestigeRank)
        {
            _religionName = religionName;
            _deityName = deityName;
            _currentFavor = favor;
            _favorRank = favorRank;
            _currentPrestige = prestige;
            _prestigeRank = prestigeRank;

            var religionText = SingleComposer.GetStaticText("religionText");
            var deityText = SingleComposer.GetStaticText("deityText");
            var favorText = SingleComposer.GetStaticText("favorText");
            var favorRankText = SingleComposer.GetStaticText("favorRankText");
            var prestigeText = SingleComposer.GetStaticText("prestigeText");
            var prestigeRankText = SingleComposer.GetStaticText("prestigeRankText");

            if (religionText != null)
            {
                religionText.SetValue($"Religion: {_religionName}");
            }

            if (deityText != null)
            {
                deityText.SetValue($"Deity: {_deityName}");
            }

            if (favorText != null)
            {
                favorText.SetValue($"Player Favor: {_currentFavor}");
            }

            if (favorRankText != null)
            {
                favorRankText.SetValue($"Favor Rank: {_favorRank}");
            }

            if (prestigeText != null)
            {
                prestigeText.SetValue($"Religion Prestige: {_currentPrestige}");
            }

            if (prestigeRankText != null)
            {
                prestigeRankText.SetValue($"Prestige Rank: {_prestigeRank}");
            }
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();
            // Request initial data from server when needed
        }
    }
}
