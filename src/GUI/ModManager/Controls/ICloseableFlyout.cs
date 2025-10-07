namespace ModManager.Controls;
public interface ICloseableFlyout
{
	void Close();
	void Toggle(Control target, bool showAtPointer = false);
}
