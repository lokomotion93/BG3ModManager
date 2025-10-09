using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

using DynamicData;
using DynamicData.Binding;

using Material.Icons;
using Material.Icons.Avalonia;

using ModManager.Models.Mod;
using ModManager.Models.Updates;
using ModManager.Services;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace ModManager.ViewModels.Main;

public class CopyModUpdatesTask
{
	public List<ModUpdateData>? Updates { get; set; }
	public string? DocumentsFolder { get; set; }
	public string? ModPakFolder { get; set; }
	public int TotalProcessed { get; set; }
}

public class ModUpdatesViewModel : ReactiveObject, IRoutableViewModel
{
	public string UrlPathSegment => "modupdates";
	public IScreen HostScreen { get; }

	[Reactive] public bool Unlocked { get; set; }
	[Reactive] public bool JustUpdated { get; set; }

	public class UpdateTaskResult
	{
		public string ModId { get; set; }
		public bool Success { get; set; }
	}

	private readonly SourceList<ModUpdateData> UpdatesSource = new();
	public ITreeDataGridSource<ModUpdateData> Updates { get; }

	[ObservableAsProperty] public bool AnySelected { get; }
	[ObservableAsProperty] public bool AllSelected { get; }
	[ObservableAsProperty] public int TotalUpdates { get; }

	public RxCommandUnit UpdateModsCommand { get; }
	public ReactiveCommand<bool, Unit> ToggleSelectCommand { get; }
	public RxCommandUnit CloseCommand { get; }

	public void Add(ModUpdateData mod) => UpdatesSource.Add(mod);
	public void Add(IEnumerable<ModUpdateData> mods) => UpdatesSource.AddRange(mods);

	public void Clear()
	{
		UpdatesSource.Clear();
		Unlocked = true;
	}

	public void SelectAll(bool select = true)
	{
		foreach (var x in UpdatesSource.Items)
		{
			x.IsSelected = select;
		}
	}

	public async void UpdateSelectedMods()
	{
		var documentsFolder = AppServices.Pathways.Data.AppDataGameFolder;
		var modPakFolder = AppServices.Pathways.Data.AppDataModsPath;

		var result = await AppServices.Interactions.ShowMessageBox.Handle(new(
			"Update Mods?",
			"Download / copy updates? Previous pak files will be moved to the Recycle Bin.",
			InteractionMessageBoxType.YesNo));
		if (result)
		{
			var updates = UpdatesSource.Items.Where(x => x.IsSelected).ToList();

			Unlocked = false;

			ProcessUpdates(new CopyModUpdatesTask()
			{
				DocumentsFolder = documentsFolder,
				ModPakFolder = modPakFolder,
				Updates = UpdatesSource.Items.Where(x => x.IsSelected).ToList(),
				TotalProcessed = 0
			});
		}
	}

	private async Task<UpdateTaskResult> AwaitDownloadPartition(IEnumerator<ModUpdateData> partition, double progressIncrement,
		string outputFolder, CancellationToken token)
	{
		var result = new UpdateTaskResult();
		using (partition)
		{
			while (partition.MoveNext())
			{
				result.ModId = partition.Current.Mod.UUID;
				if (token.IsCancellationRequested) return result;
				await Task.Yield(); // prevents a sync/hot thread hangup
				var downloadResult = await partition.Current.DownloadData.DownloadAsync(partition.Current.LocalFilePath, outputFolder, token);
				result.Success = downloadResult.Success;
				ViewModelLocator.Progress.IncreaseValue(progressIncrement);
			}
		}
		return result;
	}

	private void ProcessUpdates(CopyModUpdatesTask taskData)
	{
		var progress = ViewModelLocator.Progress;
		progress.Title = "Processing updates...";
		progress.Start(async token =>
		{
			var currentTime = DateTime.Now;
			var partitionAmount = Environment.ProcessorCount;
			var progressIncrement = Math.Ceiling(100d / taskData.Updates.Count);
			var results = await Task.WhenAll(Partitioner.Create(taskData.Updates).GetPartitions(partitionAmount).AsParallel().Select(p => AwaitDownloadPartition(p, progressIncrement, taskData.ModPakFolder, token)));
			UpdateLastUpdated(results);
			await Observable.Start(FinishUpdating, RxApp.MainThreadScheduler);
		}, true);
	}

	private static void UpdateLastUpdated(UpdateTaskResult[] results)
	{
		var settings = AppServices.Get<ISettingsService>();
		settings.UpdateLastUpdated(results.Where(x => x.Success == true).Select(x => x.ModId).ToList());
	}

	private void FinishUpdating()
	{
		CloseCommand.Execute().Subscribe();
	}

	private void OnClose()
	{
		Unlocked = true;
		JustUpdated = true;
		if(HostScreen != null)
		{
			RxApp.MainThreadScheduler.ScheduleAsync(async (sch, token) =>
			{
				await HostScreen.Router.NavigateBack.Execute();
			});
		}
	}

