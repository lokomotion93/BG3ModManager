using DynamicData.Binding;

using Material.Icons;

using System.Windows.Input;

namespace ModManager.Models.Menu;
public partial class MenuEntry : ReactiveObject, IMenuEntry
{
	[Reactive] public partial string? DisplayName { get; set; }
	[Reactive] public partial string? ToolTip { get; set; }
	[Reactive] public partial ICommand? Command { get; set; }
	[Reactive] public partial bool UseAccessShortcut { get; set; }
	[Reactive] public partial MaterialIconKind? MaterialIcon { get; set; }
	[Reactive] public partial string? IconForeground { get; set; }

	public ObservableCollectionExtended<IMenuEntry>? Children { get; set; }

	public MenuEntry WithIcon(MaterialIconKind kind, string? foregroundColor = null)
	{
		MaterialIcon = kind;
		if (foregroundColor != null) IconForeground = foregroundColor;
		return this;
	}

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