namespace ModManager.Models.Conflicts;

public partial class DivinityConflictGroup : ReactiveObject
{
	[Reactive] public partial string? Header { get; set; }

	[Reactive] public partial int TotalConflicts { get; set; }

	public List<DivinityConflictEntryData> Conflicts { get; set; } = [];

	[Reactive] public partial int SelectedConflictIndex { get; set; }

	public void OnActivated(CompositeDisposable disposables)
	{
		this.WhenAnyValue(x => x.Conflicts.Count).Subscribe(c => TotalConflicts = c).DisposeWith(disposables);
	}
}
