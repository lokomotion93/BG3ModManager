using DynamicData;
using DynamicData.Binding;

using ModManager.Locale;
using ModManager.Models;
using ModManager.Models.App;
using ModManager.Models.Mod;
using ModManager.Models.Mod.Order;
using ModManager.Models.Settings;
using ModManager.Models.Updates;
using ModManager.ModUpdater.Cache;
using ModManager.Services;
using ModManager.Util;
using ModManager.Windows;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;

namespace ModManager.ViewModels.Main;

public class MainWindowViewModel : ReactiveObject, IScreen
{
	private const int ARCHIVE_BUFFER = 128000;

	public RoutingState Router { get; }
	public ViewManager Views { get; }

	private readonly IPathwaysService _pathways;
	public PathwayData PathwayData => _pathways.Data;
	private readonly ModImportService _importer;
	private readonly IModManagerService _manager;
	private readonly IModUpdaterService _updater;
	private readonly ISettingsService _settings;
	private readonly INexusModsService _nexusMods;
	private readonly IInteractionsService _interactions;
	private readonly IDialogService _dialogs;
	private readonly IEnvironmentService _environment;
	private readonly IFileSystemService _fs;
	private readonly IGlobalCommandsService _globalCommands;

	[Reactive] public MainWindow Window { get; private set; }
	public DownloadActivityBarViewModel DownloadBar { get; private set; }

	public IProgressBarViewModel Progress => ViewModelLocator.Progress;

	[Reactive] public string Title { get; set; }
	[Reactive] public string Version { get; set; }

	//private readonly AppKeys _keys;
	//public AppKeys Keys => _keys;

	[Reactive] public bool IsInitialized { get; private set; }

	public AppSettings AppSettings => _settings.AppSettings;
	public ModManagerSettings Settings => _settings.ManagerSettings;
	public UserModConfig UserModConfig => _settings.ModConfig;
	public ScriptExtenderSettings ExtenderSettings => _settings.ExtenderSettings;
	public ScriptExtenderUpdateConfig ExtenderUpdaterSettings => _settings.ExtenderUpdaterSettings;

	[Reactive] public bool AppSettingsLoaded { get; set; }
	[Reactive] public bool GameIsRunning { get; private set; }
	[Reactive] public bool CanForceLaunchGame { get; set; }
	[Reactive] public bool IsRefreshing { get; private set; }
	[Reactive] public bool IsRefreshingModUpdates { get; private set; }

	/// <summary>Used to locked certain functionality when data is loading or the user is dragging an item.</summary>
	[Reactive] public bool IsLocked { get; private set; }
	[Reactive] public bool IsDragging { get; set; }
	[Reactive] public bool AllowDrop { get; private set; }

	[Reactive] public string StatusText { get; set; }
	[Reactive] public string StatusBarRightText { get; set; }

	[Reactive] public bool ModUpdatesAvailable { get; set; }
	[Reactive] public bool GameDirectoryFound { get; set; }

	[ObservableAsProperty] public bool CanLaunchGame { get; }
	[ObservableAsProperty] public bool IsDeletingFiles { get; }

	[ObservableAsProperty] public bool UpdatesViewIsVisible { get; }

	[Reactive] public bool StatusBarBusyIndicatorVisibility { get; set; }
	[ObservableAsProperty] public bool UpdatingBusyIndicatorVisibility { get; }
	[ObservableAsProperty] public bool UpdateCountVisibility { get; }
	[ObservableAsProperty] public bool DeveloperModeVisibility { get; }

	public RxCommandUnit CancelMainProgressCommand { get; }
	public EventHandler OnRefreshed { get; set; }

	public bool DebugMode { get; set; }

	public void RefreshModUpdates()
	{
		ViewModelLocator.ModUpdates?.Clear();
		ModUpdatesAvailable = false;
		RefreshAllModUpdatesBackground();
	}

	public void RefreshStart()
	{
		_globalCommands.CanExecuteCommands = false;

		Progress.Title = !IsInitialized ? Loca.Progress_Loading_Title : Loca.Progress_Refresh_Title;
		IsRefreshing = true;
		_manager.Refresh();
		ViewModelLocator.ModUpdates.Clear();
		ViewModelLocator.ModOrder.Clear();
		ModUpdatesAvailable = false;
		//Window.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
		//Window.TaskbarItemInfo.ProgressValue = 0;
		LoadAppConfig();
		Progress.Start(RefreshAsync, false, ViewModelLocator.ModOrder);
	}

	private async Task RefreshAsync(CancellationToken token)
	{
		DivinityApp.Log("Refreshing...");

		//Wait for UI to update
		//await View.Dispatcher.InvokeAsync(() => {}, System.Windows.Threading.DispatcherPriority.Background);

		await ViewModelLocator.ModOrder.RefreshAsync(this, token);

		await _updater.LoadCacheAsync(_manager.AllMods, Version, token);

		await Observable.Start(() =>
		{
			if (AppSettings.Features.ScriptExtender)
			{
				LoadExtenderSettingsBackground();
			}

			if (!GameDirectoryFound)
			{
				_globalCommands.ShowAlert(Loca.Alert_Error_GameDataFolderInvalid, AlertType.Danger);
			}

			IsRefreshing = false;

			RefreshModUpdates();

			if (!IsInitialized)
			{
				CheckForUpdates(false, true);
			}

			if(!IsInitialized)
			{
				InitSettingsBindings();
				IsInitialized = true;
			}
			_globalCommands.CanExecuteCommands = true;
			//AppServices.Get<ColorThemeService>().ChangeTheme("Dracula");
		}, RxApp.MainThreadScheduler);

		//RxApp.MainThreadScheduler.Schedule(ViewModelLocator.ModOrder.LoadCurrentProfile);
	}

	private bool _justDownloadedScriptExtender;

	private async Task DownloadScriptExtenderAsync(CancellationToken token)
	{
		var exeDir = _fs.Path.GetDirectoryName(Settings.GameExecutablePath);
		var dllDestination = _fs.Path.Join(exeDir, DivinityApp.EXTENDER_UPDATER_FILE);

		var successes = 0;
		Stream? webStream = null;
		Stream? unzippedEntryStream = null;
		try
		{
			Progress.WorkText = Loca.Progress_DownloadExtender_Downloading.SafeFormat($"Downloading {PathwayData.ScriptExtenderLatestReleaseUrl}", PathwayData.ScriptExtenderLatestReleaseUrl);
			webStream = await WebHelper.DownloadFileAsStreamAsync(PathwayData.ScriptExtenderLatestReleaseUrl, token);
			if (webStream != null)
			{
				successes += 1;
				Progress.IncreaseValue(25, Loca.Progress_DownloadExtender_ExtractingZip.SafeFormat($"Extracting zip to {exeDir}...", exeDir));
				using var archive = new ZipArchive(webStream);
				foreach (var entry in archive.Entries)
				{
					if (token.IsCancellationRequested) break;
					if (entry.Name.Equals(DivinityApp.EXTENDER_UPDATER_FILE, StringComparison.OrdinalIgnoreCase))
					{
						unzippedEntryStream = entry.Open(); // .Open will return a stream
						using var fs = _fs.File.Create(dllDestination, ARCHIVE_BUFFER, FileOptions.Asynchronous);
						await unzippedEntryStream.CopyToAsync(fs, ARCHIVE_BUFFER, token);
						successes += 1;
						break;
					}
				}
				Progress.IncreaseValue(50);
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error downloading the script extender: {ex}");
		}
		finally
		{
			Progress.IncreaseValue(25, Loca.Progress_DownloadExtender_CleaningUp);
			webStream?.Dispose();
			unzippedEntryStream?.Dispose();
			successes += 1;
		}

		await Observable.Start(() =>
		{
			if (successes >= 3)
			{
				_globalCommands.ShowAlert(Loca.Alert_Success_ScriptExtenderInstalled.SafeFormat($"Successfully installed the Extender updater {DivinityApp.EXTENDER_UPDATER_FILE} to '{exeDir}'", DivinityApp.EXTENDER_UPDATER_FILE, exeDir), AlertType.Success, 20);
				ViewModelLocator.CommandBar.SetExtenderHighlight(false);
				ExtenderUpdaterSettings.UpdaterIsAvailable = true;
				_justDownloadedScriptExtender = true;
			}
			else
			{
				_globalCommands.ShowAlert(Loca.Alert_Error_ScriptExtenderInstallationError.SafeFormat($"Error occurred when installing the Extender updater {DivinityApp.EXTENDER_UPDATER_FILE} - Check the log", DivinityApp.EXTENDER_UPDATER_FILE), AlertType.Danger, 30);
			}
		}, RxApp.MainThreadScheduler);

		if (ExtenderUpdaterSettings.UpdaterIsAvailable)
		{
			await LoadExtenderSettingsAsync(token);
			await Observable.Start(() => UpdateExtender(true), RxApp.TaskpoolScheduler);
		}
	}

	private void DownloadScriptExtender()
	{
		ViewModelLocator.Progress.Title = Loca.Progress_DownloadExtender_Title;
		ViewModelLocator.Progress.Start(DownloadScriptExtenderAsync, true);
	}

	private void OnToolboxOutput(object sender, DataReceivedEventArgs e)
	{
		if (e.Data.IsValid()) DivinityApp.Log($"[Toolbox] {e.Data}");
	}

	public void UpdateExtender(bool updateMods = true, CancellationToken? t = null)
	{
		if (AppSettings.Features.ScriptExtender && Settings.UpdateSettings.UpdateScriptExtender)
		{
			try
			{
				var exeDir = _fs.Path.GetDirectoryName(Settings.GameExecutablePath);
				var extenderUpdaterPath = _fs.Path.Join(exeDir, DivinityApp.EXTENDER_UPDATER_FILE);
				var toolboxPath = DivinityApp.GetToolboxPath();

				if (_fs.File.Exists(toolboxPath) && _fs.File.Exists(extenderUpdaterPath)
					&& _settings.ExtenderUpdaterSettings.UpdaterVersion >= 4)
				{
					DivinityApp.Log($"Running '{toolboxPath}' to update the script extender.");

					using var process = new Process();
					var info = process.StartInfo;
					info.FileName = toolboxPath;
					info.WorkingDirectory = _fs.Path.GetDirectoryName(toolboxPath);
					info.Arguments = $"UpdateScriptExtender -u \"{extenderUpdaterPath}\" -b \"{exeDir}\"";
					info.UseShellExecute = false;
					info.CreateNoWindow = true;
					info.RedirectStandardOutput = true;
					info.RedirectStandardError = true;
					process.ErrorDataReceived += OnToolboxOutput;
					process.OutputDataReceived += OnToolboxOutput;

					process.Start();
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					if (!process.WaitForExit(120000))
					{
						process.Kill();
					}
					process.ErrorDataReceived -= OnToolboxOutput;
					process.OutputDataReceived -= OnToolboxOutput;
				}
				else
				{
					DivinityApp.Log($"Skipping running '{toolboxPath}'");
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error updating script extender:\n{ex}");
			}
		}
		if (IsInitialized && !IsRefreshing)
		{
			CheckExtenderInstalledVersion(t);
			if (updateMods) RxApp.MainThreadScheduler.Schedule(ViewModelLocator.ModOrder.UpdateModStatusForAllMods);
		}
	}

	private bool OpenRepoLinkToDownload { get; set; }

	public async Task AskToDownloadScriptExtender(CancellationToken token)
	{
		await CheckForLatestExtenderUpdaterRelease(token);
		if (!OpenRepoLinkToDownload)
		{
			if (Settings.GameExecutablePath.IsExistingFile())
			{
				var exeDir = _fs.Path.GetDirectoryName(Settings.GameExecutablePath);
				var messageText = Loca.MessageBox_ScriptExtenderDownloadConfirmation_Message.SafeFormat(@"Download and install the Script Extender?", PathwayData.ScriptExtenderLatestReleaseUrl, exeDir);

				var result = await _interactions.ShowMessageBox.Handle(new(Loca.MessageBox_ScriptExtenderDownloadConfirmation_Title, messageText,
					InteractionMessageBoxType.YesNo));
				if (result)
				{
					DownloadScriptExtender();
				}
			}
			else
			{
				_globalCommands.ShowAlert(Loca.Alert_Error_GameExePathInvalid, AlertType.Danger);
			}
		}
		else
		{
			DivinityApp.Log($"Getting a release download link failed for some reason. Opening repo url: {DivinityApp.EXTENDER_LATEST_URL}");
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				ProcessHelper.TryOpenUrl(DivinityApp.EXTENDER_LATEST_URL);
			});
		}
	}

