using ModManager.Services;

using System.Diagnostics;

namespace ModManager.Utils;

public class ExceptionSuppressionHandler : IObserver<Exception>
{
	public void OnNext(Exception value)
	{
		AppServices.Get<LogWriterService>()?.ToggleLogging(true);
		DivinityApp.Log($"Error: [{value.Source}]({value.GetType()}): {value.Message}\n{value.StackTrace}");
		if (Debugger.IsAttached) Debugger.Break();
		AppServices.Commands.ShowAlert(value.ToString(), AlertType.Danger, 30, "Error");
	}

	public void OnError(Exception error)
	{
		if (Debugger.IsAttached) Debugger.Break();
	}

	public void OnCompleted()
	{
		if (Debugger.IsAttached) Debugger.Break();
	}
}
