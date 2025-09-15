using DynamicData.Binding;

using ModManager.Json;

using System.Reflection;

namespace ModManager.Models.Mod.Container;

[DataContract]
public class ModContainerSettings : ReactiveObject, IObjectWithId
{
	public string Id { get; set; }
	[Reactive, DataMember] public string? DisplayName { get; set; }
	[Reactive, DataMember] public string? Description { get; set; }
	[Reactive, DataMember] public string? BorderColor { get; set; }
	[Reactive, DataMember] public string? ForegroundColor { get; set; }
	[Reactive, DataMember] public string? BackgroundColor { get; set; }

	public ModContainerSettings(string id)
	{
		Id = id;
	}

	[JsonConstructor]
	public ModContainerSettings() : this(string.Empty)
	{

	}
}
