using DynamicData;
using DynamicData.Binding;

using ModManager.Locale;
using ModManager.Models;
using ModManager.Models.App;
using ModManager.Models.Menu;
using ModManager.Models.Mod;
using ModManager.Models.Settings;
using ModManager.Services;
using ModManager.Util;
using ModManager.ViewModels.Mods;
using ModManager.Views.Main;
using ModManager.Views.Mods;
using ModManager.Windows;

using System.Collections.ObjectModel;
using System.Reflection;

namespace ModManager.ViewModels.Main;

[Keybindings]
public partial class MainCommandBarViewModel : ReactiveObject
{
	[Keybinding(nameof(Resources.Keybinding_AddNewOrder), Key.M)]
	public RxCommandUnit? AddNewOrderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_CheckForAppUpdates), Key.U)]
	public RxCommandUnit? CheckForAppUpdatesCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_CheckAllModUpdates), Key.None, KeyModifiers.None, nameof(Resources.Keybinding_CheckAllModUpdates_ToolTip))]
	public RxCommandUnit? CheckAllModUpdatesCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_CheckForGitHubModUpdates), Key.None, KeyModifiers.None)]
	public RxCommandUnit? CheckForGitHubModUpdatesCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_CheckForNexusModsUpdates), Key.None, KeyModifiers.None)]
	public RxCommandUnit? CheckForNexusModsUpdatesCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_CheckForModioUpdates), Key.None, KeyModifiers.None)]
	public RxCommandUnit? CheckForModioUpdatesCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ExportOrder), Key.E, KeyModifiers.Control, nameof(Resources.Keybinding_ExportOrder_ToolTip))]
	public RxBoolCommandUnit? ExportOrderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ExportOrderToTextFile), Key.E, KeyModifiers.Control | KeyModifiers.Shift)]
	public RxCommandUnit? ExportOrderToTextFileCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ExportOrderToZip), Key.None, KeyModifiers.None, nameof(Resources.Keybinding_ExportOrderToZip_ToolTip))]
	public RxCommandUnit? ExportOrderToZipCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ExportOrderToArchiveAs), Key.None, KeyModifiers.None, nameof(Resources.Keybinding_ExportOrderToArchiveAs_ToolTip))]
	public RxCommandUnit? ExportOrderToArchiveAsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_RenameOrder), Key.R, KeyModifiers.Control | KeyModifiers.Shift, nameof(Resources.Keybinding_RenameOrder_ToolTip))]
	public RxCommandUnit? RenameOrderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_LaunchGame), Key.G, KeyModifiers.Control)]
	public RxCommandUnit? LaunchGameCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenDonationPage), Key.F10, KeyModifiers.None)]
	public RxCommandUnit? OpenDonationPageCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenGitHubRepo), Key.F11, KeyModifiers.None)]
	public RxCommandUnit? OpenGitHubRepoCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenModsFolder), Key.D1, KeyModifiers.Control)]
	public ReactiveCommand<string?, bool>? OpenModsFolderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenExtenderLogsFolder), Key.D2, KeyModifiers.Control)]
	public ReactiveCommand<string?, bool>? OpenLogsFolderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenNexusMods), Key.D3, KeyModifiers.Control)]
	public RxBoolCommandUnit? OpenNexusModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenSteamPage), Key.D4, KeyModifiers.Control)]
	public RxBoolCommandUnit? OpenSteamPageCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_RefreshMods), Key.F5, KeyModifiers.None)]
	public RxCommandUnit? RefreshCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ReloadMods), Key.F5, KeyModifiers.Shift, nameof(Resources.Keybinding_ReloadMods_ToolTip))]
	public RxCommandUnit? ReloadModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_RefreshModUpdates), Key.F6, KeyModifiers.None)]
	public RxCommandUnit? RefreshModUpdatesCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_RenameSave), Key.None, KeyModifiers.None)]
	public RxCommandUnit? RenameSaveCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_SaveOrder), Key.S, KeyModifiers.Control)]
	public RxBoolCommandUnit? SaveOrderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_SaveOrderAs), Key.S, KeyModifiers.Control | KeyModifiers.Alt)]
	public RxBoolCommandUnit? SaveOrderAsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_SaveSettings), Key.S, KeyModifiers.Control | KeyModifiers.Shift)]
	public RxCommandUnit? SaveSettingsSilentlyCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleUpdatesView), Key.U, KeyModifiers.Control | KeyModifiers.Alt)]
	public RxCommandUnit? ToggleUpdatesViewCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_TogglePakFileExplorerWindow), Key.P, KeyModifiers.Control | KeyModifiers.Alt)]
	public RxBoolCommandUnit? TogglePakFileExplorerWindowCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleStatsValidatorWindow), Key.OemBackslash, KeyModifiers.Control | KeyModifiers.Alt)]
	public RxBoolCommandUnit? ToggleStatsValidatorWindowCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleSettingsWindow), Key.OemComma, KeyModifiers.Control)]
	public RxBoolCommandUnit? ToggleSettingsWindowCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleKeybindings), Key.OemComma, KeyModifiers.Control | KeyModifiers.Alt)]
	public RxBoolCommandUnit? ToggleKeybindingsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleVersionGeneratorWindow), Key.G, KeyModifiers.Control, nameof(Resources.Keybinding_ToggleVersionGeneratorWindow_ToolTip))]
	public RxBoolCommandUnit? ToggleVersionGeneratorWindowCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleAboutWindow), Key.F1, KeyModifiers.None)]
	public RxBoolCommandUnit? ToggleAboutWindowCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleThemeMode), Key.OemComma, KeyModifiers.Control | KeyModifiers.Alt)]
	public RxCommandUnit? ToggleThemeModeCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ImportMods), Key.O, KeyModifiers.Control)]
	public RxCommandUnit? ImportModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ImportNexusModsIds), Key.None, KeyModifiers.None)]
	public RxCommandUnit? ImportNexusModsIdsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_AddNewOrder), Key.N, KeyModifiers.Control)]
	public RxCommandUnit? NewOrderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_SaveOrder), Key.S, KeyModifiers.Control)]
	public RxCommandUnit? SaveCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_SaveOrderAs), Key.S, KeyModifiers.Control | KeyModifiers.Alt)]
	public RxCommandUnit? SaveAsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ImportOrderFromSave), Key.I, KeyModifiers.Control)]
	public RxCommandUnit? ImportOrderFromSaveCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ImportOrderFromSaveAsNew), Key.I, KeyModifiers.Control | KeyModifiers.Shift)]
	public RxCommandUnit? ImportOrderFromSaveAsNewCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ImportOrderFromFile), Key.O, KeyModifiers.Control | KeyModifiers.Shift)]
	public RxCommandUnit? ImportOrderFromFileCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ImportOrderFromZipFile), Key.None, KeyModifiers.None)]
	public RxCommandUnit? ImportOrderFromZipFileCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_MoveSelectedMods), Key.Enter, KeyModifiers.None)]
	public RxCommandUnit? MoveSelectedModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_FocusActiveMods), Key.Left, KeyModifiers.None)]
	public RxCommandUnit? FocusActiveModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_FocusInactiveMods), Key.Right, KeyModifiers.None)]
	public RxCommandUnit? FocusInactiveModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_SwapListFocus), Key.Tab, KeyModifiers.None)]
	public RxCommandUnit? SwapListFocusCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_MoveToTop), Key.PageUp, KeyModifiers.Control)]
	public RxCommandUnit? MoveToTopCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_MoveToBottom), Key.PageDown, KeyModifiers.Control)]
	public RxCommandUnit? MoveToBottomCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleFilterFocus), Key.F, KeyModifiers.Control)]
	public RxCommandUnit? ToggleFilterFocusCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleFileNameDisplay), Key.None, KeyModifiers.None)]
	public RxCommandUnit? ToggleFileNameDisplayCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_DeleteSelectedMods), Key.Delete, KeyModifiers.None)]
	public RxCommandUnit? DeleteSelectedModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenGameFolder), Key.D2, KeyModifiers.Control)]
	public ReactiveCommand<string?, bool>? OpenGameFolderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenSavesFolder), Key.D3, KeyModifiers.Control)]
	public ReactiveCommand<string?, bool>? OpenSavesFolderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OpenExtenderDataFolder), Key.D4, KeyModifiers.Control)]
	public ReactiveCommand<string?, bool>? OpenExtenderDataFolderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_DownloadScriptExtender), Key.None, KeyModifiers.None)]
	public RxCommandUnit? DownloadScriptExtenderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleNXMLinkDownloader), Key.None, KeyModifiers.None, nameof(Resources.Keybinding_ToggleNXMLinkDownloader_ToolTip))]
	public RxBoolCommandUnit? ToggleNXMLinkDownloaderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ToggleCollectionDownloaderWindow), Key.None, KeyModifiers.None)]
	public RxBoolCommandUnit? ToggleCollectionDownloaderWindowCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ExtractAllSelectedMods), Key.None, KeyModifiers.None)]
	public RxCommandUnit? ExtractAllSelectedModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ExtractSelectedActiveMods), Key.None, KeyModifiers.None)]
	public RxCommandUnit? ExtractSelectedActiveModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ExtractSelectedInactiveMods), Key.None, KeyModifiers.None)]
	public RxCommandUnit? ExtractSelectedInactiveModsCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_ExtractSelectedAdventure), Key.None, KeyModifiers.None)]
	public RxCommandUnit? ExtractSelectedAdventureCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_SpeakActiveModOrder), Key.Home, KeyModifiers.Control)]
	public RxCommandUnit? SpeakActiveModOrderCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_StopSpeaking), Key.Home, KeyModifiers.Control | KeyModifiers.Shift)]
	public RxCommandUnit? StopSpeakingCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_CheckForUpdates), Key.F7, KeyModifiers.None)]
	public RxCommandUnit? CheckForUpdatesCommand { get; private set; }

	[Keybinding(nameof(Resources.Keybinding_OrganizeModPaks), Key.None, KeyModifiers.None, nameof(Resources.Keybinding_OrganizeModPaks_ToolTip))]
	public RxCommandUnit? OrganizeModPaksCommand { get; private set; }

	//Context Menu commands

	public ReactiveCommand<string?, bool>? OpenSelectedProfileFolderCommand { get; private set; }
	public RxCommandUnit? CopySelectedProfileFilePathToClipboardCommand { get; private set; }

	public ReactiveCommand<string?, bool>? OpenSelectedProfileSavesFolderCommand { get; private set; }
	public RxCommandUnit? CopySelectedProfileSavesPathToClipboardCommand { get; private set; }

	public ReactiveCommand<string?, bool>? OpenSelectedModOrderFilePathCommand { get; private set; }
	public RxCommandUnit? CopySelectedModOrderFilePathToClipboardCommand { get; private set; }
	public RxCommandUnit? CopyModsDirectoryPathToClipboardCommand { get; private set; }
	public RxCommandUnit? CopyExtenderLogsDirectoryPathToClipboardCommand { get; private set; }
	public RxCommandUnit? CopyGameExecutablePathToClipboardCommand { get; private set; }
	public RxCommandUnit? CopyGameFolderPathToClipboardCommand { get; private set; }
	public RxCommandUnit? EditExecutablePathCommand { get; private set; }

	private readonly ObservableCollectionExtended<IMenuEntry> _menuEntries = [];

	private readonly ReadOnlyObservableCollection<IMenuEntry> _uiMenuEntries;
	public ReadOnlyObservableCollection<IMenuEntry> MenuEntries => _uiMenuEntries;

	[Reactive] public IModOrderViewModel? ModOrder { get; protected set; }
	[Reactive] public bool HighlightExtenderDownload { get; private set; }
	[Reactive] public bool HasDopus { get; private set; }
	[Reactive] public bool IsDeveloperMode { get; private set; }
	[Reactive] public ModManagerSettings? Settings { get; private set; }

	public MainCommandBarViewModel()
	{
		_menuEntries.ToObservableChangeSet().Bind(out _uiMenuEntries).Subscribe();
	}

	private static bool ToggleWindow<T>(bool forceVisible = false) where T : Avalonia.Controls.Window
	{
		var window = AppServices.Get<T>();
		if(window != null)
		{
			if(forceVisible || !window.IsVisible)
			{
				RxApp.MainThreadScheduler.Schedule(() =>
				{
					if (!window.IsVisible) window.Show();
				});
				return true;
			}
			else
			{
				RxApp.MainThreadScheduler.Schedule(() =>
				{
					window.Hide();
				});
				return false;
			}
		}
		return false;
	}

	private static bool ToggleWindow<T>() where T : Avalonia.Controls.Window => ToggleWindow<T>(false);

	[Obsolete]
	private static void NotImplemented()
	{
		throw new NotImplementedException();
	}

	public void SetExtenderHighlight(bool enabled)
	{
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			HighlightExtenderDownload = enabled;
		});
	}

	private static bool OpenInExplorerOrOther(string? mode, string? path, bool createDirectory = false)
	{
		if(createDirectory && !path.IsExistingDirectory() && path.IsValid())
		{
			AppServices.FS.Directory.CreateDirectory(path);
		}
		if(mode == "dopus")
		{
			return AppServices.Dopus.OpenInDirectoryOpus(path);
		}
		else
		{
			return AppServices.Commands.OpenInFileExplorer(path);
		}
	}

	public MainCommandBarViewModel(MainWindowViewModel main, ModOrderViewModel modOrder, ModImportService modImporter, IFileSystemService fs, IDirectoryOpusService dopus, IInteractionsService interactions) : this()
	{
		ModOrder = modOrder;
		Settings = main.Settings;
		var canExecuteCommands = main.WhenAnyValue(x => x.IsLocked, b => !b);

		var isModOrderView = main.WhenAnyValue(x => x.Views.CurrentView, x => x == modOrder);
		var canExecuteModOrderCommands = canExecuteCommands.CombineLatest(isModOrderView).AllTrue();

		var hasActiveMods = modOrder.WhenAnyValue(x => x.TotalActiveMods, x => x > 0);
		var canExecuteHasActive = canExecuteModOrderCommands.CombineLatest(hasActiveMods).AllTrue();

		AddNewOrderCommand = ReactiveCommand.Create(() => modOrder.AddNewModOrder(), canExecuteModOrderCommands);

		CheckForAppUpdatesCommand = ReactiveCommand.Create(() =>
		{
			AppServices.Commands.ShowAlert("Checking for updates...", AlertType.Info, 30);
			//main.CheckForUpdates(true);
			main.SaveSettings();
		}, canExecuteCommands);

		var anyDownloadAllowed = modOrder.WhenAnyValue(x => x.GitHubModSupportEnabled, x => x.NexusModsSupportEnabled, x => x.ModioSupportEnabled).Select(x => x.Item1 || x.Item2 || x.Item3).ObserveOn(RxApp.MainThreadScheduler);

		CheckAllModUpdatesCommand = ReactiveCommand.Create(main.RefreshAllModUpdatesBackground, anyDownloadAllowed.AllTrue(canExecuteCommands));
		CheckForGitHubModUpdatesCommand = ReactiveCommand.Create(main.RefreshGitHubModsUpdatesBackground, modOrder.WhenAnyValue(x => x.GitHubModSupportEnabled).AllTrue(canExecuteCommands));
		CheckForNexusModsUpdatesCommand = ReactiveCommand.Create(main.RefreshNexusModsUpdatesBackground, modOrder.WhenAnyValue(x => x.NexusModsSupportEnabled).AllTrue(canExecuteCommands));
		CheckForModioUpdatesCommand = ReactiveCommand.Create(main.RefreshModioUpdatesBackground, modOrder.WhenAnyValue(x => x.ModioSupportEnabled).AllTrue(canExecuteCommands));

		ExportOrderCommand = ReactiveCommand.CreateFromTask(modOrder.ExportLoadOrderAsync, canExecuteCommands);

		LaunchGameCommand = ReactiveCommand.Create(main.LaunchGame, main.WhenAnyValue(x => x.CanLaunchGame).AllTrue(canExecuteCommands));

		OpenGitHubRepoCommand = ReactiveCommand.Create(() => AppServices.Commands.OpenURL(DivinityApp.URL_REPO), canExecuteCommands);
		OpenDonationPageCommand = ReactiveCommand.Create(() => AppServices.Commands.OpenURL(DivinityApp.URL_DONATION), canExecuteCommands);

		var canOpenModsDirectory = main.PathwayData.WhenAnyValue(x => x.AppDataModsPath, fs.Directory.Exists).CombineLatest(canExecuteCommands).AllTrue();
		var gameExecutableExists = main.Settings.WhenAnyValue(x => x.GameExecutablePath, fs.File.Exists);
		var logFolderExists = main.Settings.WhenAnyValue(x => x.ExtenderLogDirectory).Select(fs.Directory.Exists);
		var canOpenLogsFolder = canExecuteCommands.CombineLatest(gameExecutableExists, logFolderExists).AllTrue();
		var canOpenExecutablePath = canExecuteCommands.CombineLatest(gameExecutableExists).AllTrue();

		//Savegames\Story
		var canOpenProfileSaves = modOrder.WhenAnyValue(x => x.SelectedProfileSavesPath, fs.Directory.Exists).CombineLatest(canExecuteCommands).AllTrue();
		var canOpenExtenderDirectory = main.PathwayData.WhenAnyValue(x => x.AppDataScriptExtenderPath, Validators.IsValid).CombineLatest(canExecuteCommands).AllTrue();

		OpenModsFolderCommand = ReactiveCommand.Create<string?, bool>(mode => OpenInExplorerOrOther(mode, main.PathwayData.AppDataModsPath), canOpenModsDirectory);
		CopyModsDirectoryPathToClipboardCommand = ReactiveCommand.Create(() => AppServices.Commands.CopyToClipboard(main.PathwayData.AppDataModsPath), canOpenModsDirectory);

		OpenLogsFolderCommand = ReactiveCommand.Create<string?, bool>(mode => OpenInExplorerOrOther(mode, main.Settings.ExtenderLogDirectory), canOpenLogsFolder);
		CopyExtenderLogsDirectoryPathToClipboardCommand = ReactiveCommand.Create(() => AppServices.Commands.CopyToClipboard(main.Settings.ExtenderLogDirectory), canOpenLogsFolder);

		EditExecutablePathCommand = ReactiveCommand.Create(() =>
		{
			ToggleWindow<SettingsWindow>(true);
			var settingsWindow = AppServices.Get<SettingsWindow>();
			var vm = AppServices.Get<SettingsWindowViewModel>();
			if(settingsWindow != null && vm != null)
			{
				vm.SelectedTabIndex = SettingsWindowTab.Default;
				settingsWindow.GeneralSettingsView.GameExecutablePathTextBox.Focus();
				RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(250), () =>
				{
					settingsWindow.GeneralSettingsView.GameExecutablePathTextBox.SelectAll();
				});
			}
		}, canExecuteCommands);
		OpenGameFolderCommand = ReactiveCommand.Create<string?, bool>(mode => OpenInExplorerOrOther(mode, main.Settings.GameExecutablePath), canOpenExecutablePath);
		CopyGameFolderPathToClipboardCommand = ReactiveCommand.Create(() => AppServices.Commands.CopyToClipboard(fs.Path.GetDirectoryName(main.Settings.GameExecutablePath)), canOpenExecutablePath);
		CopyGameExecutablePathToClipboardCommand = ReactiveCommand.Create(() => AppServices.Commands.CopyToClipboard(main.Settings.GameExecutablePath), canOpenExecutablePath);

		OpenSavesFolderCommand = ReactiveCommand.Create<string?, bool>(mode => OpenInExplorerOrOther(mode, modOrder.SelectedProfileSavesPath), canOpenProfileSaves);
		
		OpenExtenderDataFolderCommand = ReactiveCommand.Create<string?, bool>(mode => OpenInExplorerOrOther(mode, main.PathwayData.AppDataScriptExtenderPath, true), canOpenExtenderDirectory);

		OpenNexusModsCommand = ReactiveCommand.Create(() => ProcessHelper.TryOpenPath(DivinityApp.URL_NEXUSMODS), canExecuteCommands);
		OpenSteamPageCommand = ReactiveCommand.Create(() => ProcessHelper.TryOpenPath(DivinityApp.URL_STEAM), canExecuteCommands);

		var canRefreshModUpdates = canExecuteCommands.CombineLatest(main.WhenAnyValue(x => x.IsRefreshingModUpdates, x => x.AppSettingsLoaded))
			.Select(x => x.First && !x.Second.Item1 && x.Second.Item2);

		RefreshCommand = ReactiveCommand.Create(main.RefreshStart, canExecuteCommands);
		RefreshModUpdatesCommand = ReactiveCommand.Create(main.RefreshModUpdates, canRefreshModUpdates);

		RenameSaveCommand = ReactiveCommand.CreateFromTask(main.RenameSaveAsync, canExecuteCommands);

		SaveOrderCommand = ReactiveCommand.CreateFromTask(modOrder.SaveLoadOrderAsync, canExecuteCommands);
		SaveOrderAsCommand = ReactiveCommand.CreateFromTask(modOrder.SaveLoadOrderAsAsync, canExecuteCommands);
		SaveSettingsSilentlyCommand = ReactiveCommand.Create(main.SaveSettings, canExecuteCommands);

		var canToggleUpdatesView = canExecuteCommands.CombineLatest(main.WhenAnyValue(x => x.ModUpdatesAvailable)).AllTrue();

		ToggleUpdatesViewCommand = ReactiveCommand.Create(() =>
		{
			if (main.Router.GetCurrentViewModel() != ViewModelLocator.ModUpdates)
			{
				main.Views.SwitchToModUpdates();
			}
			else
			{
				main.Views.SwitchToModOrderView();
			}
		}, canToggleUpdatesView);

		TogglePakFileExplorerWindowCommand = ReactiveCommand.Create(ToggleWindow<PakFileExplorerWindow>, canExecuteCommands);

		interactions.ViewModFiles.RegisterHandler(async input =>
		{
			var mods = input.Input.Mods;
			var result = false;
			if (mods != null)
			{
				var pakFileExplorer = ViewModelLocator.PakFileExplorer;
				await Observable.StartAsync(async () =>
				{
					await pakFileExplorer.LoadModsAsync(mods, CancellationToken.None);
				}, RxApp.TaskpoolScheduler);
				await Observable.Start(() =>
				{
					ToggleWindow<PakFileExplorerWindow>(true);
				}, RxApp.MainThreadScheduler);
				result = true;
			}
			input.SetOutput(result);
		});

		ToggleVersionGeneratorWindowCommand = ReactiveCommand.Create(ToggleWindow<VersionGeneratorWindow>, canExecuteCommands);
		ToggleStatsValidatorWindowCommand = ReactiveCommand.Create(ToggleWindow<StatsValidatorWindow>, canExecuteCommands);
		ToggleSettingsWindowCommand = ReactiveCommand.Create(ToggleWindow<SettingsWindow>, canExecuteCommands);
		ToggleAboutWindowCommand = ReactiveCommand.Create(ToggleWindow<AboutWindow>, canExecuteCommands);

		ToggleKeybindingsCommand = ReactiveCommand.Create(() =>
		{
			ToggleWindow<SettingsWindow>(true);
			var vm = AppServices.Get<SettingsWindowViewModel>();
			if (vm != null)
			{
				vm.SelectedTabIndex = SettingsWindowTab.Keybindings;
				return true;
			}
			return false;
		}, canExecuteCommands);

		ToggleNXMLinkDownloaderCommand = ReactiveCommand.Create(ToggleWindow<NxmDownloadWindow>, canExecuteCommands);
		ToggleCollectionDownloaderWindowCommand = ReactiveCommand.Create(ToggleWindow<NexusModsCollectionDownloadWindow>, canExecuteCommands);

		ToggleThemeModeCommand = ReactiveCommand.Create(() =>
		{
			AppServices.Settings.ManagerSettings.DarkThemeEnabled = !AppServices.Settings.ManagerSettings.DarkThemeEnabled;
		}, canExecuteCommands);

		ImportModsCommand = ReactiveCommand.CreateFromTask(modImporter.OpenModImportDialog, canExecuteCommands);
		ImportNexusModsIdsCommand = ReactiveCommand.CreateFromTask(modImporter.OpenModIdsImportDialog, canExecuteCommands);

		ImportOrderFromSaveCommand = ReactiveCommand.CreateFromTask(modOrder.ImportOrderFromSaveToCurrent, canExecuteCommands);
		ImportOrderFromSaveAsNewCommand = ReactiveCommand.CreateFromTask(modOrder.ImportOrderFromSaveAsNew, canExecuteCommands);
		ImportOrderFromFileCommand = ReactiveCommand.CreateFromTask(modOrder.ImportOrderFromFile, canExecuteCommands);
		ImportOrderFromZipFileCommand = ReactiveCommand.CreateFromTask(modImporter.ImportOrderFromArchive, canExecuteCommands);

		ExportOrderToTextFileCommand = ReactiveCommand.CreateFromTask(modOrder.ExportLoadOrderToTextFileAsAsync, canExecuteHasActive);
		ExportOrderToZipCommand = ReactiveCommand.CreateFromTask(main.ExportLoadOrderToArchiveAsync, canExecuteHasActive);
		ExportOrderToArchiveAsCommand = ReactiveCommand.CreateFromTask(main.ExportLoadOrderToArchiveAsAsync, canExecuteHasActive);

		var canRenameOrder = modOrder.WhenAnyValue(x => x.IsModSettingsOrder, x => x.SelectedModOrder, (b, order) => !b && order != null);
		RenameOrderCommand = ReactiveCommand.CreateFromTask(async () =>
		{
			var order = modOrder.SelectedModOrder!;
			var orderPath = order.FilePath!;
			var fileName = fs.Path.GetFileNameWithoutExtension(orderPath);
			var directory = fs.Path.GetDirectoryName(orderPath);
			var ext = fs.Path.GetExtension(orderPath);

			var request = new ShowMessageBoxRequest($"Rename {fileName}", $"Input a new name for {orderPath}. This will update the file path as well.", InteractionMessageBoxType.Input, fileName);
			var result = await AppServices.Interactions.ShowMessageBox.Handle(request);

			if (result.Result && result.Input.IsValid())
			{
				var nextName = fs.Path.GetFileNameWithoutExtension(result.Input);
				var nextFilePath = fs.Path.Join(directory, ModDataLoader.MakeSafeFilename(nextName + ext, '_'));

				fs.File.Move(orderPath, nextFilePath, true);
				var existingOrder = modOrder.ExternalModOrders.FirstOrDefault(x => fs.PathEquals(x.FilePath, nextFilePath));
				if (existingOrder != null)
				{
					modOrder.ExternalModOrders.Remove(existingOrder);
				}
				order.Name = nextName;
				order.FilePath = nextFilePath.NormalizeDirectorySep();
				AppServices.Commands.ShowAlert($"Renamed load order to '{nextFilePath}'", AlertType.Success, 20);
			}
		}, canExecuteCommands.CombineLatest(canRenameOrder).AllTrue());

		//TODO
		ReloadModsCommand = ReactiveCommand.Create(() => { }, canExecuteModOrderCommands);

		MoveSelectedModsCommand = ReactiveCommand.Create(() =>
		{
			DivinityApp.IsKeyboardNavigating = true;

			ObservableCollectionExtended<IModEntry>? sourceCollection = null;
			ModListViewModel? sourceList = null;
			ModListViewModel? targetList = null;
			string? targetListName = null;
			
			if(modOrder.ActiveModsView.HasAnyFocus)
			{
				sourceCollection = modOrder.ActiveMods;
				sourceList = modOrder.ActiveModsView;
				targetList = modOrder.InactiveModsView;
				targetListName = "inactive";
			}
			else if(modOrder.InactiveModsView.HasAnyFocus)
			{
				sourceCollection = modOrder.InactiveMods;
				sourceList = modOrder.InactiveModsView;
				targetList = modOrder.ActiveModsView;
				targetListName = "active";
			}

			if (sourceCollection != null && sourceList != null && targetList != null && targetListName != null)
			{
				var selectedMods = sourceCollection.Where(x => x.IsSelected).ToList();

				if (selectedMods.Count <= 0) return;

				var selectedMod = selectedMods[0];
				var nextSelectedIndex = sourceCollection.IndexOf(selectedMod);

				foreach (var mod in selectedMods)
				{
					if (mod != null) mod.PreserveSelection = true;
				}

				var rowSelection = targetList.Mods.RowSelection;
				var sourceSelectedIndexes = sourceList.Mods.RowSelection?.SelectedIndexes;
				if (rowSelection != null && sourceSelectedIndexes != null)
				{
					var targetIndex = rowSelection.SelectedIndex;
					//Clear the previous selection, so only the dropped items are selected
					rowSelection.Clear();

					ModListView.DragDropRows(sourceList.Mods, targetList.Mods,
						sourceSelectedIndexes,
						targetIndex,
						TreeDataGridRowDropPosition.After, DragDropEffects.Move);

					string countSuffix = selectedMods.Count > 1 ? "mods" : "mod";
					string text = $"Moved {selectedMods.Count} {countSuffix} to the {targetListName} mods list.";
					if (DivinityApp.IsScreenReaderActive()) AppServices.ScreenReader.Speak(text);
					AppServices.Commands.ShowAlert(text, AlertType.Info, 10);
					modOrder.CanMoveSelectedMods = false;

					if (main.Settings.ShiftListFocusOnSwap)
					{
						targetList.FocusCommand.Execute().Subscribe();
					}
				}
			}
		}, canExecuteModOrderCommands);

		FocusActiveModsCommand = ReactiveCommand.Create(() =>
		{
			DivinityApp.IsKeyboardNavigating = true;

			var modOrderView = AppServices.Get<ModOrderView>()!;

			modOrder.ActiveModsView.FocusCommand.Execute().Subscribe();

			if (modOrder.ActiveModsView.TotalModsSelected == 0)
			{
				modOrderView.ActiveModsList.ModsTreeDataGrid.RowSelection?.Select(0);
			}
			//TODO FocusSelectedItem?
		}, canExecuteModOrderCommands);

		FocusInactiveModsCommand = ReactiveCommand.Create(() =>
		{
			DivinityApp.IsKeyboardNavigating = true;

			var modOrderView = AppServices.Get<ModOrderView>()!;

			modOrder.InactiveModsView.FocusCommand.Execute().Subscribe();

			if (modOrder.InactiveModsView.TotalModsSelected == 0)
			{
				modOrderView.InactiveModsList.ModsTreeDataGrid.RowSelection?.Select(0);
			}
			//TODO FocusSelectedItem?
		}, canExecuteModOrderCommands);

		SwapListFocusCommand = ReactiveCommand.Create(() =>
		{
			DivinityApp.IsKeyboardNavigating = true;
			if (modOrder.ActiveModsView.HasAnyFocus)
			{
				modOrder.InactiveModsView.FocusCommand.Execute().Subscribe();
			}
			else if (modOrder.InactiveModsView.HasAnyFocus)
			{
				modOrder.ActiveModsView.FocusCommand.Execute().Subscribe();
			}
		}, canExecuteModOrderCommands);

		MoveToTopCommand = ReactiveCommand.Create(() =>
		{
			DivinityApp.IsKeyboardNavigating = true;
			if (modOrder.ActiveModsView.HasAnyFocus)
			{
				modOrder.ActiveModsView.Mods.RowSelection?.Select(0);
			}
			else if (modOrder.InactiveModsView.HasAnyFocus)
			{
				modOrder.InactiveModsView.Mods.RowSelection?.Select(0);
			}
		}, canExecuteModOrderCommands);

		MoveToBottomCommand = ReactiveCommand.Create(() =>
		{
			DivinityApp.IsKeyboardNavigating = true;
			if (modOrder.ActiveModsView.HasAnyFocus)
			{
				modOrder.ActiveModsView.Mods.RowSelection?.Select(modOrder.ActiveModsView.Mods.Rows.Count-1);
			}
			else if (modOrder.InactiveModsView.HasAnyFocus)
			{
				modOrder.InactiveModsView.Mods.RowSelection?.Select(modOrder.InactiveModsView.Mods.Rows.Count - 1);
			}
		}, canExecuteModOrderCommands);

		ToggleFilterFocusCommand = ReactiveCommand.Create(() =>
		{
			var modOrderView = AppServices.Get<ModOrderView>()!;

			if (modOrder.ActiveModsView.HasAnyFocus)
			{
				if(!modOrderView.ActiveModsList.FilterTextBox.IsFocused)
				{
					modOrderView.ActiveModsList.FilterTextBox.Focus();
				}
				else
				{
					modOrder.ActiveModsView.FocusCommand.Execute().Subscribe();
				}
			}
			else if (modOrder.InactiveModsView.HasAnyFocus)
			{
				if (!modOrderView.InactiveModsList.FilterTextBox.IsFocused)
				{
					modOrderView.InactiveModsList.FilterTextBox.Focus();
				}
				else
				{
					modOrder.InactiveModsView.FocusCommand.Execute().Subscribe();
				}
			}
		}, canExecuteModOrderCommands);

		ToggleFileNameDisplayCommand = ReactiveCommand.Create(() =>
		{
			main.Settings.DisplayFileNames = !main.Settings.DisplayFileNames;

			foreach (var m in AppServices.Mods.AllMods)
			{
				m.DisplayFileForName = main.Settings.DisplayFileNames;
			}
		}, canExecuteModOrderCommands);

		var totalActiveSelected = modOrder.ActiveModsView.WhenAnyValue(x => x.TotalModsSelected, count => count > 0);
		var totalInactiveSelected = modOrder.InactiveModsView.WhenAnyValue(x => x.TotalModsSelected, count => count > 0);
		var anySelected = totalActiveSelected.CombineLatest(totalInactiveSelected).Select(x => x.First || x.Second);
		var canDeleteMods = canExecuteModOrderCommands.CombineLatest(anySelected).Select(x => x.First && x.Second);

		DeleteSelectedModsCommand = ReactiveCommand.Create(() =>
		{
			AppServices.Commands.DeleteSelectedModsCommand.Execute().Subscribe();
		}, canDeleteMods);

		var keybindings = this.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetCustomAttribute<KeybindingAttribute>());

		DownloadScriptExtenderCommand = ReactiveCommand.Create(main.AskToDownloadScriptExtender, canExecuteCommands);
		ExtractAllSelectedModsCommand = ReactiveCommand.Create(NotImplemented, canExecuteCommands);
		ExtractSelectedActiveModsCommand = ReactiveCommand.Create(NotImplemented, canExecuteCommands);
		ExtractSelectedInactiveModsCommand = ReactiveCommand.Create(NotImplemented, canExecuteCommands);
		ExtractSelectedAdventureCommand = ReactiveCommand.Create(NotImplemented, canExecuteCommands);
		SpeakActiveModOrderCommand = ReactiveCommand.Create(() =>
		{
			var speaker = AppServices.Get<IScreenReaderService>();
			if(speaker != null)
			{
				if (ModOrder.ActiveMods.Count > 0)
				{
					var text = string.Join(", ", ModOrder.ActiveMods.Select(x => x.DisplayName));
					speaker.Speak($"{ModOrder.ActiveMods.Count} mods are in the active order, including:\n{text}", true);
				}
				else
				{
					speaker.Speak($"Zero mods are active.", true);
				}
			}
		}, canExecuteCommands);
		StopSpeakingCommand = ReactiveCommand.Create(() =>
		{
			AppServices.Get<IScreenReaderService>()?.Silence();
		}, canExecuteCommands);
		CheckForUpdatesCommand = ReactiveCommand.Create(() =>
		{
			main.CheckForUpdates(true);
		}, canExecuteCommands);

		var whenProfilePath = canExecuteModOrderCommands.CombineLatest(modOrder.WhenAnyValue(x => x.SelectedProfilePath, Validators.IsExistingDirectory)).AllTrue();
		var whenProfileSavesPath = canExecuteModOrderCommands.CombineLatest(modOrder.WhenAnyValue(x => x.SelectedProfileSavesPath, Validators.IsExistingDirectory)).AllTrue();
		var whenModOrderPath = canExecuteModOrderCommands.CombineLatest(modOrder.WhenAnyValue(x => x.SelectedModOrderFilePath, Validators.IsExistingFile)).AllTrue();

		dopus.WhenAnyValue(x => x.IsEnabled).BindTo(this, x => x.HasDopus);
		main.WhenAnyValue(x => x.DeveloperModeVisibility).BindTo(this, x => x.IsDeveloperMode);

		OpenSelectedProfileFolderCommand = ReactiveCommand.Create<string?, bool>(mode => OpenInExplorerOrOther(mode, modOrder.SelectedProfilePath), whenProfilePath);
		CopySelectedProfileFilePathToClipboardCommand = ReactiveCommand.Create(() => AppServices.Commands.CopyToClipboard(modOrder.SelectedProfilePath), whenProfilePath);

		OpenSelectedProfileSavesFolderCommand = ReactiveCommand.Create<string?, bool>(mode => OpenInExplorerOrOther(mode, modOrder.SelectedProfileSavesPath), whenProfileSavesPath);
		CopySelectedProfileSavesPathToClipboardCommand = ReactiveCommand.Create(() => AppServices.Commands.CopyToClipboard(modOrder.SelectedProfileSavesPath), whenProfileSavesPath);

		OpenSelectedModOrderFilePathCommand = ReactiveCommand.Create<string?, bool>(mode => OpenInExplorerOrOther(mode, modOrder.SelectedModOrderFilePath), whenModOrderPath);
		CopySelectedModOrderFilePathToClipboardCommand = ReactiveCommand.Create(() => AppServices.Commands.CopyToClipboard(modOrder.SelectedModOrderFilePath), whenModOrderPath);

		OrganizeModPaksCommand = ReactiveCommand.Create(() =>
		{
			var filesToMove = new List<ModFileMoveTask>();

			var fs = AppServices.FS;
			var pathways = AppServices.Pathways.Data;

			var userModsFolder = fs.Path.GetFullPath(pathways.AppDataModsPath);
			var userModsDisabledFolder = fs.Path.GetFullPath(pathways.AppDataInactiveModsPath);

			foreach (var mod in AppServices.Mods.UserMods)
			{
				if (mod.FilePath.IsExistingFile())
				{
					var parentDir = fs.Path.GetFullPath(fs.Path.GetDirectoryName(mod.FilePath));
					var isActive = mod.IsActive || (mod.IsForceLoaded && !mod.IsForceLoadedMergedMod && !mod.ForceAllowInLoadOrder);
					var targetDir = isActive ? userModsFolder : userModsDisabledFolder;

					var newFilePath = fs.Path.Join(targetDir, fs.Path.GetFileName(mod.FilePath));
					if (!parentDir.Equals(targetDir, StringComparison.OrdinalIgnoreCase) && !fs.File.Exists(newFilePath))
					{
						filesToMove.Add(new ModFileMoveTask(mod, mod.FilePath, newFilePath));
					}
				}
			}

			if (filesToMove.Count > 0)
			{
				modOrder.LockAll(true);
				DivinityApp.Log($"Moving '{filesToMove.Count}' mods.");

				foreach (var task in filesToMove)
				{
					task.Move(fs, false);
				}

				modOrder.LockAll(false);
			}
		}, canExecuteCommands);

		//MenuEntry.FromKeybinding(ImportNexusModsIdsCommand, nameof(ImportNexusModsIdsCommand), keybindings),
		_menuEntries.AddRange([
			new MenuEntry(nameof(Resources.TopMenu_File), useLocalization: true, useAccessShortcut:true){
				Children = [
					MenuEntry.FromKeybinding(ImportModsCommand, nameof(ImportModsCommand), keybindings),
					MenuEntry.FromKeybinding(ImportNexusModsIdsCommand, nameof(ImportNexusModsIdsCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(SaveOrderCommand, nameof(SaveOrderCommand), keybindings),
					MenuEntry.FromKeybinding(SaveOrderAsCommand, nameof(SaveOrderAsCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(RenameOrderCommand, nameof(RenameOrderCommand), keybindings),
					MenuEntry.FromKeybinding(RenameSaveCommand, nameof(RenameSaveCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(AddNewOrderCommand, nameof(AddNewOrderCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(ImportOrderFromSaveCommand, nameof(ImportOrderFromSaveCommand), keybindings),
					MenuEntry.FromKeybinding(ImportOrderFromSaveAsNewCommand, nameof(ImportOrderFromSaveAsNewCommand), keybindings),
					MenuEntry.FromKeybinding(ImportOrderFromFileCommand, nameof(ImportOrderFromFileCommand), keybindings),
					MenuEntry.FromKeybinding(ImportOrderFromZipFileCommand, nameof(ImportOrderFromZipFileCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(ExportOrderCommand, nameof(ExportOrderCommand), keybindings),
					MenuEntry.FromKeybinding(ExportOrderToTextFileCommand, nameof(ExportOrderToTextFileCommand), keybindings),
					MenuEntry.FromKeybinding(ExportOrderToZipCommand, nameof(ExportOrderToZipCommand), keybindings),
					MenuEntry.FromKeybinding(ExportOrderToArchiveAsCommand, nameof(ExportOrderToArchiveAsCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(ReloadModsCommand, nameof(ReloadModsCommand), keybindings),
					MenuEntry.FromKeybinding(RefreshModUpdatesCommand, nameof(RefreshModUpdatesCommand), keybindings),
				]},
			new MenuEntry(nameof(Resources.TopMenu_Edit), useLocalization: true, useAccessShortcut:true){
				Children = [
					MenuEntry.FromKeybinding(OrganizeModPaksCommand, nameof(OrganizeModPaksCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(MoveSelectedModsCommand, nameof(MoveSelectedModsCommand), keybindings),
					MenuEntry.FromKeybinding(FocusActiveModsCommand, nameof(FocusActiveModsCommand), keybindings),
					MenuEntry.FromKeybinding(FocusInactiveModsCommand, nameof(FocusInactiveModsCommand), keybindings),
					MenuEntry.FromKeybinding(SwapListFocusCommand, nameof(SwapListFocusCommand), keybindings),
					MenuEntry.FromKeybinding(MoveToTopCommand, nameof(MoveToTopCommand), keybindings),
					MenuEntry.FromKeybinding(MoveToBottomCommand, nameof(MoveToBottomCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(ToggleFilterFocusCommand, nameof(ToggleFilterFocusCommand), keybindings),
					MenuEntry.FromKeybinding(ToggleFileNameDisplayCommand, nameof(ToggleFileNameDisplayCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(DeleteSelectedModsCommand, nameof(DeleteSelectedModsCommand), keybindings),
				]},
			new MenuEntry(nameof(Resources.TopMenu_Settings), useLocalization: true, useAccessShortcut:true){
				Children = [
					MenuEntry.FromKeybinding(ToggleSettingsWindowCommand, nameof(ToggleSettingsWindowCommand), keybindings),
					MenuEntry.FromKeybinding(ToggleKeybindingsCommand, nameof(ToggleKeybindingsCommand), keybindings),
					MenuEntry.FromKeybinding(ToggleThemeModeCommand, nameof(ToggleThemeModeCommand), keybindings),
				]},
			new MenuEntry(nameof(Resources.TopMenu_View), useLocalization: true, useAccessShortcut:true){
				Children = [
					MenuEntry.FromKeybinding(ToggleUpdatesViewCommand, nameof(ToggleUpdatesViewCommand), keybindings),
					MenuEntry.FromKeybinding(ToggleVersionGeneratorWindowCommand, nameof(ToggleVersionGeneratorWindowCommand), keybindings),
					MenuEntry.FromKeybinding(TogglePakFileExplorerWindowCommand, nameof(TogglePakFileExplorerWindowCommand), keybindings),
					MenuEntry.FromKeybinding(ToggleStatsValidatorWindowCommand, nameof(ToggleStatsValidatorWindowCommand), keybindings),
				]},
			new MenuEntry(nameof(Resources.TopMenu_Go), useLocalization: true, useAccessShortcut:true){
				Children = [
					MenuEntry.FromKeybinding(LaunchGameCommand, nameof(LaunchGameCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(OpenNexusModsCommand, nameof(OpenNexusModsCommand), keybindings),
					MenuEntry.FromKeybinding(OpenSteamPageCommand, nameof(OpenSteamPageCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(OpenModsFolderCommand, nameof(OpenModsFolderCommand), keybindings),
					MenuEntry.FromKeybinding(OpenGameFolderCommand, nameof(OpenGameFolderCommand), keybindings),
					MenuEntry.FromKeybinding(OpenSavesFolderCommand, nameof(OpenSavesFolderCommand), keybindings),
					MenuEntry.FromKeybinding(OpenExtenderDataFolderCommand, nameof(OpenExtenderDataFolderCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(OpenGitHubRepoCommand, nameof(OpenGitHubRepoCommand), keybindings),
				]},
			new MenuEntry(nameof(Resources.TopMenu_Download), useLocalization: true, useAccessShortcut:true){
				Children = [
					MenuEntry.FromKeybinding(DownloadScriptExtenderCommand, nameof(DownloadScriptExtenderCommand), keybindings),
					MenuEntry.FromKeybinding(ToggleNXMLinkDownloaderCommand, nameof(ToggleNXMLinkDownloaderCommand), keybindings),
					MenuEntry.FromKeybinding(ToggleCollectionDownloaderWindowCommand, nameof(ToggleCollectionDownloaderWindowCommand), keybindings),
					new MenuSeparator(),
					new MenuEntry(nameof(Resources.TopMenu_Download_ModUpdates), useLocalization: true){
					Children = [
						MenuEntry.FromKeybinding(CheckAllModUpdatesCommand, nameof(CheckAllModUpdatesCommand), keybindings),
						new MenuSeparator(),
						MenuEntry.FromKeybinding(CheckForGitHubModUpdatesCommand, nameof(CheckForGitHubModUpdatesCommand), keybindings),
						MenuEntry.FromKeybinding(CheckForNexusModsUpdatesCommand, nameof(CheckForNexusModsUpdatesCommand), keybindings),
						MenuEntry.FromKeybinding(CheckForModioUpdatesCommand, nameof(CheckForModioUpdatesCommand), keybindings)
					]},
				]},
			new MenuEntry(nameof(Resources.TopMenu_Tools), useLocalization: true, useAccessShortcut:true){
				Children = [
					new MenuEntry(nameof(Resources.TopMenu_Tools_Extract), useLocalization: true){
					Children = [
						MenuEntry.FromKeybinding(ExtractAllSelectedModsCommand, nameof(ExtractAllSelectedModsCommand), keybindings),
						new MenuSeparator(),
						MenuEntry.FromKeybinding(ExtractSelectedActiveModsCommand, nameof(ExtractSelectedActiveModsCommand), keybindings),
						MenuEntry.FromKeybinding(ExtractSelectedInactiveModsCommand, nameof(ExtractSelectedInactiveModsCommand), keybindings),
						new MenuSeparator(),
						MenuEntry.FromKeybinding(ExtractSelectedAdventureCommand, nameof(ExtractSelectedAdventureCommand), keybindings),
					]},
					new MenuSeparator(),
					new MenuEntry(nameof(Resources.TopMenu_Tools_Speak), useLocalization: true){
					Children = [
						MenuEntry.FromKeybinding(SpeakActiveModOrderCommand, nameof(SpeakActiveModOrderCommand), keybindings),
						MenuEntry.FromKeybinding(StopSpeakingCommand, nameof(StopSpeakingCommand), keybindings),
					]},
				]},
			new MenuEntry(nameof(Resources.TopMenu_Help), useLocalization: true, useAccessShortcut:true){
				Children = [
					MenuEntry.FromKeybinding(ToggleAboutWindowCommand, nameof(ToggleAboutWindowCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(CheckForUpdatesCommand, nameof(CheckForUpdatesCommand), keybindings),
					new MenuSeparator(),
					MenuEntry.FromKeybinding(OpenDonationPageCommand, nameof(OpenDonationPageCommand), keybindings),
				]},
		]);

		this.RegisterKeybindings();
	}
}

public class DesignMainCommandBarViewModel : MainCommandBarViewModel
{
	public DesignMainCommandBarViewModel() : base()
	{
		ModOrder = new DesignModOrderViewModel();
	}
}