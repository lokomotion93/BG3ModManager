using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Labs.Controls;
using Avalonia.Labs.Gif;
using Avalonia.Media.Imaging;

using Humanizer;

using Material.Icons;
using Material.Icons.Avalonia;

using ModManager.Models.Mod;
using ModManager.Styling;
using ModManager.Util;

using System.Collections.Concurrent;

using ZiggyCreatures.Caching.Fusion;

namespace ModManager.Views.Mods;

public partial class ModContainerEntryView : ReactiveUserControl<ModContainer>
{
	private static readonly Thickness _defaultThickness = new(0);

	private class ContentControlIconManager(ContentControl targetControl, double multiplySize = 0d)
	{
		private readonly ContentControl _target = targetControl;
		private IDisposable? _realizeIconTask = null;
		private MemoryStream? _iconStream = null;
		public string Name { get; set; } = string.Empty;

		private async Task RealizeIcon(ModContainerIconSettings? iconSettings, CancellationToken token)
		{
			Control? result = null;
			if (iconSettings != null)
			{
				if (iconSettings.Kind.IsValid())
				{
					var kind = iconSettings.Kind;
					//Potential name taken from https://pictogrammers.com/library/mdi/
					if (kind.Contains('-'))
					{
						//Converter icon-name to IconName
						//Dehumanize will pascal-case separated words, which is why we swap hyphens for a space
						kind = kind.Replace("-", " ").Dehumanize();
					}

					if (Enum.TryParse<MaterialIconKind>(kind, out var materialIconKind))
					{
						result = await Observable.Start(() =>
						{
							var icon = new MaterialIcon()
							{
								Kind = materialIconKind,
							};
							if (iconSettings.ForegroundColor.IsValid())
							{
								var brush = ColorBrushCache.GetBrush(iconSettings.ForegroundColor);
								if (brush != null)
								{
									icon.Foreground = brush;
								}
							}
							return icon;
						}, RxApp.MainThreadScheduler);
					}
					else
					{
						var msg = Loca.Alert_Error_ContainerIconMaterialKindParsing.SafeFormat("Container '{0}' has an invalid Icon.Kind value: \"{1}\"", Name, iconSettings.Kind);
						AppServices.Commands.ShowAlert(msg, AlertType.Danger, 10);
						DivinityApp.Log(kind);
					}
				}
				else if (iconSettings.Path.IsValid())
				{
					var taskResult = await AppServices.ControlFactory.ImageFromPathAsync(iconSettings.Path, "Orders", token);
					result = taskResult.Result;
					if (taskResult.Stream != null)
					{
						_iconStream = taskResult.Stream;
					}

					if (result is TemplatedControl templatedControl)
					{
						if (iconSettings.ForegroundColor.IsValid())
						{
							var brush = ColorBrushCache.GetBrush(iconSettings.ForegroundColor);
							if (brush != null)
							{
								templatedControl.Foreground = brush;
							}
						}
					}
				}
			}
			await Observable.Start(() =>
			{
				if (result != null)
				{
					if (iconSettings?.Size.IsValid() == true)
					{
						try
						{
							if (double.TryParse(iconSettings.Size, out var singleSize))
							{
								result.Width = result.Height = singleSize;
							}
							else
							{
								var size = Size.Parse(iconSettings.Size);
								result.Width = size.Width;
								result.Height = size.Height;
							}
						}
						catch (Exception) { }
					}
					else
					{
						result.Width = 16;
						result.Height = 16;
					}
				}
				if(result != null && multiplySize != 0)
				{
					result.Width *= multiplySize;
					result.Height *= multiplySize;
				}
				_target.Content = result;
			}, RxApp.MainThreadScheduler);
		}

		public void Load(ModContainerIconSettings? icon)
		{
			_realizeIconTask?.Dispose();
			if(icon != null)
			{
				_realizeIconTask = RxApp.TaskpoolScheduler.ScheduleAsync(async (sch, t) =>
				{
					await RealizeIcon(icon, t);
				});
			}
			else
			{
				Dispose();
			}
		}

		public void Dispose()
		{
			if(_target.Content is GifImage gif && gif.Source is GifStreamSource gifSource)
			{
				gifSource.Dispose();
			}
			_realizeIconTask?.Dispose();
			_iconStream?.Dispose();
			_target.Content = null;
		}
	}

	private readonly ContentControlIconManager _iconManager;
	private readonly ContentControlIconManager _toolTipIconManager;

	private void Cleanup(ContentControlIconManager? target = null)
	{
		if(target == null)
		{
			_iconManager.Dispose();
			_toolTipIconManager.Dispose();
		}
		else
		{
			target.Dispose();
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

				LabelTextBlock[!TextBlock.ForegroundProperty] = ViewModel.Settings.WhenAnyValue(x => x.ForegroundColor).Select(x => x != null ? ColorBrushCache.GetBrush(x) : defaultForegroundColor).ToBinding();

				var hasIconSettings = ViewModel.Settings.WhenAnyValue(x => x.Icon).WhereNotNull();

				IconBorder.BorderThickness = _defaultThickness;
				IconBorder[!BorderBrushProperty] = hasIconSettings.CombineLatest(ViewModel.Settings.Icon.WhenAnyValue(x => x.BorderColor)).Select(x => x.Second.IsValid() ? ColorBrushCache.GetBrush(x.Second) : ColorBrushCache.GetResourceBrush("SukiMediumBorderBrush")).ToBinding();

				IconBorder[!BackgroundProperty] = hasIconSettings.CombineLatest(ViewModel.Settings.Icon.WhenAnyValue(x => x.BackgroundColor)).Select(x => x.Second.IsValid() ? ColorBrushCache.GetBrush(x.Second) : defaultBG).ToBinding();

				IconBorder[!BorderThicknessProperty] = hasIconSettings.CombineLatest(ViewModel.Settings.Icon.WhenAnyValue(x => x.BorderThickness)).Select(x => x.Second.IsValid() ? Thickness.Parse(x.Second) : _defaultThickness).ToBinding();

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

				d(ViewModel.WhenAnyValue(x => x.Icon).Subscribe(icon =>
				{
					_iconManager.Load(icon);
					_toolTipIconManager.Load(icon);
				}));
			}
		});
	}
}