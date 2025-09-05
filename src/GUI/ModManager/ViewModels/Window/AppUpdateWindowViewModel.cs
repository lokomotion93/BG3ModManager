using Avalonia.Controls.ApplicationLifetimes;

using ModManager.Util;

using AutoUpdateViaGitHubRelease;

using System.Net.Http;
using System.Text.RegularExpressions;

namespace ModManager.ViewModels;

public partial class AppUpdateWindowViewModel : ReactiveObject, IClosableViewModel, IRoutableViewModel
{
	#region IClosableViewModel/IRoutableViewModel
	public string UrlPathSegment => "appupdate";
	public IScreen HostScreen { get; }
	[Reactive] public bool IsVisible { get; set; }
	public RxCommandUnit CloseCommand { get; }
	#endregion

	[Reactive] public bool CanConfirm { get; set; }
	[Reactive] public bool CanSkip { get; set; }
	[Reactive] public string? SkipButtonText { get; set; }
	[Reactive] public string? UpdateDescription { get; set; }
	[Reactive] public string? ChangelogMarkdownText { get; set; }
	[Reactive] public double ScrollViewerWidth { get; set; }

	public RxCommandUnit ConfirmCommand { get; private set; }
	public RxCommandUnit SkipCommand { get; private set; }


	[GeneratedRegex(@"^\s+$[\r\n]*", RegexOptions.Multiline)]
	private static partial Regex RemoveEmptyLinesRe();

	private static readonly Regex RemoveEmptyLinesPattern = RemoveEmptyLinesRe();

	private bool _openAfterUpdateCheck = false;

	private async Task CheckForUpdatesAsync(IScheduler scheduler, CancellationToken token)
	{
		string markdownText;

		markdownText = await WebHelper.DownloadUrlAsStringAsync(DivinityApp.URL_CHANGELOG_RAW, CancellationToken.None);
		var updater = AppServices.AppUpdater;
		var result = await updater.CheckForUpdatesAsync();

		RxApp.MainThreadScheduler.Schedule(() =>
		{
			if (markdownText.IsValid())
			{
				markdownText = RemoveEmptyLinesPattern.Replace(markdownText, string.Empty);
				ChangelogMarkdownText = markdownText;
			}

			if (result.IsAvailable)
			{
				UpdateDescription = $"{updater.AppTitle} {result.Version} is now available.{Environment.NewLine}You have version {updater.CurrentVersion} installed.";

				CanConfirm = true;
				SkipButtonText = "Skip";
				CanSkip = true;
			}
			else
			{
				UpdateDescription = $"{updater.AppTitle} is up-to-date.";
				CanConfirm = false;
				CanSkip = true;
				SkipButtonText = "Close";
				if(_openAfterUpdateCheck)
				{
					AppServices.Interactions.OpenUpdatesWindow.Handle(true).Subscribe();
				}
			}
			_openAfterUpdateCheck = false;
		});
	}

	public void ScheduleUpdateCheck(bool openWindowAfterwards = false)
	{
		_openAfterUpdateCheck = openWindowAfterwards;
		RxApp.TaskpoolScheduler.ScheduleAsync(CheckForUpdatesAsync);
	}

	private async Task RunUpdateAsync(CancellationToken token)
	{
		var result = await AppServices.AppUpdater.DownloadAndInstallUpdateAsync();
		if(result && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.Shutdown();
		}
	}

	public AppUpdateWindowViewModel(IScreen? host = null)
	{
		HostScreen = host ?? Locator.Current.GetService<IScreen>()!;
		CloseCommand = this.CreateCloseCommand();

		ScrollViewerWidth = 1000;

		CanSkip = true;
		SkipButtonText = "Close";

		var canConfirm = this.WhenAnyValue(x => x.CanConfirm);
		ConfirmCommand = ReactiveCommand.CreateFromTask(RunUpdateAsync, canConfirm, RxApp.MainThreadScheduler);

		var canSkip = this.WhenAnyValue(x => x.CanSkip);
		SkipCommand = ReactiveCommand.Create(() =>
		{
			AppServices.Interactions.OpenUpdatesWindow.Handle(false).Subscribe();
		}, canSkip);
	}
}

public class DesignAppUpdateWindowViewModel : AppUpdateWindowViewModel
{
	public DesignAppUpdateWindowViewModel() : base()
	{
		ChangelogMarkdownText = """
# 1.0.12.3 

## Changes 

* Reworked script extender requirement checks. This should be more informative now.
* Added new icons/highlights for various mod issues:
  * Invalid UUID
  * Missing dependencies
  * Toolkit projects / loose mods (if colorblind support is enabled)
* Added a colorblind support option, to display icons where otherwise a color would be used (toolkit projects).
* Settings are now sorted alphabetically, with path settings sorted to the top.
* When auto-sizing the Name column, icon padding is now included in the estimated width.
* Singularly-selected mods can now be deselected with a left click (before it required CTRL + Left Click).
* Missing dependencies now display the mods that require them.
* Reworked the `Steam - Skip Launcher` setting to instead create a `steam_appid.txt`, which allows you to run bg3 directly.
  * Disabling this option will also delete `steam_appid.txt`, if the settings window is open.
* Added a new `Delete ModCrashSanityCheck` option (enabled by default), which deletes the `%LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\ModCrashSanityCheck` directory.
  * This is a workaround for what appears to be a Hotfix 30 (a.k.a. Patch 8 Hotfix 1) bug, where the presence of this folder makes the game deactivate mods that appear in the in-game mod manager, despite being activated externally.
  * If enabled, this folder will be deleted when exporting your load order.
* Path settings now only trigger saving when you unfocus the textbox (hit the Return/Enter key, or the Escape key, or click outside of the box).
""";
	}
}