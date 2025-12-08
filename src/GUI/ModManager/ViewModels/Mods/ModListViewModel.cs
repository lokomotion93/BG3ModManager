using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;

using DynamicData;
using DynamicData.Binding;

using ModManager.Models;
using ModManager.Models.Mod;
using ModManager.Util;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ModManager.ViewModels.Mods;
public partial class ModListViewModel : ReactiveObject
{
	private readonly ObservableCollectionExtended<IModEntry> _mods;
	private readonly ModEntryTreeDataGridRowSelectionModel _rowSelection;

	public HierarchicalTreeDataGridSource<IModEntry> Mods { get; }

	[Reactive] public partial string Title { get; set; }
	[Reactive] public partial string? FilterInputText { get; set; }
	[Reactive] public partial bool IsFilterEnabled { get; set; }
	[Reactive] public partial int TotalMods { get; private set; }
	[Reactive] public partial int TotalModsHidden { get; private set; }
	[Reactive] public partial int TotalModsSelected { get; private set; }

	[Reactive] public partial bool IsLocked { get; set; }
	[Reactive] public partial bool IsFocused { get; set; }
	[Reactive] public partial bool IsKeyboardFocusWithin { get; set; }
	[Reactive] public partial ModListType ListType { get; set; }

	[Reactive] public partial IModEntry? SelectedItem { get; set; }

	public List<int> VisibleRows { get; } = [];

	[ObservableAsProperty] public partial string? FilterPlaceholderText { get; }
	[ObservableAsProperty] public partial string? FilterResultText { get; }
	[ObservableAsProperty] public partial bool HasAnyFocus { get; }

	public RxCommandUnit FocusCommand { get; }
	public RxCommandUnit AddContainerCommand { get; }
	public ReactiveCommand<ModContainer, Unit> DeleteContainerCommand { get; }
	public ReactiveCommand<ModContainer, Unit> DeleteContainerModsCommand { get; }
	public ReactiveCommand<ModContainer, Unit> RenameContainerCommand { get; }

	[ReactiveCommand]
	public void EditContainer(ModContainer container)
	{
		AppServices.Interactions.OpenModContainerSettings.Handle(container).Subscribe();
	}

	private static string ToFilterResultText(ValueTuple<int, int, int, string?, bool> x)
	{
		var (total, totalHidden, totalSelected, filterText, isEnabled) = x;
		if (total <= 0 || !isEnabled) return string.Empty;

		List<string> texts = [];
		if (!string.IsNullOrWhiteSpace(filterText))
		{
			var matched = Math.Max(0, total - totalHidden);
			texts.Add($"{matched} Matched");
			if(totalHidden > 0) texts.Add($"{totalHidden} Hidden");
		}
		if(totalSelected > 0) texts.Add($"{totalSelected} Selected");
		
		return string.Join(", ", texts);
	}

	private void CountMods(Unit _)
	{
		var total = 0;
		var totalHidden = 0;
		var totalSelected = 0;
		foreach (var mod in _mods)
		{
			total++;
			if (mod.IsHidden) totalHidden++;
			if (mod.IsSelected) totalSelected++;
		}
		TotalMods = total;
		TotalModsHidden = totalHidden;
		TotalModsSelected = totalSelected;
	}

	public void UpdateIndexes()
	{
		var index = 0;
		foreach(var entry in Mods.Items)
		{
			entry.Index = index;
			entry.IsActive = ListType == ModListType.Active;
			index++;
			if(entry.EntryType == ModEntryType.Container && entry is ModContainer container)
			{
				foreach(var child in container.ForEachNested())
				{
					child.Index = index;
					child.IsActive = ListType == ModListType.Active;
					index++;
				}
			}
		}
	}

	private void UpdateSelection(TreeSelectionModelSelectionChangedEventArgs<IModEntry> e)
	{
		if(e.SelectedItems.Count > 0)
		{
			SelectedItem = e.SelectedItems[0];
		}
		else
		{
			SelectedItem = null;
		}

		foreach (var item in e.SelectedItems)
		{
			item?.IsSelected = true;
		}

		foreach (var item in e.DeselectedItems)
		{
			item?.IsSelected = false;
		}
	}

