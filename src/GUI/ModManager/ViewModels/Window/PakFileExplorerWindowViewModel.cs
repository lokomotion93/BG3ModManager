using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;

using DynamicData;
using DynamicData.Binding;

using LSLib.LS;

using ModManager.Helpers.Sorting;
using ModManager.Models.Mod;
using ModManager.Models.View;
using ModManager.Services;
using ModManager.Util;
using ModManager.Windows;

using System.Collections.Concurrent;

namespace ModManager.ViewModels.Window;

internal class PackageExtractionTask : IDisposable
{
	private readonly Package _package;

	public HashSet<string> Files = [];
	
	public PackageExtractionTask(string path)
	{
		var pr = new PackageReader();
		_package = pr.Read(path);
	}

	//Task CopyPackagedFileAsync(string outputDirectory, PackagedFileInfo f, CancellationToken token)
	public List<Task> GetTasks(Func<PackagedFileInfo, Task> buildTask)
	{
		List<Task> tasks = [];
		foreach(var f in _package.Files)
		{
			if(Files.Contains(f.Name) && !f.IsDeletion())
			{
				tasks.Add(buildTask(f));
			}
		}
		return tasks;
	}

	private bool disposedValue;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_package?.Dispose();
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

public class PakFileExplorerWindowViewModel : BaseProgressViewModel, IClosableViewModel
{
	private readonly IDialogService _dialogService;
	private readonly IGlobalCommandsService _commands;
	private readonly IFileSystemService _fs;

	private readonly SourceCache<ModFileEntry, string> _files = new(x => x.FilePath);
	protected SourceCache<ModFileEntry, string> Files => _files;
	public HierarchicalTreeDataGridSource<ModFileEntry> FileTreeSource { get; }

	public ObservableCollectionExtended<ModFileEntry> SelectedItems { get; }
	public ModFileEntry? SelectedItem { get; set; }

	public RxCommandUnit? AddModCommand { get; set; }
	public RxCommandUnit OpenFileBrowserCommand { get; }
	public ReactiveCommand<ShowAlertRequest, Unit>? ShowAlertCommand { get; set; }
	public ReactiveCommand<object?, Unit> CopyToClipboardCommand { get; }
	public ReactiveCommand<ModFileEntry, Unit> ExtractFileCommand { get; }
	public RxCommandUnit ExtractSelectedFilesCommand { get; }
	public ReactiveCommand<object?, Unit> ClearCommand { get; }

	private void AddFileToTree(string pakPath, PackagedFileInfo pakFile, ConcurrentDictionary<string, ModFileEntry> directories, CancellationToken token) => AddFileToTree(pakPath, pakFile.Name, directories, token, pakFile.UncompressedSize);

	private void AddFileToTree(string pakPath, string filePath, ConcurrentDictionary<string, ModFileEntry> directories, CancellationToken token, double? fileSize = null)
	{
		var immediateParentDirectory = _fs.Path.GetDirectoryName(filePath);

		ModFileEntry? parentDirectory = null;

		if (!string.IsNullOrEmpty(immediateParentDirectory))
		{
			immediateParentDirectory = immediateParentDirectory.Replace(_fs.Path.DirectorySeparatorChar, '/');

			if (directories.TryGetValue(immediateParentDirectory, out var parent))
			{
				parentDirectory = parent;
			}
			else
			{
				var fileDirs = immediateParentDirectory.Split('/');

				var nextFullDirPath = "";
				foreach (var dir in fileDirs)
				{
					if (token.IsCancellationRequested) break;

					if (nextFullDirPath != "") nextFullDirPath += "/";
					nextFullDirPath += dir;

					if (parentDirectory == null)
					{
						if (directories.TryGetValue(nextFullDirPath, out var nextDir))
						{
							parentDirectory = nextDir;
						}
						else
						{
							parentDirectory = new ModFileEntry(pakPath, nextFullDirPath, _fs, true);
							directories.TryAdd(nextFullDirPath, parentDirectory);
						}
					}
					else
					{
						if (parentDirectory.TryGetChild(nextFullDirPath, out var subDirectory))
						{
							parentDirectory = subDirectory;
						}
						else
						{
							subDirectory = new ModFileEntry(pakPath, nextFullDirPath, _fs, true);
							parentDirectory.AddChild(subDirectory);
							parentDirectory = subDirectory;
						}
					}
				}
			}
		}

		if (parentDirectory != null)
		{
			parentDirectory.AddChild(new ModFileEntry(pakPath, filePath, _fs, false, fileSize ?? _fs.FileInfo.New(filePath).Length));
		}
		else
		{
			directories.TryAdd(filePath, new ModFileEntry(pakPath, filePath, _fs));
		}
	}

