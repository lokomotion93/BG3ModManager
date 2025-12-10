using ModManager.Json;

using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace ModManager.Models.Mod;

[DataContract]
public partial class ModConfig : ReactiveObject, IObjectWithId
{
	public static string FileName => "ModManagerConfig.json";
	/// <summary>
	/// The mod UUID or FileName (override paks) associated with this config.
	/// </summary>
	public bool IsLoaded { get; set; }
	public string Id { get; set; }

	[Reactive]
	[DataMember]
	public partial string? Notes { get; set; }

	[Reactive]
	[DataMember]
	public partial string? GitHub { get; set; }

	[Reactive]
	[DataMember]
	public partial long NexusModsId { get; set; }

	[Reactive]
	[DataMember]
	public partial string? ModioId { get; set; }

	[ObservableAsProperty] public partial string? GitHubAuthor { get; }
	[ObservableAsProperty] public partial string? GitHubRepository { get; }


	[GeneratedRegex("^.*/([^/]+)/([^/]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
	private static partial Regex GitHubUrlPattern();

	private static readonly Regex _githubUrlPattern = GitHubUrlPattern();

	public static ValueTuple<string, string> GitHubUrlToParts(string? url)
	{
		if (url.IsValid() && !url.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
		{
			var match = _githubUrlPattern.Match(url);
			if (match.Success)
			{
				var author = match.Groups[1]?.Value ?? string.Empty;
				var repo = match.Groups[2]?.Value ?? string.Empty;
				return (author, repo);
			}
		}
		return (string.Empty, string.Empty);
	}

	[JsonConstructor]
	public ModConfig() : this(string.Empty)
	{

	}

	public ModConfig(string id)
	{
		Id = id;
		var parseGitHubUrl = this.WhenAnyValue(x => x.GitHub).Select(GitHubUrlToParts);
		_gitHubAuthorHelper = parseGitHubUrl.Select(x => x.Item1).ToProperty(this, x => x.GitHubAuthor, string.Empty, false, RxApp.MainThreadScheduler);
		_gitHubRepositoryHelper = parseGitHubUrl.Select(x => x.Item2).ToProperty(this, x => x.GitHubRepository, string.Empty, false, RxApp.MainThreadScheduler);
	}
}
