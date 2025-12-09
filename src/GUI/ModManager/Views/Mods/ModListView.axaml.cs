using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;

using DynamicData;

using ModManager.Controls;
using ModManager.Models.Mod;
using ModManager.Services;
using ModManager.Styling;
using ModManager.ViewModels.Mods;

using System.ComponentModel;
using System.Reflection;

namespace ModManager.Views.Mods;
public partial class ModListView : ReactiveUserControl<ModListViewModel>
{
	private bool _isSingleSelect = false;
	private bool _isDragging = false;

	private static IList<IModEntry> GetItems(HierarchicalTreeDataGridSource<IModEntry> from, IndexPath path)
	{
		IEnumerable<IModEntry>? children;

		if (path.Count == 0)
			children = from.Items;
		else if (from.TryGetModelAt(path, out var parent))
			children = ((ITreeDataGridSource)from)!.GetModelChildren(parent)?.Cast<IModEntry>();
		else
			throw new IndexOutOfRangeException();

		if (children is null)
			throw new InvalidOperationException("The requested drop target has no children.");

		return children as IList<IModEntry> ??
			throw new InvalidOperationException("Items does not implement IList<T>.");
	}

	public static void DragDropRows(
			HierarchicalTreeDataGridSource<IModEntry> source,
			HierarchicalTreeDataGridSource<IModEntry> target,
			IEnumerable<IndexPath> indexes,
			IndexPath targetIndex,
			TreeDataGridRowDropPosition position,
			DragDropEffects effects)
	{
		if(effects == DragDropEffects.Move)
		{
			IList<IModEntry> targetItems;
			int ti = 0;

			if (target.Rows.Count == 0)
			{
				targetItems = target.Items as IList<IModEntry> ?? throw new InvalidOperationException("Items does not implement IList<T>.");
			}
			else
			{
				if (position == TreeDataGridRowDropPosition.Inside)
				{
					targetItems = GetItems(target, targetIndex);
					ti = targetItems.Count;
				}
				else
				{
					targetItems = GetItems(target, targetIndex[..^1]);
					ti = targetIndex[^1];
				}
			}

			if (position == TreeDataGridRowDropPosition.After) ++ti;

			var sourceItems = new List<IModEntry>();

			foreach (var g in indexes.GroupBy(x => x[..^1]))
			{
				var items = GetItems(source, g.Key);

				foreach (var i in g.Select(x => x[^1]).OrderByDescending(x => x))
				{
					sourceItems.Add(items[i]);

					if (items == targetItems && i < ti)
						--ti;

					items.RemoveAt(i);
				}
			}

			if (targetItems.Count == 0)
			{
				targetItems.AddRange(sourceItems.Reverse<IModEntry>());
			}
			else
			{
				for (var si = sourceItems.Count - 1; si >= 0; --si)
				{
					targetItems.Insert(ti++, sourceItems[si]);
				}
			}
		}
	}

	public static TreeDataGridRowDropPosition GetDropPosition(bool allowInside, DragEventArgs e, TreeDataGridRow row)
	{
		var rowY = e.GetPosition(row).Y / row.Bounds.Height;

		if (allowInside)
		{
			if (rowY < 0.33)
				return TreeDataGridRowDropPosition.Before;
			else if (rowY > 0.66)
				return TreeDataGridRowDropPosition.After;
			else
				return TreeDataGridRowDropPosition.Inside;
		}
		else
		{
			if (rowY < 0.5)
				return TreeDataGridRowDropPosition.Before;
			else
				return TreeDataGridRowDropPosition.After;
		}
	}

	/// <summary>
	/// Prevents dropping mods into other mods, and instead makes the drop operation move the mod in the list.
	/// Only ModCategory models should allow dropping mods into them.
	/// </summary>
	/// <param name="e"></param>
	private static void MaybeRedirectDrop(TreeDataGridRowDragEventArgs e)
	{
		if (e.Position == TreeDataGridRowDropPosition.Inside && e.TargetRow.Model is not ModContainer)
		{
			//e.Inner.DragEffects = DragDropEffects.None;
			e.Position = GetDropPosition(false, e.Inner, e.TargetRow);
		}
	}

