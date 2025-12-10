using DynamicData.Binding;

using System.ComponentModel;
using System.Reactive.Subjects;

namespace ModManager.Models.Mod;

[DataContract]
public partial class ModContainerIconSettings : ReactiveObject
{
	[Reactive]
	[DataMember, DefaultValue(null)]
	public partial string? Path { get; set; }

	[Reactive]
	[DataMember, DefaultValue(null)]
	public partial string? Kind { get; set; }

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
	public partial string? Size { get; set; }

	[Reactive]
	[JsonIgnore]
	public partial bool IsDirty { get; set; }

	public ModContainerIconSettings()
	{
		this.WhenAnyPropertyChanged(nameof(Path), nameof(Kind), nameof(ForegroundColor), nameof(BackgroundColor), nameof(BorderThickness), nameof(Size))
			.Throttle(TimeSpan.FromTicks(5))
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(_ =>
			{
				IsDirty = true;
			});
	}
}
