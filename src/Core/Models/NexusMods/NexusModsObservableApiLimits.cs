using NexusModsNET;

namespace ModManager.Models.NexusMods;

public class NexusModsObservableApiLimits : ReactiveObject, INexusApiLimits
{
	[Reactive] public int HourlyLimit { get; set; }
	[Reactive] public int HourlyRemaining { get; set; }
	[Reactive] public DateTime HourlyReset { get; set; }
	[Reactive] public int DailyLimit { get; set; }
	[Reactive] public int DailyRemaining { get; set; }
	[Reactive] public DateTime DailyReset { get; set; }

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
	public override string ToString() => Locale.Resources.Footer_NexusModsAPILimit_Text.SafeFormat(FallbackFormat(), HourlyRemaining, HourlyLimit, DailyRemaining, DailyLimit);

	public NexusModsObservableApiLimits()
	{
		Reset();
	}
}
