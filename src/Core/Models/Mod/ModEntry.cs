using DynamicData;
using DynamicData.Binding;

using ModManager.Models.Mod.Game;
using ModManager.Models.Mod.Order;
using ModManager.Services;
using ModManager.Util;

using System.Globalization;
using System.Text;

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
	public bool PreserveExpanded { get => false; set { } }
	[Reactive] public partial bool IsDirty { get; set; }
	[Reactive] public partial object? ContextMenu { get; set; }

	[Reactive] public partial bool CanDrag { get; set; }
	[Reactive] public partial bool HasColorOverride { get; set; }
	[Reactive] public partial string? SelectedColor { get; set; }
	[Reactive] public partial string? PointerOverColor { get; set; }
	[Reactive] public partial string? ListColor { get; set; }


	protected ReadOnlyObservableCollection<ModuleShortDesc> _displayedDependencies;
	public ReadOnlyObservableCollection<ModuleShortDesc> DisplayedDependencies => _displayedDependencies;

	protected ReadOnlyObservableCollection<ModuleShortDesc> _displayedConflicts;
	public ReadOnlyObservableCollection<ModuleShortDesc> DisplayedConflicts => _displayedConflicts;


	[ObservableAsProperty] public partial string? DisplayName { get; }
	[ObservableAsProperty] public partial string? Description { get; }
	[ObservableAsProperty] public partial string? FilePath { get; }
	[ObservableAsProperty] public partial string? FileName { get; }
	[ObservableAsProperty] public partial string? Folder { get; }
	[ObservableAsProperty] public partial string? Version { get; }
	[ObservableAsProperty] public partial string? Author { get; }
	[ObservableAsProperty] public partial string? LastUpdated { get; }
	[ObservableAsProperty] public partial bool CanDelete { get; }
	[ObservableAsProperty] public partial bool IsVisible { get; }
	[ObservableAsProperty] public partial ScriptExtenderIconType ExtenderIcon { get; }

	[ObservableAsProperty] public partial int TotalDependencies { get; }
	[ObservableAsProperty] public partial int TotalConflicts { get; }

	[ObservableAsProperty] public partial string? ForceAllowInLoadOrderLabel { get; }
	[ObservableAsProperty] public partial string? ToggleModNameLabel { get; }
	[ObservableAsProperty] public partial string? DisplayVersion { get; }
	[ObservableAsProperty] public partial string? ModDisplayTypeName { get; }
	[ObservableAsProperty] public partial string? ModDisplayTypeForeground { get; }
	[ObservableAsProperty] public partial string? LastModifiedDateText { get; }
	[ObservableAsProperty] public partial string? Notes { get; }
	[ObservableAsProperty] public partial string? OsirisStatusIconPath { get; }
	[ObservableAsProperty] public partial string? OsirisStatusToolTipText { get; }
	[ObservableAsProperty] public partial string? ScriptExtenderSupportToolTipText { get; }


	[ObservableAsProperty] public partial bool HasAnyExternalLink { get; }
	[ObservableAsProperty] public partial bool HasAuthor { get; }
	[ObservableAsProperty] public partial bool HasDescription { get; }
	[ObservableAsProperty] public partial bool HasExtenderStatus { get; }
	[ObservableAsProperty] public partial bool HasFilePath { get; }
	[ObservableAsProperty] public partial bool HasGitHubLink { get; }
	[ObservableAsProperty] public partial bool HasModioLink { get; }
	[ObservableAsProperty] public partial bool HasNexusModsLink { get; }
	[ObservableAsProperty] public partial bool HasNotes { get; }
	[ObservableAsProperty] public partial bool HasDependencies { get; }
	[ObservableAsProperty] public partial bool HasConflicts { get; }
	[ObservableAsProperty] public partial bool HasToolTip { get; }
	[ObservableAsProperty] public partial bool HasInvalidUUID { get; }
	[ObservableAsProperty] public partial bool HasMissingDependency { get; }
	[ObservableAsProperty] public partial string? MissingDependencyToolTip { get; }
	[ObservableAsProperty] public partial bool HasOsirisStatus { get; }
	[ObservableAsProperty] public partial bool HasToolkitIcon { get; }
	[ObservableAsProperty] public partial bool HasLooseModIcon { get; }
	[ObservableAsProperty] public partial bool HasOverrideIcon { get; }


	#region NexusMods Properties
	[ObservableAsProperty] public partial Uri? NexusImageUrl { get; }
	[ObservableAsProperty] public partial bool NexusImageVisibility { get; }
	[ObservableAsProperty] public partial bool NexusModsInformationVisibility { get; }
	[ObservableAsProperty] public partial DateTime NexusModsCreatedDate { get; }
	[ObservableAsProperty] public partial DateTime NexusModsUpdatedDate { get; }
	[ObservableAsProperty] public partial string? NexusModsTooltipInfo { get; }

	#endregion

	public IObservableCollection<IModEntry>? Children => new ObservableCollectionExtended<IModEntry>();

	public string? Export(ModExportType exportType) => string.Empty;

	[Reactive] public partial ModData Data { get; set; }

	public ModOrderMod ToSerialized() => new(UUID) { Name = Data?.Name ?? DisplayName };

	private static string DateToString(DateTimeOffset? date)
	{
		if(date != null && date != DateTimeOffset.MinValue && date != DateTimeOffset.UnixEpoch)
		{
			return date.Value.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture);
		}
		return string.Empty;
	}

	private static ScriptExtenderIconType ExtenderModStatusToIcon(ModExtenderStatus status)
	{
		var result = ScriptExtenderIconType.None;

		if (status.HasFlag(ModExtenderStatus.MissingUpdater))
		{
			result = ScriptExtenderIconType.Missing;
		}
		else if (status.HasFlag(ModExtenderStatus.MissingRequiredVersion) || status.HasFlag(ModExtenderStatus.MissingAppData))
		{
			result = ScriptExtenderIconType.Warning;
		}
		else if (status.HasFlag(ModExtenderStatus.Supports))
		{
			result = ScriptExtenderIconType.FulfilledSupports;
		}
		else if (status.HasFlag(ModExtenderStatus.Fulfilled))
		{
			result = ScriptExtenderIconType.FulfilledRequired;
		}

		return result;
	}

	private static bool CheckForInvalidUUID(ValueTuple<string?, bool> x)
	{
		var uuid = x.Item1;
		var canAddToLoadOrder = x.Item2;
		if (!canAddToLoadOrder) return false;
		if (uuid.IsValid())
		{
			return !Guid.TryParse(uuid, out _);
		}
		return false;
	}

	private string BuildMissingDependencyToolTip()
	{
		var missingDependenciesText = string.Join(Environment.NewLine, Data.MissingDependencies.Items.Select(x => x.Name).Order());
		return Loca.Mod_MissingDependenciesToolTip.SafeFormat($"Missing Dependencies:\n{missingDependenciesText}", missingDependenciesText);
	}

	private static IColorResourceService? _colorRes => Locator.Current.GetService<IColorResourceService>();

	//Green
	private static string EditorProjectBackgroundColor => _colorRes?.GetColorHex("EditorProjectBackgroundColor", "#0C00FF4D") ?? "#0C00FF4D";
	private static string EditorProjectBackgroundSelectedColor => _colorRes?.GetColorHex("EditorProjectBackgroundSelectedColor", "#3200ED48") ?? "#3200ED48";
	private static string EditorProjectBackgroundPointerOverColor => _colorRes?.GetColorHex("EditorProjectBackgroundPointerOverColor", "#6400ED48") ?? "#6400ED48";
	//Brownish
	private static string ForceLoadedBackgroundColor => _colorRes?.GetColorHex("ForceLoadedBackgroundColor", "#32C1AE00") ?? "#32C1AE00";
	private static string ForceLoadedBackgroundSelectedColor => _colorRes?.GetColorHex("ForceLoadedBackgroundSelectedColor", "#32F2DE00") ?? "#32F2DE00";
	private static string ForceLoadedBackgroundPointerOverColor => _colorRes?.GetColorHex("ForceLoadedBackgroundPointerOverColor", "#64F2DE00") ?? "#64F2DE00";

	private void UpdateColors(ValueTuple<bool, bool, bool, bool, bool> x)
	{
		var isForceLoadedMergedMod = x.Item1;
		var isEditorMod = x.Item2;
		var isForceLoadedMod = x.Item3;
		var isActive = x.Item4 || x.Item5;

		if (isEditorMod)
		{
			SelectedColor = EditorProjectBackgroundSelectedColor;
			ListColor = EditorProjectBackgroundColor;
			PointerOverColor = EditorProjectBackgroundPointerOverColor;
		}
		else if (isForceLoadedMergedMod || isForceLoadedMod && isActive)
		{
			SelectedColor = ForceLoadedBackgroundSelectedColor;
			ListColor = ForceLoadedBackgroundColor;
			PointerOverColor = ForceLoadedBackgroundPointerOverColor;
		}
		else
		{
			ListColor = SelectedColor = PointerOverColor = string.Empty;
		}
	}

	private static string? OsirisStatusToIconPath(DivinityOsirisModStatus status)
	{
		return status switch
		{
			DivinityOsirisModStatus.SCRIPTS => "avares://ModManager/Assets/Icons/Osiris_16x.png",
			DivinityOsirisModStatus.MODFIXER => "avares://ModManager/Assets/Icons/Osiris_ModFixer_16x.png",
			_ => null,
		};
	}

	private static string OsirisStatusToTooltipText(DivinityOsirisModStatus status)
	{
		return status switch
		{
			DivinityOsirisModStatus.SCRIPTS => Loca.Mod_OsirisStatus_HasScripts,
			DivinityOsirisModStatus.MODFIXER => Loca.Mod_OsirisStatus_HasModFixer,
			_ => "",
		};
	}

	private static bool CanOpenModioBoolCheck(bool enabled, string? externalLink)
	{
		return enabled && externalLink.IsValid();
	}

	private static string NexusModsInfoToTooltip(DateTime createdDate, DateTime updatedDate, long endorsements)
	{
		var lines = new StringBuilder();

		if (endorsements > 0)
		{
			lines.AppendLine(Loca.Mod_NexusModsToolTip_Endorsements.SafeFormat($"Endorsements: {endorsements}", endorsements));
		}

		if (createdDate != DateTime.MinValue)
		{
			var createdDateStr = createdDate.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture);
			lines.AppendLine(Loca.Mod_NexusModsToolTip_CreatedOn.SafeFormat($"Created on {createdDateStr}", createdDateStr));
		}

		if (updatedDate != DateTime.MinValue)
		{
			var updatedDateStr = updatedDate.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture);
			lines.AppendLine(Loca.Mod_NexusModsToolTip_UpdatedOn.SafeFormat($"Last updated on {updatedDateStr}", updatedDateStr));
		}

		return lines.ToString();
	}

	public virtual string GetHelpText()
	{
		return "";
	}

	private static string ExtenderStatusToToolTipText(ModExtenderStatus status, int requiredVersion, int currentVersion)
	{
		var result = new StringBuilder();

		if (requiredVersion > -1)
		{
			result.AppendLine(Loca.Mod_ExtenderStatus_RequiresVersion.SafeFormat($"Requires Script Extender v{requiredVersion} or Higher", requiredVersion));
		}
		else
		{
			result.AppendLine(Loca.Mod_ExtenderStatus_RequiresDefault);
		}

		if (status.HasFlag(ModExtenderStatus.MissingAppData))
		{
			result.AppendLine(Loca.Mod_ExtenderStatus_MissingAppData.SafeFormat($"(Missing %LOCALAPPDATA%\\..\\{DivinityApp.EXTENDER_APPDATA_DLL})", DivinityApp.EXTENDER_APPDATA_DLL));
		}
		else if (status.HasFlag(ModExtenderStatus.MissingUpdater))
		{
			result.AppendLine(Loca.Mod_ExtenderStatus_MissingUpdater.SafeFormat($"(Missing {DivinityApp.EXTENDER_UPDATER_FILE})", DivinityApp.EXTENDER_UPDATER_FILE));
		}
		else if (status.HasFlag(ModExtenderStatus.MissingRequiredVersion))
		{
			result.AppendLine(Loca.Mod_ExtenderStatus_MissingRequiredVersion);
		}

		if (currentVersion > -1)
		{
			if (status.HasFlag(ModExtenderStatus.MissingUpdater))
			{
				result.AppendLine(Loca.Mod_ExtenderStatus_MissingUpdaterInfo);
			}
			else
			{
				result.AppendLine(Loca.Mod_ExtenderStatus_InstalledVersion.SafeFormat($"Currently installed version is v{currentVersion}", currentVersion));
			}
		}
		else
		{
			result.AppendLine(Loca.Mod_ExtenderStatus_NotFoundInfo);
		}
		return result.ToString();
	}

	private static string GetModDisplayTypeName(ValueTuple<bool, bool, bool, string?> x)
	{
		(bool isLooseMod, bool IsToolkitProject, bool isForceLoaded, string? modType) = x;

		if (IsToolkitProject) return Loca.Mod_Type_ToolkitProject;
		if (isLooseMod) return Loca.Mod_Type_LooseMod;
		if (isForceLoaded) return Loca.Mod_Type_Override;
		return modType != "Adventure" ? Loca.Mod_Type_Addon : Loca.Mod_Type_Adventure;
	}

	private static string GetModDisplayTypeForeground(ValueTuple<bool, bool, bool, string?> x)
	{
		(bool isLooseMod, bool IsToolkitProject, bool isForceLoaded, string? modType) = x;

		if (IsToolkitProject) return "Lime";
		if (isLooseMod) return "Yellow";
		if (isForceLoaded) return "Orange";
		return string.Empty;
	}

	private static IFileSystemService _fs => Locator.Current.GetService<IFileSystemService>()!;

	public ModEntry(ModData mod)
	{
		UUID = mod.UUID;
		Data = mod;
		IsActive = Data.IsActive;

		CanDrag = true;

		_isVisibleHelper = this.WhenAnyValue(x => x.IsHidden, b => !b).ToUIProperty(this, x => x.IsVisible, true);

		mod.WhenAnyValue(x => x.Index).BindTo(this, x => x.Index);

		_displayNameHelper = mod.WhenAnyValue(x => x.DisplayName).ToUIProperty(this, x => x.DisplayName);
		_descriptionHelper = mod.WhenAnyValue(x => x.Description).ToUIProperty(this, x => x.Description);
		_fileNameHelper = mod.WhenAnyValue(x => x.FilePath, _fs.Path.GetFileName).ToUIProperty(this, x => x.FileName);
		_filePathHelper = mod.WhenAnyValue(x => x.FilePath).ToUIProperty(this, x => x.FilePath);
		_folderHelper = mod.WhenAnyValue(x => x.Folder).ToUIProperty(this, x => x.Folder);
		_versionHelper = mod.WhenAnyValue(x => x.Version.Version).ToUIProperty(this, x => x.Version);
		_authorHelper = mod.WhenAnyValue(x => x.Author).ToUIProperty(this, x => x.Author);

		_lastUpdatedHelper = mod.WhenAnyValue(x => x.LastModified, DateToString).ToUIProperty(this, x => x.LastUpdated, string.Empty);

		_hasDescriptionHelper = mod.WhenAnyValue(x => x.Description).Select(Validators.IsValid).ToUIProperty(this, x => x.HasDescription);
		_hasAuthorHelper = mod.WhenAnyValue(x => x.AuthorDisplayName).Select(Validators.IsValid).ToUIPropertyImmediate(this, x => x.HasAuthor);

		_hasGitHubLinkHelper = mod.WhenAnyValue(x => x.GitHubEnabled, x => x.GitHubData.IsEnabled, (b1, b2) => b1 && b2)
	.ToUIProperty(this, x => x.HasGitHubLink);

		_hasNexusModsLinkHelper = mod.WhenAnyValue(x => x.NexusModsEnabled, x => x.NexusModsData.ModId, (b, id) => b && id >= DivinityApp.NEXUSMODS_MOD_ID_START)
			.ToUIProperty(this, x => x.HasNexusModsLink);

		_hasModioLinkHelper = mod.WhenAnyValue(x => x.ModioEnabled, x => x.ModioData.ExternalLink, CanOpenModioBoolCheck)
			.ToUIProperty(this, x => x.HasModioLink);

		_hasAnyExternalLinkHelper = this.WhenAnyValue(x => x.HasGitHubLink, x => x.HasNexusModsLink, x => x.HasModioLink)
			.Select(x => x.Item1 || x.Item2 || x.Item3)
			.ToUIProperty(this, x => x.HasAnyExternalLink);

		var dependenciesChanged = mod.Dependencies.CountChanged;
		_totalDependenciesHelper = dependenciesChanged.ToUIProperty(this, x => x.TotalDependencies);
		mod.Dependencies.Connect()
			.ObserveOn(RxApp.MainThreadScheduler)
			.SortAndBind(out _displayedDependencies, Sorters.ModuleShortDesc)
			.Subscribe();

		mod.Conflicts.Connect()
			.ObserveOn(RxApp.MainThreadScheduler)
			.SortAndBind(out _displayedConflicts, Sorters.ModuleShortDesc)
			.Subscribe();

		var conflictsChanged = mod.Conflicts.CountChanged;
		_totalConflictsHelper = conflictsChanged.ToUIProperty(this, x => x.TotalConflicts);

		_hasDependenciesHelper = dependenciesChanged.Select(x => x > 0).ToUIPropertyImmediate(this, x => x.HasDependencies);
		_hasConflictsHelper = conflictsChanged.Select(x => x > 0).ToUIPropertyImmediate(this, x => x.HasConflicts);

		var missingDepConn = mod.MissingDependencies.Connect().ObserveOn(RxApp.MainThreadScheduler);
		_hasMissingDependencyHelper = missingDepConn.Count().Select(x => x > 0).ToUIPropertyImmediate(this, x => x.HasMissingDependency);
		_missingDependencyToolTipHelper = missingDepConn.Select(x => BuildMissingDependencyToolTip()).ToUIProperty(this, x => x.MissingDependencyToolTip, string.Empty);

		_hasInvalidUUIDHelper = mod.WhenAnyValue(x => x.UUID, x => x.CanAddToLoadOrder)
			.Select(CheckForInvalidUUID)
			.ToUIPropertyImmediate(this, x => x.HasInvalidUUID);

		var whenDisplayExtraIcons = mod.WhenAnyValue(x => x.DisplayExtraIcons);

		_hasToolkitIconHelper = mod.WhenAnyValue(x => x.IsToolkitProject).CombineLatest(whenDisplayExtraIcons)
			.AllTrue()
			.ToUIPropertyImmediate(this, x => x.HasToolkitIcon);

		_hasLooseModIconHelper = mod.WhenAnyValue(x => x.IsLooseMod, x => x.IsToolkitProject, (b1, b2) => b1 && !b2).CombineLatest(whenDisplayExtraIcons)
			.AllTrue()
			.ToUIPropertyImmediate(this, x => x.HasLooseModIcon);

		_hasOverrideIconHelper = mod.WhenAnyValue(x => x.IsForceLoaded).CombineLatest(whenDisplayExtraIcons)
			.AllTrue()
			.ToUIPropertyImmediate(this, x => x.HasOverrideIcon);


		_extenderIconHelper = mod.WhenAnyValue(x => x.ExtenderModStatus, ExtenderModStatusToIcon).ToUIProperty(this, x => x.ExtenderIcon);

		_canDeleteHelper = mod.WhenAnyValue(x => x.CanDelete).ToUIProperty(this, x => x.CanDelete);

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

		mod.WhenAnyValue(x => x.IsActive, x => x.IsForceLoaded, x => x.IsForceLoadedMergedMod, x => x.ForceAllowInLoadOrder).Subscribe((b) =>
		{
			var isActive = b.Item1;
			var isForceLoaded = b.Item2;
			var isForceLoadedMergedMod = b.Item3;
			var forceAllowInLoadOrder = b.Item4;

			if (forceAllowInLoadOrder || isActive)
			{
				CanDrag = true;
			}
			else
			{
				CanDrag = !isForceLoaded || isForceLoadedMergedMod;
			}
		});

		this.WhenAnyValue(x => x.ListColor, x => x.SelectedColor)
			.Select(x => x.Item1.IsValid() || x.Item2.IsValid())
			.BindTo(this, x => x.HasColorOverride);

		_nexusImageUrlHelper = mod.WhenAnyValue(x => x.NexusModsData.PictureUrl).ToUIProperty(this, x => x.NexusImageUrl);
		_nexusImageVisibilityHelper = this.WhenAnyValue(x => x.NexusImageUrl, Validators.IsValid).ToUIProperty(this, x => x.NexusImageVisibility);
		_nexusModsInformationVisibilityHelper = mod.WhenAnyValue(x => x.NexusModsData.IsUpdated).ToUIProperty(this, x => x.NexusModsInformationVisibility);

		_nexusModsCreatedDateHelper = mod.WhenAnyValue(x => x.NexusModsData.CreatedTimestamp).SkipWhile(x => x <= 0).Select(DateUtils.UnixTimeStampToDateTime).ToUIProperty(this, x => x.NexusModsCreatedDate);
		_nexusModsUpdatedDateHelper = mod.WhenAnyValue(x => x.NexusModsData.UpdatedTimestamp).SkipWhile(x => x <= 0).Select(DateUtils.UnixTimeStampToDateTime).ToUIProperty(this, x => x.NexusModsUpdatedDate);

		_nexusModsTooltipInfoHelper = this.WhenAnyValue(x => x.NexusModsCreatedDate, x => x.NexusModsUpdatedDate, x => x.Data.NexusModsData.EndorsementCount)
			.Select(x => NexusModsInfoToTooltip(x.Item1, x.Item2, x.Item3)).ToUIProperty(this, x => x.NexusModsTooltipInfo);

		mod.WhenAnyValue(x => x.IsForceLoadedMergedMod, x => x.IsLooseMod, x => x.IsForceLoaded, x => x.ForceAllowInLoadOrder, x => x.IsActive)
			.ObserveOn(RxApp.MainThreadScheduler).Subscribe(UpdateColors);

		_hasToolTipHelper = this.WhenAnyValue(x => x.Data.Description, x => x.HasDependencies, x => x.UUID).
			Select(x => x.Item1.IsValid() || x.Item2 || x.Item3.IsValid())
			.ToUIProperty(this, x => x.HasToolTip, true);

		var whenModDisplayType = mod.WhenAnyValue(x => x.IsLooseMod, x => x.IsToolkitProject, x => x.IsForceLoaded, x => x.ModType);
		_modDisplayTypeNameHelper = whenModDisplayType.Select(GetModDisplayTypeName)
			.ToUIProperty(this, x => x.ModDisplayTypeName, Loca.Mod_Type_Addon);

		_modDisplayTypeForegroundHelper = whenModDisplayType.Select(GetModDisplayTypeForeground)
			.ToUIProperty(this, x => x.ModDisplayTypeForeground, string.Empty);

		_canDeleteHelper = mod.WhenAnyValue(x => x.IsLooseMod, x => x.IsHidden, x => x.FilePath,
			(isEditorMod, isHidden, path) => !isEditorMod && !isHidden && path.IsValid())
			.ToUIPropertyImmediate(this, x => x.CanDelete);

		_scriptExtenderSupportToolTipTextHelper = mod.WhenAnyValue(x => x.ExtenderModStatus, x => x.CurrentExtenderVersion,
			x => x.ScriptExtenderData, x => x.ScriptExtenderData!.RequiredVersion,
			(status, curVersion, seData, _) => ExtenderStatusToToolTipText(status, seData?.RequiredVersion ?? 0, curVersion))
			.ToUIProperty(this, x => x.ScriptExtenderSupportToolTipText);

		_hasExtenderStatusHelper = mod.WhenAnyValue(x => x.ExtenderModStatus).Select(x => x != ModExtenderStatus.None)
			.ToUIPropertyImmediate(this, x => x.HasExtenderStatus);

		var whenOsirisStatusChanges = mod.WhenAnyValue(x => x.OsirisModStatus);
		_hasOsirisStatusHelper = whenOsirisStatusChanges.Select(x => x != DivinityOsirisModStatus.NONE).ToUIProperty(this, x => x.HasOsirisStatus);
		_osirisStatusIconPathHelper = whenOsirisStatusChanges.Select(OsirisStatusToIconPath).ToUIProperty(this, x => x.OsirisStatusIconPath);
		_osirisStatusToolTipTextHelper = whenOsirisStatusChanges.Select(OsirisStatusToTooltipText).ToUIProperty(this, x => x.OsirisStatusToolTipText);

		_notesHelper = mod.WhenAnyValue(x => x.ModManagerConfig, x => x.ModManagerConfig!.Notes).Select(x => x.Item2 ?? string.Empty).ToUIProperty(this, x => x.Notes, "");
		_hasNotesHelper = this.WhenAnyValue(x => x.Notes).Select(Validators.IsValid).ToUIProperty(this, x => x.HasNotes);

		_lastModifiedDateTextHelper = mod.WhenAnyValue(x => x.LastModified).SkipWhile(x => !x.HasValue)
			.Select(x => x!.Value.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture))
			.ToUIProperty(this, x => x.LastModifiedDateText, "");

		_hasFilePathHelper = mod.WhenAnyValue(x => x.FilePath).Select(Validators.IsValid).ToUIProperty(this, x => x.HasFilePath);
		_displayVersionHelper = mod.WhenAnyValue(x => x.Version.Version).ToUIProperty(this, x => x.DisplayVersion, "0.0.0.0");

		_toggleModNameLabelHelper = mod.WhenAnyValue(x => x.DisplayFileForName).Select(b => b ? Loca.Mod_Command_DisplayFileForName_Disable : Loca.Mod_Command_DisplayFileForName_Enable).ToUIProperty(this, x => x.ToggleModNameLabel);

		_forceAllowInLoadOrderLabelHelper = mod.WhenAnyValue(x => x.ForceAllowInLoadOrder).Select(b => b ? Loca.Mod_Command_ForceAllowInLoadOrder_Disable : Loca.Mod_Command_ForceAllowInLoadOrder_Enable).ToUIProperty(this, x => x.ForceAllowInLoadOrderLabel);
	}
}
