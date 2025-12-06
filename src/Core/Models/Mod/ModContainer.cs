using DynamicData;
using DynamicData.Binding;

using ModManager.Json;

using System.Reflection;
using ModManager.Models.Mod.Order;
using ModManager.Models.Interfaces;

namespace ModManager.Models.Mod;
public partial class ModContainer : ReactiveObject, IModEntry, INested<IObservableCollection<IModEntry>, IModEntry>
{
	public ModEntryType EntryType => ModEntryType.Container;

	[Reactive] public partial string UUID { get; set; }
	[Reactive] public partial int Index { get; set; }
	[Reactive] public partial bool IsActive { get; set; }
	[Reactive] public partial bool IsHidden { get; set; }
	[Reactive] public partial bool IsSelected { get; set; }
	[Reactive] public partial bool IsExpanded { get; set; }
	[Reactive] public partial bool IsDraggable { get; set; }
	[Reactive] public partial bool PreserveSelection { get; set; }
	[Reactive] public partial bool PreserveExpanded { get; set; }
	[Reactive] public partial string? SelectedColor { get; set; }
	[Reactive] public partial string? ListColor { get; set; }
	[Reactive] public partial string? PointerOverColor { get; set; }
	[Reactive] public partial bool IsDirty { get; set; }
	[Reactive] public partial object? ContextMenu { get; set; }
	[Reactive] public partial ModContainerSettings Settings { get; set; }
	[Reactive] public partial bool EnableAutosaving { get; set; }

	public string? Version => string.Empty;
	public string? Author => string.Empty;
	public string? LastUpdated => string.Empty;
	public bool CanDelete => true;

	private readonly ObservableCollectionExtended<IModEntry> _children = [];
	public IObservableCollection<IModEntry> Children => _children;

	public RxCommandUnit ToggleForceAllowInLoadOrderCommand { get; }
	public RxCommandUnit ToggleNameDisplayCommand { get; }

	[ObservableAsProperty] public partial string? DisplayName { get; }
	[ObservableAsProperty] public partial string? Description { get; }
	[ObservableAsProperty] public partial string? ContainerToolTipTitleText { get; }
	[ObservableAsProperty] public partial ModContainerIconSettings? Icon { get; }
	[ObservableAsProperty] public partial bool CanForceAllowInLoadOrder { get; }
	[ObservableAsProperty] public partial bool ForceAllowInLoadOrder { get; }
	[ObservableAsProperty] public partial bool DisplayFileForName { get; }
	[ObservableAsProperty] public partial bool IsVisible { get; }
	[ObservableAsProperty] public partial bool HasDescription { get; }
	[ObservableAsProperty] public partial bool HasIcon { get; }
	[ObservableAsProperty] public partial string? ForceAllowInLoadOrderLabel { get; }
	[ObservableAsProperty] public partial string? ToggleModNameLabel { get; }

	public string? Export(ModExportType exportType) => string.Empty;

	private static readonly Type _modDataType = typeof(ModData);

	private static void SetProperty<T>(IModEntry entry, string propertyName, T value)
	{
		if (entry.EntryType == ModEntryType.Mod && entry is ModEntry modEntry)
		{
			_modDataType.GetProperty(propertyName)?.SetValue(modEntry.Data, value);
		}
		else if (entry.EntryType == ModEntryType.Container && entry is ModContainer modContainer && modContainer.Children != null)
		{
			foreach(var child in modContainer.Children)
			{
				SetProperty(child, propertyName, value);
			}
		}
		entry.RaisePropertyChanged(nameof(IModEntry.IsDirty));
	}

	private static bool PropertyMatches<T>(IModEntry entry, string propertyName, T value)
	{
		if (entry.EntryType == ModEntryType.Mod && entry is ModEntry modEntry)
		{
			return _modDataType.GetProperty(propertyName)?.GetValue(modEntry.Data)?.Equals(value) == true;
		}
		else if (entry.EntryType == ModEntryType.Container && entry is ModContainer modContainer && modContainer.Children != null)
		{
			return modContainer.Children.All(x => PropertyMatches(x, propertyName, value));
		}
		return false;
	}

	private static bool AllCanForceAllowInLoadOrder(IModEntry entry) => PropertyMatches(entry, nameof(ModData.CanForceAllowInLoadOrder), true);
	private static bool AllForceLoadedInLoadOrder(IModEntry entry) => PropertyMatches(entry, nameof(ModData.ForceAllowInLoadOrder), true);
	private static bool AllDisplayFileForName(IModEntry entry) => PropertyMatches(entry, nameof(ModData.DisplayFileForName), true);

	private static readonly string[] _settingsProperties = [.. typeof(ModContainerSettings)
		.GetRuntimeProperties()
		.Where(prop => Attribute.IsDefined(prop, typeof(DataMemberAttribute)))
		.Select(prop => prop.Name)];

	private static IDisposable? _autosaveDisp;
	private IDisposable? _propertyChangeDisp;

