using Avalonia.Platform.Storage;

using ModManager.Models.Mod;

using System;
using System.Collections.Generic;
using System.Text;

namespace ModManager.ViewModels.Settings;

public partial class IconSettingsViewModel : ReactiveObject
{
	public ModContainerIconSettings Settings { get; }

	[Reactive(SetModifier = AccessModifier.Private)] public partial ModContainerIconSettings? Target { get; private set; }

	[ReactiveCommand]
	public async Task BrowseForImage()
	{
		var dialog = AppServices.Dialog;
		var result = await dialog.OpenFileAsync(new OpenFileBrowserDialogRequest(
			"Open Icon Image...",
			dialog.GetInitialStartingDirectory(AppServices.Settings.ManagerSettings.LastImportDirectoryPath),
			CommonFileTypes.ImageFileTypes,
			multiSelect: false
		));
		if(result.Success)
		{
			Settings.Path = result.File;
		}
	}

	public void Open(ModContainerIconSettings target)
	{
		Settings.SetFrom(target);
		Target = target;
	}

	public void Clear()
	{
		Target = null;
		Settings.SetToDefault();
	}

	public IconSettingsViewModel()
	{
		Settings = new();
	}
}
