using ModManager.Services;
using ModManager.ViewModels;
using ModManager.ViewModels.Main;
using ModManager.ViewModels.Mods;
using ModManager.ViewModels.Settings;
using ModManager.ViewModels.Window;
using ModManager.Views;
using ModManager.Views.Main;
using ModManager.Windows;

namespace ModManager;

public static class AppServices
{
	public static ISettingsService Settings => Get<ISettingsService>()!;
	public static INexusModsService NexusMods => Get<INexusModsService>()!;
	public static IModManagerService Mods => Get<IModManagerService>()!;
	public static IPathwaysService Pathways => Get<IPathwaysService>()!;
	public static IModUpdaterService Updater => Get<IModUpdaterService>()!;
	public static IAppUpdaterService AppUpdater => Get<IAppUpdaterService>()!;
	public static ModImportService ModImporter => Get<ModImportService>()!;

	public static IGlobalCommandsService Commands => Get<IGlobalCommandsService>()!;
	public static IInteractionsService Interactions => Get<IInteractionsService>()!;
	public static IDialogService Dialog => Get<IDialogService>()!;
	public static IScreenReaderService ScreenReader => Get<IScreenReaderService>()!;
	public static AppKeysService Keybindings => Get<AppKeysService>()!;

	public static IEnvironmentService Env => Get<IEnvironmentService>()!;
	public static IFileSystemService FS => Get<IFileSystemService>()!;
	public static IRegistryService Reg => Get<IRegistryService>()!;
	public static IFileWatcherService FileWatcher => Get<IFileWatcherService>()!;
	public static IDirectoryOpusService Dopus => Get<IDirectoryOpusService>()!;

	public static ILocaleService Locale => Get<ILocaleService>()!;

	public static ControlFactoryService ControlFactory => Get<ControlFactoryService>()!;

	public static void Initialize()
	{
#if !DEBUG
		if(DivinityApp.GetAppDirectory("debug").IsExistingFile())
		{
			Get<LogWriterService>().ToggleLogging(true);
		}
#endif
	}

	static AppServices()
	{
		var resolver = AppLocator.CurrentMutable;
		resolver.AddCommonServices();
		resolver.AddAppServices();

		SplatRegistrations.RegisterConstant<IBackgroundCommandService>(new BackgroundCommandService(DivinityApp.PIPE_ID));
		SplatRegistrations.RegisterLazySingleton<ModImportService>();
		SplatRegistrations.RegisterLazySingleton<IDialogService, DialogService>();
		SplatRegistrations.RegisterLazySingleton<AppKeysService>();

		SplatRegistrations.RegisterLazySingleton<ControlFactoryService>();

		SplatRegistrations.RegisterLazySingleton<IColorResourceService, ColorResourceService>();

		//SplatRegistrations.Register<ModListDropHandler>();
		//SplatRegistrations.Register<ModListDragHandler>();

		SplatRegistrations.RegisterLazySingleton<MainWindowViewModel>();
		resolver.RegisterLazySingleton<IScreen>(() => ViewModelLocator.Main);

		SplatRegistrations.RegisterLazySingleton<ModOrderViewModel>();

		resolver.RegisterLazySingleton(() => new MainCommandBarViewModel(ViewModelLocator.Main, ViewModelLocator.ModOrder, ModImporter, Get<IFileSystemService>(), Get<IInteractionsService>()));

		SplatRegistrations.RegisterLazySingleton<DeleteFilesViewModel>();
		SplatRegistrations.RegisterLazySingleton<ModUpdatesViewModel>();
		SplatRegistrations.RegisterLazySingleton<ProgressBarViewModel, ProgressBarViewModel>();

		SplatRegistrations.RegisterLazySingleton<SettingsWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<AboutWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<AppUpdateWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<NexusModsCollectionDownloadWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<HelpWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<ModPropertiesWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<NxmDownloadWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<StatsValidatorWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<VersionGeneratorViewModel>();
		SplatRegistrations.RegisterLazySingleton<ExportOrderToArchiveViewModel>();
		SplatRegistrations.RegisterLazySingleton<PakFileExplorerWindowViewModel>();
		SplatRegistrations.RegisterLazySingleton<KeybindingsViewModel>();
		SplatRegistrations.RegisterLazySingleton<MessageBoxViewModel>();
		SplatRegistrations.RegisterLazySingleton<FooterViewModel>();
		SplatRegistrations.RegisterLazySingleton<ModPickerViewModel>();
		SplatRegistrations.RegisterLazySingleton<ModContainerSettingsViewModel>();

		SplatRegistrations.RegisterLazySingleton<MainCommandBar>();
		SplatRegistrations.RegisterLazySingleton<DeleteFilesView>();
		SplatRegistrations.RegisterLazySingleton<ModOrderView>();
		SplatRegistrations.RegisterLazySingleton<ModUpdatesView>();

		SplatRegistrations.RegisterLazySingleton<ProgressBarView>();

		SplatRegistrations.RegisterLazySingleton<SettingsWindow>();
		SplatRegistrations.RegisterLazySingleton<AppUpdateWindow>();
		SplatRegistrations.RegisterLazySingleton<ModPropertiesWindow>();
		SplatRegistrations.RegisterLazySingleton<PakFileExplorerWindow>();
		SplatRegistrations.RegisterLazySingleton<StatsValidatorWindow>();
		SplatRegistrations.RegisterLazySingleton<VersionGeneratorWindow>();
		SplatRegistrations.RegisterLazySingleton<NxmDownloadWindow>();
		SplatRegistrations.RegisterLazySingleton<AboutWindow>();
		SplatRegistrations.RegisterLazySingleton<NexusModsCollectionDownloadWindow>();
		/*
		SplatRegistrations.RegisterLazySingleton<HelpWindow>();

		SplatRegistrations.RegisterLazySingleton<DeleteFilesConfirmationView>();
		SplatRegistrations.RegisterLazySingleton<ModUpdatesLayout>();*/

		//SplatRegistrations.RegisterLazySingleton<MainWindow>();

		SplatRegistrations.SetupIOC();
	}

	public static T Get<T>(string? contract = null)
	{
		return AppLocator.Current.GetService<T>(contract)!;
	}

	public static void Register<T>(Func<object> constructorCallback, string? contract = null)
	{
		AppLocator.CurrentMutable.Register(constructorCallback, typeof(T), contract);
	}

	public static void RegisterSingleton<T>(T instance, string? contract = null)
	{
		AppLocator.CurrentMutable.RegisterConstant(instance, typeof(T), contract);
	}

	/// <summary>
	/// Register a singleton which won't get created until the first user accesses it.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="constructorCallback"></param>
	/// <param name="contract"></param>
	public static void RegisterLazySingleton<T>(Func<object> constructorCallback, string contract = null)
	{
		AppLocator.CurrentMutable.RegisterLazySingleton(constructorCallback, typeof(T), contract);
	}
}
