using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using ModManager.ViewModels;

namespace ModManager.Views;

public partial class DownloadActivityBar : ReactiveUserControl<DownloadActivityBarViewModel>
{
	public DownloadActivityBar()
	{
		InitializeComponent();

#if DEBUG
		if (Design.IsDesignMode)
		{
			Background = Brushes.DarkGray;
		}
#endif
		this.WhenActivated(d =>
		{
			if (ViewModel != null)
			{
				this.GetObservable(IsVisibleProperty).BindTo(ViewModel, x => x.IsVisible);
			}
		});
	}
}