using ModManager.Models.Mod;
using ModManager.Services;

namespace ModManager.Models.App;
public class ModFileMoveTask(ModData mod, string filePath, string newFilePath)
{
	public ModData Mod => mod;
	public string FilePath => filePath;
	public string NewFilePath => newFilePath;

	public void Move(IFileSystemService fs, bool overwrite = false)
	{
		try
		{
			fs.File.Move(FilePath, NewFilePath, overwrite);
			mod.FilePath = NewFilePath;
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error moving mod pak '{FilePath}' to '{NewFilePath}':\n{ex}");
		}
	}
}
