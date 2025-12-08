using Avalonia.Controls.Models.TreeDataGrid;

using ModManager.Models.Mod;

using System.ComponentModel;
using System.Reflection;

namespace ModManager.Controls.TreeDataGrid;

/// <summary>
/// A column in an <see cref="ITreeDataGridSource"/> which displays its values using a data
/// template.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TValue">The column data type.</typeparam>
public class ModEntryColumn<TSortType> : ColumnBase<IModEntry>, ITextSearchableColumn<IModEntry>
{
	private readonly Func<IModEntry, TSortType?> _sortValueSelector;

	public ModEntryColumn(Func<IModEntry, TSortType?> sortValueSelector, object? header = null, GridLength? width = null, ColumnOptions<IModEntry>? options = null) : base(header, width, options ?? new())
	{
		_sortValueSelector = sortValueSelector;
	}

	public override ICell CreateCell(IRow<IModEntry> row) => new ModEntryCell(row.Model);

	private int DefaultSortAscending(IModEntry? x, IModEntry? y)
	{
		if (x is null || y is null)
			return Comparer<IModEntry>.Default.Compare(x, y);
		var a = _sortValueSelector(x);
		var b = _sortValueSelector(y);
		return Comparer<TSortType>.Default.Compare(a, b);
	}

	private int DefaultSortDescending(IModEntry? x, IModEntry? y)
	{
		if (x is null || y is null)
			return -Comparer<IModEntry>.Default.Compare(x, y);
		var a = _sortValueSelector(x);
		var b = _sortValueSelector(y);
		return Comparer<TSortType>.Default.Compare(b, a);
	}

	public override Comparison<IModEntry?>? GetComparison(ListSortDirection direction)
	{
		return direction switch
		{
			ListSortDirection.Ascending => Options.CompareAscending ?? DefaultSortDescending,
			ListSortDirection.Descending => Options.CompareDescending ?? DefaultSortAscending,
			_ => Options.CompareDescending,
		};
	}

	public bool IsTextSearchEnabled => true;
	public string? SelectValue(IModEntry model) => model.DisplayName;
}
