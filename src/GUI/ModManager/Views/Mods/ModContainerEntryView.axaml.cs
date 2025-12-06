using Avalonia.Labs.Controls;

using Humanizer;

using Material.Icons;
using Material.Icons.Avalonia;

using ModManager.Models.Mod;
using ModManager.Styling;

namespace ModManager.Views.Mods;

public partial class ModContainerEntryView : ReactiveUserControl<ModContainer>
{
	private static readonly Thickness _defaultThickness = new(0);

	private void RealizeIcon(ModContainerIconSettings? iconSettings)
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

					result = icon;
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
					var icon = new AsyncImage()
					{
						Source = new Uri(finalPath),
						Stretch = Avalonia.Media.Stretch.UniformToFill
					};
					if (iconSettings.ForegroundColor.IsValid())
					{
						var brush = ColorBrushCache.GetBrush(iconSettings.ForegroundColor);
						if(brush != null)
						{
							icon.Foreground = brush;
						}
					}
					result = icon;
				}
			}

			if(result != null)
			{
				if(iconSettings.Size.IsValid())
				{
					try
					{
						var size = Size.Parse(iconSettings.Size);
						result.Width = size.Width;
						result.Height = size.Height;
					}
					catch(Exception) { }
				}
				else
				{
					result.Width = 16;
					result.Height = 16;
				}
			}
		}
		IconPresenter.Content = result;
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

				d(ViewModel.WhenAnyValue(x => x.Icon).Subscribe(RealizeIcon));
			}
		});
	}
}