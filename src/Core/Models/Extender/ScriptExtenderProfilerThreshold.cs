using ModManager.Json;

namespace ModManager.Models.Extender;

public class ScriptExtenderProfilerThreshold : ReactiveObject
{
	[Reactive] public uint Warn { get; set; }
	[Reactive] public uint Error { get; set; }

	public void Update(uint warn, uint error)
	{
		Warn = warn;
		Error = error;
	}

	public void Update(ValueTuple<uint, uint> items) => Update(items.Item1, items.Item2);

	public IObservable<(uint, uint)> GetChangeObservable(IObservable<bool> skipUntil) => this.WhenAnyValue(x => x.Warn, x => x.Error).SkipUntil(skipUntil);
}