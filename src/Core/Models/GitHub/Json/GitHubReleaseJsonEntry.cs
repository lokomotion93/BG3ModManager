using System.Runtime.Serialization;

namespace ModManager.Models.GitHub.Json;

[DataContract]
public partial class GitHubReleaseJsonEntry : ReactiveObject
{
	[Reactive]
	[property: DataMember]
	public partial string? UUID { get; set; }

	[Reactive]
	[property: DataMember]
	public partial string? Version { get; set; }

	[Reactive]
	[property: DataMember]
	public partial string? DownloadUrl { get; set; }
}