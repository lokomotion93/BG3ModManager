namespace ModManager.ViewModels.Main;

public class ViewManager : ReactiveObject
{
	private readonly RoutingState Router;

	[ObservableAsProperty] public IRoutableViewModel? CurrentView { get; }

	public void SwitchToModOrderView() => Router.Navigate.Execute(ViewModelLocator.ModOrder).Subscribe();
	public void SwitchToDeleteView() => Router.Navigate.Execute(ViewModelLocator.DeleteFiles).Subscribe();
	public void SwitchToModUpdates() => Router.Navigate.Execute(ViewModelLocator.ModUpdates).Subscribe();
	public void SwitchToProgress() => Router.Navigate.Execute(ViewModelLocator.Progress).Subscribe();

	public ViewManager(RoutingState router)
	{
		Router = router;
		Router.CurrentViewModel.ToPropertyEx(this, x => x.CurrentView, false, RxApp.MainThreadScheduler);
	}
}
