namespace ModManager.Models.Mod;

public partial class ModContainerIconSettings : ReactiveObject
{
	[Reactive, DataMember] public partial string? Path { get; set; }
	[Reactive, DataMember] public partial string? Kind { get; set; }
	[Reactive, DataMember] public partial string? ForegroundColor { get; set; }
	[Reactive, DataMember] public partial string? BackgroundColor { get; set; }
	[Reactive, DataMember] public partial string? BorderColor { get; set; }
	[Reactive, DataMember] public partial string? BorderThickness { get; set; }
	[Reactive, DataMember] public partial string? Size { get; set; }
}
