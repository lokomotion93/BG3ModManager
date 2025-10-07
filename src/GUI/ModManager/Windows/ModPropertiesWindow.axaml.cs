using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Svg.Skia;

using Material.Icons;
using Material.Icons.Avalonia;

using ModManager.Controls;
using ModManager.Models.Mod;
using ModManager.ViewModels;

using SkiaSharp;

using Svg.Skia;

namespace ModManager.Windows;
public partial class ModPropertiesWindow : HideWindowBase<ModPropertiesWindowViewModel>
{
	private object? ModToWindowIcon(ModData? mod)
	{
		if (mod != null)
		{
			if (mod.IsToolkitProject == true || mod.IsLooseMod)
			{
				var svgImage = new SvgImage
				{
					Css = ".Black { fill: #1098DF; }",
					Source = SvgSource.Load("avares://ModManager/Assets/Icons/file-cog.svg", baseUri: null),
				};
				using var mem = new MemoryStream();
				svgImage.Source.Picture.ToImage(mem, SKColors.Empty, SKEncodedImageFormat.Png, 100, 1f, 1f, SKColorType.Rgba8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb());
				mem.Position = 0;
				return new WindowIcon(mem);
			}
			else
			{
				var svgImage = new SvgImage
				{
					Css = ".Black { fill: #1098DF; }",
					Source = SvgSource.Load("avares://ModManager/Assets/Icons/folder-cog.svg", baseUri: null)
				};
				using var mem = new MemoryStream();
				svgImage.Source.Picture.ToImage(mem, SKColors.Empty, SKEncodedImageFormat.Png, 100, 1f, 1f, SKColorType.Rgba8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb());
				mem.Position = 0;
				return new WindowIcon(mem);
			}
		}
		return new WindowIcon("avares://ModManager/Assets/BG3ModManager.ico");
	}

	public ModPropertiesWindow()
	{
		InitializeComponent();

		if (!Design.IsDesignMode)
		{
			ViewModel = AppServices.Get<ModPropertiesWindowViewModel>();
		}

		this.WhenActivated(d =>
		{
			if(ViewModel != null)
			{
				this.GetObservable(IsVisibleProperty).BindTo(ViewModel, x => x.IsVisible);

				this.OneWayBind(ViewModel, x => x.IsEditorMod, x => x.ModTypeIconControl.Kind,
					b => b ? MaterialIconKind.Folder : MaterialIconKind.File);

				//this.OneWayBind(ViewModel, x => x.Mod, x => x.Icon, ModToWindowIcon);

				ViewModel.OKCommand.Subscribe(x => Hide());
				ViewModel.CancelCommand.Subscribe(x => Hide());
			}
		});
	}
}