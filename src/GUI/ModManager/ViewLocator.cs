using Avalonia.Controls.Templates;

using ModManager.Models.Mod;
using ModManager.Models.Settings;
using ModManager.Models.View;
using ModManager.ViewModels;
using ModManager.ViewModels.Main;
using ModManager.ViewModels.Mods;
using ModManager.ViewModels.Settings;
using ModManager.Views;
using ModManager.Views.Main;
using ModManager.Views.Mods;
using ModManager.Views.Settings;
using ModManager.Views.StatsValidator;

namespace ModManager;

public class ViewLocatorErrorView : TextBlock, IViewFor
{
	public object? ViewModel { get; set; }
}

public class ViewLocator : IViewLocator, IDataTemplate
{
	private static readonly Type _viewForType = typeof(IViewFor<>);

	public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract = null) where TViewModel : class
	{
		var registered = AppLocator.Current.GetService(typeof(TViewModel), contract);
		if (registered is IViewFor<TViewModel> view)
		{
			return view;
		}
		return null;
	}

	public IViewFor? ResolveView(object? instance, string? contract = null)
	{
		if (instance != null)
		{
			try
			{
				var viewType = _viewForType.MakeGenericType(instance.GetType());
				var registered = AppLocator.Current.GetService(viewType, contract);
				if (registered is IViewFor view)
				{
					return view;
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching view: {ex}");
			}
		}
		return new ViewLocatorErrorView() { Text = $"Failed to find view for {instance}" };
	}

	private static void RegisterConstant<TViewModel, TView>(IMutableDependencyResolver resolver) where TViewModel : ReactiveObject where TView : IViewFor<TViewModel>
	{
		resolver.RegisterLazySingleton<IViewFor<TViewModel>>(() => AppServices.Get<TView>());
	}

	static ViewLocator()
	{
		var resolver = AppLocator.CurrentMutable;

		RegisterConstant<ProgressBarViewModel, ProgressBarView>(resolver);
		RegisterConstant<MainCommandBarViewModel, MainCommandBar>(resolver);
		RegisterConstant<DeleteFilesViewModel, DeleteFilesView>(resolver);
		RegisterConstant<ModOrderViewModel, ModOrderView>(resolver);
		RegisterConstant<ModUpdatesViewModel, ModUpdatesView>(resolver);
		RegisterConstant<KeybindingsViewModel, KeybindingsView>(resolver);

		//RegisterConstant<FooterViewModel, FooterView>(resolver);
		//RegisterConstant<DownloadActivityBarViewModel, DownloadActivityBar>(resolver);

		//resolver.RegisterLazySingleton(() => (IViewFor<SettingsWindowViewModel>)AppServices.Settings);
		//resolver.RegisterLazySingleton(() => (IViewFor<ModManagerSettings>)AppServices.Settings.ManagerSettings);
		//resolver.RegisterLazySingleton(() => (IViewFor<ModManagerUpdateSettings>)AppServices.Settings.ManagerSettings.UpdateSettings);
		//resolver.RegisterLazySingleton(() => (IViewFor<ScriptExtenderSettings>)AppServices.Settings.ManagerSettings.ExtenderSettings);
		//resolver.RegisterLazySingleton(() => (IViewFor<ScriptExtenderUpdateConfig>)AppServices.Settings.ManagerSettings.ExtenderUpdaterSettings);


		resolver.Register(() => new ModContainerEntryView(), typeof(IViewFor<ModContainer>));
		resolver.Register(() => new ModEntryView(), typeof(IViewFor<ModEntry>));
		resolver.Register(() => new StatsValidatorFileEntryView(), typeof(IViewFor<StatsValidatorFileResults>));
		resolver.Register(() => new StatsValidatorEntryView(), typeof(IViewFor<StatsValidatorErrorEntry>));
		resolver.Register(() => new StatsValidatorLineText(), typeof(IViewFor<StatsValidatorLineView>));

		resolver.Register(() => new MessageBoxView(), typeof(IViewFor<MessageBoxViewModel>));
		resolver.Register(() => new ModPickerView(), typeof(IViewFor<ModPickerViewModel>));
	}

	//IDataTemplate
	public bool SupportsRecycling => false;

	public Control Build(object? data)
	{
		if (ResolveView(data) is Control control)
		{
			return control;
		}
		return new TextBlock { Text = "No view found for: " + data?.GetType().FullName};
	}

	public bool Match(object? data)
	{
		return data is ReactiveObject;
	}
}