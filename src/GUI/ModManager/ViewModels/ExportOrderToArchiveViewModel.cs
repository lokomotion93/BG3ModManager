using DynamicData;
using DynamicData.Binding;

using ModManager.Models.Mod;

using System.Collections.ObjectModel;

namespace ModManager.ViewModels;

public enum ExportOrderFileType
{
	[SettingsEntry("Default JSON", "The default .json load order that the mod manager uses")]
	DefaultJson,
	[SettingsEntry("Detailed JSON", "An order .json that contains more information, such as the author, description, tags, and more")]
	DetailedJson,
	[SettingsEntry("Tab-Separated Spreadsheet", "A .tsv spreadsheet contained detailed information about each mod")]
	TSV
}

public partial class ExportOrderFileEntry : ReactiveObject
{
	[Reactive] public partial bool IsSelected { get; set; }
	[Reactive] public partial bool IsVisible { get; set; }
	[Reactive] public partial ModData? Mod { get; set; }

	public ExportOrderFileEntry()
	{
		IsVisible = true;
		IsSelected = true;
	}
}

public partial class ExportOrderToArchiveViewModel : BaseProgressViewModel
{
	[Reactive] public partial string? OutputPath { get; set; }
	[Reactive] public partial ExportOrderFileType SelectedOrderType { get; set; }

	private readonly ObservableCollectionExtended<ExportOrderFileType> _orderTypes;

	public ObservableCollectionExtended<ExportOrderFileType> OrderTypes => _orderTypes;

	private readonly ObservableCollectionExtended<ExportOrderFileEntry> _entries;

	protected ReadOnlyObservableCollection<ExportOrderFileEntry> _visibleEntries;
	public ReadOnlyObservableCollection<ExportOrderFileEntry> Entries => _visibleEntries;

	[ObservableAsProperty] public partial bool AnySelected { get; }
	[ObservableAsProperty] public partial bool AllSelected { get; }
	[ObservableAsProperty] public partial string? SelectAllToolTip { get; }

	public RxCommandUnit SelectAllCommand { get; private set; }

	public override async Task<bool> Run(CancellationToken cts)
	{
		//Only visible + selected entries
		var exportedMods = Entries.Where(x => x.IsSelected);

		return true;
	}

	public override void Close()
	{
		base.Close();
		_entries.Clear();
	}

	public void ToggleSelectAll()
	{
		var b = !AllSelected;
		foreach (var f in Entries)
		{
			f.IsSelected = b;
		}
	}

	public ExportOrderToArchiveViewModel() : base()
	{
		CanRun = true;

		_orderTypes = new ObservableCollectionExtended<ExportOrderFileType>(Enum.GetValues(typeof(ExportOrderFileType)).Cast<ExportOrderFileType>());

		_entries = [];

		var changeSet = _entries.ToObservableChangeSet();
		changeSet.Filter(x => x.IsVisible).ObserveOn(RxApp.MainThreadScheduler).Bind(out _visibleEntries).Subscribe();

		var filesChanged = changeSet.AutoRefresh(x => x.IsSelected).ToCollection().Throttle(TimeSpan.FromMilliseconds(50)).ObserveOn(RxApp.MainThreadScheduler);
		_anySelectedHelper = filesChanged.Select(x => x.Any(y => y.IsSelected)).ToUIProperty(this, x => x.AnySelected);
		_allSelectedHelper = filesChanged.Select(x => x.All(y => y.IsSelected)).ToUIProperty(this, x => x.AllSelected);
		_selectAllToolTipHelper = this.WhenAnyValue(x => x.AllSelected).Select(b => $"{(b ? "Deselect" : "Select")} All").ToUIProperty(this, x => x.SelectAllToolTip);

		SelectAllCommand = ReactiveCommand.Create(ToggleSelectAll, RunCommand.IsExecuting.Select(b => !b), RxApp.MainThreadScheduler);
	}
}
