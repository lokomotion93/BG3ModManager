using DynamicData;

using ModManager.Models.GitHub;
using ModManager.Models.Mod.Game;
using ModManager.Models.Modio;
using ModManager.Models.NexusMods;
using ModManager.Models.Settings;
using ModManager.Services;
using ModManager.Util;

namespace ModManager.Models.Mod;

[DataContract]
[ScreenReaderHelper(Name = "DisplayName", HelpText = "HelpText")]
public partial class ModData : ReactiveObject, IModuleShortDesc
{
	#region meta.lsx Properties
	[Reactive]
	[DataMember]
	public partial string UUID { get; set; }

	[Reactive]
	[DataMember]
	public partial string? Folder { get; set; }

	[Reactive]
	[DataMember]
	public partial string? Name { get; set; }

	[Reactive]
	[DataMember]
	public partial string? Description { get; set; }

	[Reactive]
	[DataMember]
	public partial string? Author { get; set; }

	[Reactive]
	[DataMember]
	public partial string? ModType { get; set; }

	[Reactive]
	[DataMember]
	public partial string? MD5 { get; set; }

	[Reactive]
	[DataMember]
	public partial LarianVersion Version { get; set; }

	[Reactive]
	[DataMember]
	public partial ulong PublishHandle { get; set; }

	[Reactive]
	[DataMember]
	public partial ulong FileSize { get; set; }


	[Reactive]
	[DataMember]
	public partial LarianVersion HeaderVersion { get; set; }

	[Reactive]
	[DataMember]
	public partial LarianVersion PublishVersion { get; set; }

	public List<string> Tags { get; set; } = [];

	public SourceCache<ModuleShortDesc, string> Dependencies { get; }
	public SourceCache<ModuleShortDesc, string> MissingDependencies { get; }
	public SourceCache<ModuleShortDesc, string> Conflicts { get; }

	#endregion

	[Reactive] public partial string? NameOverride { get; set; }

	[Reactive] public partial DateTimeOffset? LastModified { get; set; }

	[Reactive] public partial bool DisplayFileForName { get; set; }
	[Reactive] public partial bool IsHidden { get; set; }

	/// <summary>True if this mod is in DivinityApp.IgnoredMods, or the author is Larian. Larian mods are hidden from the load order.</summary>
	[Reactive] public partial bool IsLarianMod { get; set; }

	/// <summary>Whether the mod was loaded from the user's mods directory.</summary>
	[Reactive] public partial bool IsUserMod { get; set; }

	/// <summary>
	/// True if the mod has a meta.lsx.
	/// </summary>
	[Reactive] public partial bool HasMetadata { get; set; }

	/// <summary>True if the mod has a base game mod directory. This data is always loaded regardless if the mod is enabled or not.</summary>
	[Reactive] public partial bool HasOverrideFiles { get; set; }

	[Reactive] public partial string? BuiltinOverrideModsText { get; set; }

	[Reactive] public partial bool DisplayExtraIcons { get; set; }

	[Reactive] public partial string? HelpText { get; set; }

	[Reactive] public partial int Index { get; set; }

	[Reactive] public partial ModExtenderStatus ExtenderModStatus { get; set; }
	[Reactive] public partial DivinityOsirisModStatus OsirisModStatus { get; set; }

	[Reactive] public partial int CurrentExtenderVersion { get; set; }

	[Reactive] public partial ModScriptExtenderConfig? ScriptExtenderData { get; set; }

	[Reactive] public partial bool HasScriptExtenderSettings { get; set; }
	[Reactive] public partial bool IsActive { get; set; }
	[Reactive] public partial bool IsSelected { get; set; }

	[Reactive] public partial bool IsLooseMod { get; set; }
	[Reactive] public partial bool IsToolkitProject { get; set; }
	[Reactive] public partial ToolkitProjectMetaData? ToolkitProjectMeta { get; set; }

	public string? OutputPakName
	{
		get
		{
			if (UUID.IsValid() && Folder?.Contains(UUID) == false)
			{
				return $"{Folder}_{UUID}.pak";
			}
			else if (FilePath.IsValid())
			{
				return $"{FileName}.pak";
			}
			return "";
		}
	}

	[Reactive] public partial string? FilePath { get; set; }
	[Reactive] public partial string? AuthorDisplayName { get; private set; }

	// This is a property instead of an ObservableAsProperty so the name is set immediately
	[Reactive] public partial string? DisplayName { get; private set; }

	[Reactive] public partial bool CanAddToLoadOrder { get; private set; }
	[Reactive] public partial bool CanDelete { get; private set; }

	[Reactive] public partial bool GitHubEnabled { get; set; }
	[Reactive] public partial bool NexusModsEnabled { get; set; }
	[Reactive] public partial bool ModioEnabled { get; set; }

