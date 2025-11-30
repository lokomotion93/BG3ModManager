using ModManager.Services.Dopus;

using System.Diagnostics;
using System.IO;

namespace ModManager.Services;

/// <inheritdoc />
public partial class DirectoryOpusService : ReactiveObject, IDirectoryOpusService
{
	private readonly IRegistryService _reg;
	private readonly IFileSystemService _fs;

	[Reactive] public partial bool IsEnabled { get; private set; }

	public DirectoryOpusService(IRegistryService registryService, IFileSystemService fs)
	{
		_reg = registryService;
		_fs = fs;

		IsEnabled = GetExecutablePath().IsExistingFile();
	}

	public bool IsInstalled(out string? exePath)
	{
		exePath = GetExecutablePath();
		return !string.IsNullOrEmpty(exePath);
	}

	public string? GetExecutablePath()
	{
		var appDirectory = _reg.GetApplicationInstallPath("Directory Opus");
		if(!string.IsNullOrEmpty(appDirectory))
		{
			return _fs.Path.Join(appDirectory, "dopusrt.exe");
		}
		return null;
	}

	/// <inheritdoc />
	public bool OpenInDirectoryOpus(string? filePath, bool focus = true, string? exePath = null)
	{
		if (!filePath.IsValid()) return false;

		exePath ??= GetExecutablePath();

		if (_fs.File.Exists(exePath) == true)
		{
			var cmd = focus ? DopusCommands.OpenFileInNewTabWithFocus : DopusCommands.OpenFileInNewTab;
			var args = cmd.ToStringWithArg(filePath, true);
			Process.Start(new ProcessStartInfo
			{
				FileName = exePath,
				Arguments = $"/cmd {args}",
				UseShellExecute = true
			});
			return true;
		}
		else
		{
			throw new FileNotFoundException($"Directory Opus not found at: {exePath}");
		}
	}
}
