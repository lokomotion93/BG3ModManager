using NexusModsNET.DataModels;

using System.Reflection;

namespace ModManager.Models.NexusMods;

[DataContract]
public class NexusModsModData : ReactiveObject
{
	[JsonPropertyName("uuid"), DataMember]
	public string? UUID { get; set; }

	[JsonPropertyName("last_file_id"), Reactive, DataMember]
	public long LastFileId { get; set; }

	[JsonPropertyName("name"), DataMember]
	public string? Name { get; set; }

	[JsonPropertyName("summary"), DataMember]
	public string? Summary { get; set; }

	//[JsonPropertyName("description")]
	//public string? Description { get; set; }

	[JsonPropertyName("picture_url"), DataMember]
	public Uri? PictureUrl { get; set; }

	[JsonPropertyName("mod_id"), Reactive, DataMember]
	public long ModId { get; set; }

	[JsonPropertyName("category_id"), DataMember]
	public long CategoryId { get; set; }

	[JsonPropertyName("version"), DataMember]
	public string? Version { get; set; }

	[JsonPropertyName("endorsement_count"), DataMember]
	public long EndorsementCount { get; set; }

	[JsonPropertyName("created_timestamp"), DataMember]
	public long CreatedTimestamp { get; set; }

	[JsonPropertyName("updated_timestamp"), DataMember]
	public long UpdatedTimestamp { get; set; }

	[JsonPropertyName("author"), DataMember]
	public string? Author { get; set; }

	[JsonPropertyName("contains_adult_content"), DataMember]
	public bool ContainsAdultContent { get; set; }

	[JsonPropertyName("status"), DataMember]
	public string? Status { get; set; }

	[JsonPropertyName("available"), DataMember]
	public bool Available { get; set; }

	public void SetModVersion(NexusModFileVersionData info)
	{
		if (info.Success)
		{
			SetModVersion(info.ModId, info.FileId);
		}
	}

	public void SetModVersion(long modId, long fileId = -1)
	{
		if (ModId != modId)
		{
			ModId = modId;
		}

		if (fileId > LastFileId)
		{
			LastFileId = fileId;
		}
	}

	public void Update(NexusModsModData data)
	{
		this.SetFrom<NexusModsModData, JsonPropertyNameAttribute>(data);
		IsUpdated = true;
	}

	private static readonly IEnumerable<PropertyInfo> _lazySerializedProperties = typeof(NexusModsModData)
		.GetProperties(BindingFlags.Public | BindingFlags.Instance)
		.Where(prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>(true) != null);

	public void Update(NexusMod data)
	{
		var t = typeof(NexusMod);
		foreach (var prop in _lazySerializedProperties)
		{
			var nexusProp = t.GetProperty(prop.Name);
			if (nexusProp != null)
			{
				var value = nexusProp.GetValue(data);
				if (value != null)
				{
					prop.SetValue(this, value);
					this.RaisePropertyChanged(prop.Name);
				}
			}
		}
		IsUpdated = true;
	}

	[Reactive, JsonIgnore] public bool IsUpdated { get; set; }

	/// <summary>
	/// True if ModId is set.
	/// </summary>
	[Reactive, JsonIgnore] public bool IsEnabled { get; private set; }

	public NexusModsModData()
	{
		ModId = -1;
		LastFileId = -1;

		this.WhenAnyValue(x => x.ModId)
		.Select(x => x >= DivinityApp.NEXUSMODS_MOD_ID_START)
		.ObserveOn(RxApp.MainThreadScheduler)
		.BindTo(this, x => x.IsEnabled);
	}
}