	private async Task LoadPakAsync(string pakPath, CancellationToken token)
	{
		DivinityApp.Log($"Loading pak... {pakPath}");

		var pr = new PackageReader();
		using var pak = pr.Read(pakPath);

		var directories = new ConcurrentDictionary<string, ModFileEntry>();

		DivinityApp.Log("Building file tree");

		var pakEntry = new ModFileEntry(pakPath, pakPath, _fs, false, _fs.FileInfo.New(pakPath).Length);
		foreach(var file in pak.Files)
		{
			AddFileToTree(pakPath, file, directories, token);
		}

		DivinityApp.Log("Finished parsing pak files. Adding to tree.");

		await Observable.Start(() =>
		{
			pakEntry.AddChild(directories.Values);
			_files.AddOrUpdate(pakEntry);
		}, RxApp.MainThreadScheduler);
	}

	private async Task LoadLooseDataAsync(ModData mod, CancellationToken token)
	{
		var modFolder = mod.Folder;
		var gameDirectory = _fs.Directory.GetParent(mod.FilePath).Parent.Parent.FullName;

		var modsFolder = _fs.Path.Join(gameDirectory, "Mods", modFolder);
		var publicFolder = _fs.Path.Join(gameDirectory, "Public", modFolder);
		var projectFolder = _fs.Directory.GetParent(mod.ToolkitProjectMeta.FilePath).FullName;
		var editorModsFolder = _fs.Path.Join(gameDirectory, "Editor", "Mods", modFolder);
		var generatedPublicFolder = _fs.Path.Join(gameDirectory, "Generated", "Public", modFolder);

		List<string> sourceDirs = [modsFolder, publicFolder, projectFolder, editorModsFolder, generatedPublicFolder];
		var directories = new ConcurrentDictionary<string, ModFileEntry>();

		DivinityApp.Log($"Loading loose data containing mod folder... {modFolder}");

		ConcurrentBag<string> files = [];

		foreach(var dir in sourceDirs)
		{
			if (token.IsCancellationRequested) break;
			if(dir.IsExistingDirectory())
			{
				foreach(var file in _fs.Directory.EnumerateFiles(dir))
				{
					AddFileToTree(modsFolder, file, directories, token);
				}
			}
		}

		DivinityApp.Log("Finished parsing loose files. Adding to tree.");

		await Observable.Start(() =>
		{
			_files.AddOrUpdate(directories.Values);
		}, RxApp.MainThreadScheduler);
	}

	private IDisposable? _extractTask;

	private async Task CopyPackagedFileAsync(string outputDirectory, PackagedFileInfo f, CancellationToken token)
	{
		var outputPath = _fs.Path.Combine(outputDirectory, f.Name);
		if (_fs.Path.GetDirectoryName(outputPath) is string parentFolder)
		{
			_fs.Directory.CreateDirectory(parentFolder);
		}
		using var inStream = f.CreateContentReader();
		await using var outStream = _fs.FileStream.New(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read, 32000, FileOptions.Asynchronous);
		await inStream.CopyToAsync(outStream, 32000, token);
	}

	private void AddExtractFileTask(string modFilePath, string sourcePakPath, Dictionary<string, PackageExtractionTask> packageExtractors)
	{
		if (packageExtractors.TryGetValue(sourcePakPath, out var pakExtractor))
		{
			pakExtractor.Files.Add(modFilePath);
		}
		else
		{
			pakExtractor = new PackageExtractionTask(sourcePakPath);
			pakExtractor.Files.Add(modFilePath);
			packageExtractors[sourcePakPath] = pakExtractor;
		}
	}

