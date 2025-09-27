using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

using DynamicData.Binding;

using ModManager.Models.Menu;
using ModManager.ViewModels.Main;

namespace ModManager.Views.Main;

public partial class MainCommandBar : ReactiveUserControl<MainCommandBarViewModel>
{
	private static void AddMenuItem(IMenuEntry entry, ItemCollection target)
	{
		if(entry is MenuEntry menuEntry)
		{
			var menuItem = new MenuItem()
			{
				Command = menuEntry.Command,
			};
			if(menuEntry.UseAccessShortcut || menuEntry.DisplayName?.StartsWith('_') == true)
			{
				var tb = new AccessText();
				tb[!AccessText.TextProperty] = menuEntry.WhenAnyValue(x => x.DisplayName).ToBinding();
				tb[!ToolTip.TipProperty] = menuEntry.WhenAnyValue(x => x.ToolTip).ToBinding();
				menuItem.Header = tb;
			}
			else
			{
				var tb = new TextBlock();
				tb[!TextBlock.TextProperty] = menuEntry.WhenAnyValue(x => x.DisplayName).ToBinding();
				tb[!ToolTip.TipProperty] = menuEntry.WhenAnyValue(x => x.ToolTip).ToBinding();
				menuItem.Header = tb;
			}
			target.Add(menuItem);
			if(menuEntry.Children != null)
			{
				foreach(var child in menuEntry.Children)
				{
					AddMenuItem(child, menuItem.Items);
				}
			}
		}
		else if(entry is MenuSeparator)
		{
			target.Add(new Separator());
		}
	}

	private bool _addedMenuItems = false;

	public MainCommandBar()
	{
		InitializeComponent();

#if DEBUG
		this.DesignSetup();
#endif

		//TODO See if there's a way to fix FAComboBox staying "pressed" when opening a context menu

		Observable.FromEventPattern<PointerPressedEventArgs>(OrdersComboBox, nameof(OrdersComboBox.PointerPressed))
		.Subscribe(x =>
		{
			if (x.Sender is Visual v && x.EventArgs.GetCurrentPoint(v).Properties.IsRightButtonPressed)
			{
				x.EventArgs.Handled = true;
			}
		});

		this.WhenActivated(d =>
		{
			if (ViewModel != null)
			{
				if(!_addedMenuItems)
				{
					RxApp.MainThreadScheduler.Schedule(() =>
					{
						foreach (var entry in ViewModel.MenuEntries)
						{
							AddMenuItem(entry, TopMenu.Items);
						}
						_addedMenuItems = true;
					});
				}

				//Temp fix for ComboBox selected items not updating from bindings

				ViewModel.WhenAnyValue(x => x.ModOrder, x => x.ModOrder.SelectedProfile, x => x.ModOrder.SelectedModOrder, x => x.ModOrder.SelectedAdventureMod)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(_ =>
				{
					if(ViewModel.ModOrder != null)
					{
						var selectedProfile = ViewModel.ModOrder.SelectedProfile;
						var selectedModOrder = ViewModel.ModOrder.SelectedModOrder;
						var selectedAdventure = ViewModel.ModOrder.SelectedAdventureMod;

						if (selectedProfile != null && ProfileComboBox.SelectedItem == null)
						{
							ProfileComboBox.SelectedItem = selectedProfile;
							ProfileComboBox.SelectedIndex = ViewModel.ModOrder.SelectedProfileIndex;
						}

						if (selectedModOrder != null && OrdersComboBox.SelectedItem == null)
						{
							OrdersComboBox.SelectedItem = selectedModOrder;
							OrdersComboBox.SelectedIndex = ViewModel.ModOrder.SelectedModOrderIndex;
						}

						if (selectedAdventure != null && CampaignComboBox.SelectedItem == null)
						{
							CampaignComboBox.SelectedItem = selectedAdventure;
							CampaignComboBox.SelectedIndex = ViewModel.ModOrder.SelectedAdventureModIndex;
						}
					}
				}).DisposeWith(d);
			}
		});
	}
}