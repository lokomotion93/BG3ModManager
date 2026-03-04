using Avalonia;

using ModManager.Utils;

using ReactiveUI.Avalonia;
using ReactiveUI.Builder;

using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace ModManager;

internal class Program
{
	private static bool EnsureSingleInstance(string[] args)
	{
		var procName = Process.GetCurrentProcess().ProcessName;
		if (Process.GetProcessesByName(procName).Length > 1)
		{
			if (args.Length > 0)
			{
				var argsMessage = string.Join(" ", args);
				try
				{
					using var pipe = new NamedPipeClientStream(".", DivinityApp.PIPE_ID,
					PipeDirection.Out, PipeOptions.WriteThrough, System.Security.Principal.TokenImpersonationLevel.Impersonation);
					pipe.Connect(500);
					using var sw = new System.IO.StreamWriter(pipe, Encoding.UTF8);
					sw.Write(argsMessage);
					sw.Flush();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error sending args to server:\n{ex}");
				}
#if DEBUG
				return true;
#endif
			}
#if !DEBUG
			return true;
#endif
		}
		return false;
	}

	[STAThread]
	public static void Main(string[] args)
	{
		//Only close if args are passed in and we're a debug build,
		//otherwise always close if another instance exists in release
		if (EnsureSingleInstance(args))
		{
			Environment.Exit(0);
			return;
		}
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	public static AppBuilder BuildAvaloniaApp()
	{
		return AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI(config =>
			{
				config
				.WithPlatformServices()
				.WithExceptionHandler(new ExceptionSuppressionHandler());
			});
	}
}
