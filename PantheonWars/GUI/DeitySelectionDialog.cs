using System;
using System.Collections.Generic;
using System.Linq;
using PantheonWars.Models;
using PantheonWars.Systems;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace PantheonWars.GUI
{
    /// <summary>
    /// Dialog for selecting a deity to pledge to
    /// </summary>
    public class DeitySelectionDialog : GuiDialog
    {
        private readonly DeityRegistry _deityRegistry;
        private readonly Action<DeityType> _onDeitySelected;
        private DeityType _selectedDeity = DeityType.None;

        public override string ToggleKeyCombinationCode => null;

        public DeitySelectionDialog(ICoreClientAPI capi, DeityRegistry deityRegistry, Action<DeityType> onDeitySelected)
            : base(capi)
        {
            _deityRegistry = deityRegistry;
            _onDeitySelected = onDeitySelected;
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            ComposeDialog();
        }

        private void ComposeDialog()
        {
            var deities = _deityRegistry.GetAllDeities().ToList();

            // Calculate dialog dimensions
            double dialogWidth = 600;
            double dialogHeight = 400;

            var bgBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle)
                .WithFixedPadding(GuiStyle.ElementToDialogPadding);

            SingleComposer = capi.Gui
                .CreateCompo("deityselection", dialogBounds)
                .AddShadedDialogBG(bgBounds, true)
                .AddDialogTitleBar("Choose Your Deity", OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var composer = SingleComposer;

            // Introduction text
            var textBounds = ElementBounds.Fixed(0, 30, dialogWidth - 40, 40);
            composer.AddStaticText(
                "Pledge yourself to a deity and gain access to their divine powers. Choose wisely, for this decision shapes your path.",
                CairoFont.WhiteSmallText(),
                textBounds
            );

            // Deity list
            double yPos = 80;
            foreach (var deity in deities)
            {
                AddDeityOption(composer, deity, ref yPos, dialogWidth);
            }

            // Confirm button
            var buttonBounds = ElementBounds.Fixed(dialogWidth / 2 - 100, yPos + 20, 200, 30);
            composer.AddSmallButton("Confirm Selection", OnConfirmClicked, buttonBounds, EnumButtonStyle.Normal, "confirmButton");

            composer.EndChildElements().Compose();
        }

        private void AddDeityOption(GuiComposer composer, Deity deity, ref double yPos, double dialogWidth)
        {
            var bounds = ElementBounds.Fixed(10, yPos, dialogWidth - 60, 60);

            // Radio button
            var radioBounds = ElementBounds.Fixed(10, yPos + 15, 20, 20);
            composer.AddToggleButton("", CairoFont.WhiteSmallText(), OnDeityToggle, radioBounds, deity.Type.ToString().ToLower());

            // Deity name and domain
            var nameBounds = ElementBounds.Fixed(40, yPos, 200, 25);
            composer.AddStaticText($"{deity.Name} - {deity.Domain}", CairoFont.WhiteDetailText(), nameBounds);

            // Deity description
            var descBounds = ElementBounds.Fixed(40, yPos + 25, dialogWidth - 80, 30);
            composer.AddStaticText(deity.Description, CairoFont.WhiteSmallText(), descBounds);

            // Playstyle
            var styleBounds = ElementBounds.Fixed(250, yPos, dialogWidth - 280, 25);
            composer.AddStaticText($"Style: {deity.Playstyle}", CairoFont.WhiteSmallishText(), styleBounds);

            yPos += 70;
        }

        private void OnDeityToggle(bool on, string deityTypeStr)
        {
            if (on && Enum.TryParse<DeityType>(deityTypeStr, true, out var deityType))
            {
                _selectedDeity = deityType;

                // Uncheck all other toggles
                var deities = _deityRegistry.GetAllDeities();
                foreach (var deity in deities)
                {
                    if (deity.Type != deityType)
                    {
                        var toggle = SingleComposer.GetToggleButton(deity.Type.ToString().ToLower());
                        if (toggle != null)
                        {
                            toggle.SetValue(false);
                        }
                    }
                }
            }
            else if (!on)
            {
                _selectedDeity = DeityType.None;
            }
        }

        private bool OnConfirmClicked()
        {
            if (_selectedDeity == DeityType.None)
            {
                capi.ShowChatMessage("Please select a deity before confirming.");
                return true;
            }

            _onDeitySelected?.Invoke(_selectedDeity);
            TryClose();
            return true;
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }
    }
}
