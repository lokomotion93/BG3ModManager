using DynamicData;

using ModManager.Util;

using NexusModsNET.DataModels.GraphQL.Types;

namespace ModManager.Models.NexusMods;

public partial class NexusModsCollectionData : ReactiveObject
{
	[Reactive] public partial bool HasAdultContent { get; set; }
	[Reactive] public partial string? Name { get; set; }
	[Reactive] public partial string? Author { get; set; }
	[Reactive] public partial string? Description { get; set; }
	[Reactive] public partial Uri? AuthorAvatarUrl { get; set; }
	[Reactive] public partial Uri? TileImageUrl { get; set; }
	[Reactive] public partial Uri? TileImageThumbnailUrl { get; set; }
	[Reactive] public partial DateTimeOffset CreatedAt { get; set; }
	[Reactive] public partial DateTimeOffset UpdatedAt { get; set; }

	public SourceList<NexusModsCollectionModData> Mods { get; }

	public NexusModsCollectionData()
	{
		Mods = new();
	}

	private static NexusModsCollectionModData ModFileToReactiveData(int index, NexusGraphCollectionRevisionMod mod)
	{
		return new NexusModsCollectionModData(mod)
		{
			Index = index
		};
	}

	public static NexusModsCollectionData FromCollectionRevision(NexusGraphCollectionRevision collectionRevision)
	{
		var collection = collectionRevision.Collection;
		var data = new NexusModsCollectionData()
		{
			HasAdultContent = collectionRevision.AdultContent,
			Name = collection.Name,
			Description = collection.Summary,
			Author = collection.User.Name,
			TileImageUrl = StringUtils.StringToUri(collection.TileImage?.Url),
			TileImageThumbnailUrl = StringUtils.StringToUri(collection.TileImage?.ThumbnailUrl),
			CreatedAt = collectionRevision.CreatedAt,
			UpdatedAt = collectionRevision.UpdatedAt
		};
		if(collection.User?.Avatar != null)
		{
			data.AuthorAvatarUrl = new Uri(collection.User.Avatar);
		}
		var mods = Enumerable.Range(0, collectionRevision.ModFiles.Length).Select(i => ModFileToReactiveData(i, collectionRevision.ModFiles[i]));
		data.Mods.AddRange(mods);

		return data;
	}
}
