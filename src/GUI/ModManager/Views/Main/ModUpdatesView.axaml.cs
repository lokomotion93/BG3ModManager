using ModManager.ViewModels.Main;

namespace ModManager.Views.Main;

public partial class ModUpdatesView : ReactiveUserControl<ModUpdatesViewModel>
{
	public ModUpdatesView()
	{
		InitializeComponent();

#if DEBUG
		this.DesignSetup();
#endif

		this.WhenActivated(d =>
		{
#if DEBUG
			if (Design.IsDesignMode && ViewModel is DesignModUpdatesViewModel designVM)
			{
				designVM.AddTestEntries(AppServices.Locale);
			}
#endif
		});
	}
}