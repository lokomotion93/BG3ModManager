using ModManager.Controls;
using ModManager.Models.Settings;
using ModManager.ViewModels;
using ModManager.ViewModels.Settings;
using ModManager.Views.Generated;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ModManager.Windows;
public partial class SettingsWindow : HideWindowBase<SettingsWindowViewModel>
{
	private static SettingsWindowTab ValidateIndex(int index)
	{
		var result = SettingsWindowTab.Default;
		if (index > 0)
		{
			result = (SettingsWindowTab)index;
		}
		return result;
	}

	private static readonly Type _modManagerSettingsType = typeof(ModManagerSettings);

	private static bool TryGetSettingsEntry(string propName, [NotNullWhen(true)] out SettingsEntryAttribute? attribute)
	{
		attribute = null;
		if (_modManagerSettingsType.GetProperty(propName) is PropertyInfo prop && prop.GetCustomAttribute<SettingsEntryAttribute>() is SettingsEntryAttribute att)
		{
			attribute = att;
			return true;
		}
		return false;
	}

	public SettingsWindow()
	{
		InitializeComponent();

		this.WhenActivated(d =>
		{
			ViewModel = AppServices.Get<SettingsWindowViewModel>();

			if (ViewModel != null)
			{
				this.GetObservable(IsVisibleProperty).BindTo(ViewModel, x => x.IsVisible);
				SettingsTabControl.GetObservable(TabIndexProperty).Select(ValidateIndex).BindTo(ViewModel, x => x.SelectedTabIndex);
				ViewModel.WhenAnyValue(x => x.SelectedTabIndex).Select(x => (int)x).BindTo(SettingsTabControl, x => x.TabIndex);

				/* Required due to DataContext not working for some reason
				DataContext is set to null in the axaml to avoid the context being set to SettingsWindowViewModel, and then
				we update it here. */
				GeneralSettingsView.ViewModel = ViewModel.Settings;
				UpdateSettingsView.ViewModel = ViewModel.UpdateSettings;
				ExtenderSettingsView.ViewModel = ViewModel.ExtenderSettings;
				ExtenderUpdateSettingsView.ViewModel = ViewModel.ExtenderUpdaterSettings;
				KeybindingsView.ViewModel = ViewModelLocator.Keybindings;
			}
		});
	}
}