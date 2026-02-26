using LSLib.LS;
using LSLib.LS.Pak;

namespace ModManager.Util;

public static class SaveTools
{
	private static readonly IFileSystemService _fs;
	static SaveTools()
	{
		_fs = AppLocator.Current.GetService<IFileSystemService>()!;
	}

	public static bool RenameSave(string pathToSave, string newName)
	{
		try
		{
			var baseOldName = _fs.Path.GetFileNameWithoutExtension(pathToSave);
			var baseNewName = _fs.Path.GetFileNameWithoutExtension(newName);
			var output = _fs.Path.ChangeExtension(_fs.Path.Join(_fs.Path.GetDirectoryName(pathToSave), newName), ".lsv");

			var reader = new PackageReader();
			using var package = reader.Read(pathToSave);
			var saveScreenshotImage = package.Files.FirstOrDefault(p => p.Name.EndsWith(".WebP"));
			if (saveScreenshotImage != null)
			{
				saveScreenshotImage.Name = saveScreenshotImage.Name.Replace(_fs.Path.GetFileNameWithoutExtension(saveScreenshotImage.Name), baseNewName);

				DivinityApp.Log($"Renamed internal screenshot '{saveScreenshotImage.Name}' in '{output}'.");
			}

			var conversionParams = ResourceConversionParameters.FromGameVersion(DivinityApp.GAME);

			var build = new PackageBuildData
			{
				Version = conversionParams.PAKVersion,
				Compression = CompressionMethod.Zlib,
				CompressionLevel = LSCompressionLevel.Default
			};

			using var writer = PackageWriterFactory.Create(build, output);
			writer.Write();

			_fs.File.SetLastWriteTime(output, _fs.File.GetLastWriteTime(pathToSave));
			_fs.File.SetLastAccessTime(output, _fs.File.GetLastAccessTime(pathToSave));

			return true;

		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Failed to rename save: {ex}");
		}

		return false;
	}
}
