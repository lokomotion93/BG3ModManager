using LSLib.Stats;

namespace ModManager.Models.View;
public partial class StatsValidatorErrorEntry : TreeViewEntry
{
	public override object ViewModel => this;

	public StatLoadingError Error { get; }

	[Reactive] public partial string? Message { get; set; }
	[Reactive] public partial string? Code { get; set; }
	[Reactive] public partial string? LineText { get; set; }

	[ObservableAsProperty] public partial bool IsError { get; }

	private static string FormatMessage(StatLoadingError message) => $"{message.Location?.StartLine}: {message.Message} [{message.Code}]";

	public StatsValidatorErrorEntry(StatLoadingError error, string lineText = "")
	{
		Error = error;

		Message = FormatMessage(Error);
		Code = Error.Code;
		_isErrorHelper = this.WhenAnyValue(x => x.Code, code => code == DiagnosticCode.StatSyntaxError).ToUIProperty(this, x => x.IsError);
		LineText = lineText;
		//TODO Highlight the text accoding to the StartColumn/EndColumn
		if (lineText.IsValid() && error.Location != null)
		{
			AddChild(new StatsValidatorLineText
			{
				Text = lineText,
				Start = error.Location.StartColumn,
				End = error.Location.EndColumn,
				IsError = Code == DiagnosticCode.StatSyntaxError
			});
		}
	}
}
