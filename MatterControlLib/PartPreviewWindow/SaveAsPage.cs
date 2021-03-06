﻿/*
Copyright (c) 2017, Lars Brubaker, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System;
using MatterHackers.Agg;
using MatterHackers.Agg.Platform;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.Library;
using MatterHackers.VectorMath;

namespace MatterHackers.MatterControl
{
	public class SaveAsPage : LibraryBrowserPage
	{
		public SaveAsPage(Action<string, ILibraryContainer> itemSaver)
			: base(itemSaver, "Save".Localize())
		{
			this.WindowTitle = "MatterControl - " + "Save As".Localize();
			this.Name = "Save As Window";
			this.WindowSize = new Vector2(480 * GuiWidget.DeviceScale, 500 * GuiWidget.DeviceScale);
			this.HeaderText = "Save New Design".Localize() + ":";

			// put in the area to type in the new name
			var fileNameHeader = new TextWidget("Design Name".Localize(), pointSize: 12)
			{
				TextColor = theme.TextColor,
				Margin = new BorderDouble(5),
				HAnchor = HAnchor.Left
			};
			contentRow.AddChild(fileNameHeader);

			// Adds text box and check box to the above container
			itemNameWidget = new MHTextEditWidget("", theme, pixelWidth: 300, messageWhenEmptyAndNotSelected: "Enter a Design Name Here".Localize())
			{
				HAnchor = HAnchor.Stretch,
				Margin = new BorderDouble(5)
			};
			itemNameWidget.ActualTextEditWidget.EnterPressed += (s, e) =>
			{
				acceptButton.InvokeClick();
			};
			contentRow.AddChild(itemNameWidget);

			var icon = AggContext.StaticData.LoadIcon("fa-folder-new_16.png", 16, 16, ApplicationController.Instance.MenuTheme.InvertIcons);
			var isEnabled = false;
			if (librarySelectorWidget.ActiveContainer is ILibraryWritableContainer writableContainer)
			{
				isEnabled = writableContainer?.AllowAction(ContainerActions.AddContainers) == true;
			}

			var createFolderButton = new TextIconButton("Create Folder".Localize(), icon, theme)
			{
				Enabled = isEnabled,
				HAnchor = HAnchor.Left,
				VAnchor = VAnchor.Absolute,
				DrawIconOverlayOnDisabled = true
			};

			libraryNavContext.ContainerChanged += (s, e) =>
			{
				createFolderButton.Enabled = libraryNavContext.ActiveContainer is ILibraryWritableContainer;
			};

			createFolderButton.Name = "Create Folder In Button";
			contentRow.AddChild(createFolderButton);

			createFolderButton.Click += CreateFolder_Click;
		}

		private void CreateFolder_Click(object sender, MouseEventArgs e)
		{
			DialogWindow.Show(
				new InputBoxPage(
					"Create Folder".Localize(),
					"Folder Name".Localize(),
					"",
					"Enter New Name Here".Localize(),
					"Create".Localize(),
					(newName) =>
					{
						if (librarySelectorWidget.ActiveContainer is ILibraryWritableContainer writableContainer)
						{

							if (!string.IsNullOrEmpty(newName)
								&& writableContainer != null)
							{
								writableContainer.Add(new[]
								{
									new CreateFolderItem() { Name = newName }
								});
							}
						}
					}));
		}
	}
}