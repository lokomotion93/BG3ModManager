using DynamicData;
using DynamicData.Binding;

using ModManager.Models.NexusMods;

using NexusModsNET.DataModels.GraphQL.Types;

namespace ModManager.ViewModels;

public partial class NexusModsCollectionDownloadWindowViewModel : ReactiveObject, IClosableViewModel, IRoutableViewModel
{
	#region IClosableViewModel/IRoutableViewModel
	public string UrlPathSegment => "collectiondownload";
	public IScreen HostScreen { get; }
	[Reactive] public partial bool IsVisible { get; set; }
	public RxCommandUnit CloseCommand { get; }
	#endregion

	[Reactive] public partial NexusModsCollectionData? Data { get; private set; }
	[Reactive] public partial bool IsCardView { get; set; }

	public ObservableCollectionExtended<NexusModsCollectionModData> Mods { get; }

	[ObservableAsProperty] public partial string? Heading { get; }
	[ObservableAsProperty] public partial Uri? AuthorAvatarUri { get; }
	[ObservableAsProperty] public partial bool HasAuthorAvatar { get; }
	[ObservableAsProperty] public partial bool IsGridView { get; }
	[ObservableAsProperty] public partial bool AnyEnabled { get; }
	[ObservableAsProperty] public partial bool AnyDisabled { get; }
	[ObservableAsProperty] public partial bool HasMods { get; }

	private readonly IObservable<bool> _hasModsObs;
	private readonly IObservable<bool> _anyEnabledObs;

	public RxCommandUnit? CancelCommand { get; set; }

	public void Load(NexusGraphCollectionRevision collectionRevision)
	{
		Data = NexusModsCollectionData.FromCollectionRevision(collectionRevision);

		Mods.Clear();

		if (Data?.Mods?.Count > 0)
		{
			Mods.AddRange(Data.Mods.Items);
		}

		this.RaisePropertyChanged(nameof(Mods));
	}

	private static string ToHeadingText(string? name, string? author)
	{
		if(name.IsValid())
		{
			if (author.IsValid())
			{
				return Loca.Window_CollectionDownloader_Heading_WithAuthor.SafeFormat($"Download {name} by {author}", name, author);
			}
			return Loca.Window_CollectionDownloader_Heading_ModOnly.SafeFormat($"Download {name}", name);
		}
		return Loca.Window_CollectionDownloader_Heading_Unknown;
	}

	[ReactiveCommand(CanExecute = nameof(_hasModsObs), OutputScheduler = "RxApp.MainThreadScheduler")]
	private void SelectAll(bool b)
	{
		foreach (var mod in Mods)
		{
			mod.IsSelected = b;
		}
	}

	[ReactiveCommand(OutputScheduler = "RxApp.MainThreadScheduler")]
	private void SetGridView() => IsCardView = false;

	[ReactiveCommand(OutputScheduler = "RxApp.MainThreadScheduler")]
	private void SetCardView() => IsCardView = true;

	[ReactiveCommand(OutputScheduler = "RxApp.MainThreadScheduler")]
	private void EnableAll() => SelectAll(true);

	[ReactiveCommand(OutputScheduler = "RxApp.MainThreadScheduler")]
	private void DisableAll() => SelectAll(false);

	[ReactiveCommand(CanExecute = nameof(_anyEnabledObs), OutputScheduler = "RxApp.TaskpoolScheduler")]
	private async Task Confirm()
	{
		var nexus = AppServices.NexusMods;
		var progress = ViewModelLocator.Progress;
		progress.Title = "Downloading mods...";
		await progress.StartAsync(async token =>
		{
			var modFiles = Mods.Where(x => x.IsSelected && x.ModFileData != null).Select(x => x.ModFileData).ToList();
			var results = await nexus.DownloadModFilesAsync(modFiles!, token);
		}, true);
	}

