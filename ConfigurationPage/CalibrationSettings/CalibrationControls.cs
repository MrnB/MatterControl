﻿using System;
using MatterHackers.Agg;
using MatterHackers.Agg.Platform;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.ConfigurationPage.PrintLeveling;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.PrinterCommunication;
using MatterHackers.MatterControl.SlicerConfiguration;

namespace MatterHackers.MatterControl.PrinterControls
{
	public class CalibrationControls : FlowLayoutWidget
	{
		private EventHandler unregisterEvents;
		private GuiWidget configureLevelingButton;

		private TextImageButtonFactory buttonFactory;
		private PrinterConfig printer;

		private CalibrationControls(PrinterConfig printer, ThemeConfig theme)
			: base(FlowDirection.TopToBottom)
		{
			this.printer = printer;
			this.buttonFactory = theme.ButtonFactory;

			// add in the controls for configuring auto leveling
			{
				var autoLevelRow = new FlowLayoutWidget()
				{
					Name = "AutoLevelRowItem",
					HAnchor = HAnchor.Stretch,
				};

				autoLevelRow.AddChild(
					new IconButton(AggContext.StaticData.LoadIcon("leveling_32x32.png", 24, 24, IconColor.Theme), theme)
					{
						Margin = new BorderDouble(right: 6),
						VAnchor = VAnchor.Center
					});

				// label
				autoLevelRow.AddChild(
					new TextWidget("Software Print Leveling".Localize(), textColor: theme.Colors.PrimaryTextColor, pointSize: theme.DefaultFontSize)
					{
						AutoExpandBoundsToText = true,
						VAnchor = VAnchor.Center,
					});

				autoLevelRow.AddChild(new HorizontalSpacer());

				// configure button
				var configureIcon = AggContext.StaticData.LoadIcon("fa-cog_16.png", IconColor.Theme);
				configureLevelingButton = new IconButton(configureIcon, theme)
				{
					ToolTipText = "Configure".Localize(),
					Margin = theme.ButtonSpacing,
					VAnchor = VAnchor.Center
				};
				configureLevelingButton.Click += (s, e) =>
				{
					UiThread.RunOnIdle(() =>
					{
						DialogWindow.Show(new EditLevelingSettingsPage(printer));
					});
				};
				autoLevelRow.AddChild(configureLevelingButton);

				// put in the switch
				CheckBox printLevelingSwitch = ImageButtonFactory.CreateToggleSwitch(printer.Settings.GetValue<bool>(SettingsKey.print_leveling_enabled));
				printLevelingSwitch.VAnchor = VAnchor.Center;
				printLevelingSwitch.Margin = new BorderDouble(left: 16);
				printLevelingSwitch.CheckedStateChanged += (sender, e) =>
				{
					printer.Settings.Helpers.DoPrintLeveling(printLevelingSwitch.Checked);
				};

				printer.Settings.PrintLevelingEnabledChanged.RegisterEvent((sender, e) =>
				{
					printLevelingSwitch.Checked = printer.Settings.GetValue<bool>(SettingsKey.print_leveling_enabled);
				}, ref unregisterEvents);

				// only show the switch if leveling can be turned off (it can't if it is required).
				if (!printer.Settings.GetValue<bool>(SettingsKey.print_leveling_required_to_print))
				{
					autoLevelRow.AddChild(printLevelingSwitch);
				}

				this.AddChild(autoLevelRow);
			}

			printer.Connection.CommunicationStateChanged.RegisterEvent(PrinterStatusChanged, ref unregisterEvents);
			printer.Connection.EnableChanged.RegisterEvent(PrinterStatusChanged, ref unregisterEvents);

			SetVisibleControls();
		}

		public static SectionWidget CreateSection(PrinterConfig printer, ThemeConfig theme)
		{
			return new SectionWidget(
				"Calibration".Localize(),
				new CalibrationControls(printer, theme),
				theme);
		}

		public override void OnClosed(ClosedEventArgs e)
		{
			unregisterEvents?.Invoke(this, null);
			base.OnClosed(e);
		}

		private void PrinterStatusChanged(object sender, EventArgs e)
		{
			SetVisibleControls();
			this.Invalidate();
		}

		private void SetVisibleControls()
		{
			if (!printer.Settings.PrinterSelected
				|| printer.Connection.CommunicationState == CommunicationStates.Printing
				|| printer.Connection.PrinterIsPaused)
			{
				this.Enabled = false;
				configureLevelingButton.Enabled = true; // setting this true when the element is disabled makes the colors stay correct
			}
			else
			{
				this.Enabled = true;
				configureLevelingButton.Enabled = printer.Connection.IsConnected;
			}
		}
	}
}