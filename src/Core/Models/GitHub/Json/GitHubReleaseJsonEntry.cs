using System.Runtime.Serialization;

namespace ModManager.Models.GitHub.Json;

[DataContract]
public partial class GitHubReleaseJsonEntry : ReactiveObject
{
	[Reactive, DataMember] public partial string? UUID { get; set; }
	[Reactive, DataMember] public partial string? Version { get; set; }
	[Reactive, DataMember] public partial string? DownloadUrl { get; set; }
}