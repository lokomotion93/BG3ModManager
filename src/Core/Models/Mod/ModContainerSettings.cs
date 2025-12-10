using ModManager.Json;

using System.ComponentModel;

namespace ModManager.Models.Mod;

[DataContract]
public partial class ModContainerSettings(string id) : ReactiveObject, IObjectWithId
{
	public string Id { get; set; } = id;
	[Reactive] public partial string? DisplayName { get; set; }
	[Reactive] public partial bool IsGlobal { get; set; }


	[Reactive]
	[DataMember, DefaultValue(null)] 
	public partial string? Description { get; set; }

	[Reactive]
	[DataMember, DefaultValue(null)] 
	public partial string? ForegroundColor { get; set; }

	[Reactive]
	[DataMember, DefaultValue(null)] 
	public partial string? BackgroundColor { get; set; }

	[Reactive]
	[DataMember, DefaultValue(null)] 
	public partial string? BorderColor { get; set; }

	[Reactive]
	[DataMember, DefaultValue(null)] 
	public partial string? BorderThickness { get; set; }

	[Reactive]
	[DataMember, DefaultValue(null)] 
	public partial ModContainerIconSettings? Icon { get; set; }

	[Reactive]
	[DataMember, DefaultValue(false)] 
	public partial bool IsExpanded { get; set; }

	[JsonConstructor]
	public ModContainerSettings() : this(string.Empty)
	{
		
	}
}
