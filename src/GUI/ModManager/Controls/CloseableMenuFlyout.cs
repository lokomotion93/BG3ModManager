namespace ModManager.Controls;
public class CloseableMenuFlyout : MenuFlyout, ICloseableFlyout
{
	public void Close()
	{
		if (IsOpen)
		{
			Hide();
		}
	}

	public void Toggle(Control target, bool showAtPointer = false)
	{
		if (IsOpen)
		{
			Close();
		}
		else
		{
			ShowAt(target, showAtPointer);
		}
	}
}