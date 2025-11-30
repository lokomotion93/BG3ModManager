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
	[Reactive] public partial bool IsVisible { get; set; }
	public RxCommandUnit CloseCommand { get; }
	#endregion

	[Reactive] public partial string Title { get; set; }
	[Reactive] public partial bool CanConfirm { get; set; }
	[Reactive] public partial bool CanSkip { get; set; }
	[Reactive] public partial string? SkipButtonText { get; set; }
	[Reactive] public partial string? UpdateDescription { get; set; }
	[Reactive] public partial string? ChangelogMarkdownText { get; set; }
	[Reactive] public partial double ScrollViewerWidth { get; set; }

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
				UpdateDescription = Loca.Window_AppUpdate_Available.SafeFormat($"{updater.AppTitle} {result.Version} is now available.\nYou have version {updater.CurrentVersion} installed.", updater.AppTitle, result.Version, updater.CurrentVersion);
				CanConfirm = true;
				SkipButtonText = Loca.Button_Skip;
				CanSkip = true;
			}
			else
			{
				UpdateDescription = Loca.Window_AppUpdate_UpToDate.SafeFormat($"{updater.AppTitle} is up-to-date.", updater.AppTitle);
				CanConfirm = false;
				CanSkip = true;
				SkipButtonText = Loca.Button_Close;
				if (_openAfterUpdateCheck)
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
		Title = Loca.Window_AppUpdate_Title;

		ScrollViewerWidth = 1000;

		CanSkip = true;
		SkipButtonText = Loca.Button_Close;

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
#if DEBUG
		ChangelogMarkdownText = """
# 1.0.12.9 

* Fixed loading if duplicate Data folder loose mods exist (like duplicate toolkit projects). This should display a warning now and continue loading.
* Added more safeguards for various steps of the loading process - If an error occurs, it should still be able to finish loading now.

# 1.0.12.8 

## Fixes 

* Fixed `ScriptExtenderUpdaterConfig.json` UpdateChannel being written as a number.
* Fixed "Open Folder in File Explorer" etc commands when right clicking the current profile.
* Renaming an order (right click order -> Rename, enter new name, hit enter) now renames the underlying file.

# 1.0.12.7 

## Changes 

* `ScriptExtenderSettings.json` and `ScriptExtenderUpdaterConfig.json` are now loaded when initially configuring settings on a fresh install.

## Fixes 

* Fixed an issue with loading occasionally getting stuck at the "Building mod order list" stage.
* Fixed restoring the saved window size/position with non-standard aspect ratios.
* Fixed the "Override AppData Path" not being used when checking for the Script Extender.
* Fixed an issue with settings being reset.
* Fixed override mods being counted on the inactive side (in the selected text at the top)
* Fixed override mods disappearing in the load order
* Fixed deleting override mods.

# 1.0.12.6 

## Changes 

* A warning is displayed if BG3MM is started in admin mode.
* Imported mods are selected (and previous selections are cleared), which should make it easier to identify what you just imported.
* Deprecated options should be migrated when settings are loaded.

### Launch Game - Action  

The `Steam - Skip Launcher` and `Launch Through Steam` options have been consolidated into a new `Launch Game - Action` option.  
This option allows you to specify how to launch the game:  
* Default (Exe) - Run the exe directly. This will create a `steam_appid.txt` automatically, if needed.
* Steam - Run the game through Steam, using the steam protocol.
* Custom - Run a given file or protocol, with optional arguments. This allows you to launch the game through other means, such as through Playnite.

## Fixes 

* The initial update check now happens in the background.
* Fixed override mods getting their UUIDs validated.
* Fixed "Save Window Position" setting not immediately saving the window size/position.
  * Made the window size/position save when resizing the window (previously it was just position and state change, like maximizing).
  * Made the window size/position save on quit.
  * The window will use the default size (1600x800) if no size was specified in the settings.
* Fixed imported mods of existing entries being moved to the inactive side.

# 1.0.12.5 

## Changes 

* Added additional dice sets to `IgnoredMods.json`.
* Changed "missing dependencies" icon.

## Fixes 

* Fixed an issue with getting the initial game exe path if not set.
* Fixed some base mods getting detected as missing (CrossplayUI, MainUI).

# 1.0.12.4 

## Changes 

* The ModCrashSanityCheck folder is now deleted when launching the game (if `Delete ModCrashSanityCheck` is enabled).
* Tweaked single-click deselection to better support dragging (this only matters when a single mod is selected).
* Added new Script Extender `InsanityCheck` option for `ScriptExtenderSettings.json` - This deletes the `ModCrashSanityCheck` folder when entering the main menu.
* Added Script Extender `Nightly` update channel.

## Fixes 

* Fixed an issue with imported mods not being activated if dragged into the active order.
* Fixed dropping mods into an empty list - previously their order would get shifted incorrectly.

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

## Fixes 

* Fixed relative path for the Orders directory not working when the current working directory differs from the application path.
  * Relative paths for the AppData path override work now.
* Fixes subfolders being scanned when parsing paks from the Data directory.
* Fixed the extender download highlight (Tools -> Download & Extract the Script Extender is highlighted if it's required and not installed).
* Fixed hyperlinks not opening.
* Fixed the logs folder shortcut not working.
* Added support for opening paths using environment variables (ex. `%LOCALAPPDATA%`).
* Fixed an issue with loading paks from the data folder, if you had a large amount.

# 1.0.12.2 

* Fixed unintended region-locking (swapped back to previous auto-updater).
* Added a "Tools -> Stop Speech" command to stop the "Tools -> Speak Active Order" command.
* Tweaked application directory retrieval method, which may help proton users (needs confirmation).
  * The directory is fetched using the current process, instead of the probing path. This shouldn't affect Windows users.

# 1.0.12.1 

* Fixed mods not appearing (`LSLibNative.dll` failing to load). If you still get this issue, make sure you have the [C++ runtime](https://aka.ms/vs/17/release/vc_redist.x64.exe) installed.
* Fixed screen reader support (still a bit WIP, but Tools -> Speak Active Order should work once more).
* Added PhotoMode / CrossPlatformUI to `IgnoredMods.json`.

# 1.0.12.0 

This is an update to .NET 8, which will require a new [runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.15-windows-x64-installer) (this is most likely already installed if you're using a newer version of Windows 10/11).

Download the .NET 8 runtime from Microsoft here:
[https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.15-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.15-windows-x64-installer)

## Changes 

* Updated program from .NET Framework 4.8 to .NET 8.0.
* Added BG3 Patch 8 base mods to `IgnoredMods.json`.
* The last exported order is now automatically saved as a separate json (`LastExported.json`), and a prompt will ask if you want to restore it if your "Current" order gets cleared.
* Swapped from GustavDev to GustavX for the "base" Larian campaign mod (this is a patch 8 change).

## Fixes 

* Fixed importing orders from saves.
  * Updated lslib to the latest version (this fixed save importing).
* Fixed an issue that could arise if a mod is both a pak in the Data folder and a toolkit project. Duplicates are now detected across the Data folder and Mods folder.
* Fixed an issue where dropping mods into an index-sorted view results in unsorted items.

# 1.0.11.1 

* Fixed mod conflicts loading as dependencies.
* Added a "Clear" option when right clicking textboxes.

## Tooltip Changes  

* Conflicting mods (going by what a mod provides in their metadata) are now displayed in tooltips (that's all for now, as this is new metadata Larian added).
* Mods displayed under Dependencies and Conflicts in tooltips will try and fetch the mod's actual name, if that mod exists. Previously it used the name provided in the metadata, which could differ from the actual display name.
* Dependency/conflicts in tooltips are now sorted by Base Mod > Display Name, i.e. GustavDev/Main will display at the top, and other mods below it will be sorted alphabetically.

# 1.0.11.0 

## Patch 7 Support  

* Updated modsettings.lsx for the latest changes.
* Added "ModBrowser" to the list of ignored mods (this is a builtin Larian mod).
* Fixed an issue with loading getting stuck on loading profiles.
* Fixed the "Main" campaign not showing up in the campaign dropdown (mods no longer have a "Type", i.e. Add-on vs Adventure).

# 1.0.10.0 

* The Windows username path in the log is now replaced with `%USERPROFILE%`, while the local appdata path is replaced with `%LOCALAPPDATA%`. This is to obscure usernames when sharing logs.
* Fixed the Version Generator Window defaulting to the wrong value for `1.0.0.0`.
* Fixed dialog window starting in the relative Orders folder.
* Fixed the updates window not showing when manually checking for updates, when the mod manager is already up-to-date.
* Fixed importing json load orders from archives.
* Fixed a bug with deleting load orders.
* Orders saved with "Save As..." are now added to the orders dropdown.
* The `Game/GUI/Assets` directory is now ignored as a whole. This will fix certain mods displaying as having file overrides, when it's just adding new class icons.
* Removed displaying Modes/Targets (not really needed in BG3 currently).
* Added a new splash screen for when the mod manager is starting.
* Duplicate mod paks are now detected, and a delete view is forced open, to display older versions of mods that should be removed. This is to help fix issues where the game resets the mod order do to having multiple paks with the same UUID.
* Tabs in the Settings -> Preferences window now have tooltips explaining what the related file is, and you can right click them for shortcuts to the related files.
* The Updater tab (visible if Mod Developer Mode or the extender Developer Mode is enabled) can now immediately download the script extender when selecting a specific version. Available versions are fetched from the online manifest.
* A new command-line program has been added to a new Tools folder (Tools/Toolbox.exe). This is used to automatically update the script extender without having to start the game.
  * Toolbox requires [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.13-windows-x64-installer)

# 1.0.9.11

* Fixed the version number in the Version Generator tool window not updating when changing the numbers.

# 1.0.9.10

* Fixed the version string not updating past the first number (ex. 5.0.0.0 when it should be 5.7.23.56).

# 1.0.9.9

* Fixed the Version column being empty.
* Mod tooltips (hover over the mod's name) will now display the version, whether it requires the extender, and if it's an editor project (green highlight) or override mod.
* Fixed BG3MM relaunching in administrator mode after updating, which prevent drag & dropping files onto the window. The updater will likely open the update to 1.0.9.9 in admin mode, so restart the program afterwards.

# 1.0.9.8

* Fixed the "last import directory" being reset.
* Fixed fetching the currently installed extender version (if any).
* Updated lslib version to allow parsing v7 files (such as saves).
* The Import Mod option now supports importing .gzip, .rar, .tar, and .tar.gz archives, alongside .bz2, .xz, and .zst compressed files.
* Mods now display if they have a "Mod Fixer" script.
* Mods with file overrides will now display up to 10 of those files in the mod's tooltip.
* Added additional Script Extender settings, including a new settings tab for `ScriptExtenderUpdaterConfig.json`.
* Preferences in the settings window will now automatically save when you make changes.
* Enabled compiler optimizations (this was mistakenly disabled in release builds, but it may not have any noticeable effect).

# 1.0.9.7

* Fixed the game closing after it's launched - Turns out it needs the "current working directory" to be in the bin folder.
* Fixed override mods that are in the load order not being visible after refreshing.
* If a profile display name is empty, it now defaults to the folder name.
* Launcher preferences should now only update if one of the "disable launcher" options is set, like "Disable Launcher Telemetry". They will also be updated if mods are active (telemetry / the mod warning should be disabled).
* Mod paks and archives (zip, 7z) can now be dropped into the active or inactive mod list to import mods.

# 1.0.9.6

* Fixed an issue preventing dragging mods.
* The auto-resizing of the "Name" column in the Active Mods list will now include the names in the Overrides list as well.

# 1.0.9.5

* The "Script Extender" settings tab is now hidden unless the [script extender](https://github.com/Norbyte/bg3se/releases/latest) is installed.
* Added DiceSet_04 to IgnoredMods.json (added in Patch 1 as a dependency of Gustav).
* Added additional GUI icon folders to ignore in IgnoredMods.json.
* A mod's script extender requirements are now checked after importing it.
* Fixed an issue with exporting load orders to archives that was causing the load order itself to be empty.
* Fixed importing/exporting mods with non-standard characters in the name.
* Override mods are now included when exporting the load order to an archive.
* Fixed some override mods with a meta.lsx being positionable within the load order, like ImprovedUI. ImprovedUI in particular is only overwriting files, so this doesn't need to be in the load order.
* Added an option to disable warnings in the launcher about mods existing.
* Added an option to launch the game through Steam.
* Fixed launcher telemetry not being disabled when exporting a load order with mods (may not be needed anymore).
* Override mods with metadata (meaning it has a name / author etc) can be force-added to the load order by right clicking it and picking "Allow in Load Order". Generally speaking, this isn't needed, but if you want your save to track that the mod was active, this option allows that.
* Unhandled exceptions should now display in a message box, and the log will automatically enable to save the error information. 
* "Export Order to Text File" will now include override mods.

# 1.0.9.4

* Fix for relative paths starting in _Lib instead of where the exe is.

# 1.0.9.3

* Quick fix for certain paths being relative to the "current working directory".

# 1.0.9.2

## Changes

* Fixed relative file loading being relative to the current working directory, instead of relative to the exe.
* Fixed importing 7z archives.
* Fixed imported override mods being added to the inactive list.
* `Public/Game/GUI/Assets/Tooltips` is now ignored as an "override" folder (ex. BagsBagsBags shouldn't be highlighted now). Additional paths can be set in IgnoredMods.json.
* Fixed adding fallback base mod data if Data path is wrong.
* Re-enabled Script Extender features (experimental).

# 1.0.9.1

## Changes

* Removed the Tools -> Download Script Extender option (outdated currently).
* The selected profile will now always default to "Public", since other profiles aren't used by the game.
* If the game data folder is still not found, the campaign data should default to the GustavDev entry in "IgnoredMods.json".
* The "Save Window Location" preferences option is now disabled by default, due to some specific monitor setuping having an issue.  This can be altered in `Data\settings.json` if it needs to be changed externally.
* Added a hint for the `-continueGame` launch param, under Preferences -> Advanced. This game launch param makes the game load your last save automatically.
* Manually checking for updates will now display an alert to indicate that it's working.
* Updated lslib version.

# 1.0.9.0

## Changes

* Added support for displaying override mods (.paks that ony override files and have no metadata).
* Added support for rearranging override mods that also include their own metadata / files.
* Added a "Save Window Settings" option to the preferences window.
* Added a "Skip Launcher" option to the preferences window (defaults to being enabled), in lieue of Hotfix #2 forcing the launcher open when opening the game exe.
* Added a dialog window to set the game installation directory if the mod manager is unable to find it through Steam/GoG.
* Added a warning if the game data folder is not correctly set.
* Imported mods no longer require a refresh to be visible.
* Fixed the DX11 setting not actually being implemented.
* Fixed an issue where deleted mods weren't being removed from the UI.
* Fixed (Definitive Edition) being visible in mod tooltips.

# 1.0.8.1

## Changes

* Made only the main Larian campaign mod display in the Campaign selection box.
* Fixed dependencies of the main campaign getting added to the load order, which could result in issues.
* Add automatic migration for past profiles from the old campaign mod Gustav to GustavDev (Main).

# 1.0.8.0

## Changes

* Updated lslib to the latest release (1.18.0).
* Updated to the lastest upstream (Divinity Mod Manager).
* Fixed keybindings not being visible.
* Fixed profile-loading not using the last profile.
* Fixed duplicate dependencies being added to the mod order when exporting.

# 1.0.7.1

## Changes

* Fixed importing zips failing with the "Cannot determine compressed stream type" error.
* Fixed .zip files not being visible in the Import Mods dialog window for "All formats".
* Fixed .json files in mod zips being imported as load orders, when running the File -> Import Mods command.

# 1.0.7.0

## Changes

* Updated to the latest patch and LSLib (fixes playerProfiles errors).
* Non-Larian mods located in the Data folder will now be visible, like regular mods.
* Mods ending in a _number.pak (like MyMod_2022.pak) will no longer be ignored if they're not a "partial" pak (Textures_1.pak, Textures_2.pak etc).
* Mod tooltips will now always show, even if the mod description is empty.
* Mods can now be "imported" by file (.pak), which essentially just copies them to the documents mods folder.
* Fixed line breaks in certain settings tooltips being displayed as `&#x0a;`.
* Disabled the first mod in the active mod list always being selected. This was initially done so you could easily nagivate with a keyboard, but simply pressing tab (or left/right) will shift focus to a list, and allow arrow key navigation.  
  * Pressing the up or down arrow keys, when no mods are selected, will now select the first item in the active list.

# 1.0.6.1

* Quick fix for the enter key no longer moving mods between the active or inactive list.

# 1.0.6.0

## Changes

* Updated to the latest Patch 7 changes:
  * Larian changed Baldur Gate 3's folder from `Documents\Larian Studios\Baldur's Gate 3` to `AppData\Local\Larian Studios\Baldur's Gate 3`.
  * The launcher preferences folder went from `AppData\Local\LarianStudios\Launcher\Settings\preferences.json` to ``AppData\Local\Larian Studios\Launcher\Settings\preferences.json`.
* Updated various libraries.
* Updated LSLib usage (ModPathVisitor needed a game set).
* Load order files are now saved in the exported .zip, when exporting mod orders to an archive.
* CTRL + S should now work in the preferences window.

# 1.0.5.2

## Changes  

* Quick tweak to support mods using the id "Version" for their version node (technically a typo nowadays, since it should be "Version64").

# 1.0.5.1

## Changes  

* Fixed incorrect conversion for the revision part of a version number (0xFFFF vs 0x7FFFF, thanks Zerd).
* Updated package dependencies.

# 1.0.5.0

## Changes  

* Fix for the wrong node name being checked when loading mod versions (Version->Version64).
* Fix for the "active profile" not being saved.
* Tweak to make sure profile6.lsf is loaded when loading profiles (new Patch6 profile file, otherwise the previous ones are checked).
* Disabled some unused settings for BG3 (GM-related).
* Disabled the "Mods Require the Script Extender" warning, due to it not being quite ready yet for Patch 6.

# 1.0.4.1

## Changes  

* Fix for the active profile export error.
  * Larian changed the player profile file again to playerprofiles6.lsf for Patch 6, so new folder setups are lacking the previous playerprofiles.lsb or playerprofiles5.lsb.

# 1.0.4.0

## Changes  

Various minor updates.

* Updated to the latest game version.
* Updated lslib usage/version.
* Updated to latest Divinity Mod Manager build.

# 1.0.3.1

## Changes  

* Updated profile loading to reflect the new Patch 5 changes (profile5.lsb, playerProfiles5.lsb).
* Updated modsettings.lsx template for the new Patch 5 changes (Version64 for instance).
* Mods whose version is 1 or 268435456 (old 1.0.0.0 value) will now be exported with 36028797018963968 (new format's value). Might fix some things, or be completely inconsequential.

# 1.0.3.0

## Changes  

* Implemented a custom update window that can render markdown (and this changelog, but you won't see that until after this update).
* The name column is no longer auto-resized in the active mods list if you've manually resized it.
* The Extender Settings tab is now always visible, even if the extender isn't installed.
* The BG3 Script Extender can now be downloaded/installed with the usual Tools button.
* Updated version display to support the new uint64 change for version numbers.

## Fixes  

* Fixed the "Download Extender" button not working ([#12](https://github.com/LaughingLeader-DOS2-Mods/DivinityModManager/issues/12)).
* Fixed startup issue when no GMCampaigns folder is available ([#13](https://github.com/LaughingLeader-DOS2-Mods/DivinityModManager/issues/13)).
Even though this isn't available (yet?) for BG3, this change was important to get startup to work properly.

# 1.0.2.1
* Updated LSLib to fix pak extraction from the latest game update.

# 1.0.2.0
* Merged in changes from main repo (Divinity Mod Manager).

# 1.0.1.0
* Fix for bg3_dx11.exe not launching if "Enable DirectX Mode" is set.
* Set the process working directory when launching the game exe. This should allow hooks to work (DXGI.dll from the script extender for instance).
* Added a new tools window under Tools -> Toggle Version Generator. Mod authors can use this window to generate proper version numbers for their mods.
* Added progress messages for each mod loading stage (documents mods, editor mods, base game mods, etc).

# 1.0.0.0

Initial release.

# 1.0.12.0 (Upcoming)

This is an update to .NET 8, which will require a new [runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.11-windows-x64-installer) (this is most likely already installed if you're using a newer version of Windows 10/11).

Download the .NET 8 runtime from Microsoft here:
[https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.11-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.11-windows-x64-installer)

## Changes 

* Updated program from .NET Framework 4.8 to .NET 8.0.
* Fixed importing orders from saves not working after Patch 7.
  * Updated lslib to the latest version (this fixed the save parsing).
* Fixed an issue that could arise if a mod is both a pak in the Data folder and a toolkit project. Duplicates are now detected across the Data folder and Mods folder.
* The last exported order is now automatically saved as a separate json (`LastExported.json`), and a prompt will ask if you want to restore it if your "Current" order gets cleared.
""";
#endif
	}
}