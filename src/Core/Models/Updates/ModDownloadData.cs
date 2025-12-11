using ModManager.Services;
using ModManager.Util;

using System.IO.Compression;

namespace ModManager.Models.Updates;

public enum ModDownloadPathType
{
	FILE,
	URL
}

public struct ModDownloadResult
{
	public bool Success;
	public string? OutputFilePath;
}

public partial class ModDownloadData : ReactiveObject
{
	[Reactive] public partial string? DownloadPath { get; set; }
	[Reactive] public partial ModDownloadPathType DownloadPathType { get; set; }
	[Reactive] public partial ModSourceType DownloadSourceType { get; set; }
	[Reactive] public partial DateTimeOffset? Date { get; set; }
	[Reactive] public partial string? Version { get; set; }
	[Reactive] public partial string? Description { get; set; }
	[Reactive] public partial bool IsIndirectDownload { get; set; }


	private static readonly IFileSystemService _fs;
	static ModDownloadData()
	{
		_fs = Locator.Current.GetService<IFileSystemService>()!;
	}

	private static bool FileNamesMatch(string localFilePath, string newFilePath) => _fs.Path.GetFileName(localFilePath).Equals(_fs.Path.GetFileName(newFilePath), StringComparison.OrdinalIgnoreCase);

	private static void MoveOldPakToRecycleBin(string previousFilePath, string newFilePath)
	{
		if (previousFilePath.IsValid() && FileNamesMatch(previousFilePath, newFilePath))
		{
			RecycleBinHelper.DeleteFile(previousFilePath, false, false);
		}
	}

	public async Task<ModDownloadResult> DownloadAsync(string? currentFilePath, string outputDirectory, CancellationToken token)
	{
		var fs = Locator.Current.GetService<IFileSystemService>()!;

		var result = new ModDownloadResult();
		try
		{
			if(!DownloadPath.IsValid())
			{
				throw new InvalidOperationException($"DownloadPath({DownloadPath}) is not valid.");
			}
			if(!currentFilePath.IsValid())
			{
				throw new InvalidOperationException($"currentFilePath({currentFilePath}) is not valid.");
			}
			_fs.Directory.CreateDirectory(outputDirectory);
			DivinityApp.Log($"Downloading {DownloadPath} - DownloadPathType({DownloadPathType}) DownloadSourceType({DownloadSourceType})");
			if (DownloadPathType == ModDownloadPathType.FILE)
			{
				var outputFilePath = fs.Path.Join(outputDirectory, DownloadPath);
				//This covers when an update changes the pak name
				MoveOldPakToRecycleBin(currentFilePath, outputFilePath);
				await FileUtils.CopyFileAsync(DownloadPath, outputFilePath, token);
				result.Success = true;
				result.OutputFilePath = outputFilePath;
				return result;
			}
			else if (DownloadPathType == ModDownloadPathType.URL)
			{
				if (IsIndirectDownload)
				{
					//Nexus non-premium users need to go to the website and get a nxm:// link to have download authorization.
					ProcessHelper.TryOpenPath(DownloadPath);
					result.Success = true;
					result.OutputFilePath = DownloadPath;
					return result;
				}
				else
				{
					using var webStream = await WebHelper.DownloadFileAsStreamAsync(DownloadPath, token);
					if (webStream == null) return result;

					if (DownloadPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
					{
						var outputFilePath = fs.Path.Join(outputDirectory, fs.Path.GetFileName(DownloadPath));
						MoveOldPakToRecycleBin(currentFilePath, outputFilePath);
						await using var outputFile = fs.FileStream.New(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
						await webStream.CopyToAsync(outputFile, 128000, token);
						result.Success = true;
						result.OutputFilePath = outputFilePath;
					}
					else
					{
						var archive = new ZipArchive(webStream);
						foreach (var entry in archive.Entries)
						{
							if (entry.Name.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
							{
								using var entryStream = entry.Open();
								var outputFilePath = fs.Path.Join(outputDirectory, fs.Path.GetFileName(entry.Name));
								MoveOldPakToRecycleBin(currentFilePath, outputFilePath);
								await using var outputFile = fs.FileStream.New(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
								await entryStream.CopyToAsync(outputFile, 128000, token);
								result.Success = true;
								result.OutputFilePath = outputFilePath;
							}
						}
					}
				}
				return result;
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error downloading update ({DownloadPath}): {ex}");
		}
		return result;
	}
}