	public HashSet<string> Files { get; set; }

	[Reactive] public partial ModioModData ModioData { get; set; }
	[Reactive] public partial NexusModsModData NexusModsData { get; set; }
	[Reactive] public partial GitHubModData GitHubData { get; set; }

	public bool HasDependencies => Dependencies.Count > 0;
	public bool HasConflicts => Conflicts.Count > 0;
	public string? FileName => _fs.Path.GetFileName(FilePath);

	private static string GetDisplayName(string? name, string? filePath, string? folder, string uuid, bool isLooseMod, bool isToolkitProject, bool displayFileForName, string? nameOverride)
	{
		if (displayFileForName)
		{
			if (!isLooseMod)
			{
				if (filePath.IsValid())
				{
					return _fs.Path.GetFileName(filePath);
				}
			}
			else if(folder.IsValid())
			{
				if(isToolkitProject)
				{
					return Loca.Mod_DisplayName_FileView_Pattern.SafeFormat(folder, folder, Loca.Mod_DisplayName_FileView_Type_Toolkit);
				}
				else
				{
					return Loca.Mod_DisplayName_FileView_Pattern.SafeFormat(folder, folder, Loca.Mod_DisplayName_FileView_Type_Loose);
				}
			}
		}
		else
		{
			if (nameOverride.IsValid()) return nameOverride;
			if (name.IsValid()) return name;
		}
		return "";
	}

	public void AddTag(string tag)
	{
		if (tag.IsValid() && !Tags.Contains(tag))
		{
			Tags.Add(tag);
			Tags.Sort((x, y) => string.Compare(x, y, true));
		}
	}

	public void AddTags(IEnumerable<string> tags)
	{
		if (tags == null)
		{
			return;
		}
		var addedTags = false;
		foreach (var tag in tags)
		{
			if (tag.IsValid() && !Tags.Contains(tag))
			{
				Tags.Add(tag);
				addedTags = true;
			}
		}
		Tags.Sort((x, y) => string.Compare(x, y, true));
		if (addedTags)
		{
			this.RaisePropertyChanged("Tags");
		}
	}

	public bool PakEquals(string fileName, StringComparison comparison = StringComparison.Ordinal)
	{
		var outputPackage = _fs.Path.ChangeExtension(Folder, "pak");
		//Imported Classic Projects
		if (Folder.IsValid() && !Folder.Contains(UUID))
		{
			outputPackage = _fs.Path.ChangeExtension(_fs.Path.Join(Folder + "_" + UUID), "pak");
		}
		return outputPackage?.Equals(fileName, comparison) == true;
	}

	public bool IsNewerThan(DateTimeOffset date)
	{
		if (LastModified.HasValue)
		{
			return LastModified.Value > date;
		}
		return false;
	}

	public bool IsNewerThan(IModuleShortDesc mod)
	{
		if (LastModified.HasValue && mod.LastModified.HasValue)
		{
			return LastModified.Value > mod.LastModified.Value;
		}
		return false;
	}

	public string? GetURL(ModSourceType modSourceType, bool asProtocol = false)
	{
		switch (modSourceType)
		{
			case ModSourceType.MODIO:
				if (ModioData.IsEnabled)
				{
					return ModioData.ExternalLink;
				}
				break;
			case ModSourceType.NEXUSMODS:
				if (NexusModsData.IsEnabled)
				{
					return string.Format(DivinityApp.NEXUSMODS_MOD_URL, NexusModsData.ModId);
				}
				break;
			case ModSourceType.GITHUB:
				if (GitHubData.IsEnabled)
				{
					return $"https://github.com/{GitHubData.Author}/{GitHubData.Repository}";
				}
				break;
		}
		return "";
	}

	public List<string> GetAllURLs(bool asProtocol = false)
	{
		var urls = new List<string>();
		var modioUrl = GetURL(ModSourceType.MODIO, asProtocol);
		if (modioUrl.IsValid())
		{
			urls.Add(modioUrl);
		}
		var nexusUrl = GetURL(ModSourceType.NEXUSMODS, asProtocol);
		if (nexusUrl.IsValid())
		{
			urls.Add(nexusUrl);
		}
		var githubUrl = GetURL(ModSourceType.GITHUB, asProtocol);
		if (githubUrl.IsValid())
		{
			urls.Add(githubUrl);
		}
		return urls;
	}

	public override string ToString()
	{
		return Loca.Mod_StringFormatPattern.SafeFormat($"Name({Name}) Version({Version?.Version}) Author({Author}) UUID({UUID}) File({FilePath})", Name, Version?.Version ?? string.Empty, Author, UUID, FilePath);
	}