	private async Task ExtractFilesAsync(IEnumerable<ModFileEntry> files, CancellationToken token)
	{
		var settings = AppServices.Settings;
		var pathways = AppServices.Pathways.Data;

		var dialogResult = await _dialogService.OpenFolderAsync(new OpenFolderBrowserDialogRequest(
			Loca.Window_PakFileExplorer_Picker_ExtractFiles_Title,
			_dialogService.GetInitialStartingDirectory(settings.ManagerSettings.LastExtractOutputPath),
			null,
			null,
			AppServices.Get<PakFileExplorerWindow>()
		));

		if (dialogResult.Success)
		{
			var extractToDirectory = dialogResult.File!;

			if (settings.ManagerSettings.LastExtractOutputPath != extractToDirectory)
			{
				settings.ManagerSettings.LastExtractOutputPath = extractToDirectory;
				pathways.LastSaveFilePath = extractToDirectory;
				settings.ManagerSettings.Save(out _);
			}

			List<(string, string)> looseFiles = [];
			Dictionary<string, PackageExtractionTask> packageExtractors = [];
			Dictionary<string, bool> extractPackages = [];

			var tasks = new List<Task>();
			var total = 0;

			foreach (var modFile in files)
			{
				total++;
				if (modFile.SourcePakFilePath == modFile.FilePath)
				{
					extractPackages[modFile.SourcePakFilePath] = true;
				}
				else if(!extractPackages.ContainsKey(modFile.SourcePakFilePath))
				{
					if(modFile.IsFromPak)
					{
						if (modFile.IsDirectory)
						{
							foreach(var subfile in modFile.GetAllFiles())
							{
								AddExtractFileTask(subfile.FilePath, subfile.SourcePakFilePath, packageExtractors);
							}
						}
						else
						{
							AddExtractFileTask(modFile.FilePath, modFile.SourcePakFilePath, packageExtractors);
						}
					}
					else
					{
						looseFiles.Add((modFile.FilePath, _fs.Path.GetRelativePath(modFile.SourcePakFilePath, modFile.FilePath)));
					}
				}
			}

			try
			{
				foreach (var extractor in packageExtractors)
				{
					tasks.AddRange(extractor.Value.GetTasks(f => CopyPackagedFileAsync(extractToDirectory, f, token)));
				}

				foreach((string filePath, string outputPath) in looseFiles)
				{
					if(_fs.File.Exists(filePath))
					{
						tasks.Add(FileUtils.CopyFileAsync(filePath, outputPath, token));
					}
					else if(_fs.Directory.Exists(filePath))
					{
						tasks.Add(FileUtils.CopyDirectoryAsync(filePath, outputPath, token));
					}
				}

				await Task.WhenAll(tasks).WaitAsync(token);

				var msg = Loca.Alert_Success_PakFileExplorer_ExtractedFiles.SafeFormat($"Extracted {total} files to {extractToDirectory}", total, extractToDirectory);
				ShowAlertCommand?.Execute(new ShowAlertRequest(msg, AlertType.Success)).Subscribe();
			}
			finally
			{
				foreach (var extractor in packageExtractors)
				{
					extractor.Value?.Dispose();
				}
			}
		}
	}

	internal class TopModFileSorter : IComparer<ModFileEntry>
	{
		public int Compare(ModFileEntry? s1, ModFileEntry? s2)
		{
			var isS1Pak = s1?.FileExtension == ".pak";
			var isS2Pak = s2?.FileExtension == ".pak";
			if (isS1Pak == isS2Pak)
			{
				return Sorters.FileIgnoreCase.Compare(s1, s2);
			}
			if(isS1Pak)
			{
				return -1;
			}
			else if(isS2Pak)
			{
				return 1;
			}
			if(s1?.IsDirectory == true)
			{
				return -1;
			}
			else if(s2?.IsDirectory == true)
			{
				return 1;
			}
			return Sorters.FileIgnoreCase.Compare(s1, s2);
		}
	}

	private static readonly TopModFileSorter _fileSort = new();

