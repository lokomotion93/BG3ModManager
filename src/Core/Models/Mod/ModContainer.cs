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
	[Reactive] public partial bool PreserveExpanded { get; set; }
	[Reactive] public partial string? SelectedColor { get; set; }
	[Reactive] public partial string? ListColor { get; set; }
	[Reactive] public partial string? PointerOverColor { get; set; }
	[Reactive] public partial bool IsDirty { get; set; }
	[Reactive] public partial object? ContextMenu { get; set; }
	[Reactive] public partial ModContainerSettings Settings { get; set; }
	[Reactive] public partial ModContainerSettings? GlobalSettings { get; set; }
	[Reactive] public partial bool EnableAutosaving { get; set; }

	public string? Version => string.Empty;
	public string? Author => string.Empty;
	public string? LastUpdated => string.Empty;
	public bool CanDelete => true;

	private readonly ObservableCollectionExtended<IModEntry> _children = [];
	public IObservableCollection<IModEntry> Children => _children;

	public RxCommandUnit ToggleNameDisplayCommand { get; }
	public ReactiveCommand<ModContainerIconSettings?, Unit> RenderIconCommand { get; }

	[ObservableAsProperty] public partial string? DisplayName { get; }
	[ObservableAsProperty] public partial string? Description { get; }
	[ObservableAsProperty] public partial string? ContainerToolTipTitleText { get; }
	[ObservableAsProperty] public partial bool DisplayFileForName { get; }
	[ObservableAsProperty] public partial bool IsVisible { get; }
	[ObservableAsProperty] public partial bool HasDescription { get; }
	[ObservableAsProperty] public partial bool HasIcon { get; }
	[ObservableAsProperty] public partial string? ToggleModNameLabel { get; }

	[ObservableAsProperty] public partial string? ForegroundColor { get; }
	[ObservableAsProperty] public partial string? BackgroundColor { get; }
	[ObservableAsProperty] public partial string? BorderColor { get; }
	[ObservableAsProperty] public partial string? BorderThickness { get; }
	[ObservableAsProperty] public partial ModContainerIconSettings? Icon { get; }

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
		else if (entry.EntryType == ModEntryType.Container && entry is ModContainer modContainer && modContainer.Children.Count > 0)
		{
			return modContainer.Children.All(x => PropertyMatches(x, propertyName, value));
		}
		return false;
	}

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

	public void SetChildSelection(bool isSelected)
	{
		foreach(var child in this.ForEachNested())
		{
			child.IsSelected = isSelected;
		}
	}

	public int[]? GetIndexPath(string uuid, int? parentIndex)
	{
		for (var i = 0; i < Children.Count; i++)
		{
			var child = Children[i];
			if (child.UUID == uuid)
			{
				if(parentIndex != null)
				{
					return [parentIndex.Value, i];
				}
				return [i];
			}
			else if (child.EntryType == ModEntryType.Container && child is ModContainer container)
			{
				var path = container.GetIndexPath(uuid, i);
				if (path != null)
				{
					return path;
				}
			}
		}
		return null;
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

		Settings.WhenAnyValue(x => x.IsExpanded).ObserveOn(RxApp.MainThreadScheduler).BindTo(this, x => x.IsExpanded);
		this.WhenAnyValue(x => x.IsExpanded).BindTo(Settings, x => x.IsExpanded);

		_containerToolTipTitleTextHelper = _children.ToObservableChangeSet().CountChanged()
			.Select(_ => GetContainerToolTipTitleText())
			.ToUIProperty(this, x => x.ContainerToolTipTitleText);

		var modsConn = _children.ToObservableChangeSet().AutoRefresh(x => x.IsDirty, TimeSpan.FromMilliseconds(500));

		var childrenObs = modsConn.CountChanged().Throttle(TimeSpan.FromMilliseconds(50));
		var hasChildren = childrenObs.Select(_ => _children.Count > 0);

		_displayFileForNameHelper = childrenObs.Select(_ => _children.All(AllDisplayFileForName)).ObserveOn(RxApp.MainThreadScheduler).ToUIProperty(this, x => x.DisplayFileForName);

		_toggleModNameLabelHelper = this.WhenAnyValue(x => x.DisplayFileForName).Select(b => b ? Loca.Mod_Command_DisplayFileForName_Disable : Loca.Mod_Command_DisplayFileForName_Enable).ToUIProperty(this, x => x.ToggleModNameLabel);

		ToggleNameDisplayCommand = ReactiveCommand.Create(() =>
		{
			var b = !DisplayFileForName;
			SetProperty(this, nameof(ModData.DisplayFileForName), b);
		}, hasChildren);

		RenderIconCommand = ReactiveCommand.Create<ModContainerIconSettings?>(settings => { 
			Settings.Icon?.IsDirty = false;
		});

		this.WhenAnyValue(x => x.EnableAutosaving).Subscribe(ToggleAutosaving);

		string?[] nullStart = [null];

		var whenGlobalForegroundColor = this.WhenAnyValue(x => x.GlobalSettings, x => x.GlobalSettings.ForegroundColor, (g, _) => g?.ForegroundColor).StartWith(RxApp.MainThreadScheduler, nullStart);
		var whenGlobalBackgroundColor = this.WhenAnyValue(x => x.GlobalSettings, x => x.GlobalSettings.BackgroundColor, (g, _) => g?.BackgroundColor).StartWith(RxApp.MainThreadScheduler, nullStart);
		var whenGlobalBorderColor = this.WhenAnyValue(x => x.GlobalSettings, x => x.GlobalSettings.BorderColor, (g, _) => g?.BorderColor).StartWith(RxApp.MainThreadScheduler, nullStart);
		var whenGlobalThickness = this.WhenAnyValue(x => x.GlobalSettings, x => x.GlobalSettings.BorderThickness, (g, _) => g?.BorderThickness).StartWith(RxApp.MainThreadScheduler, nullStart);

		var whenGlobalIcon = this.WhenAnyValue(x => x.GlobalSettings, x => x.GlobalSettings.Icon, (g, _) => g?.Icon).StartWith(RxApp.MainThreadScheduler, [null]);

		_foregroundColorHelper = this.WhenAnyValue(x => x.Settings.ForegroundColor).CombineLatest(whenGlobalForegroundColor)
			.Select(x => x.First ?? x.Second).ToUIPropertyImmediate(this, x => x.ForegroundColor);

		_backgroundColorHelper = this.WhenAnyValue(x => x.Settings.BackgroundColor).CombineLatest(whenGlobalBackgroundColor)
			.Select(x => x.First ?? x.Second).ToUIPropertyImmediate(this, x => x.BackgroundColor);

		_borderColorHelper = this.WhenAnyValue(x => x.Settings.BorderColor).CombineLatest(whenGlobalBorderColor)
			.Select(x => x.First ?? x.Second).ToUIPropertyImmediate(this, x => x.BorderColor);

		_borderThicknessHelper = this.WhenAnyValue(x => x.Settings.BorderThickness).CombineLatest(whenGlobalThickness)
			.Select(x => x.First ?? x.Second).ToUIPropertyImmediate(this, x => x.BorderThickness);

		_iconHelper = this.WhenAnyValue(x => x.Settings.Icon).CombineLatest(whenGlobalIcon)
			.Select(x => x.First ?? x.Second).ToUIPropertyImmediate(this, x => x.Icon);

		var whenGlobalIconDirty = this.WhenAnyValue(x => x.GlobalSettings, x => x.GlobalSettings.Icon, x => x.GlobalSettings.Icon.IsDirty, (x, _, _) => x?.Icon?.IsDirty == true).StartWith(false);
		var whenIconDirty = this.WhenAnyValue(x => x.Icon, x => x.Icon.IsDirty, (x, _) => x?.IsDirty == true).StartWith(false);

		whenIconDirty.Merge(whenGlobalIconDirty)
			.Where(x => x || GlobalSettings?.Icon?.IsDirty == true)
			.Throttle(TimeSpan.FromMilliseconds(500))
			.Select(_ => Icon)
			.ObserveOn(RxApp.MainThreadScheduler)
			.InvokeCommand(RenderIconCommand);

		_hasIconHelper = this.WhenAnyValue(x => x.Icon).Select(x => x != null).ToUIProperty(this, x => x.HasIcon);
	}

	public ModContainer(string id, string name) : this(id)
	{
		Settings.DisplayName = name;
	}
}