	public ModuleShortDesc ToModuleShortDesc()
	{
		return new ModuleShortDesc(UUID)
		{
			Folder = Folder,
			MD5 = MD5,
			Name = Name,
			Version = new LarianVersion(Version?.VersionInt ?? 0)
		};
	}

	private CompositeDisposable? _modConfigDisposables;
	private ModConfig? _modManagerConfig;

	public ModConfig? ModManagerConfig
	{
		get => _modManagerConfig;
		set
		{
			this.RaiseAndSetIfChanged(ref _modManagerConfig, value, nameof(ModManagerConfig));
			if (_modManagerConfig != null)
			{
				if (_modConfigDisposables == null)
				{
					_modConfigDisposables = [];

					this.WhenAnyValue(x => x.UUID).BindTo(ModManagerConfig, x => x.Id).DisposeWith(_modConfigDisposables);

					this.WhenAnyValue(x => x.NexusModsData.ModId).BindTo(this, x => x.ModManagerConfig!.NexusModsId).DisposeWith(_modConfigDisposables);
					this.WhenAnyValue(x => x.ModioData.NameId).BindTo(this, x => x.ModManagerConfig!.ModioId).DisposeWith(_modConfigDisposables);
					this.WhenAnyValue(x => x.GitHubData.Url).BindTo(this, x => x.ModManagerConfig!.GitHub).DisposeWith(_modConfigDisposables);
				}
			}
			else
			{
				_modConfigDisposables?.Dispose();
			}
		}
	}

	public void ApplyModConfig(ModConfig config)
	{
		if (ModManagerConfig != null)
		{
			if (config != ModManagerConfig) ModManagerConfig.SetFrom<ModConfig, ReactiveAttribute>(config);
		}
		else
		{
			ModManagerConfig = config;
		}

		if (config.NexusModsId > DivinityApp.NEXUSMODS_MOD_ID_START) NexusModsData.ModId = config.NexusModsId;
		if (config.ModioId.IsValid()) ModioData.NameId = config.ModioId;
		if (config.GitHub.IsValid()) GitHubData.Url = config.GitHub;
	}

	private static string GetAuthor(ValueTuple<string?, string?, string?, string?, bool> x)
	{
		var metaAuthor = x.Item1;
		var nexusAuthor = x.Item2;
		var githubAuthor = x.Item3;
		var modioAuthor = x.Item4;
		var isLarianMod = x.Item5;

		if (modioAuthor.IsValid()) return modioAuthor;
		if (metaAuthor.IsValid()) return metaAuthor;
		if (nexusAuthor.IsValid()) return nexusAuthor;
		if (githubAuthor.IsValid()) return githubAuthor;

		if (isLarianMod) return Loca.LarianStudios;

		return string.Empty;
	}

	private static bool CanAddToLoadOrderCheck(string? modType, bool isHidden)
	{
		return modType != "Adventure" && !isHidden;
	}

	private static string ModToXml(string? pattern, ModData mod, bool isFixedString = false)
	{
		pattern ??= DivinityApp.XML_MODULE_SHORT_DESC;
		if (!isFixedString)
		{
			return string.Format(pattern ?? DivinityApp.XML_MODULE_SHORT_DESC, mod.Folder, mod.MD5, System.Security.SecurityElement.Escape(mod.Name), mod.UUID, mod.Version.VersionInt, mod.PublishHandle);
		}
		else
		{
			return string.Format(pattern ?? DivinityApp.XML_MODULE_SHORT_DESC, mod.Folder, mod.MD5, System.Security.SecurityElement.Escape(mod.Name), mod.UUID, mod.Version.VersionInt);
		}
	}

	public string? Export(ModExportType exportType, string? pattern = null, bool isFixedString = false)
	{
		var result = exportType switch
		{
			ModExportType.XML => ModToXml(pattern, this, isFixedString),
			ModExportType.JSON => JsonSerializer.Serialize(this, JsonUtils.DefaultSerializerSettings),
			ModExportType.TXT => StringUtils.ModToTextLine(this),
			ModExportType.TSV => StringUtils.ModToTSVLine(this),
			_ => string.Empty,
		};
		return result;
	}

