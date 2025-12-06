using DynamicData;
using DynamicData.Binding;

using ModManager.Models.Mod;
using ModManager.Util;
using ModManager.Windows;

namespace ModManager.ViewModels.Main;

public class FileDeletionCompleteEventArgs : EventArgs
{
	public int TotalFilesDeleted => DeletedFiles?.Count ?? 0;
	public List<ModFileDeletionData> DeletedFiles { get; set; }
	public bool RemoveFromLoadOrder { get; set; }
	public bool IsDeletingDuplicates { get; set; }

	public FileDeletionCompleteEventArgs()
	{
		DeletedFiles = [];
	}
}

public partial class DeleteFilesViewModel : BaseProgressViewModel, IRoutableViewModel
{
	public string UrlPathSegment => "deletefiles";
	public IScreen HostScreen { get; }

	[Reactive] public partial bool PermanentlyDelete { get; set; }
	[Reactive] public partial bool RemoveFromLoadOrder { get; set; }
	[Reactive] public partial bool IsDeletingDuplicates { get; set; }

	public ObservableCollectionExtended<ModFileDeletionData> Files { get; set; } = [];

	[ObservableAsProperty] public partial bool AnySelected { get; }
	[ObservableAsProperty] public partial bool AllSelected { get; }
	[ObservableAsProperty] public partial string? SelectAllToolTip { get; }
	[ObservableAsProperty] public partial string? Title { get; }
	[ObservableAsProperty] public partial bool RemoveFromLoadOrderVisibility { get; }

	public RxCommandUnit SelectAllCommand { get; private set; }

	public event EventHandler<FileDeletionCompleteEventArgs>? FileDeletionComplete;

	public override async Task<bool> Run(CancellationToken token)
	{
		var targetFiles = Files.Where(x => x.IsSelected).ToList();

		await UpdateProgress($"Confirming deletion...", "", 0d);

		var result = await AppServices.Interactions.ConfirmModDeletion.Handle(new(targetFiles.Count, PermanentlyDelete, token));
		if (result)
		{
			var eventArgs = new FileDeletionCompleteEventArgs()
			{
				IsDeletingDuplicates = IsDeletingDuplicates,
				RemoveFromLoadOrder = !IsDeletingDuplicates && RemoveFromLoadOrder,
			};

			await Observable.Start(() => IsProgressActive = true, RxApp.MainThreadScheduler);
			await UpdateProgress($"Deleting {targetFiles.Count} mod file(s)...", "", 0d);
			var progressInc = 1d / targetFiles.Count;
			foreach (var f in targetFiles)
			{
				try
				{
					if (token.IsCancellationRequested)
					{
						DivinityApp.Log("Deletion stopped.");
						break;
					}
					if(f.EntryType == ModEntryType.Mod)
					{
						if (AppServices.FS.File.Exists(f.FilePath))
						{
							await UpdateProgress("", $"Deleting {f.FilePath}...");
							if (RecycleBinHelper.DeleteFile(f.FilePath, false, PermanentlyDelete))
							{
								eventArgs.DeletedFiles.Add(f);
								DivinityApp.Log($"Deleted mod file '{f.FilePath}'");
							}
						}
					}
					else
					{
						//TODO Delete the container from orders?
						eventArgs.DeletedFiles.Add(f);
					}
				}
				catch (Exception ex)
				{
					DivinityApp.Log($"Error deleting file '${f.FilePath}':\n{ex}");
				}
				await UpdateProgress("", "", ProgressValue + progressInc);
			}
			await UpdateProgress("", "", 1d);
			await Task.Delay(500, token);
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				FileDeletionComplete?.Invoke(this, eventArgs);
				AppServices.Interactions.NotifyEntriesDeleted.Handle(eventArgs.DeletedFiles).Subscribe();
				Close();
			});
		}
		return true;
	}

	public override void Close()
	{
		base.Close();
		Files.Clear();

		if(HostScreen is MainWindowViewModel main)
		{
			main.Views.SwitchToModOrderView();
		}
	}

	public void ToggleSelectAll()
	{
		var b = !AllSelected;
		foreach (var f in Files)
		{
			f.IsSelected = b;
		}
	}

	public DeleteFilesViewModel(IScreen? host = null) : base()
	{
		HostScreen = host ?? Locator.Current.GetService<IScreen>()!;

		RemoveFromLoadOrder = true;
		PermanentlyDelete = false;

		_removeFromLoadOrderVisibilityHelper = this.WhenAnyValue(x => x.IsDeletingDuplicates).ToUIProperty(this, x => x.RemoveFromLoadOrderVisibility);
		_titleHelper = this.WhenAnyValue(x => x.IsDeletingDuplicates).Select(b => !b ? "Files to Delete" : "Duplicate Mods to Delete").ToUIProperty(this, x => x.Title);

		var filesChanged = Files.ToObservableChangeSet().AutoRefresh(x => x.IsSelected).ToCollection().Throttle(TimeSpan.FromMilliseconds(50)).ObserveOn(RxApp.MainThreadScheduler);
		_anySelectedHelper = filesChanged.Select(x => x.Any(y => y.IsSelected)).ToUIProperty(this, x => x.AnySelected);
		_allSelectedHelper = filesChanged.Select(x => x.All(y => y.IsSelected)).ToUIProperty(this, x => x.AllSelected);
		_selectAllToolTipHelper = this.WhenAnyValue(x => x.AllSelected).Select(b => $"{(b ? "Deselect" : "Select")} All").ToUIProperty(this, x => x.SelectAllToolTip);

		SelectAllCommand = ReactiveCommand.Create(ToggleSelectAll, RunCommand.IsExecuting.Select(b => !b), RxApp.MainThreadScheduler);

		this.WhenAnyValue(x => x.AnySelected).BindTo(this, x => x.CanRun);
	}
}

public class DesignDeleteFilesViewModel : DeleteFilesViewModel
{
	public DesignDeleteFilesViewModel() : base()
	{
		for (var i = 0; i < 30; i++)
		{
			Files.Add(new() { UUID = $"{i}", DisplayName = $"Mod{i}", FilePath = $@"C:\Users\RandomTestUser0000000000001\AppData\Local\Larian Studios\Baldur's Gate 3\Mods\Mod{i}.pak" });
		}
	}
}