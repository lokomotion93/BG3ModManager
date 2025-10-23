namespace ModManager;

public readonly struct MessageBoxResult(bool result, string? input, bool remember = false)
{
	public readonly bool Result = result;
	public readonly string? Input = input;
	public readonly bool RememberChoice = remember;

	public static bool operator true(MessageBoxResult x) => x.Result == true;
	public static bool operator false(MessageBoxResult x) => x.Result == false;
}