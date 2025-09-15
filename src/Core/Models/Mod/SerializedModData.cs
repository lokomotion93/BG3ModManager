using ModManager.Models.Mod.Game;
using ModManager.Services;

using System.Runtime.Serialization;

namespace ModManager.Models.Mod;

[DataContract]
public class SerializedModData(string uuid) : IModuleShortDesc
{
	[DataMember] public string UUID { get; set; } = uuid;
	[DataMember] public int Index { get; set; }
	[DataMember] public string? FileName { get; set; }
	[DataMember] public string? Folder { get; set; }
	[DataMember] public string? Name { get; set; }
	[DataMember] public string? Description { get; set; }
	[DataMember] public string? Author { get; set; }
	[DataMember] public ulong PublishHandle { get; set; }

	[DataMember] public LarianVersion Version { get; set; } = new LarianVersion();

	[DataMember] public string? Type { get; set; }

	[DataMember] public ModScriptExtenderConfig? ScriptExtenderData { get; set; }
	[DataMember] public List<ModuleShortDesc> Dependencies { get; set; } = [];

	[DataMember] public string? MD5 { get; set; }
	[DataMember] public Version? ModManagerVersion { get; set; }

	public DateTimeOffset? LastModified { get; set; }

	[JsonConstructor]
	public SerializedModData() : this(string.Empty)
	{
		
	}

	public static SerializedModData FromMod(ModData mod)
	{
		var version = Locator.Current.GetService<IEnvironmentService>()?.AppVersion;
		var result = new SerializedModData(mod.UUID)
		{
			Author = mod.AuthorDisplayName,
			Description = mod.Description,
			FileName = mod.FileName,
			Folder = mod.Folder,
			Name = mod.Name,
			Version = mod.Version,
			Type = mod.ModType,
			Index = mod.Index,
			ScriptExtenderData = mod.ScriptExtenderData,
			UUID = mod.UUID,
			MD5 = mod.MD5,
			LastModified = mod.LastModified,
			ModManagerVersion = version
		};
		result.Dependencies.AddRange(mod.Dependencies.Items);
		return result;
	}
}
