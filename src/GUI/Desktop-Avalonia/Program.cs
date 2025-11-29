using Avalonia;
using ReactiveUI.Avalonia;

using System;

namespace ModManager;

internal class Program
{
	[STAThread]
	public static void Main(string[] args)
	{
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	public static AppBuilder BuildAvaloniaApp()
	{
#if DEBUG
		//Avalonia Previewer fix
		GC.KeepAlive(typeof(Avalonia.Svg.Skia.SvgImageExtension).Assembly);
		GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);
#endif
		return AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();
	}
}
