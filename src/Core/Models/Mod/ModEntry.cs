using DynamicData.Binding;

using ModManager.Models.Mod.Order;

using System.Globalization;

namespace ModManager.Models.Mod;
public partial class ModEntry : ReactiveObject, IModEntry
{
	public ModEntryType EntryType => ModEntryType.Mod;

	public string UUID { get; }
	[Reactive] public partial int Index { get; set; }

	[Reactive] public partial bool IsActive { get; set; }
	[Reactive] public partial bool IsHidden { get; set; }
	[Reactive] public partial bool IsSelected { get; set; }
	[Reactive] public partial bool IsExpanded { get; set; }
	[Reactive] public partial bool IsDraggable { get; set; }
	[Reactive] public partial bool PreserveSelection { get; set; }
	public bool PreserveExpanded { get => false; set { } }
	[Reactive] public partial bool IsDirty { get; set; }
	[Reactive] public partial object? ContextMenu { get; set; }

	[ObservableAsProperty] public partial string? DisplayName { get; }
	[ObservableAsProperty] public partial string? FilePath { get; }
	[ObservableAsProperty] public partial string? Folder { get; }
	[ObservableAsProperty] public partial string? Version { get; }
	[ObservableAsProperty] public partial string? Author { get; }
	[ObservableAsProperty] public partial string? LastUpdated { get; }
	[ObservableAsProperty] public partial bool CanDelete { get; }
	[ObservableAsProperty] public partial bool IsVisible { get; }
	[ObservableAsProperty] public partial string? SelectedColor { get; }
	[ObservableAsProperty] public partial string? PointerOverColor { get; }
	[ObservableAsProperty] public partial string? ListColor { get; }

	public IObservableCollection<IModEntry>? Children => new ObservableCollectionExtended<IModEntry>();

	public string? Export(ModExportType exportType) => string.Empty;

	[Reactive] public partial ModData? Data { get; set; }

	public ModOrderMod ToSerialized() => new(UUID) { Name = Data?.Name ?? DisplayName };

	private static string DateToString(DateTimeOffset? date)
	{
		if(date != null && date != DateTimeOffset.MinValue)
		{
			return date.Value.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture);
		}
		return string.Empty;
	}

	public ModEntry(string uuid)
	{
		UUID = uuid;

		_isVisibleHelper = this.WhenAnyValue(x => x.IsHidden, b => !b).ToUIProperty(this, x => x.IsVisible, true);

		var whenMod = this.WhenAnyValue(x => x.Data).WhereNotNull();

		whenMod.Select(x => x.Index).BindTo(this, x => x.Index);

		_displayNameHelper = whenMod.Select(x => x.DisplayName).ToUIProperty(this, x => x.DisplayName);
		_filePathHelper = whenMod.Select(x => x.FilePath).ToUIProperty(this, x => x.FilePath);
		_folderHelper = whenMod.Select(x => x.Folder).ToUIProperty(this, x => x.Folder);
		_versionHelper = whenMod.Select(x => x.Version.Version).ToUIProperty(this, x => x.Version);
		_authorHelper = whenMod.Select(x => x.Author).ToUIProperty(this, x => x.Author);
		_listColorHelper = whenMod.Select(x => x.ListColor).ToUIProperty(this, x => x.ListColor);
		_selectedColorHelper = whenMod.Select(x => x.SelectedColor).ToUIProperty(this, x => x.SelectedColor);
		_pointerOverColorHelper = whenMod.Select(x => x.PointerOverColor).ToUIProperty(this, x => x.PointerOverColor);
		_lastUpdatedHelper = whenMod.Select(x => DateToString(x.LastModified)).ToUIProperty(this, x => x.LastUpdated, string.Empty);

		_canDeleteHelper = whenMod.Select(x => x.CanDelete).ToUIProperty(this, x => x.CanDelete);

		this.WhenAnyValue(x => x.IsHidden).Subscribe(b =>
		{
			if (b) IsSelected = false;
		});

		this.WhenAnyValue(x => x.IsSelected).Subscribe(b =>
		{
			Data?.IsSelected = b;
		});

		this.WhenAnyValue(x => x.IsActive).Subscribe(b =>
		{
			Data?.IsActive = b;
		});
	}

	public ModEntry(ModData modData) : this(modData.UUID)
	{
		Data = modData;
		IsSelected = false;
	}
}