	private static bool CanDropEntry(IModEntry entry, ModListType listType)
	{
		if(entry.EntryType == ModEntryType.Mod && entry is ModEntry modEntry && modEntry.Data != null)
		{
			if(!modEntry.Data.IsForceLoaded)
			{
				if (listType == ModListType.Override)
				{
					return false;
				}
			}
			else
			{
				if(listType == ModListType.Active)
				{
					return modEntry.Data.CanAddToLoadOrder;
				}
			}
		}
		else if(entry.EntryType == ModEntryType.Container && entry is ModContainer modContainer)
		{
			foreach(var child in modContainer.Children)
			{
				if(!CanDropEntry(child, listType))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void OnTreeDataGridDragStarted(TreeDataGridRowDragStartedEventArgs e)
	{
		_isDragging = true;

		foreach(var obj in e.Models)
		{
			if(obj is IModEntry entry && entry.EntryType == ModEntryType.Container)
			{
				entry.PreserveExpanded = entry.IsExpanded;
			}
		}
	}

	private void OnTreeDataGridDrag(TreeDataGridRowDragEventArgs e)
	{
		if (ViewModel == null) return;

		if (ViewModel.IsLocked == true)
		{
			e.Inner.DragEffects = DragDropEffects.None;
			return;
		}

		MaybeRedirectDrop(e);

		if (e.Info is DragInfo di && di.Source.Selection is ITreeDataGridRowSelectionModel<IModEntry> selection)
		{
			e.Inner.DragEffects = DragDropEffects.Move;
			foreach (var entry in selection.SelectedItems)
			{
				if (entry != null && !CanDropEntry(entry, ViewModel.ListType))
				{
					e.Inner.DragEffects = DragDropEffects.None;
				}
			}

			//List to List
			if (di.Source != ModsTreeDataGrid.Source && e.Position == TreeDataGridRowDropPosition.None)
			{
				e.Position = GetDropPosition(e.TargetRow.Model is ModContainer, e.Inner, e.TargetRow);
			}
		}
	}

	private void OnTreeDataGridDrop(TreeDataGridRowDragEventArgs e)
	{
		_isDragging = false;
		var allowInside = false;

		if (ViewModel?.IsLocked == true)
		{
			e.Inner.DragEffects = DragDropEffects.None;
			return;
		}

		MaybeRedirectDrop(e);

		if (e.TargetRow.Model is ModContainer modContainer)
		{
			allowInside = true;
			/*if (!modContainer.IsExpanded && e.Position == TreeDataGridRowDropPosition.Inside)
			{
				//Need to delay by a few frames since the expander cell may not be rendered yet
				RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(10), () =>
				{
					modContainer.IsExpanded = true;
				});
			}*/
		}

		//List to List
		if (e.Info is DragInfo di
			&& di.Source is HierarchicalTreeDataGridSource<IModEntry> listSource
			&& ModsTreeDataGrid.Source is HierarchicalTreeDataGridSource<IModEntry> target)
		{
			foreach (var entry in listSource.RowSelection!.SelectedItems)
			{
				if (entry != null)
				{
					entry.PreserveSelection = true;
					entry.IsExpanded = entry.PreserveExpanded;
					entry.PreserveExpanded = false;
				}
			}

			//From one list to another
			if (di.Source != ModsTreeDataGrid.Source)
			{
				if (e.Position == TreeDataGridRowDropPosition.None)
				{
					e.Position = GetDropPosition(allowInside, e.Inner, e.TargetRow);
				}

				IndexPath targetIndex = 0;

				//Clear the previous selection, so only the dropped items are selected
				if(target.RowSelection != null)
				{
					foreach(var entry in target.RowSelection.SelectedItems)
					{
						entry?.IsSelected = false;
					}
					target.RowSelection.Clear();
				}

				if (ViewModel?.Mods.Rows.Count > 0)
				{
					targetIndex = target.Rows.RowIndexToModelIndex(e.TargetRow.RowIndex);
				}
				DragDropRows(listSource, target, listSource.RowSelection!.SelectedIndexes, targetIndex, e.Position, e.Inner.DragEffects);
			}
		}
	}

	private static void OnError(Exception ex)
	{
		DivinityApp.Log($"Error: {ex}");
	}

	private void OnPointerDown(object? sender, PointerPressedEventArgs e)
	{
		_isSingleSelect = false;

		if(sender is Control source && !e.GetCurrentPoint(source).Properties.IsLeftButtonPressed)
		{
			return;
		}

		//Allow deselecting with just left click, if no modifiers are pressed and a single item is selected
		if (!_isDragging && e.KeyModifiers == KeyModifiers.None && sender is TreeDataGridRow row && row.IsSelected && ModsTreeDataGrid.RowSelection?.Count == 1)
		{
			_isSingleSelect = true;
		}
	}

	private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		var row = sender as TreeDataGridRow;

		if(row != null)
		{
			if (!e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Shift) && row.Model is IModEntry entry && entry.EntryType == ModEntryType.Container)
			{
				entry.IsExpanded = !entry.IsExpanded;
			}

			if (!e.GetCurrentPoint(row).Properties.IsLeftButtonPressed)
			{
				return;
			}

			//Allow deselecting with just left click, if no modifiers are pressed and a single item is selected
			if (_isSingleSelect && !_isDragging && e.KeyModifiers == KeyModifiers.None)
			{
				if (row.IsSelected && ModsTreeDataGrid.RowSelection?.Count == 1)
				{
					ModsTreeDataGrid.RowSelection?.Deselect(row.RowIndex);
					e.Handled = true;
				}
			}
		}
		
		_isSingleSelect = false;
	}

	private Control? _openedToolTip = null;

	private static readonly MethodInfo _m_OnPointerWheelChanged = typeof(ScrollContentPresenter).GetMethod("OnPointerWheelChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;

	private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
	{
		if (_openedToolTip != null && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
		{
			var scrollViewer = _openedToolTip.FindDescendantOfType<ScrollViewer>();
			if (scrollViewer != null)
			{
				var verticalScrollBar = scrollViewer.FindVisualDescendantWithName<ScrollBar>("PART_VerticalScrollBar");
				var scrollContentPresenter = scrollViewer.FindDescendantOfType<ScrollContentPresenter>();
				if (verticalScrollBar?.IsVisible == true && scrollContentPresenter != null)
				{
					e.Source = _openedToolTip;
					_m_OnPointerWheelChanged?.Invoke(scrollContentPresenter, [e]);
					e.Handled = true;
				}
			}
		}
	}

	private void OnToolTipOpenedChanged(AvaloniaPropertyChangedEventArgs<bool> e)
	{
		if (e.NewValue.HasValue && e.Sender is Control sender)
		{
			if (e.NewValue.Value)
			{
				var tooltip = ToolTip.GetTip(sender);
				if (tooltip != null && tooltip is Control tt)
				{
					_openedToolTip = tt;
				}
			}
			else
			{
				_openedToolTip = null;
			}
		}
	}

	private void OnDrop(object? sender, DragEventArgs e)
	{
		if (ViewModel?.IsLocked == true)
		{
			e.DragEffects = DragDropEffects.None;
			return;
		}
		if (e.DragEffects.HasFlag(DragDropEffects.Copy))
		{
			var droppedFiles = e.DataTransfer.TryGetFiles();
			if (droppedFiles != null)
			{
				List<string> files = [];
				foreach (var file in droppedFiles)
				{
					var ext = AppServices.FS.Path.GetExtension(file.Name);
					if (ModImportService.IsImportableFile(ext))
					{
						files.Add(file.Path.LocalPath);
					}
				}
				AppServices.ModImporter.ImportMods(files, files.Count, ViewModel?.ListType == ModListType.Active);
			}
		}
	}

	private void OnDragOver(object? sender, DragEventArgs e)
	{
		if (ViewModel?.IsLocked == true)
		{
			e.DragEffects = DragDropEffects.None;
			return;
		}
		var canImport = false;
		var files = e.DataTransfer.TryGetFiles();
		if (files != null)
		{
			foreach (var file in files)
			{
				var ext = AppServices.FS.Path.GetExtension(file.Name);
				if(ModImportService.IsImportableFile(ext))
				{
					canImport = true;
				}
			}
		}
		
		if (canImport)
		{
			e.DragEffects = DragDropEffects.Copy;
		}
	}

	private IDisposable? _trackVisibleTask;

	private void TrackVisibleRows()
	{
		var rows = ModsTreeDataGrid.FindVisualDescendantsOfType<TreeDataGridRow>();
		if(rows != null && ViewModel != null)
		{
			ViewModel.VisibleRows.Clear();
			foreach (var row in rows.OrderBy(x => x.RowIndex))
			{
				ViewModel.VisibleRows.Add(row.RowIndex);
			}
		}
	}

	private Dictionary<int, CompositeDisposable> _rowEventHandlers = [];

	private static HashSet<int> FlattenIndexes(IReadOnlyList<IndexPath> indexes)
	{
		return [.. indexes.SelectMany(x => x)];
	}

	public ModListView()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			Background = Brushes.Black;
			return;
		}

		AddHandler(PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel);
		ToolTip.IsOpenProperty.Changed.Subscribe(OnToolTipOpenedChanged);

		AddHandler(DragDrop.DragOverEvent, OnDragOver);
		AddHandler(DragDrop.DropEvent, OnDrop);

		ModsTreeDataGrid.RowPrepared += (o,e) => { };

		this.WhenActivated(d =>
		{
			if (ViewModel != null)
			{
				d(Observable.FromEvent<EventHandler<TreeDataGridRowEventArgs>, TreeDataGridRowEventArgs>(
					h => (sender, e) => h(e),
					h => ModsTreeDataGrid.RowPrepared += h,
					h => ModsTreeDataGrid.RowPrepared -= h
				).Subscribe(e =>
				{
					var row = e.Row;
					var clickDownDisp = Observable.FromEventPattern<EventHandler<PointerPressedEventArgs>, PointerPressedEventArgs>(
						h => row.PointerPressed += h,
						h => row.PointerPressed -= h
					).Subscribe(x => OnPointerDown(x.Sender, x.EventArgs));

					var clickUpDisp = Observable.FromEventPattern<EventHandler<PointerReleasedEventArgs>, PointerReleasedEventArgs>(
						h => row.PointerReleased += h,
						h => row.PointerReleased -= h
					).Subscribe(x => OnPointerReleased(x.Sender, x.EventArgs));

					if (_rowEventHandlers.TryGetValue(row.RowIndex, out var disp))
					{
						disp?.Dispose();
					}

					var compDisp = new CompositeDisposable(clickDownDisp, clickUpDisp);
					_rowEventHandlers[row.RowIndex] = compDisp;
					d(compDisp);
				}, OnError));

				d(Observable.FromEvent<EventHandler<TreeDataGridRowEventArgs>, TreeDataGridRowEventArgs>(
					h => (sender, e) => h(e),
					h => ModsTreeDataGrid.RowClearing += h,
					h => ModsTreeDataGrid.RowClearing -= h
				).Subscribe(e =>
				{
					var row = e.Row;
					if (_rowEventHandlers.TryGetValue(row.RowIndex, out var disp))
					{
						disp?.Dispose();
						_rowEventHandlers.Remove(row.RowIndex);
					}
				}, OnError));

				d(FilterExpander.GetObservable(Expander.IsExpandedProperty).Skip(1).BindTo(ViewModel, x => x.IsFilterEnabled));

				//Throttle filtering here so we can be sure we're delaying when the user may be typing
				d(FilterTextBox.GetObservable(TextBox.TextProperty)
				.Skip(1)
				.Throttle(TimeSpan.FromMilliseconds(500))
				.ObserveOn(RxApp.MainThreadScheduler)
				.BindTo(ViewModel, x => x.FilterInputText));

				d(ViewModel.WhenAnyValue(x => x.FilterInputText).BindTo(this, x => x.FilterTextBox.Text));

				d(Observable.FromEventPattern<KeyEventArgs>(FilterTextBox, nameof(FilterTextBox.KeyDown)).Subscribe(e =>
				{
					var key = e.EventArgs.Key;
					if (key == Key.Return || key == Key.Enter || key == Key.Escape)
					{
						ModsTreeDataGrid.Focus(NavigationMethod.Pointer);
					}
				}));

				ModsTreeDataGrid.GetObservable(IsFocusedProperty).BindTo(ViewModel, x => x.IsFocused);
				ModsTreeDataGrid.GetObservable(IsKeyboardFocusWithinProperty).BindTo(ViewModel, x => x.IsKeyboardFocusWithin);

				d(Observable.FromEvent<EventHandler<TreeDataGridRowDragEventArgs>, TreeDataGridRowDragEventArgs>(
					h => (sender, e) => h(e),
					h => ModsTreeDataGrid.RowDragOver += h,
					h => ModsTreeDataGrid.RowDragOver -= h
				).Subscribe(OnTreeDataGridDrag, OnError));

				d(Observable.FromEvent<EventHandler<TreeDataGridRowDragEventArgs>, TreeDataGridRowDragEventArgs>(
					h => (sender, e) => h(e),
					h => ModsTreeDataGrid.RowDrop += h,
					h => ModsTreeDataGrid.RowDrop -= h
				).Subscribe(OnTreeDataGridDrop, OnError));

				d(Observable.FromEvent<EventHandler<TreeDataGridRowDragStartedEventArgs>, TreeDataGridRowDragStartedEventArgs>(
					h => (sender, e) => h(e),
					h => ModsTreeDataGrid.RowDragStarted += h,
					h => ModsTreeDataGrid.RowDragStarted -= h
				).Subscribe(OnTreeDataGridDragStarted, OnError));
				
				var modContext = this.FindResource("ModContextFlyout");
				var modContainerContext = this.FindResource("ModContainerMenuFlyout");
				var modThickness = new Thickness(0);
				var modContainerThickness = new Thickness(0, 0, 0, 1);

				d(Observable.FromEventPattern(ModsTreeDataGrid.RowSelection!, nameof(ModsTreeDataGrid.RowSelection.SelectionChanged))
					.Throttle(TimeSpan.FromTicks(50))
					.Select(_ => FlattenIndexes(ModsTreeDataGrid.RowSelection.SelectedIndexes)).InvokeCommand(ViewModel.UpdateSelectionsCommand));

				d(Observable.FromEventPattern(ModsTreeDataGrid.RowSelection!, nameof(ModsTreeDataGrid.RowSelection.SourceReset))
					.Throttle(TimeSpan.FromTicks(50))
					.Select(_ => FlattenIndexes(ModsTreeDataGrid.RowSelection.SelectedIndexes)).InvokeCommand(ViewModel.UpdateSelectionsCommand));

				void PrepareRow(TreeDataGridRow row, IModEntry entry)
				{
					row[!IsVisibleProperty] = entry.WhenAnyValue(x => x.IsVisible).ToBinding();
					row.GetObservable(TreeDataGridRow.IsSelectedProperty).BindTo(entry, x => x.IsSelected);

					entry.IsActive = ViewModel.ListType == ModListType.Active;

					if (entry.EntryType == ModEntryType.Mod)
					{
						entry.ContextMenu = modContext;
						row.BorderThickness = modThickness;
					}
					else if (entry.EntryType == ModEntryType.Container && entry is ModContainer container)
					{
						entry.ContextMenu = modContainerContext;
						row.BorderThickness = modContainerThickness;
						row[!BorderBrushProperty] = container.Settings.WhenAnyValue(x => x.BorderColor).Select(x => x.IsValid() ? ColorBrushCache.GetBrush(x) : ColorBrushCache.GetResourceBrush("SukiMediumBorderBrush")).ToBinding();

						var defaultBG = row.Background;
						row[!BackgroundProperty] = container.Settings.WhenAnyValue(x => x.BackgroundColor).Select(x => x != null ? ColorBrushCache.GetBrush(x) : defaultBG).ToBinding();
						row[!BorderThicknessProperty] = container.Settings.WhenAnyValue(x => x.BorderThickness).Select(x => x.IsValid() ? Thickness.Parse(x) : modContainerThickness).ToBinding();

						if(row.Rows != null && row.Rows[row.RowIndex] is HierarchicalRow<IModEntry> hierarchicalRow)
						{
							if (container.IsExpanded) hierarchicalRow.IsExpanded = true;
						}

						foreach (var child in container.ForEachNested())
						{
							child.IsActive = ViewModel.ListType == ModListType.Active;
						}
					}
				}

				//Initialize context menus. RowPrepared apparently doesn't fire until a row is interacted with
				d(Observable.FromEvent<EventHandler<RoutedEventArgs>, RoutedEventArgs>(
					h => (sender, e) => h(e),
					h => ModsTreeDataGrid.Loaded += h,
					h => ModsTreeDataGrid.Loaded -= h
				).Subscribe(e =>
				{
					if (ModsTreeDataGrid.RowsPresenter != null)
					{
						foreach (var child in ModsTreeDataGrid.RowsPresenter.GetVisualChildren())
						{
							if (child is TreeDataGridRow row && row.Model is IModEntry entry)
							{
								PrepareRow(row, entry);
							}
						}
					}
				}));

				d(Observable.FromEvent<EventHandler<TreeDataGridRowEventArgs>, TreeDataGridRowEventArgs>(
					h => (sender, e) => h(e),
					h => ModsTreeDataGrid.RowPrepared += h,
					h => ModsTreeDataGrid.RowPrepared -= h
				).Subscribe(e =>
				{
					_trackVisibleTask?.Dispose();
					_trackVisibleTask = RxApp.MainThreadScheduler.Schedule(TimeSpan.FromTicks(30), TrackVisibleRows);
					if (e.Row.Model is IModEntry entry)
					{
						if (entry.PreserveSelection)
						{
							/*ModEntryTreeDataGridRowSelectionModel preserves the selection after drag + drop visually,
							while this makes sure the backing data stays selected.
							If this isn't set here, then the next drag + drop will only move the directly selected item.
							*/
							ModsTreeDataGrid.RowSelection!.Select(e.RowIndex);
							entry.PreserveSelection = false;
						}
						//entry.IsExpanded = false;
						PrepareRow(e.Row, entry);
					}
				}));

				IDisposable? _reindexTask = null;

				d(Observable.FromEvent<EventHandler<ChildIndexChangedEventArgs>, ChildIndexChangedEventArgs>(
					h => (sender, e) => h(e),
					h => ModsTreeDataGrid.RowsPresenter!.ChildIndexChanged += h,
					h => ModsTreeDataGrid.RowsPresenter!.ChildIndexChanged -= h
				).Subscribe(e =>
				{
					if (e.Index > -1 && e.Child is TreeDataGridRow row && row.Model is IModEntry mod)
					{
						mod.IsActive = ViewModel.ListType == ModListType.Active;
						_reindexTask?.Dispose();
						_reindexTask = RxApp.MainThreadScheduler.Schedule(TimeSpan.FromTicks(2), ViewModel.UpdateIndexes);
					}
				}));

				d(ViewModel.FocusCommand.Subscribe(e =>
				{
					ModsTreeDataGrid.Focus(NavigationMethod.Pointer);
				}));
			}
		});
	}
}