	private void CheckExtenderUpdaterVersion()
	{
		var extenderUpdaterPath = _fs.Path.Join(_fs.Path.GetDirectoryName(Settings.GameExecutablePath), DivinityApp.EXTENDER_UPDATER_FILE);
		DivinityApp.Log($"Looking for Script Extender at '{extenderUpdaterPath}'.");
		if (_fs.File.Exists(extenderUpdaterPath))
		{
			DivinityApp.Log($"Checking {DivinityApp.EXTENDER_UPDATER_FILE} for Script Extender ASCII bytes.");
			try
			{
				var fvi = FileVersionInfo.GetVersionInfo(extenderUpdaterPath);
				if (fvi != null && fvi.ProductName.IndexOf("Script Extender", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					_settings.ExtenderUpdaterSettings.UpdaterIsAvailable = true;
					DivinityApp.Log($"Found the Extender at '{extenderUpdaterPath}'.");
					var extenderInfo = FileVersionInfo.GetVersionInfo(extenderUpdaterPath);
					if (extenderInfo.FileVersion.IsValid())
					{
						var version = extenderInfo.FileVersion.Split('.')[0];
						if (int.TryParse(version, out var intVersion))
						{
							_settings.ExtenderUpdaterSettings.UpdaterVersion = intVersion;
						}
					}
				}
				else
				{
					DivinityApp.Log($"'{extenderUpdaterPath}' isn't the Script Extender?");
				}
			}
			catch (System.IO.IOException)
			{
				// This can happen if the game locks up the dll.
				// Assume it's the extender for now.
				_settings.ExtenderUpdaterSettings.UpdaterIsAvailable = true;
				DivinityApp.Log($"WARNING: {extenderUpdaterPath} is locked by a process.");
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error reading: '{extenderUpdaterPath}'\n{ex}");
			}
		}
		else
		{
			_settings.ExtenderUpdaterSettings.UpdaterIsAvailable = false;
			DivinityApp.Log($"Extender updater {DivinityApp.EXTENDER_UPDATER_FILE} not found.");
		}
	}

	private IDisposable? _warnExtenderUpdateFailureTask = null;

	public bool CheckExtenderInstalledVersion(CancellationToken? t)
	{
		var extenderAppDataDir = _fs.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DivinityApp.EXTENDER_APPDATA_DIRECTORY);
		if (_fs.Directory.Exists(extenderAppDataDir))
		{
			var files = FileUtils.EnumerateFiles(extenderAppDataDir, FileUtils.RecursiveOptions, (f) => f.EndsWith(DivinityApp.EXTENDER_APPDATA_DLL, StringComparison.OrdinalIgnoreCase));
			var isInstalled = false;
			var fullExtenderVersion = "";
			var majorVersion = -1;
			var targetVersion = _settings.ExtenderUpdaterSettings.TargetVersion;

			foreach (var f in files)
			{
				isInstalled = true;
				try
				{
					var extenderInfo = FileVersionInfo.GetVersionInfo(f);
					if (extenderInfo != null)
					{
						var fileVersion = $"{extenderInfo.FileMajorPart}.{extenderInfo.FileMinorPart}.{extenderInfo.FileBuildPart}.{extenderInfo.FilePrivatePart}";
						if (fileVersion == targetVersion)
						{
							majorVersion = extenderInfo.FileMajorPart;
							fullExtenderVersion = fileVersion;
							break;
						}
						if (extenderInfo.FileMajorPart > majorVersion)
						{
							majorVersion = extenderInfo.FileMajorPart;
							fullExtenderVersion = fileVersion;
						}
					}
				}
				catch (Exception ex)
				{
					DivinityApp.Log($"Error getting file info from: '{f}'\n\t{ex}");
				}
			}
			if (majorVersion > -1)
			{
				DivinityApp.Log($"Script Extender version found ({majorVersion})");
				_settings.ExtenderSettings.ExtenderIsAvailable = isInstalled;
				_settings.ExtenderSettings.ExtenderVersion = fullExtenderVersion;
				_settings.ExtenderSettings.ExtenderMajorVersion = majorVersion;
				return true;
			}
		}
		else
		{
			DivinityApp.Log($"Extender Local AppData folder not found at '{extenderAppDataDir}'. Skipping.");
		}

		//Recently downloaded DWrite.dll, but Toolbox may have failed to invoke an update
		if (t?.IsCancellationRequested == false && _justDownloadedScriptExtender)
		{
			_warnExtenderUpdateFailureTask?.Dispose();
			_warnExtenderUpdateFailureTask = RxApp.MainThreadScheduler.Schedule(() =>
			{
				_justDownloadedScriptExtender = false;
				_interactions.ShowMessageBox.Handle(new(Loca.MessageBox_ScriptExtenderDownloadCompleted_Title,
					Loca.MessageBox_ScriptExtenderDownloadCompleted_Message,
					InteractionMessageBoxType.Warning)).Subscribe();
			});
		}
		return false;
	}

