using NexusModsNET.DataModels.GraphQL.Types;

namespace ModManager.Models.NexusMods;

public record struct NexusModsDownloadedFile(string FilePath, NexusGraphModFile Mod);
public record struct NexusModsDownloadResults(bool Success, List<NexusModsDownloadedFile> DownloadedFiles);
