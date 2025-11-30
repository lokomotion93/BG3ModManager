using NexusModsNET;

namespace ModManager.Models.NexusMods;

public partial class NexusModsObservableApiLimits : ReactiveObject, INexusApiLimits
{
	[Reactive] public partial int HourlyLimit { get; set; }
	[Reactive] public partial int HourlyRemaining { get; set; }
	[Reactive] public partial DateTime HourlyReset { get; set; }
	[Reactive] public partial int DailyLimit { get; set; }
	[Reactive] public partial int DailyRemaining { get; set; }
	[Reactive] public partial DateTime DailyReset { get; set; }

	public void Reset()
	{
		HourlyLimit = 100;
		HourlyRemaining = 100;
		DailyLimit = 2500;
		DailyRemaining = 2500;
		HourlyReset = DateTime.MaxValue;
		DailyReset = DateTime.MaxValue;
	}

	private string FallbackFormat() => $"NexusMods API Limit [Hourly ({HourlyRemaining}/{HourlyLimit}) Daily ({DailyRemaining}/{DailyLimit})]";
	public override string ToString() => Loca.Footer_NexusModsAPILimit_Text.SafeFormat(FallbackFormat(), HourlyRemaining, HourlyLimit, DailyRemaining, DailyLimit);

	public NexusModsObservableApiLimits()
	{
		Reset();
	}
}
