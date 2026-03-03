using ModManager.ViewModels.Main;

namespace ModManager.Views.Main;

public partial class ModOrderView : ReactiveUserControl<ModOrderViewModel>
{
	public ModOrderView()
	{
		InitializeComponent();

#if DEBUG
		this.DesignSetup();
#endif
		this.WhenActivated(d =>
		{
			d(AppServices.Interactions.OpenModContainerSettings.RegisterHandler(context =>
			{
				var container = context.Input;

				if(container != null)
				{
					RxApp.MainThreadScheduler.Schedule(() =>
					{
						ModContainerSettingsControl.ViewModel!.Open(container);
					});
					context.SetOutput(true);
				}
				else
				{
					context.SetOutput(false);
				}
			}));
		});
	}
}