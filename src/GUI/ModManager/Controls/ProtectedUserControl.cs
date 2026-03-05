using Avalonia.Media;

using System.Diagnostics;

namespace ModManager.Controls;
public class ProtectedUserControl<TViewModel> : ReactiveUserControl<TViewModel> where TViewModel : class
{
	private static readonly Type _vmType = typeof(TViewModel);

	public ProtectedUserControl()
	{
#if DEBUG
		this.DesignSetup();
#endif
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
	{
		if ((e.Property == ViewModelProperty || e.Property == DataContextProperty) && e.NewValue != null && e.NewValue?.GetType() != _vmType)
		{
			return;
		}
		base.OnPropertyChanged(e);
	}
}