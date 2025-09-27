using Avalonia.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModManager.Controls;
public abstract class HideWindowBase<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
	public HideWindowBase()
	{
#if DEBUG
		if (Design.IsDesignMode)
		{
			Background = Brushes.Black;
		}
#endif
		Closing += OnHideWindow;
	}

	public virtual void OnHideWindow(object? sender, WindowClosingEventArgs e)
	{
		//Only prevent actual closing when the window itself is closed,
		//otherwise this would prevent closing the application when closing the parent window
		if (e.CloseReason == WindowCloseReason.WindowClosing)
		{
			e.Cancel = true;
			this.Hide();
		}
	}
}
