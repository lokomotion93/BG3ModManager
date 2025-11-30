using ModManager.Util;

using NexusModsNET.DataModels.GraphQL.Types;

namespace ModManager.Models.NexusMods;

public partial class NexusModsCollectionModData : ReactiveObject
{
	public NexusGraphModFile? ModFileData { get; }

	[Reactive] public partial int Index { get; set; }
	[Reactive] public partial string? Name { get; set; }
	[Reactive] public partial string? Author { get; set; }
	[Reactive] public partial string? Summary { get; set; }
	[Reactive] public partial string? Description { get; set; }
	[Reactive] public partial string? Version { get; set; }
	[Reactive] public partial string? Category { get; set; }
	[Reactive] public partial long SizeInBytes { get; set; }
	[Reactive] public partial Uri? AuthorAvatarUrl { get; set; }
	[Reactive] public partial Uri? ImageUrl { get; set; }
	[Reactive] public partial DateTimeOffset CreatedAt { get; set; }
	[Reactive] public partial DateTimeOffset UpdatedAt { get; set; }
	[Reactive] public partial bool IsOptional { get; set; }

	//UI-related properties
	[Reactive] public partial bool IsSelected { get; set; }

	[ObservableAsProperty] public partial string? SizeText { get; }
	[ObservableAsProperty] public partial string? AuthorDisplayText { get; }
	[ObservableAsProperty] public partial string? CreatedDateText { get; }
	[ObservableAsProperty] public partial string? UpdatedDateText { get; }
	public string? NexusModsURL { get; }
	public string? NexusModsURLDisplayText { get; }
	[ObservableAsProperty] public partial bool DescriptionVisibility { get; }
	[ObservableAsProperty] public partial bool AuthorAvatarVisibility { get; }
	[ObservableAsProperty] public partial bool ImageVisibility { get; }


	public NexusModsCollectionModData(NexusGraphCollectionRevisionMod mod)
	{
		IsSelected = true;
		var modFile = mod.File;
		ModFileData = modFile;

		if(ModFileData != null)
		{
			Name = ModFileData.Name;
			Summary = ModFileData.Mod.Summary;
			Description = ModFileData.Description;
			Author = ModFileData.Owner?.Name;
			if (ModFileData.Owner?.Avatar != null) AuthorAvatarUrl = new Uri(ModFileData.Owner.Avatar);
			ImageUrl = StringUtils.StringToUri(ModFileData.Mod.PictureUrl);
			CreatedAt = ModFileData.Mod.CreatedAt;
			UpdatedAt = ModFileData.Mod.UpdatedAt;
			Version = ModFileData.Mod.Version;
			SizeInBytes = ModFileData.SizeInBytes;
			Category = ModFileData.Mod.Category;
		}

		IsOptional = mod.Optional;

		NexusModsURL = $"https://www.nexusmods.com/{DivinityApp.NEXUSMODS_GAME_DOMAIN}/mods/{modFile?.ModId}";
		NexusModsURLDisplayText = $"/{DivinityApp.NEXUSMODS_GAME_DOMAIN}/mods/{modFile?.ModId}";

		_sizeTextHelper = this.WhenAnyValue(x => x.SizeInBytes).Select(StringUtils.BytesToString).ToUIProperty(this, x => x.SizeText);
		_authorDisplayTextHelper = this.WhenAnyValue(x => x.Author).Select(x => $"Created by {x}").ToUIProperty(this, x => x.AuthorDisplayText);

		_descriptionVisibilityHelper = this.WhenAnyValue(x => x.Description).Select(Validators.IsValid).ToUIProperty(this, x => x.DescriptionVisibility);
		_imageVisibilityHelper = this.WhenAnyValue(x => x.ImageUrl).Select(Validators.IsValid).ToUIProperty(this, x => x.ImageVisibility);
		_authorAvatarVisibilityHelper = this.WhenAnyValue(x => x.AuthorAvatarUrl).Select(Validators.IsValid).ToUIProperty(this, x => x.AuthorAvatarVisibility);

		_createdDateTextHelper = this.WhenAnyValue(x => x.CreatedAt).Select(PropertyConverters.DateToString).ToUIProperty(this, x => x.CreatedDateText);
		_updatedDateTextHelper = this.WhenAnyValue(x => x.UpdatedAt).Select(PropertyConverters.DateToString).ToUIProperty(this, x => x.UpdatedDateText);
	}
}
