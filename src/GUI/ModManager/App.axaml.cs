using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

using ModManager.Services;
using ModManager.Windows;

using SukiUI;

using System.Globalization;

namespace ModManager;
public partial class App : Application
{
	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public IClassicDesktopStyleApplicationLifetime? DesktopLifetime => ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

	public void Shutdown(int errorCode = 0)
	{
		DesktopLifetime?.Shutdown(errorCode);
	}

	public static new App Current => (App)Application.Current!;

	public override void OnFrameworkInitializationCompleted()
	{
		//Locale.Resources.Culture = new CultureInfo("en-US");
#if DEBUG
		if (Design.IsDesignMode)
		{
			base.OnFrameworkInitializationCompleted();
			return;
		}
#endif
		var desktop = DesktopLifetime;
		if (desktop != null)
		{
			ToolTip.ShowDelayProperty.OverrideDefaultValue<Panel>(500);
			ToolTip.BetweenShowDelayProperty.OverrideDefaultValue<Panel>(500);
			ToolTip.PlacementProperty.OverrideDefaultValue<Panel>(PlacementMode.RightEdgeAlignedTop);

			var viewLocator = new ViewLocator();
			Locator.CurrentMutable.RegisterConstant<IViewLocator>(viewLocator);
			DataTemplates.Add(viewLocator);

			var mainWindow = new MainWindow();
			desktop.MainWindow = mainWindow;
			Locator.CurrentMutable.RegisterConstant(mainWindow);
			mainWindow.DataContext = ViewModelLocator.Main;

			Locator.CurrentMutable.InitializeSplat();
			Locator.CurrentMutable.InitializeReactiveUI();

			Locator.CurrentMutable.RegisterConstant(new WindowManagerService(mainWindow, AppServices.Interactions));

			RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

			SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);
			SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);

			AppServices.Initialize();
			SplatRegistrations.RegisterConstant(new ColorThemeService(AppServices.Settings, AppServices.Locale));
		}

		base.OnFrameworkInitializationCompleted();
	}
}