using DynamicData.Binding;

using System.Windows.Input;

namespace ModManager.Models.Menu;
public class MenuEntry : ReactiveObject, IMenuEntry
{
	[Reactive] public string? DisplayName { get; set; }
	[Reactive] public string? ToolTip { get; set; }
	[Reactive] public ICommand? Command { get; set; }
	[Reactive] public bool UseAccessShortcut { get; set; }
	public ObservableCollectionExtended<IMenuEntry>? Children { get; set; }

	public MenuEntry(string? name = null, ICommand? command = null, string? tooltip = null, bool useLocalization = false, bool useAccessShortcut = false)
	{
		DisplayName = name;
		Command = command;
		ToolTip = tooltip;
		UseAccessShortcut = useAccessShortcut;

		if (useLocalization)
		{
			if (DisplayName.IsValid())
			{
				AppServices.Locale.EntryToObservable(DisplayName).BindTo(this, x => x.DisplayName);
			}
			if (ToolTip.IsValid())
			{
				AppServices.Locale.EntryToObservable(ToolTip).BindTo(this, x => x.ToolTip);
			}
		}
	}

	public static MenuEntry FromKeybinding(ICommand command, string propertyName,
		Dictionary<string,KeybindingAttribute?> properties,
		ObservableCollectionExtended<IMenuEntry>? children = null)
	{
		if(properties.TryGetValue(propertyName, out var keybinding) && keybinding != null)
		{
			var entry = new MenuEntry(keybinding.DisplayName, command, keybinding.ToolTip, true);
			
			if (children != null)
			{
				entry.Children = children;
			}
			return entry;
		}
		throw new ArgumentException($"Property '{propertyName}' is invalid or is missing a KeybindingAttribute.", nameof(propertyName));
	}

	public override string ToString() => DisplayName ?? "";
}