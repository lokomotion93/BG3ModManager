using DynamicData;
using DynamicData.Binding;

using ModManager.Json;

using System.Reflection;
using ModManager.Models.Mod.Order;
using ModManager.Models.Interfaces;

namespace ModManager.Models.Mod;
public class ModContainer : ReactiveObject, IModEntry, INested<IObservableCollection<IModEntry>, IModEntry>
{
	public ModEntryType EntryType => ModEntryType.Container;

	[Reactive] public string UUID { get; set; }
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
	[Reactive] public bool IsDirty { get; set; }
	[Reactive] public object? ContextMenu { get; set; }
	[Reactive] public ModContainerSettings Settings { get; set; }
	[Reactive] public bool EnableAutosaving { get; set; }

	public string? Version => string.Empty;
	public string? Author => string.Empty;
	public string? LastUpdated => string.Empty;
	public bool CanDelete => true;

	private readonly ObservableCollectionExtended<IModEntry> _children = [];
	public IObservableCollection<IModEntry> Children => _children;

	public RxCommandUnit ToggleForceAllowInLoadOrderCommand { get; }
	public RxCommandUnit ToggleNameDisplayCommand { get; }

	[ObservableAsProperty] public string? DisplayName { get; }
	[ObservableAsProperty] public string? Description { get; }
	[ObservableAsProperty] public string? ContainerToolTipTitleText { get; }
	[ObservableAsProperty] public bool CanForceAllowInLoadOrder { get; }
	[ObservableAsProperty] public bool ForceAllowInLoadOrder { get; }
	[ObservableAsProperty] public bool DisplayFileForName { get; }
	[ObservableAsProperty] public bool IsVisible { get; }
	[ObservableAsProperty] public bool HasDescription { get; }
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
		var container = new ModOrderContainer(UUID) { Name = this.DisplayName, Settings = Settings };
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

	public ModContainer(string uuid)
	{
		UUID = uuid;
		this.WhenAnyValue(x => x.IsHidden).Subscribe(b =>
		{
			if (!b) IsSelected = false;
		});

		this.WhenAnyValue(x => x.IsHidden, b => !b).ToUIProperty(this, x => x.IsVisible, true);

		Settings = new(uuid);
		this.WhenAnyValue(x => x.Settings.DisplayName).ToUIProperty(this, x => x.DisplayName);
		this.WhenAnyValue(x => x.Settings.Description).ToUIProperty(this, x => x.Description);
		this.WhenAnyValue(x => x.Description, x => x.IsValid()).ToUIProperty(this, x => x.HasDescription, false);
		_children.ToObservableChangeSet().CountChanged().Select(_ => GetContainerToolTipTitleText()).ToUIProperty(this, x => x.ContainerToolTipTitleText);

		var modsConn = _children.ToObservableChangeSet().AutoRefresh(x => x.IsDirty, TimeSpan.FromMilliseconds(25));

		var hasChildren = modsConn.CountChanged().Select(_ => _children.Count > 0);

		modsConn.Select(_ => _children.All(AllCanForceAllowInLoadOrder)).ToUIProperty(this, x => x.CanForceAllowInLoadOrder);
		modsConn.Select(_ => _children.All(AllForceLoadedInLoadOrder)).ToUIProperty(this, x => x.ForceAllowInLoadOrder);
		modsConn.Select(_ => _children.All(AllDisplayFileForName)).ToUIProperty(this, x => x.DisplayFileForName);

		this.WhenAnyValue(x => x.DisplayFileForName).Select(b => b ? Loca.Mod_Command_DisplayFileForName_Disable : Loca.Mod_Command_DisplayFileForName_Enable).ToUIProperty(this, x => x.ToggleModNameLabel);

		this.WhenAnyValue(x => x.ForceAllowInLoadOrder).Select(b => b ? Loca.Mod_Command_ForceAllowInLoadOrder_Disable : Loca.Mod_Command_ForceAllowInLoadOrder_Enable).ToUIProperty(this, x => x.ForceAllowInLoadOrderLabel);

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
