namespace ModManager.Models.View;
public partial class StatsValidatorLineText : TreeViewEntry
{
	public override object ViewModel => this;

	[Reactive] public partial string? Text { get; set; }
	[Reactive] public partial int Start { get; set; }
	[Reactive] public partial int End { get; set; }
	[Reactive] public partial bool IsError { get; set; }

	[ObservableAsProperty] public partial string? HighlightedText { get; }

	private static string GetHighlightedText(string? text, int start, int end)
	{
		if (!text.IsValid()) return string.Empty;

		var length = Math.Min(text.Length, end - start);

		if (length > 0)
		{
			try
			{
				var result = text.Substring(start, length);
				DivinityApp.Log($"{result}");
				return result;
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"{ex}");
			}
		}
		return string.Empty;
	}

	public StatsValidatorLineText()
	{
		IsExpanded = true;

		_highlightedTextHelper = this.WhenAnyValue(x => x.Text, x => x.Start, x => x.End, GetHighlightedText).ToUIProperty(this, x => x.HighlightedText);
	}
}
