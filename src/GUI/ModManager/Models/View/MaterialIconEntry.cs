using Material.Icons;

using ModManager.Models.Interfaces;

namespace ModManager.Models.View;

public class MaterialIconEntry(MaterialIconKind kind, string? name = null) : INamedEntry
{
	public MaterialIconKind Kind { get; } = kind;
	public string Name { get; } = name ?? kind.ToString();
}
