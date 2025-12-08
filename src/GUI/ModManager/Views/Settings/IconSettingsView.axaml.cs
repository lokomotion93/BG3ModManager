using Avalonia.Interactivity;

using ModManager.Controls;
using ModManager.Styling;
using ModManager.ViewModels.Settings;

namespace ModManager.Views.Settings;

public partial class IconSettingsView : ReactiveUserControl<IconSettingsViewModel>
{
	private static readonly Thickness _defaultThickness = new(0);

	private readonly ContentControlIconManager _iconManager;

	private static Thickness StringToThickness(string? value)
	{
		if(value.IsValid())
		{
			try
			{
				Thickness.Parse(value);
			}
			catch (Exception) { }
		}
		return _defaultThickness;
	}

	public IconSettingsView()
    {
        InitializeComponent();

#if DEBUG
		this.DesignSetup();
#endif
		_iconManager = new(IconPresenter);

		this.WhenActivated(d =>
		{
			if (ViewModel != null)
			{
				var fallbackBrush = ColorBrushCache.GetResourceBrush("SukiMediumBorderBrush");

				IconBorder.BorderThickness = _defaultThickness;
				IconBorder[!BorderBrushProperty] = ViewModel.Settings.WhenAnyValue(x => x.BorderColor, x => ColorBrushCache.GetBrush(x, fallbackBrush)).ToBinding();

				IconBorder[!BackgroundProperty] = ViewModel.Settings.WhenAnyValue(x => x.BackgroundColor, x => ColorBrushCache.GetBrush(x, fallbackBrush)).ToBinding();

				IconBorder[!BorderThicknessProperty] = ViewModel.Settings.WhenAnyValue(x => x.BorderThickness, StringToThickness).ToBinding();

				d(Observable.FromEvent<EventHandler<RoutedEventArgs>, RoutedEventArgs>(
					h => (sender, e) => h(e),
					h => Unloaded += h,
					h => Unloaded -= h
				).Subscribe(_ => _iconManager.Dispose()));

				d(Observable.FromEvent<EventHandler<VisualTreeAttachmentEventArgs>, VisualTreeAttachmentEventArgs>(
					h => (sender, e) => h(e),
					h => DetachedFromVisualTree += h,
					h => DetachedFromVisualTree -= h
				).Subscribe(_ => _iconManager.Dispose()));

				//ViewModel.BrowseForImageCommand
				_iconManager.Load(ViewModel.Settings);
			}
		});
	}
}