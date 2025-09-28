namespace ModManager;

[Flags]
public enum ModExtenderStatus
{
	None,
	Supports,
	Fulfilled,
	MissingRequiredVersion,
	MissingAppData,
	MissingUpdater,
}