using ModManager.Json;

namespace ModManager.Models.Mod;

[DataContract]
public class ModContainerSettings(string id) : ReactiveObject, IObjectWithId
{
	public string Id { get; set; } = id;
	[Reactive] public string? DisplayName { get; set; }
	[Reactive, DataMember] public string? Description { get; set; }
	[Reactive, DataMember] public string? BorderColor { get; set; }
	[Reactive, DataMember] public string? ForegroundColor { get; set; }
	[Reactive, DataMember] public string? BackgroundColor { get; set; }
	[Reactive, DataMember] public string? BorderThickness { get; set; }
	[Reactive, DataMember] public bool IsExpanded { get; set; }

	[JsonConstructor]
	public ModContainerSettings() : this(string.Empty)
	{

	}
}
