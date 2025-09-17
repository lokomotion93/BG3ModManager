using DynamicData.Binding;

using ModManager.Json;

using System.Reflection;

namespace ModManager.Models.Mod;

[DataContract]
public class ModContainerSettings(string id) : ReactiveObject, IObjectWithId
{
	public string Id { get; set; } = id;
	[Reactive, DataMember] public string? DisplayName { get; set; }
	[Reactive, DataMember] public string? Description { get; set; }
	[Reactive, DataMember] public string? BorderColor { get; set; }
	[Reactive, DataMember] public string? ForegroundColor { get; set; }
	[Reactive, DataMember] public string? BackgroundColor { get; set; }

	[JsonConstructor]
	public ModContainerSettings() : this(string.Empty)
	{

	}
}