	internal ModUpdatesViewModel(IScreen? host = null)
	{
		HostScreen = host ?? Locator.Current.GetService<IScreen>()!;

		Unlocked = true;
		AllSelected = true;

		CloseCommand = ReactiveCommand.Create(OnClose);

		UpdatesSource.CountChanged.ToUIProperty(this, x => x.TotalUpdates);

		var updatesConnection = UpdatesSource.Connect().ObserveOn(RxApp.MainThreadScheduler);

		ObservableCollectionExtended<ModUpdateData> readonlyUpdates = [];
		updatesConnection.DisposeMany().Bind(readonlyUpdates).Subscribe();

		//var checkboxHeader = new Button() {
		//	VerticalAlignment = VerticalAlignment.Center,
		//	HorizontalAlignment = HorizontalAlignment.Center,
		//	Content = new MaterialIcon() { Kind = MaterialIconKind.CheckAll, Foreground = Brushes.SpringGreen },
		//};
		//checkboxHeader.Classes.Add("icon");
		//checkboxHeader.Classes.Add("flat");
		var checkboxHeader = new CheckBox()
		{
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Left,
			IsChecked = true
		};

		var checkboxHeaderOptions = new CheckBoxColumnOptions<ModUpdateData>()
		{
			CanUserResizeColumn = false,
			CanUserSortColumn = false,
		};

		var templateColOptions = new TemplateColumnOptions<ModUpdateData>()
		{
			CanUserResizeColumn = true,
			CanUserSortColumn = true,
			IsTextSearchEnabled = true,
		};

		Updates = new FlatTreeDataGridSource<ModUpdateData>(readonlyUpdates)
		{
			Columns =
			{
				//Avalonia.Controls.Models.TreeDataGrid.
				new CheckBoxColumn<ModUpdateData>(checkboxHeader, x => x.IsSelected, (entry, b) => entry.IsSelected = b, GridLength.Auto, checkboxHeaderOptions),
				new TextColumn<ModUpdateData, string>("Name", x => x.DisplayName, GridLength.Auto),
				new TextColumn<ModUpdateData, string>("Current Version", x => x.CurrentVersion, GridLength.Auto),
				new TextColumn<ModUpdateData, string>("New Version", x => x.UpdateVersion, GridLength.Auto),
				new TextColumn<ModUpdateData, string>("Date", x => x.UpdateDateText, GridLength.Auto),
				new TextColumn<ModUpdateData, string>("Source", x => x.SourceText, GridLength.Auto),
				new TemplateColumn<ModUpdateData>("Path", "DownloadSourcePathCell", null, GridLength.Star, templateColOptions),
			},
		};

		var selectedConn = updatesConnection.AutoRefresh(x => x.IsSelected).ToCollection().Throttle(TimeSpan.FromMilliseconds(50)).ObserveOn(RxApp.MainThreadScheduler);
		selectedConn.Select(x => x.Any(x => x.IsSelected)).ToUIProperty(this, x => x.AnySelected, initialValue: false);
		selectedConn.Select(x => x.All(x => x.IsSelected)).ToUIProperty(this, x => x.AllSelected, initialValue: true);
		this.WhenAnyValue(x => x.AllSelected).Subscribe(b => checkboxHeader.IsChecked = b);

		var anySelectedObservable = this.WhenAnyValue(x => x.AnySelected);

		UpdateModsCommand = ReactiveCommand.Create(UpdateSelectedMods, anySelectedObservable);

		ToggleSelectCommand = ReactiveCommand.Create<bool>(b =>
		{
			foreach (var x in UpdatesSource.Items)
			{
				x.IsSelected = b;
			}
		});

		Observable.FromEventPattern<RoutedEventArgs>(checkboxHeader, nameof(checkboxHeader.Click))
			.Select(_ => checkboxHeader.IsChecked == true)
			.ObserveOn(RxApp.MainThreadScheduler)
			.InvokeCommand(ToggleSelectCommand);
	}
}


public class DesignModUpdatesViewModel : ModUpdatesViewModel
{
	public void AddTestEntries(ILocaleService localeService)
	{
		Add(new ModUpdateData(new ModData("0") { Name = "Test Mod", Author = "LaughingLeader" },
			new ModDownloadData()
			{
				DownloadPath = "https://github.com/LaughingLeader/TestBG3Mod/releases/latest",
				DownloadPathType = ModDownloadPathType.URL,
				DownloadSourceType = ModSourceType.GITHUB,
				Version = "1.0.0.1",
				Date = DateTimeOffset.Now
			}));
		Add(new ModUpdateData(new ModData("1") { Name = "Test Mod 2", Author = "LaughingLeader" },
			new ModDownloadData()
			{
				DownloadPath = "https://www.nexusmods.com/Core/Libs/Common/Widgets/DownloadPopUp?id=-1&game_id=-1&nmm=1",
				DownloadPathType = ModDownloadPathType.URL,
				DownloadSourceType = ModSourceType.NEXUSMODS,
				Version = "0.1.0.0",
				Date = DateTimeOffset.Now
			}));
	}

	public DesignModUpdatesViewModel() : base()
	{
		
	}
}