	public async Task LoadModsAsync(IEnumerable<ModData> mods, CancellationToken token)
	{
		var pakLoadingTasks = new List<Task>();
		var looseLoadingTasks = new List<Task>();
		foreach(var mod in mods)
		{
			if (token.IsCancellationRequested) break;
			if(mod.FilePath.IsExistingFile())
			{
				if (!mod.IsLooseMod)
				{
					pakLoadingTasks.Add(LoadPakAsync(mod.FilePath, token));
				}
				else if (mod.FilePath.IsExistingFile())
				{
					looseLoadingTasks.Add(LoadLooseDataAsync(mod, token));
				}
			}
		}
		await Task.WhenAll(pakLoadingTasks);
		await Task.WhenAll(looseLoadingTasks);
	}

	public PakFileExplorerWindowViewModel()
	{
		_commands ??= AppServices.Commands;
		_fs ??= AppServices.FS;
		_dialogService ??= AppServices.Dialog;
		var cf = AppServices.ControlFactory;

		ObservableCollectionExtended<ModFileEntry> readOnlyFiles = [];
		Files.Connect()
			.ObserveOn(RxApp.MainThreadScheduler)
			.SortAndBind(readOnlyFiles, _fileSort, new SortAndBindOptions { UseBinarySearch = true })
			.DisposeMany()
			.Subscribe();

		FileTreeSource = new HierarchicalTreeDataGridSource<ModFileEntry>(readOnlyFiles)
		{
			Columns =
			{
				new HierarchicalExpanderColumn<ModFileEntry>(
					//new TextColumn<PakFileEntry, string>("Name", x => x.FileName, GridLength.Star),
					new TemplateColumn<ModFileEntry>(cf.LocalizedTextBlock(nameof(Loca.Column_Name), "Name"), "FileNameWithIconCell", null, new GridLength(10d, GridUnitType.Star)),
					x => x.Subfiles, x => x.Subfiles != null && x.Subfiles.Count > 0, x => x.IsExpanded),
				new TextColumn<ModFileEntry, string>(cf.LocalizedTextBlock(nameof(Loca.Column_Size), "Size"), x => x.Size, GridLength.Auto),
				new TextColumn<ModFileEntry, string>(cf.LocalizedTextBlock(nameof(Loca.Column_Ext), "Ext"), x => x.ExtensionDisplayName, new GridLength(1d, GridUnitType.Star)),
			},
		};

		SelectedItems = [];

		Observable.FromEvent<EventHandler<TreeSelectionModelSelectionChangedEventArgs<ModFileEntry>>?, TreeSelectionModelSelectionChangedEventArgs<ModFileEntry>>(
			h => (sender, e) => h(e),
			h => FileTreeSource.RowSelection!.SelectionChanged += h,
			h => FileTreeSource.RowSelection!.SelectionChanged -= h
		).ObserveOn(RxApp.MainThreadScheduler)
		.Subscribe((e) =>
		{
			SelectedItems.Clear();
			if (FileTreeSource.RowSelection!.SelectedItems != null && FileTreeSource.RowSelection!.SelectedItems.Count > 0)
			{
				SelectedItems.AddRange(FileTreeSource.RowSelection!.SelectedItems);
			}
			SelectedItem = FileTreeSource.RowSelection!.SelectedItem;
		});

		CopyToClipboardCommand = _commands.CopyToClipboardCommand;

		//AddModCommand = ReactiveCommand.CreateFromTask(async () =>
		//{
		//	var results = await AppServices.Interactions.PickMods.Handle(new(Loca.Window_PakFileExplorer_Picker_AddMod_Title));
		//	if(results.Confirmed)
		//	{
		//		await LoadModsAsync(results.Mods, CancellationToken.None);
		//	}
		//});

		OpenFileBrowserCommand = ReactiveCommand.CreateFromTask(async () =>
		{
			if (_dialogService != null)
			{
				var settings = AppServices.Settings;
				var pathways = AppServices.Pathways.Data;

				var dialogResult = await _dialogService.OpenFileAsync(new OpenFileBrowserDialogRequest(
					Loca.Window_PakFileExplorer_Picker_AddPak_Title,
					_dialogService.GetInitialStartingDirectory(settings.ManagerSettings.LastImportDirectoryPath),
					[CommonFileTypes.ModPak],
					window: AppServices.Get<PakFileExplorerWindow>()
				));

				if (dialogResult.Success)
				{
					var filePath = dialogResult.File;
					if(filePath.IsValid())
					{
						var savedDirectory = _fs.Path.GetDirectoryName(filePath)!;
						if (settings.ManagerSettings.LastImportDirectoryPath != savedDirectory)
						{
							settings.ManagerSettings.LastImportDirectoryPath = savedDirectory;
							pathways.LastSaveFilePath = savedDirectory;
							settings.ManagerSettings.Save(out _);
						}

						RxApp.TaskpoolScheduler.ScheduleAsync(async (sch, token) =>
						{
							await LoadPakAsync(filePath, token);
						});
					}
				}
			}
		});

		var hasFilesSelected = SelectedItems.ToObservableChangeSet().CountChanged().Select(x => SelectedItems.Count > 0).ObserveOn(RxApp.MainThreadScheduler);

		ExtractFileCommand = ReactiveCommand.Create<ModFileEntry>(pakFile =>
		{
			_extractTask?.Dispose();
			_extractTask = RxApp.TaskpoolScheduler.ScheduleAsync(async (sch, t) =>
			{
				await ExtractFilesAsync([pakFile], t);
			});
		});

		ExtractSelectedFilesCommand = ReactiveCommand.Create(() =>
		{
			_extractTask?.Dispose();
			var files = SelectedItems.ToList();
			if (files.Count > 0)
			{
				_extractTask = RxApp.TaskpoolScheduler.ScheduleAsync(async (sch, t) =>
				{
					await ExtractFilesAsync(files, t);
				});
			}
		}, hasFilesSelected);

		ClearCommand = ReactiveCommand.Create<object?>(obj =>
		{
			_extractTask?.Dispose();
			if (obj == null)
			{
				_files.Clear();
			}
			else if (obj is ModFileEntry entry)
			{
				entry.ClearChildren();
			}
		});
	}

