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
		if (!IsOpen)
		{
			ShowAt(target, showAtPointer);
		}
		else
		{
			Close();
		}
	}
}