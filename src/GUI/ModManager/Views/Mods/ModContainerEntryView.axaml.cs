using ModManager.Models.Mod;
using ModManager.Styling;

namespace ModManager.Views.Mods;

public partial class ModContainerEntryView : ReactiveUserControl<ModContainer>
{
	public ModContainerEntryView()
	{
		InitializeComponent();

		this.WhenActivated(d =>
		{
			if(ViewModel != null)
			{
				var defaultForegroundColor = LabelTextBlock.Foreground;

				LabelTextBlock[!TextBlock.ForegroundProperty] = ViewModel.Settings.WhenAnyValue(x => x.ForegroundColor).Select(x => x != null ? ColorBrushCache.GetBrush(x) : defaultForegroundColor).ToBinding();
			}
		});
	}
}