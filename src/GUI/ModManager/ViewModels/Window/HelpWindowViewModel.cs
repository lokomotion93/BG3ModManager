namespace ModManager.ViewModels;

public partial class HelpWindowViewModel : ReactiveObject, IClosableViewModel, IRoutableViewModel
{
	#region IClosableViewModel/IRoutableViewModel
	[Reactive] public partial string WindowTitle { get; set; }
	[Reactive] public partial string HelpTitle { get; set; }
	[Reactive] public partial string HelpText { get; set; }
	#endregion

	//IClosableViewModel
	public string UrlPathSegment => "help";
	public IScreen HostScreen { get; }
	[Reactive] public partial bool IsVisible { get; set; }
	public RxCommandUnit CloseCommand { get; }

	public HelpWindowViewModel(IScreen? host = null)
	{
		HostScreen = host ?? AppLocator.Current.GetService<IScreen>()!;
		CloseCommand = this.CreateCloseCommand();

		WindowTitle = "Help";
		HelpTitle = "";
		HelpText = "";
	}
}
