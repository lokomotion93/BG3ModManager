using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Labs.Controls;
using Avalonia.Labs.Gif;
using Avalonia.Media.Imaging;

using Humanizer;

using Material.Icons;
using Material.Icons.Avalonia;

using ModManager.Controls;
using ModManager.Models.Mod;
using ModManager.Styling;
using ModManager.Util;

using System.Collections.Concurrent;

using ZiggyCreatures.Caching.Fusion;

namespace ModManager.Views.Mods;

public partial class ModContainerEntryView : ReactiveUserControl<ModContainer>
{
	private static readonly Thickness _defaultThickness = new(0);

	private readonly ContentControlIconManager _iconManager;
	private readonly ContentControlIconManager _toolTipIconManager;

	private void Cleanup(ContentControlIconManager? target = null)
	{
		if(target == null)
		{
			_iconManager.Unload();
			_toolTipIconManager.Unload();
		}
		else
		{
			target.Unload();
		}
	}

	public ModContainerEntryView()
	{
		InitializeComponent();

		_iconManager = new(IconPresenter);
		_toolTipIconManager = new(ToolTipIconPresenter, 3d);

		this.WhenActivated(d =>
		{
			if(ViewModel != null)
			{
				_iconManager.Name = ViewModel.DisplayName ?? ViewModel.UUID;
				_toolTipIconManager.Name = ViewModel.DisplayName ?? ViewModel.UUID;

				var defaultForegroundColor = LabelTextBlock.Foreground;
				var defaultBG = IconBorder.Background;

				LabelTextBlock[!TextBlock.ForegroundProperty] = ViewModel.WhenAnyValue(x => x.ForegroundColor).Select(x => x != null ? ColorBrushCache.GetBrush(x) : defaultForegroundColor).ToBinding();

				var hasIconSettings = ViewModel.WhenAnyValue(x => x.Icon).WhereNotNull();

				IconBorder.BorderThickness = _defaultThickness;
				IconBorder[!BorderBrushProperty] = hasIconSettings.CombineLatest(ViewModel.Icon.WhenAnyValue(x => x.BorderColor)).Select(x => x.Second.IsValid() ? ColorBrushCache.GetBrush(x.Second) : ColorBrushCache.GetResourceBrush("SukiMediumBorderBrush")).ToBinding();

				IconBorder[!BackgroundProperty] = hasIconSettings.CombineLatest(ViewModel.Icon.WhenAnyValue(x => x.BackgroundColor)).Select(x => x.Second.IsValid() ? ColorBrushCache.GetBrush(x.Second) : defaultBG).ToBinding();

				IconBorder[!BorderThicknessProperty] = hasIconSettings.CombineLatest(ViewModel.Icon.WhenAnyValue(x => x.BorderThickness)).Select(x => x.Second.IsValid() ? Thickness.Parse(x.Second) : _defaultThickness).ToBinding();

				d(Observable.FromEvent<EventHandler<RoutedEventArgs>, RoutedEventArgs>(
					h => (sender, e) => h(e),
					h => Unloaded += h,
					h => Unloaded -= h
				).Subscribe(_ => Cleanup()));

				d(Observable.FromEvent<EventHandler<VisualTreeAttachmentEventArgs>, VisualTreeAttachmentEventArgs>(
					h => (sender, e) => h(e),
					h => DetachedFromVisualTree += h,
					h => DetachedFromVisualTree -= h
				).Subscribe(_ => Cleanup()));

				d(Observable.FromEvent<EventHandler<VisualTreeAttachmentEventArgs>, VisualTreeAttachmentEventArgs>(
					h => (sender, e) => h(e),
					h => ToolTipIconPresenter.DetachedFromVisualTree += h,
					h => ToolTipIconPresenter.DetachedFromVisualTree -= h
				).Subscribe(_ => Cleanup(_toolTipIconManager)));

				d(Observable.FromEvent<EventHandler<VisualTreeAttachmentEventArgs>, VisualTreeAttachmentEventArgs>(
					h => (sender, e) => h(e),
					h => ToolTipIconPresenter.AttachedToVisualTree += h,
					h => ToolTipIconPresenter.AttachedToVisualTree -= h
				).Subscribe(_ =>
				{
					_toolTipIconManager.Load(ViewModel.Icon);
				}));

				d(ViewModel.RenderIconCommand.IsExecuting.ObserveOn(RxApp.MainThreadScheduler).Subscribe(b =>
				{
					if(!b)
					{
						Cleanup();
						if(ViewModel.Icon != null)
						{
							_iconManager.Load(ViewModel.Icon);
							_toolTipIconManager.Load(ViewModel.Icon);
						}
					}
				}));
			}
		});
	}
}