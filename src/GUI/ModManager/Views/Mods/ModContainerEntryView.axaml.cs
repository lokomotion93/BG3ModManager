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

using ZiggyCreatures.Caching.Fusion;

namespace ModManager.Views.Mods;

public partial class ModContainerEntryView : ReactiveUserControl<ModContainer>
{
	private static readonly Thickness _defaultThickness = new(0);
	private MemoryStream? _gifStream = null;

	private async Task RealizeIcon(ModContainerIconSettings? iconSettings, CancellationToken token)
	{
		Control? result = null;
		if(iconSettings != null)
		{

			if(iconSettings.Kind.IsValid())
			{
				var kind = iconSettings.Kind;
				//Potential name taken from https://pictogrammers.com/library/mdi/
				if (kind.Contains('-'))
				{
					//Converter icon-name to IconName
					//Dehumanize will pascal-case separated words, which is why we swap hyphens for a space
					kind = kind.Replace("-", " ").Dehumanize();
				}

				if(Enum.TryParse<MaterialIconKind>(kind, out var materialIconKind))
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
					var msg = Loca.Alert_Error_ContainerIconMaterialKindParsing.SafeFormat("Container '{0}' has an invalid Icon.Kind value: \"{1}\"", ViewModel?.DisplayName ?? string.Empty, iconSettings.Kind);
					AppServices.Commands.ShowAlert(msg, AlertType.Danger, 10);
					DivinityApp.Log(kind);
				}
			}
			else if(iconSettings.Path.IsValid())
			{
				var path = iconSettings.Path;
				var fs = AppServices.FS;

				string? finalPath = null;

				//Remote icons
				if(Uri.IsWellFormedUriString(path, UriKind.Absolute))
				{
					finalPath = path;
				}
				else
				{
					if (!fs.Path.IsPathRooted(path))
					{
						var potentialPath = DivinityApp.GetAppDirectory("Orders", path);
						if (fs.File.Exists(potentialPath))
						{
							finalPath = potentialPath;
						}
					}
					else
					{
						finalPath = path;
					}
				}

				if (finalPath != null)
				{
					if (finalPath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
					{
						if(!finalPath.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
						{
							if(_gifStream != null)
							{
								await _gifStream.DisposeAsync();
							}
							
							_gifStream = new MemoryStream();

							var cache = AppServices.Get<IFusionCache>()!;
							var data = await cache.TryGetAsync<byte[]?>(finalPath, token: token);
							if (data.HasValue)
							{
								var imageData = data.Value!;
								try
								{
									await _gifStream.WriteAsync(imageData, token);
								}
								catch (Exception)
								{
									//Handling exception
								}
							}
							else
							{
								var isUrl = Uri.IsWellFormedUriString(finalPath, UriKind.Absolute);
								if (isUrl)
								{
									var imageBytes = await WebHelper.DownloadUrlAsBytesAsync(finalPath, token);
									if (imageBytes != null)
									{
										try
										{
											await cache.SetAsync(finalPath, imageBytes, token: token);
											await _gifStream.WriteAsync(imageBytes, token);
										}
										catch (Exception)
										{
											//Handling exception
										}
									}
								}
								else if (AppServices.FS.File.Exists(finalPath))
								{
									try
									{
										var imageBytes = await AppServices.FS.File.ReadAllBytesAsync(finalPath, token);
										await cache.SetAsync(finalPath, imageBytes, token: token);
										await _gifStream.WriteAsync(imageBytes, token);
									}
									catch (Exception ex)
									{
										DivinityApp.Log($"Error reading gif file: {ex}");
									}
								}
							}

							if (token.IsCancellationRequested) return;

							_gifStream.Position = 0;

							result = await Observable.Start(() =>
							{
								var gifSource = GifStreamSource.FromStream(_gifStream);
								var gif = new GifImage()
								{
									Source = gifSource,
									Stretch = Avalonia.Media.Stretch.UniformToFill
								};
								return gif;
							}, RxApp.MainThreadScheduler);

							DivinityApp.Log(finalPath);
						}
						else
						{
							result = await Observable.Start(() =>
							{
								var gifSource = GifStreamSource.FromUriString(finalPath);
								var gif = new GifImage()
								{
									Source = gifSource,
									Stretch = Avalonia.Media.Stretch.UniformToFill
								};
								return gif;
							}, RxApp.MainThreadScheduler);
						}
					}
					else
					{
						result = await Observable.Start(() =>
						{
							var icon = new AsyncImage()
							{
								Source = new Uri(finalPath),
								Stretch = Avalonia.Media.Stretch.UniformToFill,
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
						var size = Size.Parse(iconSettings.Size);
						result.Width = size.Width;
						result.Height = size.Height;
					}
					catch (Exception) { }
				}
				else
				{
					result.Width = 16;
					result.Height = 16;
				}
			}
			IconPresenter.Content = result;
		}, RxApp.MainThreadScheduler);
	}

	IDisposable? _realizeIconTask = null;

	private void Cleanup()
	{
		_realizeIconTask?.Dispose();
		_gifStream?.Dispose();
	}

	public ModContainerEntryView()
	{
		InitializeComponent();

		this.WhenActivated(d =>
		{
			if(ViewModel != null)
			{
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

				d(ViewModel.WhenAnyValue(x => x.Icon).Subscribe(icon =>
				{
					_realizeIconTask?.Dispose();
					_realizeIconTask = RxApp.TaskpoolScheduler.ScheduleAsync(async (sch, t) =>
					{
						await RealizeIcon(icon, t);
					});
				}));
			}
		});
	}
}