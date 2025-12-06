using Avalonia.Labs.Controls;
using Avalonia.Labs.Gif;
using Avalonia.Media;

using ModManager.Util;

using ZiggyCreatures.Caching.Fusion;

namespace ModManager.Services;
public class ControlFactoryService(ILocaleService localeService, IFusionCache cache)
{
	private readonly ILocaleService _locale = localeService;
	private readonly IFusionCache _cache = cache;

	public TextBlock LocalizedTextBlock(string key, string fallback)
	{
		var tb = new TextBlock();
		tb[!TextBlock.TextProperty] = _locale.EntryToObservable(key, fallback).ToBinding();
		return tb;
	}

	public async Task<Control?> ImageFromPathAsync(string pathOrUrl, string? fromRelativePath, CancellationToken token)
	{
		Control? result = null;

		if (pathOrUrl.IsValid())
		{
			var path = pathOrUrl;
			var fs = AppServices.FS;

			string? finalPath = null;

			//Remote icons
			if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
			{
				finalPath = path;
			}
			else
			{
				if (!fs.Path.IsPathRooted(path))
				{
					string potentialPath;
					if (fromRelativePath.IsValid())
					{
						potentialPath = DivinityApp.GetAppDirectory(fromRelativePath, path);
					}
					else
					{
						potentialPath = DivinityApp.GetAppDirectory(path);
					}
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
					if (!finalPath.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
					{
						var stream = new MemoryStream();

						var data = await _cache.TryGetAsync<byte[]?>(finalPath, token: token);
						if (data.HasValue)
						{
							var imageData = data.Value!;
							try
							{
								await stream.WriteAsync(imageData, token);
							}
							catch (Exception) { }
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
										await _cache.SetAsync(finalPath, imageBytes, token: token);
										await stream.WriteAsync(imageBytes, token);
									}
									catch (Exception) { }
								}
							}
							else if (AppServices.FS.File.Exists(finalPath))
							{
								try
								{
									var imageBytes = await AppServices.FS.File.ReadAllBytesAsync(finalPath, token);
									await _cache.SetAsync(finalPath, imageBytes, token: token);
									await stream.WriteAsync(imageBytes, token);
								}
								catch (Exception ex)
								{
									DivinityApp.Log($"Error reading gif file: {ex}");
								}
							}
						}

						if (token.IsCancellationRequested)
						{
							stream?.Dispose();
							return null;
						}

						stream.Position = 0;

						result = await Observable.Start(() =>
						{
							var gifSource = GifStreamSource.FromStream(stream);
							var gif = new GifImage()
							{
								Source = gifSource,
								Stretch = Stretch.UniformToFill
							};
							RenderOptions.SetBitmapInterpolationMode(gif, Avalonia.Media.Imaging.BitmapInterpolationMode.HighQuality);
							gif.DetachedFromVisualTree += (o, e) =>
							{
								stream?.Dispose();
							};
							return gif;
						}, RxApp.MainThreadScheduler);
					}
					else
					{
						result = await Observable.Start(() =>
						{
							var gifSource = GifStreamSource.FromUriString(finalPath);
							var gif = new GifImage()
							{
								Source = gifSource,
								Stretch = Stretch.UniformToFill
							};
							RenderOptions.SetBitmapInterpolationMode(gif, Avalonia.Media.Imaging.BitmapInterpolationMode.HighQuality);
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
							Stretch = Stretch.UniformToFill,
							IsCacheEnabled = true
						};
						RenderOptions.SetBitmapInterpolationMode(icon, Avalonia.Media.Imaging.BitmapInterpolationMode.HighQuality);
						return icon;
					}, RxApp.MainThreadScheduler);
				}
			}
		}

		return result;
	}
}
