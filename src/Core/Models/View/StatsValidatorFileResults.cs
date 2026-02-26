using DynamicData;
using DynamicData.Binding;

namespace ModManager.Models.View;
public partial class StatsValidatorFileResults : TreeViewEntry
{
	public override object ViewModel => this;

	[Reactive] public partial string? FilePath { get; set; }

	[ObservableAsProperty] public partial string? Name { get; }
	[ObservableAsProperty] public partial int Total { get; }
	[ObservableAsProperty] public partial string? DisplayName { get; }
	[ObservableAsProperty] public partial string? ToolTip { get; }
	[ObservableAsProperty] public partial bool HasErrors { get; }

	private readonly ReadOnlyObservableCollection<StatsValidatorErrorEntry> _errors;
	public ReadOnlyObservableCollection<StatsValidatorErrorEntry> Errors => _errors;

	private string ToToolTip(string? filePath, int total)
	{
		var errors = Children.Cast<StatsValidatorErrorEntry>().Count(x => x.IsError);
		return $"{filePath ?? string.Empty}\nErrors: {errors}\nWarnings: {total - errors}";
	}

	public StatsValidatorFileResults()
	{
		var childrenChanged = Children.ToObservableChangeSet().Transform(x => (StatsValidatorErrorEntry)x);
		_totalHelper = childrenChanged.CountChanged().Select(_ => Children.Count).ToUIProperty(this, x => x.Total);
		childrenChanged.Bind(out _errors).Subscribe();
		var fs = AppLocator.Current.GetService<IFileSystemService>()!;
		_nameHelper = this.WhenAnyValue(x => x.FilePath).Select(fs.Path.GetFileName).ToUIProperty(this, x => x.Name);
		_displayNameHelper = this.WhenAnyValue(x => x.Name, x => x.Total).Select(x => $"{x.Item1} ({x.Item2})").ToUIProperty(this, x => x.DisplayName);
		_toolTipHelper = this.WhenAnyValue(x => x.FilePath, x => x.Total, ToToolTip).ToUIProperty(this, x => x.ToolTip);
		_hasErrorsHelper = Errors.ToObservableChangeSet().AutoRefresh(x => x.IsError).ToCollection().Select(_ => Errors.Any(x => x.IsError)).ToUIProperty(this, x => x.HasErrors);
	}
}
