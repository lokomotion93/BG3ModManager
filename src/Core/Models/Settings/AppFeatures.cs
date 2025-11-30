using System.Reflection;

namespace ModManager.Models.Settings;

public partial class AppFeatures : ReactiveObject
{
	[Reactive] public partial bool ScriptExtender { get; set; }
	[Reactive] public partial bool GitHub { get; set; }
	[Reactive] public partial bool NexusMods { get; set; }
	[Reactive] public partial bool Modio { get; set; }

	private static readonly List<PropertyInfo> _props = typeof(AppFeatures)
		.GetRuntimeProperties()
		.Where(prop => Attribute.IsDefined(prop, typeof(ReactiveAttribute)))
		.ToList();

	public void ApplyDictionary(Dictionary<string, bool> dict)
	{
		foreach (var prop in _props)
		{
			if (dict.TryGetValue(prop.Name, out var b))
			{
				prop.SetValue(this, b);
			}
		}
	}

	public AppFeatures()
	{
		ScriptExtender = true;
		GitHub = true;
		NexusMods = true;
		Modio = true;
	}
}
