using System.ComponentModel;

namespace ModManager.Models.Mod;

public partial class ModContainerIconSettings : ReactiveObject
{
	[Reactive]
	[property: DataMember, DefaultValue(null)]
	public partial string? Path { get; set; }

	[Reactive]
	[property: DataMember, DefaultValue(null)]
	public partial string? Kind { get; set; }

	[Reactive]
	[property: DataMember, DefaultValue(null)]
	public partial string? ForegroundColor { get; set; }

	[Reactive]
	[property: DataMember, DefaultValue(null)]
	public partial string? BackgroundColor { get; set; }

	[Reactive]
	[property: DataMember, DefaultValue(null)]
	public partial string? BorderColor { get; set; }

	[Reactive]
	[property: DataMember, DefaultValue(null)]
	public partial string? BorderThickness { get; set; }

	[Reactive]
	[property: DataMember, DefaultValue(null)]
	public partial string? Size { get; set; }
}
