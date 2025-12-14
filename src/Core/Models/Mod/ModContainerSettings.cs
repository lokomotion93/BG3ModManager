using ModManager.Json;

using System.ComponentModel;

namespace ModManager.Models.Mod;

[DataContract]
public partial class ModContainerSettings(string id) : ReactiveObject, IObjectWithId
{
	public string Id { get; set; } = id;
	[Reactive] public partial string? DisplayName { get; set; }

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


	/// <summary>
	/// Loads settings from another container with the given Id. These settings are applied before the settings from this container.
	/// </summary>
	[Reactive]
	[DataMember, DefaultValue(null)]
	public partial string? ParentSettingsId { get; set; }

	[JsonConstructor]
	public ModContainerSettings() : this(string.Empty)
	{
		if(BorderThickness == "0")
		{
			BorderThickness = null;
		}
		if(Icon != null && Icon.IsDefault())
		{
			Icon = null;
		}
	}
}