	[DependencyInjectionConstructor]
	public PakFileExplorerWindowViewModel(IDialogService dialogService, IGlobalCommandsService commands, IFileSystemService fs) : this()
	{
		_dialogService = dialogService;
		_commands = commands;
		_fs = fs;
	}
}

public class DesignPakFileExplorerWindowViewModel : PakFileExplorerWindowViewModel
{
	public DesignPakFileExplorerWindowViewModel() : base()
	{
		var random = new Random();

		var fs = AppServices.FS;

		string[] extRan = [".txt", ".json", ".lsj", ".lsx", ".xml", ".xaml", ".dds", ".png", ".tiff", ".gif"];
		string getExt()
		{
			return extRan[random.Next(extRan.Length-1)];
		}

		var testPak = new ModFileEntry("TestMod.pak", "TestMod.pak", fs);
		var testPublicDir = new ModFileEntry("TestMod.pak", "Public", fs, true);
		var testPublicModDir = new ModFileEntry("TestMod.pak", "Public\\TestMod", fs, true);
		testPak.AddChild(testPublicDir);
		testPublicDir.AddChild(testPublicModDir);
		for (var i = 0; i < 20; i++)
		{
			testPublicModDir.AddChild(new ModFileEntry("TestMod.pak", $"Public\\TestMod\\File_{i}{getExt()}", fs, false, random.NextDouble() * (random.NextInt64(1024, 6464) ^ 2)));
		}

		var directory2 = new ModFileEntry(string.Empty, "Directory2", fs, true);
		for (var i = 0; i < 20; i++)
		{
			directory2.AddChild(new ModFileEntry(string.Empty, $"Directory2\\File_{i}{getExt()}", fs, false, random.NextDouble() * (random.NextInt64(1024, 6464) ^ 2)));
		}

		Files.AddOrUpdate([testPak, directory2]);

		Files.AddOrUpdate(new ModFileEntry(string.Empty, $"BootstrapServer.lua", fs, false, 200));

		for (var i = 0; i < 20; i++)
		{
			Files.AddOrUpdate(new ModFileEntry(string.Empty, $"File_{i}{getExt()}", fs, false, random.NextDouble() * (random.NextInt64(1024, 6464) ^ 2)));
		}
	}
}