	private async Task<bool> CheckForLatestExtenderUpdaterRelease(CancellationToken token)
	{
		try
		{
			var latestReleaseZipUrl = "";
			DivinityApp.Log($"Checking for latest {DivinityApp.EXTENDER_UPDATER_FILE} release at 'https://github.com/{DivinityApp.EXTENDER_GITHUB_USER}/{DivinityApp.EXTENDER_GITHUB_REPO}'");

			var github = AppServices.Get<IGitHubService>();
			var limits = await github.GetRateLimitsAsync();
			if(limits.Resources.Core.Remaining <= 0)
			{
				DivinityApp.Log($"GitHub rate limit exceeded ({limits.Resources.Core.Remaining}/{limits.Resources.Core.Limit}). Skipping.");
				OpenRepoLinkToDownload = true;
				return false;
			}
			var latestRelease = await github.GetLatestReleaseRawAsync(DivinityApp.EXTENDER_GITHUB_USER, DivinityApp.EXTENDER_GITHUB_REPO);

			if (latestRelease != null)
			{
				foreach (var entry in latestRelease.Assets)
				{
					if (entry.BrowserDownloadUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && !entry.BrowserDownloadUrl.Contains("Console"))
					{
						latestReleaseZipUrl = entry.BrowserDownloadUrl;
						PathwayData.ScriptExtenderLatestReleaseVersion = latestRelease.TagName;
					}
				}
				if (latestReleaseZipUrl.IsValid())
				{
					OpenRepoLinkToDownload = false;
					PathwayData.ScriptExtenderLatestReleaseUrl = latestReleaseZipUrl;
					DivinityApp.Log($"Script Extender latest release url found: {latestReleaseZipUrl}");
					return true;
				}
				else
				{
					DivinityApp.Log($"Script Extender latest release not found.");
				}
			}
			else
			{
				OpenRepoLinkToDownload = true;
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error checking for latest Script Extender release: {ex}");

			OpenRepoLinkToDownload = true;
		}

		return false;
	}

	private async Task<Unit> LoadExtenderSettingsAsync(CancellationToken token)
	{
		await Observable.Start(() =>
		{
			_settings.TryLoad(_settings.ExtenderSettings, out _, false);
			_settings.TryLoad(_settings.ExtenderUpdaterSettings, out _, false);

			CheckExtenderUpdaterVersion();
			CheckExtenderInstalledVersion(token);
			ViewModelLocator.ModOrder.UpdateModStatusForAllMods();

			return Unit.Default;
		}, RxApp.MainThreadScheduler);

		return Unit.Default;
	}

	public void LoadExtenderSettingsBackground()
	{
		DivinityApp.Log($"Loading extender settings.");
		RxApp.TaskpoolScheduler.ScheduleAsync(async (c, t) =>
		{
			await LoadExtenderSettingsAsync(t);
			UpdateExtender(true, t);
			return Disposable.Empty;
		});
	}

	private void TryStartGameExe(string exePath, string workingDirectory, string launchParams = "")
	{
		if (!ProcessHelper.TryOpenPath(exePath, _fs.File.Exists, launchParams, workingDirectory))
		{
			_globalCommands.ShowAlert(Loca.Alert_Error_StartGameExeFailed.SafeFormat($"Failed to start game exe '{exePath}' - Check the 'Game Executable Path' in the preferences", exePath), AlertType.Danger);
		}
		else
		{
			//Update whether the game is running or not
			RxApp.TaskpoolScheduler.Schedule(TimeSpan.FromSeconds(5), () =>
			{
				AppServices.Get<IGameUtilitiesService>().CheckForGameProcess();
			});
		}
	}

	public void LaunchGame()
	{
		ViewModelLocator.ModOrder.DeleteModCrashSanityCheck();

		if (Settings.DisableLauncherTelemetry || Settings.DisableLauncherModWarnings)
		{
			RxApp.TaskpoolScheduler.ScheduleAsync(async (sch, t) =>
			{
				await ModDataLoader.UpdateLauncherPreferencesAsync(_pathways.GetLarianStudiosAppDataFolder(), !Settings.DisableLauncherTelemetry, !Settings.DisableLauncherModWarnings, t);
			});
		}

		var exeArgs = new List<string>();
		var userLaunchParams = Settings.GameLaunchParams.IsValid() ? Settings.GameLaunchParams : "";

		if (Settings.GameStoryLogEnabled && !_settings.ExtenderSettings.EnableLogging)
		{
			exeArgs.Add("-storylog 1");
		}

		if (userLaunchParams.IsValid())
		{
			foreach (var entry in exeArgs)
			{
				userLaunchParams.Replace(entry, "");
			}
		}

		exeArgs.Add(userLaunchParams);

		var launchParams = string.Join(" ", exeArgs);

		if (Settings.LaunchType == LaunchGameType.Exe)
		{
			if(Settings.GameExecutablePath.IsValid())
			{
				//Args always set by the launcher
				exeArgs.Add("-externalcrashhandler");
				var sendStats = !Settings.DisableLauncherTelemetry ? 1 : 0;
				exeArgs.Add($"-stats {sendStats}");
				var isModded = ViewModelLocator.ModOrder.ActiveMods.Count > 0 ? 1 : 0;
				exeArgs.Add($"-modded {isModded}");

				var exePath = Environment.ExpandEnvironmentVariables(Settings.GameExecutablePath);
				var exeDir = _fs.Path.GetDirectoryName(exePath)!;

				if (Settings.LaunchDX11)
				{
					var nextExe = _fs.Path.Combine(exeDir, "bg3_dx11.exe");
					if (_fs.File.Exists(nextExe))
					{
						exePath = nextExe;
					}
				}

				if (!exePath.IsExistingFile())
				{
					if (string.IsNullOrWhiteSpace(exePath))
					{
						_globalCommands.ShowAlert(Loca.Alert_Error_GameExePathInvalid, AlertType.Danger, 30);
					}
					else
					{
						_globalCommands.ShowAlert(Loca.Alert_Error_LaunchGame_NotFound.SafeFormat($"Failed to find game exe at, \"{exePath}\"", exePath), AlertType.Danger, 90);
					}
					return;
				}

				DivinityApp.Log($"Opening game exe at: {exePath} with args {launchParams}");
				TryStartGameExe(exePath, exeDir, launchParams);
			}
			else
			{
				_globalCommands.ShowAlert(Loca.Alert_Error_GameExePathInvalid, AlertType.Danger, 30);
			}
		}
		else if (Settings.LaunchType == LaunchGameType.Steam)
		{
			var appid = AppSettings.DefaultPathways.Steam.AppID ?? "1086940";
			var steamUrl = $"steam://run/{appid}//{launchParams}";
			DivinityApp.Log($"Opening game through steam via '{steamUrl}'");
			ProcessHelper.TryOpenUrl(steamUrl);
		}
		else
		{
			if (!string.IsNullOrWhiteSpace(Settings.CustomLaunchAction))
			{
				var args = Settings.CustomLaunchArgs;
				DivinityApp.Log($"Running custom launch action '{Settings.CustomLaunchAction}' with args ({args})");
				try
				{
					ProcessHelper.TryRunCommand(Settings.CustomLaunchAction, Settings.CustomLaunchArgs ?? string.Empty);
				}
				catch (Exception ex)
				{
					var msg = Loca.MessageBox_CustomLaunchError_Message.SafeFormat($"Error running custom launch '{Settings.CustomLaunchAction}' with args '{Settings.CustomLaunchArgs}':\n{ex}", Settings.CustomLaunchAction, Settings.CustomLaunchArgs, ex);
					DivinityApp.Log(msg);
					_interactions.ShowMessageBox.Handle(new(Loca.MessageBox_CustomLaunchError_Title, msg, InteractionMessageBoxType.Error)).Subscribe();
				}
			}
			else
			{
				_globalCommands.ShowAlert(Loca.Alert_Error_LaunchGame_CustomActionEmpty, AlertType.Warning, 30);
			}
		}

		if (Settings.ActionOnGameLaunch != GameLaunchWindowAction.None)
		{
			switch (Settings.ActionOnGameLaunch)
			{
				case GameLaunchWindowAction.Minimize:
					Window.WindowState = WindowState.Minimized;
					break;
				case GameLaunchWindowAction.Close:
					App.Current.Shutdown();
					break;
			}
		}
	}

	private static bool CanLaunchGameCheck(string? gameExe, bool singleInstance, bool gameIsRunning, bool canForceLaunch)
	{
		return gameExe.IsExistingFile() && (!singleInstance || !gameIsRunning || canForceLaunch);
	}

	private void InitSettingsBindings()
	{
		var gameUtils = AppServices.Get<IGameUtilitiesService>();
		gameUtils.WhenAnyValue(x => x.GameIsRunning).BindTo(this, x => x.GameIsRunning);

		Settings.WhenAnyValue(x => x.GameExecutablePath).Subscribe(path =>
		{
			if (path.IsValid())
			{
				gameUtils.AddGameProcessName(_fs.Path.GetFileNameWithoutExtension(path));
				AppServices.Get<IModSettingsExportService>()?.SetGameVersion(path);
			}
		});

		this.WhenAnyValue(x => x.Settings.GameExecutablePath, x => x.Settings.LimitToSingleInstance, x => x.GameIsRunning, x => x.CanForceLaunchGame, CanLaunchGameCheck)
			.ToUIProperty(this, x => x.CanLaunchGame);

		/*Keys.LaunchGame.AddAction(LaunchGame, canOpenGameExe);

		var canOpenLogDirectory = Settings.WhenAnyValue(x => x.ExtenderLogDirectory, (f) => Directory.Exists(f));

		var canDownloadScriptExtender = this.WhenAnyValue(x => x.PathwayData.ScriptExtenderLatestReleaseUrl, (p) => !string.IsNullOrEmpty(p));
		Keys.DownloadScriptExtender.AddAction(() => AskToDownloadScriptExtender(), canDownloadScriptExtender);

		var canOpenModsFolder = this.WhenAnyValue(x => x.PathwayData.AppDataModsPath, (p) => !string.IsNullOrEmpty(p) && Directory.Exists(p));
		Keys.OpenModsFolder.AddAction(() =>
		{
			FileUtils.TryOpenPath(PathwayData.AppDataModsPath);
		}, canOpenModsFolder);

		var canOpenGameFolder = Settings.WhenAnyValue(x => x.GameExecutablePath, (p) => !string.IsNullOrEmpty(p) && File.Exists(p));
		Keys.OpenGameFolder.AddAction(() =>
		{
			var folder = _fs.Path.GetDirectoryName(Settings.GameExecutablePath);
			if (Directory.Exists(folder))
			{
				FileUtils.TryOpenPath(folder);
			}
		}, canOpenGameFolder);

		Keys.OpenLogsFolder.AddAction(() =>
		{
			FileUtils.TryOpenPath(Settings.ExtenderLogDirectory);
		}, canOpenLogDirectory);

		Keys.OpenWorkshopFolder.AddAction(() =>
		{
			//DivinityApp.Log($"WorkshopSupportEnabled:{WorkshopSupportEnabled} canOpenWorkshopFolder CanExecute:{OpenWorkshopFolderCommand.CanExecute(null)}");
			if (!string.IsNullOrEmpty(Settings.WorkshopPath) && Directory.Exists(Settings.WorkshopPath))
			{
				FileUtils.TryOpenPath(Settings.WorkshopPath);
			}
		}, canOpenWorkshopFolder);*/

		Settings.WhenAnyValue(x => x.LogEnabled).Subscribe((logEnabled) =>
		{
			//Window.ToggleLogging(logEnabled);
		});

		var actionLaunchChanged = Settings.WhenAnyValue(x => x.ActionOnGameLaunch).Skip(1).ObserveOn(RxApp.MainThreadScheduler);
		actionLaunchChanged.Subscribe((action) =>
		{
			if (!Settings.SettingsWindowIsOpen)
			{
				SaveSettings();
			}
		});

		Settings.WhenAnyValue(x => x.DisplayFileNames).Subscribe((b) =>
		{
			//TODO
			/*if (View != null && View.MenuItems.TryGetValue("ToggleFileNameDisplay", out var menuItem))
			{
				if (b)
				{
					menuItem.Header = "Show Display Names for Mods";
				}
				else
				{
					menuItem.Header = "Show File Names for Mods";
				}
			}*/
		});

		Settings.WhenAnyValue(x => x.DocumentsFolderPathOverride).Skip(1).Subscribe((x) =>
		{
			if (!IsLocked)
			{
				_pathways.SetGamePathways(Settings.GameDataPath, x);
				if (AppSettings.Features.ScriptExtender && IsInitialized && !IsRefreshing)
				{
					LoadExtenderSettingsBackground();
				}
				_globalCommands.ShowAlert(Loca.Alert_Warning_DocumentsFolderPathOverrideChanged.SafeFormat($"AppData path override set to '{x}' - Make sure to refresh", x), AlertType.Warning, 60);
			}
		});

		Settings.WhenAnyValue(x => x.LaunchType, x => x.GameExecutablePath)
		.Throttle(TimeSpan.FromMilliseconds(250))
		.ObserveOn(RxApp.MainThreadScheduler)
		.Subscribe(x =>
		{
			if(x.Item2.IsValid())
			{
				var exePath = x.Item2.ToRealPath();

				if (_fs.File.Exists(exePath))
				{
					if (x.Item1 == LaunchGameType.Exe)
					{
						CreateSteamApiTextFile(exePath);
					}
					else if (Settings.SettingsWindowIsOpen)
					{
						//RemoveSteamApiTextFile(exePath);
					}
				}
			}
		});

		Settings.WhenAnyValue(x => x.DeleteModCrashSanityCheck, x => x.GameExecutablePath).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
		{
			if (x.Item1 && x.Item2.IsValid() && ExtenderSettings.InsanityCheck != true && !Settings.SettingsWindowIsOpen)
			{
				ExtenderSettings.InsanityCheck = true;
				_settings.TrySave(ExtenderSettings, out _);
			}
		});
	}