	public NexusModsCollectionDownloadWindowViewModel(IScreen? host = null)
	{
		HostScreen = host ?? AppLocator.Current.GetService<IScreen>()!;
		CloseCommand = this.CreateCloseCommand();

		Mods = [];

		var whenData = this.WhenAnyValue(x => x.Data).WhereNotNull();
		_headingHelper = whenData.Select(x => ToHeadingText(x.Name, x.Author)).ToUIProperty(this, x => x.Heading, Loca.Window_CollectionDownloader_Heading_Unknown);
		_authorAvatarUriHelper = whenData.Select(x => x.AuthorAvatarUrl).ToUIProperty(this, x => x.AuthorAvatarUri);
		_hasAuthorAvatarHelper = this.WhenAnyValue(x => x.AuthorAvatarUri).Select(x => x.IsValid()).ToUIProperty(this, x => x.HasAuthorAvatar);

		_isGridViewHelper = this.WhenAnyValue(x => x.IsCardView).Select(b => !b).ToUIProperty(this, x => x.IsGridView, true);

		_hasModsHelper = Mods.ToObservableChangeSet().CountChanged().Select(_ => Mods.Count > 0).ToUIProperty(this, x => x.HasMods);

		var modsConn = Mods.ToObservableChangeSet().AutoRefresh(x => x.IsSelected).ToCollection().Throttle(TimeSpan.FromMilliseconds(50)).ObserveOn(RxApp.MainThreadScheduler);
		_anyEnabledHelper = modsConn.Select(x => x.Any(x => x.IsSelected)).ToUIProperty(this, x => x.AnyEnabled, initialValue: false);
		_anyDisabledHelper = modsConn.Select(x => x.Any(x => !x.IsSelected)).ToUIProperty(this, x => x.AnyDisabled, initialValue: true);

		_hasModsObs = this.WhenAnyValue(x => x.HasMods);
		_anyEnabledObs = this.WhenAnyValue(x => x.AnyEnabled);
	}
}

public class CollectionDownloadWindowDesignViewModel : NexusModsCollectionDownloadWindowViewModel
{
	public CollectionDownloadWindowDesignViewModel() : base()
	{
		var mod1 = new NexusGraphMod()
		{
			PictureUrl = "https://staticdelivery.nexusmods.com/mods/3474/images/746/746-1691682009-457832810.png",
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now,
			Version = "1.13",
			Category = "Gameplay",
			ModId = 746
		};
		var mod2 = new NexusGraphMod()
		{
			PictureUrl = "https://staticdelivery.nexusmods.com/mods/3474/images/956/956-1692067257-2128087246.png",
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now,
			Version = "1.0.0.1",
			Category = "Gameplay"
		};
		var mod3 = new NexusGraphMod()
		{
			PictureUrl = "https://staticdelivery.nexusmods.com/mods/3474/images/522/522-1691008217-392937994.jpeg",
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now,
			Version = "3.0",
			Category = "Gameplay"
		};
		var user = new NexusGraphUser()
		{
			Name = "LaughingLeader",
			Avatar = "https://avatars.nexusmods.com/8743560/100",
		};
		var designData = new NexusGraphCollectionRevision()
		{
			AdultContent = false,
			CreatedAt = DateTimeOffset.Now,
			UpdatedAt = DateTimeOffset.Now,
			Collection = new NexusGraphCollection()
			{
				Name = "Test Collection",
				Summary = "A collection of various mods",
				TileImage = new NexusGraphCollectionImage()
				{
					Url = "https://media.nexusmods.com/d/0/d01c8b3d-4849-457f-8754-71ce4ee27b8b.webp",
					ThumbnailUrl = "https://media.nexusmods.com/d/0/t/small/d01c8b3d-4849-457f-8754-71ce4ee27b8b.webp"
				},
				User = user
			},
			ModFiles = new NexusGraphCollectionRevisionMod[3] {
				new(){ Optional = true, File = new NexusGraphModFile(){ SizeInBytes = 66920, Name = "Companion AI", Description = "This mod will give you 10 AI for you to choose from for any class or any role your companions may have. there are also 3 blank customizable AI for you to edit it yourself, just like in Baldur's Gate 1&amp;2 in case you want to.", Owner = new NexusGraphUser(){ Name = "TPadvance", Avatar = "https://avatars.nexusmods.com/3054620/100" }, Mod = mod1}},
				new(){ File = new NexusGraphModFile(){ SizeInBytes = 2773, Name = "Extra Warlock Spell Slots", Description = "This mod adds additional spell slots to the Warlock class.", Owner = new NexusGraphUser(){ Name = "Some1ellse", Avatar = "https://avatars.nexusmods.com/8049857/100" }, Mod = mod2}},
				new(){ File = new NexusGraphModFile(){ SizeInBytes = 1302, Name = "Carry Weight Increased - Up To Over 9000", Description = "Get ready for your extensive loot hoarding with plenty of options to bolster your carry weight limit. Ranges from a minor x1.5 increase all the way up to the quite legendary x9000!", Owner = new NexusGraphUser(){ Name = "Mharius", Avatar = "https://avatars.nexusmods.com/14200939/100" }, Mod = mod3}},
			}
		};
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			Load(designData);
			DivinityApp.Log($"Mods: {Mods.Count} / {Data?.Mods?.Count}");
		});
	}
}
