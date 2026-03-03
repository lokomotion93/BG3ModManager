using ModManager.ViewModels;
using ModManager.ViewModels.Main;

namespace ModManager.Views.Main;

public partial class FooterView : ReactiveUserControl<FooterViewModel>
{
    public FooterView()
    {
        InitializeComponent();

#if DEBUG
		this.DesignSetup();
#endif
		DownloadActivity.ViewModel = AppServices.Get<DownloadActivityBarViewModel>();
		this.WhenActivated(d =>
		{
			ViewModel ??= ViewModelLocator.Footer;
		});
	}
}