	public void ToggleAutosaving(bool b)
	{
		if(b)
		{
			_propertyChangeDisp?.Dispose();
			_propertyChangeDisp = Settings.WhenAnyPropertyChanged(_settingsProperties).Skip(1).Throttle(TimeSpan.FromMilliseconds(50)).Subscribe(c =>
			{
				_autosaveDisp?.Dispose();
				_autosaveDisp = RxApp.TaskpoolScheduler.Schedule(TimeSpan.FromMilliseconds(250), () =>
				{
					var settings = Locator.Current.GetService<ISettingsService>();
					if (settings != null)
					{
						settings.TrySave(settings.ContainerSettings, out _);
					}
				});
			});
		}
		else
		{
			_propertyChangeDisp?.Dispose();
		}
	}

	public ModOrderContainer ToSerialized()
	{
		var container = new ModOrderContainer(UUID) { Name = Settings.DisplayName, Settings = Settings};
		if(Children != null)
		{
			foreach (var child in Children)
			{
				if (child.EntryType == ModEntryType.Container && child is ModContainer subContainer)
				{
					container.Children.Add(subContainer.ToSerialized());
				}
				else if (child.EntryType == ModEntryType.Mod && child is ModEntry mod)
				{
					container.Children.Add(mod.ToSerialized());
				}
			}
		}
		return container;
	}

	private string? GetContainerToolTipTitleText()
	{
		if(_children.Count < 0)
		{
			return Loca.ModContainer_ToolTip_Children.SafeFormat("Container (0 Children)", 0);
		}
		else
		{
			var total = 0;
			foreach(var entry in this.ForEachNested())
			{
				total++;
			}
			return Loca.ModContainer_ToolTip_Children.SafeFormat($"Container ({total} Children)", total);
		}
	}

	public void RemoveNested(HashSet<string> uuids)
	{
		List<IModEntry> toRemove = [];
		foreach(var child in Children)
		{
			if(uuids.Contains(child.UUID))
			{
				toRemove.Add(child);
			}
			if(child.EntryType == ModEntryType.Container && child is ModContainer container)
			{
				container.RemoveNested(uuids);
			}
		}
		Children.Remove(toRemove);
	}

	public ModContainer(string uuid)
	{
		UUID = uuid;
		this.WhenAnyValue(x => x.IsHidden).Subscribe(b =>
		{
			if (!b) IsSelected = false;
		});

		_isVisibleHelper = this.WhenAnyValue(x => x.IsHidden, b => !b).ToUIProperty(this, x => x.IsVisible, true);

		Settings = new(uuid);
		_displayNameHelper = Settings.WhenAnyValue(x => x.DisplayName).ToUIProperty(this, x => x.DisplayName);
		_descriptionHelper = Settings.WhenAnyValue(x => x.Description).ToUIProperty(this, x => x.Description);
		_hasDescriptionHelper = this.WhenAnyValue(x => x.Description, Validators.IsValid).ToUIProperty(this, x => x.HasDescription, false);

		_iconHelper = Settings.WhenAnyValue(x => x.Icon).ToUIProperty(this, x => x.Icon);
		_hasIconHelper = this.WhenAnyValue(x => x.Icon).Select(x => x != null).ToUIProperty(this, x => x.HasIcon);

		Settings.WhenAnyValue(x => x.IsExpanded).ObserveOn(RxApp.MainThreadScheduler).BindTo(this, x => x.IsExpanded);
		this.WhenAnyValue(x => x.IsExpanded).BindTo(Settings, x => x.IsExpanded);

		_containerToolTipTitleTextHelper = _children.ToObservableChangeSet().CountChanged()
			.Select(_ => GetContainerToolTipTitleText())
			.ToUIProperty(this, x => x.ContainerToolTipTitleText);

		var modsConn = _children.ToObservableChangeSet().AutoRefresh(x => x.IsDirty, TimeSpan.FromMilliseconds(25));

		var hasChildren = modsConn.CountChanged().Select(_ => _children.Count > 0);

		_canForceAllowInLoadOrderHelper = modsConn.Select(_ => _children.All(AllCanForceAllowInLoadOrder)).ToUIProperty(this, x => x.CanForceAllowInLoadOrder);
		_forceAllowInLoadOrderHelper = modsConn.Select(_ => _children.All(AllForceLoadedInLoadOrder)).ToUIProperty(this, x => x.ForceAllowInLoadOrder);
		_displayFileForNameHelper = modsConn.Select(_ => _children.All(AllDisplayFileForName)).ToUIProperty(this, x => x.DisplayFileForName);

		_toggleModNameLabelHelper = this.WhenAnyValue(x => x.DisplayFileForName).Select(b => b ? Loca.Mod_Command_DisplayFileForName_Disable : Loca.Mod_Command_DisplayFileForName_Enable).ToUIProperty(this, x => x.ToggleModNameLabel);

		_forceAllowInLoadOrderLabelHelper = this.WhenAnyValue(x => x.ForceAllowInLoadOrder)
			.Select(b => b ? Loca.Mod_Command_ForceAllowInLoadOrder_Disable : Loca.Mod_Command_ForceAllowInLoadOrder_Enable)
			.ToUIProperty(this, x => x.ForceAllowInLoadOrderLabel);

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

		this.WhenAnyValue(x => x.EnableAutosaving).Subscribe(ToggleAutosaving);
	}

	public ModContainer(string id, string name) : this(id)
	{
		Settings.DisplayName = name;
	}
}
