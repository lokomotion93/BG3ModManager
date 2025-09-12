using DynamicData;
using DynamicData.Binding;

namespace ModManager.Models.Mod;
public class ModContainer : ReactiveObject, IModEntry
{
	public ModEntryType EntryType => ModEntryType.Container;

	[Reactive] public string UUID { get; set; }
	[Reactive] public string? DisplayName { get; set; }
	[Reactive] public int Index { get; set; }
	[Reactive] public bool IsActive { get; set; }
	[Reactive] public bool IsHidden { get; set; }
	[Reactive] public bool IsSelected { get; set; }
	[Reactive] public bool IsExpanded { get; set; }
	[Reactive] public bool IsDraggable { get; set; }
	[Reactive] public bool PreserveSelection { get; set; }
	[Reactive] public string? SelectedColor { get; set; }
	[Reactive] public string? ListColor { get; set; }
	[Reactive] public string? PointerOverColor { get; set; }

	public string? Version => string.Empty;
	public string? Author => string.Empty;
	public string? LastUpdated => string.Empty;
	public bool CanDelete => true;

	public ObservableCollectionExtended<IModEntry> Mods { get; } = [];
	public IObservableCollection<IModEntry>? Children => Mods;

	public RxCommandUnit ToggleForceAllowInLoadOrderCommand { get; }
	public RxCommandUnit ToggleNameDisplayCommand { get; }

	[ObservableAsProperty] public bool CanForceAllowInLoadOrder { get; }
	[ObservableAsProperty] public bool ForceAllowInLoadOrder { get; }
	[ObservableAsProperty] public bool DisplayFileForName { get; }
	[ObservableAsProperty] public string? ForceAllowInLoadOrderLabel { get; }
	[ObservableAsProperty] public string? ToggleModNameLabel { get; }

	public string? Export(ModExportType exportType) => string.Empty;

	private static readonly Type _modDataType = typeof(ModData);

	private static void SetProperty<T>(IModEntry entry, string propertyName, T value)
	{
		if (entry.EntryType == ModEntryType.Mod && entry is ModEntry modEntry)
		{
			_modDataType.GetProperty(propertyName)?.SetValue(modEntry.Data, value);
		}
		else if (entry.EntryType == ModEntryType.Container && entry is ModContainer modContainer)
		{
			foreach(var mod in modContainer.Mods)
			{
				SetProperty(mod, propertyName, value);
			}
		}
	}

	private static bool PropertyMatches<T>(IModEntry entry, string propertyName, T value)
	{
		if (entry.EntryType == ModEntryType.Mod && entry is ModEntry modEntry)
		{
			return _modDataType.GetProperty(propertyName)?.GetValue(modEntry.Data)?.Equals(value) == true;
		}
		else if (entry.EntryType == ModEntryType.Container && entry is ModContainer modContainer)
		{
			return modContainer.Mods.All(x => PropertyMatches(x, propertyName, value));
		}
		return false;
	}

	private static bool AllCanForceAllowInLoadOrder(IModEntry entry) => PropertyMatches(entry, nameof(ModData.CanForceAllowInLoadOrder), true);
	private static bool AllForceLoadedInLoadOrder(IModEntry entry) => PropertyMatches(entry, nameof(ModData.ForceAllowInLoadOrder), true);
	private static bool AllDisplayFileForName(IModEntry entry) => PropertyMatches(entry, nameof(ModData.DisplayFileForName), true);

	public ModContainer(string uuid)
	{
		UUID = uuid;
		this.WhenAnyValue(x => x.IsHidden).Subscribe(b =>
		{
			if (!b) IsSelected = false;
		});


		var modsConn = this.Mods.ToObservableChangeSet().ToCollection();

		var hasChildren = modsConn.Select(_ => Mods.Count > 0);

		modsConn.Select(x => x.All(AllCanForceAllowInLoadOrder)).ToUIProperty(this, x => x.CanForceAllowInLoadOrder);
		modsConn.Select(x => x.All(AllForceLoadedInLoadOrder)).ToUIProperty(this, x => x.ForceAllowInLoadOrder);
		modsConn.Select(x => x.All(AllDisplayFileForName)).ToUIProperty(this, x => x.DisplayFileForName);

		this.WhenAnyValue(x => x.DisplayFileForName).Select(b => b ? Loca.Mod_Command_DisplayFileForName_Disable : Loca.Mod_Command_DisplayFileForName_Enable).ToUIProperty(this, x => x.ToggleModNameLabel);

		this.WhenAnyValue(x => x.ForceAllowInLoadOrder).Select(b => b ? Loca.Mod_Command_ForceAllowInLoadOrder_Disable : Loca.Mod_Command_ForceAllowInLoadOrder_Enable).ToUIProperty(this, x => x.ForceAllowInLoadOrderLabel);

		this.WhenAnyValue(x => x.DisplayFileForName).Subscribe(b =>
		{
			DivinityApp.Log($"DisplayFileForName: {b}");
		});

		var canForceAllowInLoadOrder = this.WhenAnyValue(x => x.CanForceAllowInLoadOrder);
		ToggleForceAllowInLoadOrderCommand = ReactiveCommand.Create(() =>
		{
			var b = !ForceAllowInLoadOrder;
			SetProperty(this, nameof(ModData.ForceAllowInLoadOrder), b);
		}, canForceAllowInLoadOrder);

		ToggleNameDisplayCommand = ReactiveCommand.Create(() =>
		{
			var b = !DisplayFileForName;
			SetProperty(this, nameof(ModData.DisplayFileForName), b);
		}, hasChildren);
	}
}