	[ReactiveCommand]
	public void UpdateSelections(HashSet<int> selectedIndexes)
	{
		foreach(var entry in _mods)
		{
			entry.IsSelected = selectedIndexes.Contains(entry.Index);
		}
	}

	private void OnSorted(TreeDataGridSortedEventArgs e)
	{
		if (e.Column.Header is string columnName && columnName == "Index" && e.Direction == System.ComponentModel.ListSortDirection.Ascending)
		{
			Mods.Sort(null);
		}
	}

	public ModListViewModel(HierarchicalTreeDataGridSource<IModEntry> treeGridSource,
		ObservableCollectionExtended<IModEntry> backingCollection,
		IObservable<IChangeSet<IModEntry>> connection,
		string title = "")
	{
		_mods = backingCollection;
		Mods = treeGridSource;
		Title = title;

		_rowSelection = new ModEntryTreeDataGridRowSelectionModel(treeGridSource) { SingleSelect = false };
		treeGridSource.Selection = _rowSelection;

		Observable.FromEvent<EventHandler<TreeDataGridSortedEventArgs>?, TreeDataGridSortedEventArgs>(
			h => (sender, e) => h(e),
			h => treeGridSource.Sorted += h,
			h => treeGridSource.Sorted -= h
		).ObserveOn(RxApp.MainThreadScheduler)
		.Subscribe(OnSorted);

		Observable.FromEvent<EventHandler<TreeSelectionModelSelectionChangedEventArgs<IModEntry>>?, TreeSelectionModelSelectionChangedEventArgs<IModEntry>>(
			h => (sender, e) => h(e),
			h => _rowSelection.SelectionChanged += h,
			h => _rowSelection.SelectionChanged -= h
		).ObserveOn(RxApp.MainThreadScheduler)
		.Subscribe(UpdateSelection);

		var recountMods = connection.WhenAnyPropertyChanged(nameof(IModEntry.IsVisible));
		Observable.FromEvent<NotifyCollectionChangedEventHandler?, NotifyCollectionChangedEventArgs>(
			h => (sender, e) => h(e),
			h => backingCollection.CollectionChanged += h,
			h => backingCollection.CollectionChanged -= h
		)
		.CombineLatest(recountMods)
		.Throttle(TimeSpan.FromMilliseconds(50))
		.ObserveOn(RxApp.MainThreadScheduler)
		.Select(_ => Unit.Default)
		.Subscribe(CountMods);

		backingCollection.WhenAnyPropertyChanged(nameof(IModEntry.IsVisible));

		_filterResultTextHelper = this.WhenAnyValue(x => x.TotalMods, x => x.TotalModsHidden, x => x.TotalModsSelected,
			x => x.FilterInputText, x => x.IsFilterEnabled)
			.Select(ToFilterResultText)
			.ToUIProperty(this, x => x.FilterResultText);

		//Disable/enable filtering depending on the expander
		this.WhenAnyValue(x => x.IsFilterEnabled, x => x.FilterInputText)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Select(x => x.Item1 ? x.Item2 : null)
			.Subscribe(searchText =>
			{
				ModUtils.FilterMods(searchText, _mods);
			});

		_filterPlaceholderTextHelper = this.WhenAnyValue(x => x.Title).Select(x => $"Filter {x}").ToUIProperty(this, x => x.FilterPlaceholderText);

		_hasAnyFocusHelper = this.WhenAnyValue(x => x.IsFocused, x => x.IsKeyboardFocusWithin).Select(x => x.Item1 || x.Item2).ToUIPropertyImmediate(this, x => x.HasAnyFocus);

		FocusCommand = ReactiveCommand.Create(() => { });

		AddContainerCommand = ReactiveCommand.CreateFromTask(async () => {
			var insertAt = treeGridSource.RowSelection?.AnchorIndex;
			var result = await AppServices.Interactions.ShowMessageBox.Handle(new("Add Container", "Enter container name...", InteractionMessageBoxType.Input, "Container1"));
			if(result.Result)
			{
				var container = new ModContainer(Guid.NewGuid().ToString())
				{
					IsActive = ListType == ModListType.Active,
					EnableAutosaving = true
				};
				container.Settings.DisplayName = result.Input ?? string.Empty;
				AppServices.Settings.ContainerSettings.Containers.AddOrUpdate(container.Settings);
				if(insertAt != null)
				{
					var indexPath = insertAt.Value;
					var index = indexPath[0];
					_mods.Insert(index, container);
				}
				else
				{
					if(VisibleRows.Count > 0)
					{
						if(VisibleRows.Count > 2)
						{
							var middleRow = VisibleRows[VisibleRows.Count / 2];
							_mods.Insert(middleRow, container);
						}
						else
						{
							_mods.Insert(VisibleRows[0], container);
						}
					}
					else
					{
						_mods.Insert(0, container);
					}
				}
			}
		});

		RenameContainerCommand = ReactiveCommand.CreateFromTask<ModContainer>(async modContainer => {
			var result = await AppServices.Interactions.ShowMessageBox.Handle(new("Rename Container", "Enter container name...", InteractionMessageBoxType.Input, modContainer.DisplayName));
			if (result.Result)
			{
				modContainer.Settings.DisplayName = result.Input ?? string.Empty;
			}
		});

		DeleteContainerCommand = ReactiveCommand.CreateFromTask<ModContainer>(async modContainer =>
		{
			await AppServices.Interactions.DeleteMods.Handle(new([modContainer]));
		});

		DeleteContainerModsCommand = ReactiveCommand.CreateFromTask<ModContainer>(async modContainer =>
		{
			var nestedChildMods = new List<IModEntry>();
			foreach (var entry in modContainer.ForEachNested())
			{
				if(entry.EntryType == ModEntryType.Mod)
				{
					nestedChildMods.Add(entry);
				}
			}
			if(nestedChildMods.Count > 0)
			{
				await AppServices.Interactions.DeleteMods.Handle(new(nestedChildMods));
			}
		});
	}
}