	private async Task<bool> LoadSettings()
	{
		var success = true;
		if (!_settings.TryLoadAll(out var errors))
		{
			var errorMessage = string.Join("\n", errors.Select(x => x.ToString()));
			_globalCommands.ShowAlert($"Error loading settings: {errorMessage}", AlertType.Danger);
			success = false;
		}

		LoadAppConfig();

#if DOS2
		Settings.DefaultExtenderLogDirectory = _fs.Path.Join(Pathways.GetLarianStudiosAppDataFolder(), "Divinity Original Sin 2 Definitive Edition", "Extender Logs");
#else
		Settings.DefaultExtenderLogDirectory = _fs.Path.Join(_pathways.GetLarianStudiosAppDataFolder(), "Baldur's Gate 3", "Extender Logs");
#endif

		var githubSupportEnabled = AppSettings.Features.GitHub;
		var nexusModsSupportEnabled = AppSettings.Features.NexusMods;
		var modioSupportEnabled = AppSettings.Features.Modio;

		if (!_pathways.SetGamePathways(Settings.GameDataPath, Settings.DocumentsFolderPathOverride))
		{
			GameDirectoryFound = false;

			if (!FileUtils.HasDirectoryReadPermission(Settings.GameDataPath, Settings.DocumentsFolderPathOverride))
			{
				var message = Loca.MessageBox_LoadSettings_PermissionDenied_Message.SafeFormat($"BG3MM lacks permission to read one or both of the following paths:\nGame Data Path: ({Settings.GameDataPath})\nGame Executable Path: ({Settings.GameExecutablePath})", Settings.GameDataPath, Settings.GameExecutablePath);
				await _interactions.ShowMessageBox.Handle(new(Loca.MessageBox_LoadSettings_PermissionDenied_Title, message, InteractionMessageBoxType.Error));
			}
			else
			{
				var result = await _dialogs.OpenFolderAsync(new(Loca.Picker_GameDataPath_Title,
					Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
					Loca.Picker_GameDataPath_Description));

				if (result.Success)
				{
					var data = PathwayData;
					var dir = result.File;
					var dataDirectory = _fs.Path.Join(dir, AppSettings.DefaultPathways.GameDataFolder);
					var exePath = _fs.Path.Join(dir, AppSettings.DefaultPathways.Steam.ExePath);
					if (!_fs.File.Exists(exePath))
					{
						exePath = _fs.Path.Join(dir, AppSettings.DefaultPathways.GOG.ExePath);
					}
					if (_fs.Directory.Exists(dataDirectory))
					{
						Settings.GameDataPath = dataDirectory;
						GameDirectoryFound = true;
					}
					else
					{
						_globalCommands.ShowAlert(Loca.Alert_Error_Picker_GameDataPath_NoDataFolder, AlertType.Danger);
					}
					if (_fs.File.Exists(exePath))
					{
						Settings.GameExecutablePath = exePath;
					}
					else
					{
						_globalCommands.ShowAlert(Loca.Alert_Error_Picker_GameDataPath_NoExe, AlertType.Danger);
					}
					data.InstallPath = dir;
					//Services.Settings.TrySaveAll(out _);
				}
			}
		}
		else
		{
			GameDirectoryFound = true;
		}

		if (AppSettings.Features.ScriptExtender && IsInitialized && !IsRefreshing)
		{
			LoadExtenderSettingsBackground();
		}

		if (success)
		{
			ViewModelLocator.Settings.CanSaveSettings = false;
		}

		return success;
	}

	public void SaveSettings()
	{
		if (!_settings.TrySaveAll(out var errors))
		{
			var errorMessage = string.Join("\n", errors.Select(x => x.ToString()));
			_globalCommands.ShowAlert(Loca.Alert_Error_SaveSettings.SafeFormat($"Error saving settings: {errorMessage}", errorMessage), AlertType.Danger);
		}
		else
		{
			ViewModelLocator.Settings.CanSaveSettings = false;
			/*if (!Keys.SaveKeybindings(out var errorMsg))
			{
				_globalCommands.ShowAlert(errorMsg, AlertType.Danger);
			}*/
		}
	}

	private IDisposable _deferSave;

