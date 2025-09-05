using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using ModManager.Controls;
using ModManager.ViewModels;
using ModManager.Windows;

namespace ModManager;

public partial class AppUpdateWindow : HideWindowBase<AppUpdateWindowViewModel>
{
    public AppUpdateWindow()
    {
        InitializeComponent();

		ViewModel ??= AppServices.Get<AppUpdateWindowViewModel>();

		this.WhenActivated(d =>
		{
			if (ViewModel != null)
			{
				this.GetObservable(IsVisibleProperty).BindTo(ViewModel, x => x.IsVisible);
				this.MarkdownScrollViewer.GetObservable(BoundsProperty).Select(x => x.Width - 100).BindTo(ViewModel, x => x.ScrollViewerWidth);
			}
		});
	}
}