	public bool UpdateModExtenderStatus(ScriptExtenderSettings scriptExtenderSettings, ScriptExtenderUpdateConfig updateConfig)
	{
		CurrentExtenderVersion = scriptExtenderSettings.ExtenderMajorVersion;
		ExtenderModStatus = ModExtenderStatus.None;

		if (ScriptExtenderData != null && ScriptExtenderData.HasAnySettings)
		{
			if (ScriptExtenderData.Lua)
			{
				if (scriptExtenderSettings.ExtenderMajorVersion > -1)
				{
					if (ScriptExtenderData.RequiredVersion > -1 && scriptExtenderSettings.ExtenderMajorVersion < ScriptExtenderData.RequiredVersion)
					{
						ExtenderModStatus |= ModExtenderStatus.MissingRequiredVersion;
					}
					else
					{
						ExtenderModStatus |= ModExtenderStatus.Fulfilled;
					}
				}
				else
				{
					ExtenderModStatus |= ModExtenderStatus.MissingRequiredVersion;
				}
			}
			else
			{
				ExtenderModStatus |= ModExtenderStatus.Supports;
			}
			if (!updateConfig.UpdaterIsAvailable)
			{
				ExtenderModStatus |= ModExtenderStatus.MissingUpdater;
			}
		}

		// Blinky animation on the tools/download buttons if the extender is required by mods and is missing
		if (ExtenderModStatus.HasFlag(ModExtenderStatus.MissingUpdater))
		{
			return true;
		}
		return false;
	}

	private static IFileSystemService _fs => Locator.Current.GetService<IFileSystemService>()!;

	public ModData(string uuid)
	{
		Version = LarianVersion.Empty;
		HeaderVersion = LarianVersion.Empty;
		PublishVersion = LarianVersion.Empty;
		MD5 = "";
		Author = "";
		Folder = "";
		UUID = uuid;
		Name = "";
		PublishHandle = 0ul;
		FileSize = 0ul;

		HelpText = "";
		Index = -1;

		Dependencies = new SourceCache<ModuleShortDesc, string>(x => x.UUID);
		MissingDependencies = new SourceCache<ModuleShortDesc, string>(x => x.UUID);
		Conflicts = new SourceCache<ModuleShortDesc, string>(x => x.UUID);

		Tags = [];
		Files = [];

		ModioData = new ModioModData();
		NexusModsData = new NexusModsModData();
		GitHubData = new GitHubModData();

		this.WhenAnyValue(x => x.Author, x => x.NexusModsData.Author, x => x.GitHubData.Author, x => x.ModioData.Author, x => x.IsLarianMod).Select(GetAuthor).BindTo(this, x => x.AuthorDisplayName);

		this.WhenAnyValue(x => x.Name, x => x.FilePath, x => x.Folder, x => x.UUID, x => x.IsLooseMod, 
			x => x.IsToolkitProject, x => x.DisplayFileForName, x => x.NameOverride, GetDisplayName)
			.ObserveOn(RxApp.MainThreadScheduler)
			.BindTo(this, x => x.DisplayName);

		this.WhenAnyValue(x => x.ModType, x => x.IsHidden, CanAddToLoadOrderCheck).BindTo(this, x => x.CanAddToLoadOrder, true);

		this.WhenAnyValue(x => x.UUID).BindTo(NexusModsData, x => x.UUID);

		this.WhenAnyValue(x => x.ModioData.Description).Subscribe(desc =>
		{
			if(desc.IsValid() && desc.Length > Description?.Length)
			{
				Description = desc;
			}
		});

		this.WhenAnyValue(x => x.ModioData.LastUpdated).Where(x => x != DateTimeOffset.UnixEpoch).Subscribe(lastUpdated =>
		{
			if(lastUpdated > LastModified)
			{
				LastModified = lastUpdated;
			}
		});

		SetIsBaseGameMod(false);
	}

	public void SetIsBaseGameMod(bool isBaseGameMod)
	{
		if (!isBaseGameMod)
		{
			IsHidden = false;
			if (ModManagerConfig == null)
			{
				ModManagerConfig = new ModConfig
				{
					Id = UUID
				};
			}
		}
		else
		{
			IsHidden = true;
			ModManagerConfig = null;
		}
	}

	public static ModData Clone(ModData mod)
	{
		var cloneMod = new ModData(mod.UUID)
		{
			HasMetadata = mod.HasMetadata,
			Name = mod.Name,
			Author = mod.Author,
			Version = new LarianVersion(mod.Version.VersionInt),
			HeaderVersion = new LarianVersion(mod.HeaderVersion.VersionInt),
			PublishVersion = new LarianVersion(mod.PublishVersion.VersionInt),
			Folder = mod.Folder,
			Description = mod.Description,
			MD5 = mod.MD5,
			ModType = mod.ModType,
			Tags = [.. mod.Tags]
		};
		cloneMod.Conflicts.AddOrUpdate(mod.Conflicts.Items);
		cloneMod.Dependencies.AddOrUpdate(mod.Dependencies.Items);
		cloneMod.NexusModsData.Update(mod.NexusModsData);
		cloneMod.ModioData.Update(mod.ModioData.Data);
		cloneMod.GitHubData.Update(mod.GitHubData);
		if (mod.ModManagerConfig != null) cloneMod.ApplyModConfig(mod.ModManagerConfig);
		return cloneMod;
	}
}
