using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ModManager.Services;

public partial class GameUtilitiesService : ReactiveObject, IGameUtilitiesService
{
	[Reactive] public partial bool GameIsRunning { get; private set; }
	[Reactive] public partial TimeSpan ProcessCheckInterval { get; set; }
	[ObservableAsProperty] public partial bool IsActive { get; }

	private IDisposable? _backgroundCheckTask;

	private readonly HashSet<string> _processNames = [];

	public void CheckForGameProcess()
	{
		foreach (var process in Process.GetProcesses())
		{
			if (_processNames.Contains(process.ProcessName))
			{
				GameIsRunning = true;
				return;
			}
		}
		GameIsRunning = false;
	}

	public void AddGameProcessName(string name) => _processNames.Add(name);
	public void AddGameProcessName(IEnumerable<string> names)
	{
		foreach (var x in names)
		{
			_processNames.Add(x);
		}
	}

	public void RemoveGameProcessName(string name) => _processNames.Remove(name);
	public void RemoveGameProcessName(IEnumerable<string> names)
	{
		foreach (var x in names)
		{
			_processNames.Remove(x);
		}
	}

	public GameUtilitiesService()
	{
		var whenInterval = this.WhenAnyValue(x => x.ProcessCheckInterval);
		_isActiveHelper = whenInterval.Select(x => x.Ticks > 0).ToProperty(this, x => x.IsActive);
		whenInterval.Subscribe(interval =>
		{
			_backgroundCheckTask?.Dispose();
			if (interval.Ticks > 0)
			{
				//Run once to update the bool
				CheckForGameProcess();
				_backgroundCheckTask = RxApp.TaskpoolScheduler.SchedulePeriodic(interval, () => CheckForGameProcess());
			}
		});

		ProcessCheckInterval = TimeSpan.FromSeconds(10);
	}
}