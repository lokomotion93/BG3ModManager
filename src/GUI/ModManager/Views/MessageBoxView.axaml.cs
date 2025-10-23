using Avalonia.Media;

using ModManager.ViewModels;

namespace ModManager.Views;
public partial class MessageBoxView : ReactiveUserControl<MessageBoxViewModel>
{
	public MessageBoxView()
	{
		InitializeComponent();

#if DEBUG
		if (Design.IsDesignMode)
		{
			Background = Brushes.Black;
		}
#endif

		//Something is setting the DataContext to MainWindowViewModel briefly, which throws an exception if x:CompileBindings is enabled

		this.WhenActivated(d =>
		{
			ViewModel ??= ViewModelLocator.MessageBox;
			if (ViewModel != null)
			{
				this.GetObservable(IsVisibleProperty).BindTo(ViewModel, x => x.IsVisible);

				ViewModel.WhenAnyValue(x => x.IsInput).Subscribe(b =>
				{
					if(ViewModel.IsVisible)
					{
						InputTextBox.Focus(NavigationMethod.Pointer);
						InputTextBox.SelectAll();
					}
				});
			}
		});
	}
}