public class DesignModListViewModel : ModListViewModel
{
	private static class DesignModListViewModelDataSource
	{
		public static ObservableCollectionExtended<IModEntry> Mods { get; }
		public static HierarchicalTreeDataGridSource<IModEntry> DataSource { get; }
		public static IObservable<IChangeSet<IModEntry>> ModsConnection { get; }

		static DesignModListViewModelDataSource()
		{
			Mods = [];
			Mods.AddRange(ModelGlobals.TestMods);

			ModsConnection = Mods.ToObservableChangeSet().ObserveOn(RxApp.MainThreadScheduler);

			DataSource = new HierarchicalTreeDataGridSource<IModEntry>(Mods)
			{
				Columns =
				{
					//Avalonia.Controls.Models.TreeDataGrid.
					new TextColumn<IModEntry, int>("Index", x => x.Index, GridLength.Auto),
					new HierarchicalExpanderColumn<IModEntry>(
					new TextColumn<IModEntry, string>("Name", x => x.DisplayName, GridLength.Star),
					x => x.Children, x => x.Children != null && x.Children.Count > 0, x => x.IsExpanded),
					new TextColumn<IModEntry, string>("Version", x => x.Version, GridLength.Auto),
					new TextColumn<IModEntry, string>("Author", x => x.Author, GridLength.Auto),
					new TextColumn<IModEntry, string>("Last Updated", x => x.LastUpdated, GridLength.Auto),
				}
			};
		}
	}

	public DesignModListViewModel() : base(DesignModListViewModelDataSource.DataSource,
		DesignModListViewModelDataSource.Mods,
		DesignModListViewModelDataSource.ModsConnection, "Active")
	{

	}

	public DesignModListViewModel(HierarchicalTreeDataGridSource<IModEntry> treeGridSource,
		ObservableCollectionExtended<IModEntry> collection,
		IObservable<IChangeSet<IModEntry>> connection,
		string title = "") : base(treeGridSource, collection, connection, title)
	{

	}
}