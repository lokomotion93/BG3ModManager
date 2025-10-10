using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace ModManager.Models.View;
public class ColorThemeEntry : ReactiveObject
{
	public ColorThemeType Id { get; }
	public Uri ResourceAssetPath { get; }
	public ResourceInclude Resources { get; }
	public Uri? StyleAssetPath { get; }
	public StyleInclude? Style { get; }

	public ColorThemeEntry(ColorThemeType id, string resourceAssetPath, string? styleAssetPath = null)
	{
		Id = id;
		ResourceAssetPath = new(resourceAssetPath);
		Resources = new ResourceInclude(ResourceAssetPath) { Source = ResourceAssetPath };
		if (styleAssetPath.IsValid())
		{
			StyleAssetPath = new(styleAssetPath);
			Style = new StyleInclude(StyleAssetPath) { Source = StyleAssetPath };
		}
	}
}
