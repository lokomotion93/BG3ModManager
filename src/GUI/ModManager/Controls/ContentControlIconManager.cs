using Avalonia.Controls.Primitives;
using Avalonia.Labs.Gif;

using Humanizer;

using Material.Icons;
using Material.Icons.Avalonia;

using ModManager.Models.Mod;
using ModManager.Styling;
using ModManager.Util;

using Stateless;

namespace ModManager.Controls;

public class ContentControlIconManager
{
	private enum IconManagerIconState
	{
		Disposed,
		Rendering
	}

	private enum IconManagerAction
	{
		CreateIcon,
		Dispose
	}

	private readonly ContentControl _target;
	private double _sizeMult;
	private ModContainerIconSettings? _iconSettings;

	private CancellationTokenSource? _tokenSource;
	private CompositeDisposable? _iconDisp = null;

	private readonly RxStateMachine<IconManagerIconState, IconManagerAction> _sm;

	private static readonly Thickness _defaultThickness = new(0d);
	private readonly ValueTuple<double, double> _defaultSize;

	public string Name { get; set; } = string.Empty;

	public ContentControlIconManager(ContentControl targetControl, double multiplySize = 1d)
	{
		_target = targetControl;
		_sizeMult = multiplySize;
		_defaultSize = new(16d * multiplySize, 16d * multiplySize);

		_sm = new(IconManagerIconState.Disposed);

		_sm.Configure(IconManagerIconState.Disposed)
			.OnEntryAsync(DisposeAsync)
			.Permit(IconManagerAction.CreateIcon, IconManagerIconState.Rendering)
			.Ignore(IconManagerAction.Dispose);

		_sm.Configure(IconManagerIconState.Rendering)
			.OnEntryAsync(RealizeIcon)
			.OnExitAsync(DisposeAsync)
			.Permit(IconManagerAction.Dispose, IconManagerIconState.Disposed)
			.Ignore(IconManagerAction.CreateIcon);
	}

	private void CreateToken()
	{
		_tokenSource?.Cancel();
		_tokenSource?.Dispose();
		_tokenSource = new CancellationTokenSource();
	}

	private ValueTuple<double, double> SizeStrToSize(string? sizeStr)
	{
		if (double.TryParse(sizeStr, out var singleSize))
		{
			return (singleSize * _sizeMult, singleSize * _sizeMult);
		}
		else
		{
			var size = Size.Parse(sizeStr);
			return (size.Width * _sizeMult, size.Height * _sizeMult);
		}
	}

	private async Task RealizeIcon()
	{
		CreateToken();

		Control? result = null;
		var token = _tokenSource!.Token;

		_iconDisp?.Dispose();
		_iconDisp = [];

		if (_iconSettings != null)
		{
			if (_iconSettings.Path.IsValid())
			{
				var taskResult = await AppServices.ControlFactory.ImageFromPathAsync(_iconSettings.Path, "Orders", token);
				result = taskResult.Result;
				if (taskResult.Stream != null)
				{
					_iconDisp.Add(taskResult.Stream);
				}
				else
				{
					DivinityApp.Log($"Failed to load image from path ({_iconSettings.Path})");
				}
			}
			if (result == null && _iconSettings.Kind.IsValid())
			{
				var kind = _iconSettings.Kind;
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
						return icon;
					}, RxApp.MainThreadScheduler);
				}
				else
				{
					var msg = Loca.Alert_Error_ContainerIconMaterialKindParsing.SafeFormat("Container '{0}' has an invalid Icon.Kind value: \"{1}\"", Name, _iconSettings.Kind);
					AppServices.Commands.ShowAlert(msg, AlertType.Danger, 10);
					DivinityApp.Log(kind);
				}
			}
		}
		await Observable.Start(() =>
		{
			if (result != null)
			{
				if (_iconSettings?.Size.IsValid() == false)
				{
					result.Width = 16;
					result.Height = 16;
				}

				if (result is TemplatedControl templatedControl)
				{
					templatedControl.Bind(TemplatedControl.ForegroundProperty, _iconSettings.WhenAnyValue(x => x.ForegroundColor).Where(Validators.IsValid).Select(x => ColorBrushCache.GetBrush(x))).DisposeWith(_iconDisp);
					templatedControl.Bind(TemplatedControl.BackgroundProperty, _iconSettings.WhenAnyValue(x => x.BackgroundColor).Where(Validators.IsValid).Select(x => ColorBrushCache.GetBrush(x))).DisposeWith(_iconDisp);
					templatedControl.Bind(TemplatedControl.BorderBrushProperty, _iconSettings.WhenAnyValue(x => x.BorderColor).Where(Validators.IsValid).Select(x => ColorBrushCache.GetBrush(x))).DisposeWith(_iconDisp);
					templatedControl.Bind(TemplatedControl.BorderThicknessProperty, _iconSettings.WhenAnyValue(x => x.BorderThickness).SafeValueOrFallback(Thickness.Parse, _defaultThickness)).DisposeWith(_iconDisp);
				}

				_iconSettings.WhenAnyValue(x => x.Size).SafeValueOrFallback(SizeStrToSize, _defaultSize).Subscribe(size =>
				{
					result.Width = size.Item1;
					result.Height = size.Item2;
				}).DisposeWith(_iconDisp);

				if (result is GifImage gif && gif.Source is GifStreamSource gifSource)
				{
					_iconDisp.Add(gifSource);
				}
			}
			_target.Content = result;
		}, RxApp.MainThreadScheduler);
	}

	private void Dispose()
	{
		_tokenSource?.Cancel();
		_iconDisp?.Dispose();
		_target.Content = null;
	}

	private async Task DisposeAsync()
	{
		await Observable.Start(Dispose, RxApp.MainThreadScheduler);
	}

	public void Load(ModContainerIconSettings? icon)
	{
		_iconSettings = icon;
		if (_iconSettings != null)
		{
			_sm.Enqueue(IconManagerAction.CreateIcon);
		}
		else
		{
			_sm.Enqueue(IconManagerAction.Dispose);
		}
	}

	public void Unload()
	{
		_sm.Enqueue(IconManagerAction.Dispose);
	}
}
