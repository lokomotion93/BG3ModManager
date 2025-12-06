using ModManager.Models.Mod;
using ModManager.Util;

using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;

using TextCopy;

namespace ModManager.Services;

public partial class GlobalCommandsService : ReactiveObject, IGlobalCommandsService
{
	private readonly IInteractionsService _interactions;
	private readonly IFileSystemService _fs;

	[Reactive] public partial bool CanExecuteCommands { get; set; }
	[Reactive] public partial bool HasAnySelectedMods { get; set; }
	[Reactive] public partial bool HasMultipleSelectedMods { get; set; }

	private readonly IObservable<bool> _canExecuteCommandsObs;
	private readonly IObservable<bool> _canExecuteSelectedObs;

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	public void OpenFile(string? path)
	{
		if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path), "path is null or empty");

		path = _fs.GetRealPath(path);

		if (_fs.File.Exists(path))
		{
			if(!ProcessHelper.TryOpenPath(path))
			{
				OpenInFileExplorer(path);
			}
		}
		else if (_fs.Directory.Exists(path))
		{
			Process.Start("explorer.exe", $"\"{path}\"");
		}
		else
		{
			ShowAlert($"Error opening '{path}': File does not exist!", AlertType.Danger, 10);
		}
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	public bool OpenInFileExplorer(string? path)
	{
		if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path), "path is null or empty");

		if (_fs.File.Exists(path))
		{
			return ProcessHelper.TryRunCommand("explorer.exe", $"/select, \"{_fs.Path.GetFullPath(path)}\"");
		}
		else if (_fs.Directory.Exists(path))
		{
			return ProcessHelper.TryRunCommand("explorer.exe", $"\"{_fs.Path.GetFullPath(path)}\"");
		}
		else
		{
			ShowAlert($"Error opening '{path}': File does not exist!", AlertType.Danger, 10);
		}
		return false;
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	public void CopyToClipboard(object? obj)
	{
		if (obj == null) throw new ArgumentNullException(nameof(obj), "data to copy is null");
		try
		{
			if(obj is string text)
			{
				ClipboardService.SetText(text);
				ShowAlert($"Copied to clipboard: {text}", 0, 10);
			}
			else if(obj is Uri url)
			{
				ClipboardService.SetText(url.ToString());
				ShowAlert($"Copied url to clipboard: {url}", 0, 10);
			}
		}
		catch (Exception ex)
		{
			ShowAlert($"Error copying text to clipboard: {ex}", AlertType.Danger, 10);
		}
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private void CopyModAsDependency(ModData? mod)
	{
		if (mod == null) throw new ArgumentNullException(nameof(mod));
		try
		{
			var modExportService = Locator.Current.GetService<IModSettingsExportService>()!;
			var safeName = System.Security.SecurityElement.Escape(mod.Name);
			var text = modExportService.ToFormattedModuleShortDesc(mod);
			ClipboardService.SetText(text);
			ShowAlert($"Copied ModuleShortDesc for mod '{mod.Name}' to clipboard", 0, 10);
		}
		catch (Exception ex)
		{
			ShowAlert($"Error copying text to clipboard: {ex}", AlertType.Danger, 10);
		}
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	public void OpenURL(string? url)
	{
		
		if (!url.IsValid()) throw new ArgumentNullException(nameof(url));
		ProcessHelper.TryOpenUrl(url);
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private void OpenGitHubPage(ModData? mod)
	{
		ArgumentNullException.ThrowIfNull(mod);
		var url = mod.GetURL(ModSourceType.GITHUB);
		OpenURL(url);
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private void OpenNexusModsPage(ModData? mod)
	{
		ArgumentNullException.ThrowIfNull(mod);
		var url = mod.GetURL(ModSourceType.NEXUSMODS);
		OpenURL(url);
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private void OpenModioPage(ModData? mod)
	{
		ArgumentNullException.ThrowIfNull(mod);
		var url = mod.GetURL(ModSourceType.MODIO);
		OpenURL(url);
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private async Task ToggleForceAllowInLoadOrder(ModData? mod)
	{
		ArgumentNullException.ThrowIfNull(mod);
		var b = !mod.ForceAllowInLoadOrder;
		await _interactions.ForceAllowInLoadOrder.Handle(new(mod, b));
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private void OpenModProperties(ModData? mod)
	{
		ArgumentNullException.ThrowIfNull(mod);
		_interactions.OpenModProperties.Handle(mod).Subscribe();
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private void ValidateStats(ModData? mod)
	{
		ArgumentNullException.ThrowIfNull(mod);
		_interactions.ValidateModStats.Handle(new([mod], CancellationToken.None)).Subscribe();
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private void ToggleNameDisplay(object? obj)
	{
		ArgumentNullException.ThrowIfNull(obj);
		if (obj is ModEntry modEntry && modEntry.Data != null)
		{
			modEntry.Data.DisplayFileForName = !modEntry.Data.DisplayFileForName;
		}
		else
		{
			throw new ArgumentException($"Wrong parameter type: {obj}({obj?.GetType()})");
		}
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private void DeleteMod(object? obj)
	{
		ArgumentNullException.ThrowIfNull(obj);
		if (obj is IModEntry modEntry)
		{
			_interactions.DeleteMods.Handle(new([modEntry])).Subscribe();
		}
		else
		{
			throw new ArgumentException($"Wrong parameter type: {obj}({obj?.GetType()})");
		}
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteSelectedObs))]
	private async Task DeleteSelectedMods()
	{
		await _interactions.DeleteSelectedMods.Handle(Unit.Default);
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteCommandsObs))]
	private async Task ExploreModFiles(ModData? mod)
	{
		ArgumentNullException.ThrowIfNull(mod);
		await _interactions.ViewModFiles.Handle(new([mod]));
	}

	[ReactiveCommand(CanExecute = nameof(_canExecuteSelectedObs))]
	private async Task ExploreSelectedModFiles()
	{
		List<ModData> selectedMods = [];
		var modManager = Locator.Current.GetService<IModManagerService>();
		if (modManager != null)
		{
			selectedMods.AddRange(modManager.AllMods.Where(x => x.IsSelected));
		}
		if(selectedMods.Count > 0)
		{
			await _interactions.ViewModFiles.Handle(new(selectedMods));
		}
		else
		{
			throw new ArgumentException("No mods are selected");
		}
	}

	public void ShowAlert(string message, AlertType alertType = AlertType.Info, int timeout = 0, string? title = "")
	{
		_interactions.ShowAlert.Handle(new(StringUtils.ReplaceSpecialPathways(message)!, alertType, timeout, title)).Subscribe();
	}

	public async Task ShowAlertAsync(string message, AlertType alertType = AlertType.Info, int timeout = 0, string? title = "")
	{
		await _interactions.ShowAlert.Handle(new(StringUtils.ReplaceSpecialPathways(message)!, alertType, timeout, title));
	}

	public GlobalCommandsService(IInteractionsService interactionsService, IFileSystemService fileSystemService)
	{
		_interactions = interactionsService;
		_fs = fileSystemService;

		_canExecuteCommandsObs = this.WhenAnyValue(x => x.CanExecuteCommands).ObserveOn(RxApp.MainThreadScheduler);
		_canExecuteSelectedObs = this.WhenAnyValue(x => x.CanExecuteCommands, x => x.HasAnySelectedMods).AllTrue();
	}
}
