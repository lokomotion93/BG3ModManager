using DynamicData;

using LSLib.LS;

using ModManager.Extensions;

using System.Reflection;

namespace ModManager.Models.Mod.Game;
public partial class ToolkitProjectMetaData : ReactiveObject
{
	[Reactive] public partial string? GameProject { get; set; }
	[Reactive] public partial string? Module { get; set; }
	[Reactive] public partial string? Name { get; set; }
	[Reactive] public partial string? UUID { get; set; }
	[Reactive] public partial bool UpdatedDependencies { get; set; }
	//[Reactive] public partial SourceList<string> Categories { get; set; }

	public string? FilePath;

	private static readonly NodeSerializationSettings nodeSerializationSettings = new();

	private static object? TryGetAttribute(string property, Node node, Type propType)
	{
		if (node.Attributes.TryGetValue(property, out var nodeAttribute))
		{
			if (propType == typeof(string))
			{
				return nodeAttribute.AsString(nodeSerializationSettings);
			}
			else
			{
				return nodeAttribute.Value;
			}
		}
		return null;
	}

	private static readonly PropertyInfo[] _props = typeof(ToolkitProjectMetaData).GetProperties(BindingFlags.Instance | BindingFlags.Public);

	public static ToolkitProjectMetaData FromResource(Resource res)
	{
		var meta = new ToolkitProjectMetaData();
		if (res != null && res.Regions.Values.FirstOrDefault() is Region region)
		{
			foreach (var prop in _props)
			{
				var value = TryGetAttribute(prop.Name, region, prop.PropertyType);
				if (value != null)
				{
					prop.SetValue(meta, value);
				}
			}
		}
		return meta;
	}
}
