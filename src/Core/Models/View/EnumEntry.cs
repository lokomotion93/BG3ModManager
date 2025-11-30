namespace ModManager.Models.View;

public partial class EnumEntry : ReactiveObject
{
	[Reactive] public partial string? Description { get; set; }
	[Reactive] public partial object Value { get; set; }

	public EnumEntry(string description, object value)
	{
		Description = description;
		Value = value;
	}
}