	public void QueueSave()
	{
		_deferSave?.Dispose();
		_deferSave = RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(250), () => SaveSettings());
	}

	private string GetInitialStartingDirectory(string? prioritizePath = "")
	{
		var directory = prioritizePath;

		if (prioritizePath.IsValid() && FileUtils.TryGetDirectoryOrParent(prioritizePath, out var actualDir))
		{
			directory = actualDir;
		}
		else
		{
			if (Settings.LastImportDirectoryPath.IsValid())
			{
				directory = Settings.LastImportDirectoryPath;
			}

			if (!_fs.Directory.Exists(directory) && PathwayData.LastSaveFilePath.IsValid() && FileUtils.TryGetDirectoryOrParent(PathwayData.LastSaveFilePath, out var lastDir))
			{
				directory = lastDir;
			}
		}

		if (!directory.IsExistingDirectory())
		{
			directory = DivinityApp.GetAppDirectory();
		}

		return directory;
	}

	/*private void MainWindowMessageBox_Closed_ResetColor(object sender, EventArgs e)
	{
		if (sender is Xceed.Wpf.Toolkit.MessageBox messageBox)
		{
			messageBox.WindowBackground = new SolidColorBrush(Color.FromRgb(78, 56, 201));
			messageBox.Closed -= MainWindowMessageBox_Closed_ResetColor;
		}
	}*/

	private bool CanUpdateMod(ModData mod, DateTime now, TimeSpan minWaitPeriod, ISettingsService settingsService)
	{
		if (settingsService.ModConfig.LastUpdated.TryGetValue(mod.UUID, out var last))
		{
			var time = new DateTime(last);
			return now - time >= minWaitPeriod;
		}
		return true;
	}

	private List<ModData> GetUpdateableMods(ModDownloadSource modDownloadSource)
	{
		var settingsService = AppServices.Get<ISettingsService>();
		var minUpdateTime = Settings.UpdateSettings.MinimumUpdateTimePeriod;
		if (minUpdateTime > TimeSpan.Zero)
		{
			var now = DateTime.Now;
			List<ModData> mods = [.. modDownloadSource switch
			{
				ModDownloadSource.Modio => _manager.UserMods.Where(x => x.PublishHandle != 0 && CanUpdateMod(x, now, minUpdateTime, settingsService)),
				_ => _manager.UserMods.Where(x => CanUpdateMod(x, now, minUpdateTime, settingsService)),
			}];

			return mods;
		}
		return [];
	}

	private IDisposable _refreshGitHubModsUpdatesBackgroundTask;

	private async Task<Unit> RefreshGitHubModsUpdatesBackgroundAsync(IScheduler sch, CancellationToken token)
	{
		var results = await _updater.GetGitHubUpdatesAsync(GetUpdateableMods(ModDownloadSource.GitHub), Version, token);
		await sch.Yield(token);
		if (!token.IsCancellationRequested && results.Count > 0)
		{
			await Observable.Start(() =>
			{
				foreach (var kvp in results)
				{
					if (_manager.TryGetMod(kvp.Key, out var mod))
					{
						var updateData = new ModUpdateData(mod, new ModDownloadData()
						{
							DownloadPath = kvp.Value.BrowserDownloadLink,
							DownloadPathType = ModDownloadPathType.URL,
							DownloadSourceType = ModSourceType.GITHUB,
							Version = kvp.Value.Version,
							Date = kvp.Value.Date
						});
						ViewModelLocator.ModUpdates.Add(updateData);
					}
				}
			}, RxApp.MainThreadScheduler);
		}
		return Unit.Default;
	}

	public void RefreshGitHubModsUpdatesBackground()
	{
		_refreshGitHubModsUpdatesBackgroundTask?.Dispose();
		_refreshGitHubModsUpdatesBackgroundTask = RxApp.TaskpoolScheduler.ScheduleAsync(RefreshGitHubModsUpdatesBackgroundAsync);
	}

	private IDisposable _refreshNexusModsUpdatesBackgroundTask;

	private async Task<Unit> RefreshNexusModsUpdatesBackgroundAsync(IScheduler sch, CancellationToken token)
	{
		var updates = await _updater.GetNexusModsUpdatesAsync(GetUpdateableMods(ModDownloadSource.NexusMods), Version, token);
		await sch.Yield(token);
		if (!token.IsCancellationRequested && updates.Count > 0)
		{
			var isPremium = _nexusMods.IsPremium;
			//$"https://www.nexusmods.com/Core/Libs/Common/Widgets/DownloadPopUp?id={DownloadPath}6&nmm=1&game_id={DivinityApp.NEXUSMODS_GAME_ID}";
			await Observable.Start(() =>
			{
				foreach (var update in updates.Values)
				{
					var displayVersion = update.File.ModVersion;
					if(System.Version.TryParse(displayVersion, out _))
					{
						//Versions may be like 1.3.0, so this ensures it remains in the same format
						displayVersion = new LarianVersion(displayVersion)?.ToString() ?? update.File.ModVersion;
					}
					var updateData = new ModUpdateData(update.Mod, new ModDownloadData()
					{
						DownloadPath = update.DownloadLink.Uri.ToString(),
						DownloadPathType = ModDownloadPathType.URL,
						DownloadSourceType = ModSourceType.NEXUSMODS,
						Version = displayVersion,
						Date = DateUtils.UnixTimeStampToDateTime(update.File.UploadedTimestamp)
					});
					if (!isPremium)
					{
						var nxmEnabled = "";
						if (Settings.UpdateSettings.IsAssociatedWithNXM)
						{
							nxmEnabled = "&nmm=1";
						}
						//Make this a link to the browser, where the user needs to initiate a .nxm url
						updateData.DownloadData.IsIndirectDownload = true;
						updateData.DownloadData.DownloadPath = $"https://www.nexusmods.com/Core/Libs/Common/Widgets/DownloadPopUp?id={update.File.FileId}{nxmEnabled}&game_id={DivinityApp.NEXUSMODS_GAME_ID}";
					}
					ViewModelLocator.ModUpdates.Add(updateData);
				}
			}, RxApp.MainThreadScheduler);
		}
		return Unit.Default;
	}

	public void RefreshNexusModsUpdatesBackground()
	{
		_refreshNexusModsUpdatesBackgroundTask?.Dispose();
		_refreshNexusModsUpdatesBackgroundTask = RxApp.TaskpoolScheduler.ScheduleAsync(RefreshNexusModsUpdatesBackgroundAsync);
	}

	private IDisposable _refreshModioUpdatesBackgroundTask;

	private async Task<Unit> RefreshModioUpdatesBackgroundAsync(IScheduler sch, CancellationToken token)
	{
		var results = await _updater.GetModioUpdatesAsync(Settings, GetUpdateableMods(ModDownloadSource.Modio), Version, token);
		await sch.Yield(token);
		if (!token.IsCancellationRequested && results.Count > 0)
		{
			await Observable.Start(() =>
			{
				foreach (var kvp in results)
				{
					if (_manager.TryGetMod(kvp.Key, out var mod))
					{
						var downloadFile = kvp.Value;
						//TODO
						var updateData = new ModUpdateData(mod, new ModDownloadData()
						{
							DownloadPath = downloadFile.Download?.BinaryUrl?.ToString(),
							DownloadPathType = ModDownloadPathType.URL,
							DownloadSourceType = ModSourceType.MODIO,
							Version = downloadFile.Version,
							Date = DateUtils.UnixTimeStampToDateTime(downloadFile.DateAdded)
						});
						ViewModelLocator.ModUpdates.Add(updateData);
					}
				}
			}, RxApp.MainThreadScheduler);
		}
		return Unit.Default;
	}

	public void RefreshModioUpdatesBackground()
	{
		_refreshModioUpdatesBackgroundTask?.Dispose();
		_refreshModioUpdatesBackgroundTask = RxApp.TaskpoolScheduler.ScheduleAsync(RefreshModioUpdatesBackgroundAsync);
	}

	private IDisposable _refreshAllModUpdatesBackgroundTask;

	private async Task UpdateRefreshingStateAsync(bool b)
	{
		await Observable.Start(() =>
		{
			IsRefreshingModUpdates = b;
		}, RxApp.MainThreadScheduler);
	}

	public void RefreshAllModUpdatesBackground()
	{
		_refreshAllModUpdatesBackgroundTask?.Dispose();
		_refreshAllModUpdatesBackgroundTask = RxApp.TaskpoolScheduler.ScheduleAsync(TimeSpan.FromMilliseconds(250), async (sch, token) =>
		{
			await UpdateRefreshingStateAsync(true);

			if (Settings.UpdateSettings.UpdateGitHubMods) await RefreshGitHubModsUpdatesBackgroundAsync(sch, token);
			if (Settings.UpdateSettings.UpdateNexusMods) await RefreshNexusModsUpdatesBackgroundAsync(sch, token);
			if (Settings.UpdateSettings.UpdateModioMods) await RefreshModioUpdatesBackgroundAsync(sch, token);

			await UpdateRefreshingStateAsync(false);
		});
	}

	public async Task ExportLoadOrderToArchiveAsync()
	{
		var result = await _interactions.ShowMessageBox.Handle(new(
			Loca.MessageBox_ExportLoadOrderToArchiveAsync_Title,
			Loca.MessageBox_ExportLoadOrderToArchiveAsync_Message,
			InteractionMessageBoxType.YesNo));
		if (result)
		{
			Progress.Title = Loca.Progress_ExportLoadOrderToArchiveAsync_Title;
			await Progress.Start(async token =>
			{
				ViewModelLocator.ModOrder.UpdateOrderFromActiveMods();
				await _importer.ExportLoadOrderToArchiveAsync(ViewModelLocator.ModOrder.SelectedProfile, ViewModelLocator.ModOrder.SelectedModOrder, "", token);
			}, true);
		}
	}

	public async Task ExportLoadOrderToArchiveAsAsync()
	{
		ViewModelLocator.ModOrder.UpdateOrderFromActiveMods();
		await ExportLoadOrderToArchiveAsAsync(ViewModelLocator.ModOrder.SelectedProfile, ViewModelLocator.ModOrder.SelectedModOrder);
	}

	public async Task ExportLoadOrderToArchiveAsAsync(ProfileData? profile = null, ModOrder? order = null)
	{
		if (profile != null && order != null)
		{
			var sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");
			var baseOrderName = order.Name;
			if (order.IsModSettings)
			{
				baseOrderName = $"{profile.Name}_{order.Name}";
			}

			var outputName = ModDataLoader.MakeSafeFilename($"{baseOrderName}-{DateTime.Now.ToString(sysFormat + "_HH-mm-ss")}.zip", '_');

			var result = await _dialogs.SaveFileAsync(new(
				Loca.Picker_ExportLoadOrderToArchiveAs_Title,
				GetInitialStartingDirectory(),
				CommonFileTypes.ArchiveFileTypes,
				outputName));

			if (result.Success)
			{
				Progress.Title = Loca.Progress_ExportLoadOrderToArchiveAsync_Title;
				await Progress.Start(async token =>
				{
					await _importer.ExportLoadOrderToArchiveAsync(ViewModelLocator.ModOrder.SelectedProfile, ViewModelLocator.ModOrder.SelectedModOrder, result.File, token);
				}, true);
			}
		}
		else
		{
			_globalCommands.ShowAlert(Loca.Alert_Error_ExportLoadOrderToArchiveAs, AlertType.Danger);
		}
	}

	public async Task RenameSaveAsync()
	{
		var profileSavesDirectory = "";
		if (ViewModelLocator.ModOrder.SelectedProfile != null)
		{
			profileSavesDirectory = _fs.Path.GetFullPath(_fs.Path.Join(ViewModelLocator.ModOrder.SelectedProfile.FilePath, "Savegames"));
		}

		var startPath = "";
		if (ViewModelLocator.ModOrder.SelectedProfile != null)
		{
			var profilePath = _fs.Path.GetFullPath(_fs.Path.Join(ViewModelLocator.ModOrder.SelectedProfile.FilePath, "Savegames"));
			var storyPath = _fs.Path.Join(profilePath, "Story");
			if (_fs.Directory.Exists(storyPath))
			{
				startPath = storyPath;
			}
			else
			{
				startPath = profilePath;
			}
		}

		var pickFile = await _dialogs.OpenFileAsync(new(
			Loca.Picker_RenameSave_FileInput_Title,
			GetInitialStartingDirectory(startPath),
			[CommonFileTypes.LarianSaveFile]));

		if (pickFile.Success)
		{
			var rootFolder = _fs.Path.GetDirectoryName(pickFile.File);
			var rootFileName = _fs.Path.GetFileNameWithoutExtension(pickFile.File);
			PathwayData.LastSaveFilePath = rootFolder;

			var renameFile = await _dialogs.SaveFileAsync(new(
				Loca.Picker_RenameSave_FileOutput_Title,
				rootFolder,
				[CommonFileTypes.LarianSaveFile],
				rootFileName + "_1.lsv",
				true));

			if (renameFile.Success)
			{
				AppServices.Get<IFileSystemService>()!.Directory.CreateDirectory(rootFolder);

				rootFolder = _fs.Path.GetDirectoryName(renameFile.File);
				PathwayData.LastSaveFilePath = rootFolder;
				DivinityApp.Log($"Renaming '{pickFile.File}' to '{renameFile.File}'.");

				if (SaveTools.RenameSave(pickFile.File, renameFile.File))
				{
					try
					{
						var previewImage = _fs.Path.Join(rootFolder, rootFileName + ".WebP");
						var renamedImage = _fs.Path.Join(rootFolder, _fs.Path.GetFileNameWithoutExtension(renameFile.File) + ".WebP");
						if (_fs.File.Exists(previewImage))
						{
							_fs.File.Move(previewImage, renamedImage);
							DivinityApp.Log($"Renamed save screenshot '{previewImage}' to '{renamedImage}'.");
						}

						var originalDirectory = _fs.Path.GetDirectoryName(pickFile.File);
						var desiredDirectory = _fs.Path.GetDirectoryName(renameFile.File);

						if (profileSavesDirectory.IsValid() && desiredDirectory.IsValid() && FileUtils.IsSubdirectoryOf(profileSavesDirectory, desiredDirectory))
						{
							if (originalDirectory == desiredDirectory)
							{
								var dirInfo = _fs.DirectoryInfo.New(originalDirectory);
								if (dirInfo != null && dirInfo.Parent != null && dirInfo.Name.Equals(_fs.Path.GetFileNameWithoutExtension(pickFile.File)))
								{
									desiredDirectory = _fs.Path.Join(dirInfo.Parent.FullName, _fs.Path.GetFileNameWithoutExtension(renameFile.File));
									RecycleBinHelper.DeleteFile(pickFile.File, false, false);
									_fs.Directory.Move(originalDirectory, desiredDirectory);
									DivinityApp.Log($"Renamed save folder '{originalDirectory}' to '{desiredDirectory}'.");
								}
							}
						}

						_globalCommands.ShowAlert(Loca.Alert_Success_RenameSave.SafeFormat($"Successfully renamed '{pickFile.File}' to '{renameFile.File}'", pickFile.File, renameFile.File), AlertType.Success, 15);
					}
					catch (Exception ex)
					{
						DivinityApp.Log($"Failed to rename '{pickFile.File}' to '{renameFile.File}':\n" + ex.ToString());
					}
				}
				else
				{
					DivinityApp.Log($"Failed to rename '{pickFile.File}' to '{renameFile.File}'");
				}
			}
		}
	}

	public void CheckForUpdates(bool force = false, bool skipTimeCheck = false)
	{
		var appUpdateVM = ViewModelLocator.AppUpdate;

		Settings.LastUpdateCheck = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (!force)
		{
			if (skipTimeCheck || Settings.LastUpdateCheck == -1 || (DateTimeOffset.Now.ToUnixTimeSeconds() - Settings.LastUpdateCheck >= 43200))
			{
				appUpdateVM.ScheduleUpdateCheck();
			}
		}
		else
		{
			appUpdateVM.ScheduleUpdateCheck(true);
		}
	}

	public async Task OnViewActivated(MainWindow window)
	{
		Window = window;

		var canExtractAdventure = ViewModelLocator.ModOrder.WhenAnyValue(x => x.SelectedAdventureMod).Select(x => x != null && !x.IsLooseMod && !x.IsLarianMod);
		//Keys.ExtractSelectedAdventure.AddAsyncAction(ExtractSelectedAdventure, canExtractAdventure);

		ViewModelLocator.DeleteFiles.WhenAnyValue(x => x.IsVisible).ToUIProperty(this, x => x.IsDeletingFiles);
		ViewModelLocator.ModUpdates.WhenAnyValue(x => x.TotalUpdates, total => total > 0).BindTo(this, x => x.ModUpdatesAvailable);

		//ViewModelLocator.ModUpdates.CloseView = new Action<bool>((bool refresh) =>
		//{
		//	ViewModelLocator.ModUpdates.Clear();
		//	//TODO Replace with reloading the individual mods that changed
		//	if (refresh) Dispatcher.UIThread.Invoke(RefreshStart, DispatcherPriority.Background);
		//	Window.Activate();
		//});

		var loaded = await LoadSettings();
		if(!loaded)
		{
			SaveSettings();
		}

		AppServices.Get<WindowManagerService>().RestoreSavedWindowPosition();

		LoadInitial();
	}

	public void LoadInitial()
	{
		if (!IsInitialized)
		{
			RefreshStart();
		}
	}

	private readonly MainWindowExceptionHandler exceptionHandler;

	private void DeleteMods(IEnumerable<IModEntry> targetMods, bool isDeletingDuplicates = false, IEnumerable<ModData>? loadedMods = null)
	{
		var deleteFilesView = ViewModelLocator.DeleteFiles;
		if (!deleteFilesView.IsVisible)
		{
			var targetUUIDs = targetMods.Select(x => x.UUID).ToHashSet();

			List<ModFileDeletionData> deleteFilesData = [];
			foreach (var entry in targetMods)
			{
				var data = ModFileDeletionData.FromModEntry(entry, isDeletingDuplicates, loadedMods ?? AppServices.Mods.AllMods);
				if (data != null)
				{
					deleteFilesData.Add(data);
				}
			}
			deleteFilesView.IsDeletingDuplicates = isDeletingDuplicates;
			deleteFilesView.Files.AddRange(deleteFilesData);

			if(!IsRefreshing)
			{
				Views.SwitchToDeleteView();
			}
		}
	}

	private async Task ExtractSelectedMods_ChooseFolder()
	{
		var result = await _dialogs.OpenFolderAsync(new(
			Loca.Picker_ExtractSelectedMods_Title,
			GetInitialStartingDirectory(Settings.LastExtractOutputPath)));

		if (result.Success && result.File.IsValid())
		{
			Settings.LastExtractOutputPath = result.File;
			SaveSettings();

			var outputDirectory = result.File;
			DivinityApp.Log($"Extracting selected mods to '{outputDirectory}'.");

			var targetMods = _manager.SelectedPakMods.ToImmutableList();

			var totalWork = targetMods.Count;
			var taskStepAmount = 100d / totalWork;
			Progress.Title = Loca.Progress_ExtractSelectedMods_Title.SafeFormat($"Extracting {totalWork} mods...", totalWork);

			var openOutputPath = result.File;
			var doOpenOutput = true;

			var successes = 0;

			List<string> filesToProcess = [.. targetMods.Where(x => x.FilePath.IsValid()).Select(x => x.FilePath)];
			await Progress.Start(async token =>
			{
				await Parallel.ForEachAsync(filesToProcess, token, async (path, t) =>
				{
					if (t.IsCancellationRequested) return;

					try
					{
						//Put each pak into its own folder
						var pakName = _fs.Path.GetFileNameWithoutExtension(path);
						Progress.WorkText += Loca.Progress_ExtractSelectedMods_ExtractPak.SafeFormat($"Extracting {pakName}...\n", pakName);
						var destination = _fs.Path.Join(outputDirectory, pakName);

						//In case the foldername == the pak name and we're only extracting one pak
						if (totalWork == 1 && _fs.Path.GetDirectoryName(outputDirectory)?.Equals(pakName) == true)
						{
							destination = outputDirectory;
						}
						var success = await FileUtils.ExtractPackageAsync(path, destination, token);
						if (success)
						{
							successes += 1;
							if (totalWork == 1)
							{
								openOutputPath = destination;
							}
						}
					}
					catch (Exception ex)
					{
						DivinityApp.Log($"Error extracting package: {ex}");
					}
					Progress.Value += taskStepAmount;

					if(_settings.ManagerSettings.Confirmations.OpenExtractedModFolder == null)
					{
						var doOpenFolderResult = await AppServices.Interactions.ShowMessageBox.Handle(new(Loca.MessageBox_ExtractSelectedMods_OpenOutputFolder_Title, Loca.MessageBox_ExtractSelectedMods_OpenOutputFolder_Message, InteractionMessageBoxType.YesNo | InteractionMessageBoxType.Remember));
						doOpenOutput = doOpenFolderResult.Result;
						if(doOpenFolderResult.RememberChoice)
						{
							_settings.ManagerSettings.Confirmations.OpenExtractedModFolder = doOpenOutput;
							_settings.QueueSave(_settings.ManagerSettings, TimeSpan.FromMilliseconds(250));
						}
					}
					else
					{
						doOpenOutput = _settings.ManagerSettings.Confirmations.OpenExtractedModFolder == true;
					}
				});
				RxApp.MainThreadScheduler.Schedule(() =>
				{
					if (successes >= totalWork)
					{
						_globalCommands.ShowAlert($"Successfully extracted all selected mods to '{result.File}'", AlertType.Success, 20);
						if(doOpenOutput)
						{
							ProcessHelper.TryOpenPath(openOutputPath, _fs.Directory.Exists);
						}
					}
					else
					{
						_globalCommands.ShowAlert($"Error occurred when extracting selected mods to '{result.File}'", AlertType.Danger, 30);
					}
				});
			}, true);
		}
	}

	private async Task ExtractSelectedMods_Start()
	{
		//var selectedMods = Mods.Where(x => x.IsSelected && !x.IsEditorMod).ToList();

		if (_manager.SelectedPakMods.Count == 1)
		{
			await ExtractSelectedMods_ChooseFolder();
		}
		else
		{
			var selectedModNames = string.Join("\n", _manager.SelectedPakMods.Select(x => $"{x.DisplayName}"));
			var msg = Loca.MessageBox_ExtractSelectedMods_Message.SafeFormat($"Extract the following mods?\n{selectedModNames}", selectedModNames);
			var result = await _interactions.ShowMessageBox.Handle(new(Loca.MessageBox_ExtractSelectedMods_Title, msg, InteractionMessageBoxType.YesNo));
			if (result)
			{
				await ExtractSelectedMods_ChooseFolder();
			}
		}
	}

	private async Task ExtractSelectedAdventure()
	{
		var mod = ViewModelLocator.ModOrder.SelectedAdventureMod;

		if (mod == null || mod.IsLooseMod || mod.IsLarianMod || !_fs.File.Exists(mod.FilePath))
		{
			var displayName = mod != null ? mod.DisplayName : "";
			_globalCommands.ShowAlert(Loca.Alert_Warning_ExtractSelectedAdventure_NotExtratable.SafeFormat($"Current adventure mod '{displayName}' is not extractable", displayName), AlertType.Warning, 30);
			return;
		}

		var result = await _dialogs.OpenFolderAsync(new(Loca.Picker_ExtractSelectedAdventure_Title,
			GetInitialStartingDirectory(Settings.LastExtractOutputPath)));

		if (result.Success && result.File.IsValid())
		{
			Settings.LastExtractOutputPath = result.File;
			SaveSettings();

			var outputDirectory = result.File;
			DivinityApp.Log($"Extracting adventure mod to '{outputDirectory}'.");

			Progress.Title = Loca.Progress_ExtractSelectedAdventure_Title.SafeFormat($"Extracting {mod.DisplayName}...", mod.DisplayName);

			var openOutputPath = result.File;

			var path = mod.FilePath;
			var success = false;

			await Progress.Start(async token =>
			{
				try
				{
					var pakName = _fs.Path.GetFileNameWithoutExtension(path);
					Progress.WorkText = Loca.Progress_ExtractSelectedMods_ExtractPak.SafeFormat($"Extracting {pakName}...", pakName);
					var destination = _fs.Path.Join(outputDirectory, pakName);
					if (_fs.Path.GetDirectoryName(outputDirectory)?.Equals(pakName) == true)
					{
						destination = outputDirectory;
					}
					openOutputPath = destination;
					success = await FileUtils.ExtractPackageAsync(path, destination, token);
				}
				catch (Exception ex)
				{
					DivinityApp.Log($"Error extracting package: {ex}");
				}

				RxApp.MainThreadScheduler.Schedule(() =>
				{
					if (success)
					{
						_globalCommands.ShowAlert(Loca.Alert_Success_ExtractSelectedAdventure.SafeFormat($"Successfully extracted adventure mod to '{result.File}'", result.File), AlertType.Success, 20);
						ProcessHelper.TryOpenPath(openOutputPath, _fs.Directory.Exists);
					}
					else
					{
						_globalCommands.ShowAlert(Loca.Alert_Error_ExtractSelectedAdventure.SafeFormat($"Error occurred when extracting adventure mod to '{result.File}'", result.File), AlertType.Danger, 30);
					}
				});
			}, true);
		}
	}

	private void LoadAppConfig()
	{
		AppSettingsLoaded = false;
		if (!_settings.TryLoadAppSettings(out var ex))
		{
			_globalCommands.ShowAlert(Loca.Alert_Error_LoadAppConfig.SafeFormat($"Error loading app settings: {ex?.Message}", ex?.Message), AlertType.Danger);
			return;
		}
		AppSettingsLoaded = true;
	}

	private void CreateSteamApiTextFile(string exePath)
	{
		var binFolder = _fs.Path.GetDirectoryName(exePath);
		// Steam folder check essentially
		var steamApiDLL = _fs.Path.Join(binFolder, "steam_api64.dll");
		var steamAppIdPath = _fs.Path.Join(binFolder, "steam_appid.txt");
		if (!_fs.File.Exists(steamAppIdPath) && _fs.File.Exists(steamApiDLL))
		{
			_fs.File.WriteAllText(steamAppIdPath, DivinityApp.STEAM_APPID);
			_globalCommands.ShowAlert(Loca.Alert_Success_CreateSteamApiTextFile.SafeFormat($"Skip Launcher - Created '{steamAppIdPath}'", steamAppIdPath), AlertType.Success, 10);
		}
	}

	private void RemoveSteamApiTextFile(string exePath)
	{
		var binFolder = _fs.Path.GetDirectoryName(exePath);
		var steamAppidPath = _fs.Path.Join(binFolder, "steam_appid.txt");
		if (_fs.File.Exists(steamAppidPath))
		{
			try
			{
				_fs.File.Delete(steamAppidPath);
				_globalCommands.ShowAlert($"Skip Launcher - Deleted '{steamAppidPath}'", AlertType.Danger, 10);
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Failed to delete '{steamAppidPath}':\n{ex}");
			}
		}
	}

	public MainWindowViewModel(
		IPathwaysService pathwaysService,
		ISettingsService settingsService,
		ModImportService modImportService,
		IModManagerService modManagerService,
		IModUpdaterService modUpdaterService,
		INexusModsService nexusModsService,
		IInteractionsService interactionsService,
		IEnvironmentService environmentService,
		IFileSystemService fileSystemService,
		IGlobalCommandsService globalCommands,
		IDialogService dialogsService
		)
	{
		_importer = modImportService;
		_pathways = pathwaysService;
		_manager = modManagerService;
		_updater = modUpdaterService;
		_settings = settingsService;
		_nexusMods = nexusModsService;
		_interactions = interactionsService;
		_environment = environmentService;
		_fs = fileSystemService;
		_dialogs = dialogsService;
		_globalCommands = globalCommands;

		//var canCancelProgress = RefreshCommand.IsExecuting.CombineLatest(this.WhenAnyValue(x => x.CanCancelProgress)).Select(x => x.First && x.Second);

		//CancelMainProgressCommand = ReactiveCommand.Create(() =>
		//{
		//	if (MainProgressToken != null && token.CanBeCanceled)
		//	{
		//		token.Register(() => { MainProgressIsActive = IsRefreshing = false; });
		//		MainProgressToken.Cancel();
		//	}
		//}, canCancelProgress);

		//_keys = new AppKeys(this);
		//_keys.SaveDefaultKeybindings();

		Router = new RoutingState();

		//var progressIsActive = this.WhenAnyValue(x => x.Router.CurrentViewModel).OfType<IProgressBarViewModel>();
		this.WhenAnyValue(x => x.IsRefreshing, x => x.IsDragging)
			//.CombineLatest(progressIsActive)
			.Select(x => x.Item1 || x.Item2)
			.BindTo(this, x => x.IsLocked);
		this.WhenAnyValue(x => x.IsLocked, x => x.IsInitialized, (b1, b2) => !b1 && b2).BindTo(this, x => x.AllowDrop);

		interactionsService.DeleteMods.RegisterHandler(input =>
		{
			var data = input.Input;
			if (IsRefreshing) Progress.NextView = ViewModelLocator.DeleteFiles;
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				DeleteMods(data.TargetMods, data.IsDeletingDuplicates, data.LoadedMods);
			});
			input.SetOutput(true);
		});

		interactionsService.DeleteSelectedMods.RegisterHandler(input =>
		{
			return Observable.Start(() =>
			{
				var data = input.Input;
				var selectedMods = new List<IModEntry>();
				var modOrder = ViewModelLocator.ModOrder;
				foreach (var mod in modOrder.InactiveMods)
				{
					if(mod.IsSelected)
					{
						selectedMods.Add(mod);
					}
				}
				foreach (var mod in modOrder.ActiveMods)
				{
					if(mod.IsSelected)
					{
						selectedMods.Add(mod);
					}
				}
				foreach (var mod in modOrder.OverrideMods)
				{
					if(mod.IsSelected)
					{
						selectedMods.Add(mod);
					}
				}
				DeleteMods(selectedMods, false);
				input.SetOutput(true);
			}, RxApp.MainThreadScheduler);
		});

		DownloadBar = new DownloadActivityBarViewModel();

		exceptionHandler = new MainWindowExceptionHandler(this);
		RxApp.DefaultExceptionHandler = exceptionHandler;

		Version = environmentService.AppVersion.ToString();
		AppServices.Locale.EntryToObservable(nameof(Resources.Window_Main_Title)).Select(x => x.SafeFormat($"{environmentService.AppProductName} {Version}", Version)).BindTo(this, x => x.Title);
		DivinityApp.Log($"{Title} initializing...");

		AppServices.AppUpdater.Configure(DivinityApp.GITHUB_USER, DivinityApp.GITHUB_REPO, DivinityApp.GITHUB_RELEASE_ASSET);

		_nexusMods.WhenAnyValue(x => x.DownloadProgressValue, x => x.DownloadProgressText, x => x.CanCancel).Subscribe(x =>
		{
			DownloadBar.UpdateProgress(x.Item1, x.Item2);
			if (x.Item3)
			{
				DownloadBar.CancelAction = () => _nexusMods.CancelDownloads();
			}
			else
			{
				DownloadBar.CancelAction = null;
			}
		});

		this.WhenAnyValue(x => x.Settings.UpdateSettings.NexusModsAPIKey).BindTo(nexusModsService, x => x.ApiKey);

		IDisposable? importDownloadsTask = null;
		_nexusMods.DownloadResults
		.ToObservableChangeSet()
		.CountChanged()
		.ThrottleFirst(TimeSpan.FromMilliseconds(50))
		.Subscribe(f =>
		{
			importDownloadsTask?.Dispose();
			importDownloadsTask = RxApp.TaskpoolScheduler.ScheduleAsync(TimeSpan.FromMilliseconds(250), async (sch, token) =>
			{
				var files = _nexusMods.DownloadResults.ToList();
				_nexusMods.DownloadResults.Clear();

				var result = new ImportOperationResults()
				{
					TotalFiles = files.Count
				};
				var builtinMods = DivinityApp.IgnoredMods.Items.ToSafeDictionary(x => x.Folder);
				foreach (var filePath in files)
				{
					await _importer.ImportModFromFile(builtinMods, result, filePath, token, false);
				}

				if (result.Mods.Count > 0 && result.Mods.Any(x => x.NexusModsData.ModId >= DivinityApp.NEXUSMODS_MOD_ID_START))
				{
					await _updater.NexusMods.Update(result.Mods, token);
					await _updater.NexusMods.SaveCacheAsync(false, Version, token);

					await Observable.Start(() =>
					{
						var total = result.Mods.Count;
						if (result.Success)
						{
							if (result.Mods.Count > 1)
							{
								_globalCommands.ShowAlert($"Successfully imported {total} downloaded mods", AlertType.Success, 20);
							}
							else if (total == 1)
							{
								var modFileName = result.Mods.First().FileName;
								var fileNames = string.Join(", ", files.Select(x => _fs.Path.GetFileName(x)));
								_globalCommands.ShowAlert($"Successfully imported '{modFileName}' from '{fileNames}'", AlertType.Success, 20);
							}
							else
							{
								_globalCommands.ShowAlert("Skipped importing mod - No .pak file found", AlertType.Success, 20);
							}
						}
						else
						{
							if (total == 0)
							{
								_globalCommands.ShowAlert("No mods imported. Does the file contain a .pak?", AlertType.Warning, 60);
							}
							else
							{
								_globalCommands.ShowAlert($"Only imported {total}/{result.TotalPaks} mods - Check the log", AlertType.Danger, 60);
							}
						}
					}, RxApp.MainThreadScheduler);
				}
			});
		});

		this.WhenAnyValue(x => x.AppSettings.Features.GitHub, x => x.Settings.UpdateSettings.UpdateGitHubMods).AllTrue()
			.BindTo(_updater.GitHub, x => x.IsEnabled);

		this.WhenAnyValue(x => x.AppSettings.Features.NexusMods, x => x.Settings.UpdateSettings.UpdateNexusMods).AllTrue()
			.BindTo(_updater.NexusMods, x => x.IsEnabled);

		this.WhenAnyValue(x => x.AppSettings.Features.Modio, x => x.Settings.UpdateSettings.UpdateModioMods).AllTrue()
			.BindTo(_updater.Modio, x => x.IsEnabled);

		var whenRefreshing = _updater.WhenAnyValue(x => x.IsRefreshing);
		whenRefreshing.ToUIProperty(this, x => x.UpdatingBusyIndicatorVisibility);
		whenRefreshing.Select(b => !b).ToUIProperty(this, x => x.UpdateCountVisibility);

		this.WhenAnyValue(x => x.Settings.DebugModeEnabled, x => x._settings.ExtenderSettings.DeveloperMode)
		.Select(x => x.Item1 || x.Item2).ToUIProperty(this, x => x.DeveloperModeVisibility);

		#region Keys Setup

		//Keys.OpenCollectionDownloaderWindow.AddAction(() => App.WM.CollectionDownload.Toggle(true));

		//var canStartExport = this.WhenAny(x => x.MainProgressToken, (t) => t != null);
		//Keys.ExportOrderToZip.AddAsyncAction(ExportLoadOrderToArchiveAsync, canStartExport);
		//Keys.ExportOrderToArchiveAs.AddAsyncAction(ExportLoadOrderToArchiveAsAsync, canStartExport);

		/*Keys.OpenDonationLink.AddAction(() =>
		{
			FileUtils.TryOpenPath(DivinityApp.URL_DONATION);
		});

		Keys.OpenRepositoryPage.AddAction(() =>
		{
			FileUtils.TryOpenPath(DivinityApp.URL_REPO);
		});

		Keys.ToggleViewTheme.AddAction(() =>
		{
			Settings.DarkThemeEnabled = !Settings.DarkThemeEnabled;
		});

		Keys.ToggleFileNameDisplay.AddAction(() =>
		{
			Settings.DisplayFileNames = !Settings.DisplayFileNames;

			foreach (var m in _manager.AllMods)
			{
				m.DisplayFileForName = Settings.DisplayFileNames;
			}
		});

		var canDownloadNexusFiles = this.WhenAnyValue(x => x.Settings.UpdateSettings.NexusModsAPIKey, x => x.NexusModsSupportEnabled)
			.Select(x => !string.IsNullOrEmpty(x.Item1) && x.Item2);
		Keys.DownloadNXMLink.AddCanExecuteCondition(canDownloadNexusFiles);
		Keys.DownloadNXMLink.AddAction(() =>
		{
			//App.WM.NxmDownload.Toggle();
		});*/

		#endregion

		Router.CurrentViewModel.Select(x => x == ViewModelLocator.ModUpdates).ToUIProperty(this, x => x.UpdatesViewIsVisible, false);

		var anyPakModSelectedObservable = _manager.SelectedPakMods.ToObservableChangeSet().CountChanged().Select(x => _manager.SelectedPakMods.Count > 0);
		//Keys.ExtractSelectedMods.AddAsyncAction(ExtractSelectedMods_Start, anyPakModSelectedObservable);

		_interactions.ConfirmModDeletion.RegisterHandler(async interaction =>
		{
			var sentenceStart = interaction.Input.PermanentlyDelete ? "Permanently delete" : "Delete";
			var msg = $"{sentenceStart} {interaction.Input.Total} mod file(s)?";

			var result = await _interactions.ShowMessageBox.Handle(new("Confirm Mod Deletion", msg, InteractionMessageBoxType.YesNo));
			interaction.SetOutput(result.Result);
		});

		_interactions.OpenModProperties.RegisterHandler(interaction =>
		{
			var modPropertiesWindow = AppServices.Get<ModPropertiesWindow>();
			if(modPropertiesWindow?.ViewModel != null)
			{
				modPropertiesWindow.ViewModel.SetMod(interaction.Input);
				if (!modPropertiesWindow.IsVisible) modPropertiesWindow.Show(Window);
				interaction.SetOutput(true);
			}
			else
			{
				interaction.SetOutput(false);
			}
		});

		Views = new ViewManager(Router);

		Views.WhenAnyValue(x => x.CurrentView).Subscribe(vm =>
		{
			DivinityApp.Log($"VM: {vm} | {Router}");
		});
	}
}
