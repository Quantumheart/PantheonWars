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

        private void SetupDialog(ElementBounds dialogBounds)
        {
            var insetBounds = ElementBounds.Fixed(0, 0, 210, 75);

            SingleComposer = capi.Gui
                .CreateCompo("pantheonwars_favorhud", dialogBounds)
                .AddInset(insetBounds, 2)
                .AddStaticText("Deity: None", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, 10, 190, 15), "deityText")
                .AddStaticText("Favor: 0", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, 30, 190, 15), "favorText")
                .AddStaticText("Rank: Initiate", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, 50, 190, 15), "rankText")
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
