using System.Diagnostics;
using System.Globalization;

namespace ModManager.Services;

public class LogWriterService
{
	private readonly IFileSystemService _fs;

	private static TraceListener? _logListener;

	public void ToggleLogging(bool enabled)
	{
		if (enabled)
		{
			if (_logListener == null)
			{
				var logsDir = DivinityApp.GetAppDirectory("_Logs");
				_fs.Directory.CreateDirectory(logsDir);
				var sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");
				var logFileName = "";
#if DEBUG
				logFileName = _fs.Path.Join(logsDir, "debug_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".log");
#else
			logFileName = _fs.Path.Join(logsDir, "release_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".log");
#endif
				_logListener = new TextWriterTraceListener(logFileName);
			}
			if (!Trace.Listeners.Contains(_logListener)) Trace.Listeners.Add(_logListener);
			Trace.AutoFlush = true;
		}
		else if (_logListener != null)
		{
			Trace.Listeners.Remove(_logListener);
			_logListener.Dispose();
			Trace.AutoFlush = false;
		}
	}

	public LogWriterService(IFileSystemService fs)
	{
		_fs = fs;

#if DEBUG
		ToggleLogging(true);
#endif
	}
}
