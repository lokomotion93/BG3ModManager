using Avalonia.Interactivity;

using ModManager.Controls;
using ModManager.Styling;
using ModManager.ViewModels.Settings;

namespace ModManager.Views.Settings;

public partial class IconSettingsView : ProtectedUserControl<IconSettingsViewModel>
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
		_iconManager = new(IconPresenter, 3);

		this.WhenActivated(d =>
		{
			if (ViewModel != null)
			{
				var fallbackBrush = ColorBrushCache.GetResourceBrush("SukiMediumBorderBrush");

				//PathTextBox[!TextBox.TextProperty] = ViewModel.Settings.WhenAnyValue(x => x.Path).ToBinding();

				IconBorder.BorderThickness = _defaultThickness;
				IconBorder[!BorderBrushProperty] = ViewModel.Settings.WhenAnyValue(x => x.BorderColor, x => ColorBrushCache.GetBrush(x, fallbackBrush)).ToBinding();

				IconBorder[!BackgroundProperty] = ViewModel.Settings.WhenAnyValue(x => x.BackgroundColor, x => ColorBrushCache.GetBrush(x, fallbackBrush)).ToBinding();

				IconBorder[!BorderThicknessProperty] = ViewModel.Settings.WhenAnyValue(x => x.BorderThickness, StringToThickness).ToBinding();

				//d(Observable.FromEvent<EventHandler<RoutedEventArgs>, RoutedEventArgs>(
				//	h => (sender, e) => h(e),
				//	h => Unloaded += h,
				//	h => Unloaded -= h
				//).Subscribe(_ => _iconManager.Dispose()));

				//d(Observable.FromEvent<EventHandler<VisualTreeAttachmentEventArgs>, VisualTreeAttachmentEventArgs>(
				//	h => (sender, e) => h(e),
				//	h => DetachedFromVisualTree += h,
				//	h => DetachedFromVisualTree -= h
				//).Subscribe(_ => _iconManager.Dispose()));

				d(ViewModel.RenderImageCommand.IsExecuting.ObserveOn(RxApp.MainThreadScheduler).Subscribe(b =>
				{
					if (!b) _iconManager.Load(ViewModel.Settings);
				}));

				d(ViewModel.ClearImageCommand.IsExecuting.ObserveOn(RxApp.MainThreadScheduler).Subscribe(b =>
				{
					if (!b) _iconManager.Dispose();
				}));
			}
		});
	}
}