using ModManager.Models.App;

namespace ModManager.Models.Settings;

public partial class AppSettings : ReactiveObject
{
	[Reactive] public partial DefaultPathwayData DefaultPathways { get; set; }
	[Reactive] public partial AppFeatures Features { get; set; }

	public static string GetDirectory() => DivinityApp.GetAppDirectory("Resources");

	public AppSettings()
	{
		DefaultPathways = new DefaultPathwayData();
		Features = new AppFeatures();
	}
}
