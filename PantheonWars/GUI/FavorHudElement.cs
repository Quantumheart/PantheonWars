using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace PantheonWars.GUI
{
    /// <summary>
    /// HUD element that displays player's current deity and favor
    /// </summary>
    public class FavorHudElement : HudElement
    {
        private string _deityName = "None";
        private int _currentFavor = 0;
        private string _devotionRank = "Initiate";

        public FavorHudElement(ICoreClientAPI capi) : base(capi)
        {
            // Position in top-right area, below other HUD elements
            var bounds = ElementStdBounds.Statbar(EnumDialogArea.RightTop, 100)
                .WithFixedOffset(0, 60);

            SetupDialog(bounds);
        }

        private void SetupDialog(ElementBounds bounds)
        {
            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            SingleComposer = capi.Gui
                .CreateCompo("pantheonwars_favorhud", bounds)
                .AddShadedDialogBG(bgBounds, false)
                .AddStaticText("Deity: None", CairoFont.WhiteSmallText(), ElementBounds.Fixed(5, 5, 200, 20), "deityText")
                .AddStaticText("Favor: 0", CairoFont.WhiteSmallText(), ElementBounds.Fixed(5, 25, 200, 20), "favorText")
                .AddStaticText("Rank: Initiate", CairoFont.WhiteSmallText(), ElementBounds.Fixed(5, 45, 200, 20), "rankText")
                .Compose();
        }

        /// <summary>
        /// Updates the displayed information
        /// </summary>
        public void UpdateDisplay(string deityName, int favor, string rank)
        {
            _deityName = deityName;
            _currentFavor = favor;
            _devotionRank = rank;

            var deityText = SingleComposer.GetStaticText("deityText");
            var favorText = SingleComposer.GetStaticText("favorText");
            var rankText = SingleComposer.GetStaticText("rankText");

            if (deityText != null)
            {
                deityText.SetValue($"Deity: {_deityName}");
            }

            if (favorText != null)
            {
                favorText.SetValue($"Favor: {_currentFavor}");
            }

            if (rankText != null)
            {
                rankText.SetValue($"Rank: {_devotionRank}");
            }
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();
            // Request initial data from server when needed
        }
    }
}
