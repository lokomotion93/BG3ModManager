using DynamicData;

using ModManager.Extensions;
using ModManager.Json;
using ModManager.Models.Interfaces;
using ModManager.Models.Mod;
using ModManager.Models.Settings;
using ModManager.Util;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace ModManager;

public static class ModelExtensions
{
	private static readonly Dictionary<string, PropertyInfo[]> _typeDataMemberProperties;
	private static readonly Dictionary<Type, Dictionary<string, UpdatePropertyCallback>> _updatePropertyActions;

	public delegate void UpdatePropertyCallback(object target, object? existing, object? newValue);

	private static void AddTypeProperties(Type type)
	{
		var name = type.Name;
		var props = type.GetRuntimeProperties().Where(prop => Attribute.IsDefined(prop, typeof(DataMemberAttribute))).ToArray();
		_typeDataMemberProperties.Add(name, props);
	}

	private static bool TryGetPropertyUpdater(Type dataType, string property, [NotNullWhen(true)] out UpdatePropertyCallback? action)
	{
		action = null;
		if(_updatePropertyActions.TryGetValue(dataType, out var actions) && actions.TryGetValue(property, out var propAction))
		{
			action = propAction;
			return true;
		}
		return false;
	}

	static ModelExtensions()
	{
		_typeDataMemberProperties = [];

		AddTypeProperties(typeof(ModManagerSettings));
		AddTypeProperties(typeof(UserModConfig));
		AddTypeProperties(typeof(ScriptExtenderSettings));
		AddTypeProperties(typeof(ScriptExtenderUpdateConfig));
		AddTypeProperties(typeof(ModManagerContainerSettings));

		_updatePropertyActions = [];
		_updatePropertyActions.Add(typeof(UserModConfig), new(){
			[nameof(UserModConfig.Mods)] = UserModConfig.UpdateMods
		});
	}

	private static PropertyInfo[]? GetDataMemberProperties(Type type)
	{
		if (!_typeDataMemberProperties.ContainsKey(type.Name)) AddTypeProperties(type);

		if (_typeDataMemberProperties.TryGetValue(type.Name, out var result))
		{
			return result;
		}
		return null;
	}

	public static void SetToDefault(this ReactiveObject model)
	{
		var props = TypeDescriptor.GetProperties(model.GetType());
		foreach (PropertyDescriptor pr in props)
		{
			if (pr.CanResetValue(model))
			{
				pr.ResetValue(model);
				model.RaisePropertyChanged(pr.Name);
			}
		}
	}

	public static void SetFrom<T>(this T target, T from) where T : ReactiveObject
	{
		var props = TypeDescriptor.GetProperties(target.GetType());
		foreach (PropertyDescriptor pr in props)
		{
			var value = pr.GetValue(from);
			if (value != null)
			{
				pr.SetValue(target, value);
				target.RaisePropertyChanged(pr.Name);
			}
		}
	}

	public static void SetFrom<T, T2>(this T target, T from) where T : ReactiveObject where T2 : Attribute
	{
		var attributeType = typeof(T2);
		var props = typeof(T).GetRuntimeProperties().Where(prop => Attribute.IsDefined(prop, attributeType)).ToList();
		foreach (var pr in props)
		{
			var value = pr.GetValue(from);
			if (value != null)
			{
				pr.SetValue(target, value);
				target.RaisePropertyChanged(pr.Name);
			}
		}
	}

	public static void SetFromDataMember<T>(this T target, T from) where T : ReactiveObject => SetFrom<T, DataMemberAttribute>(target, from);

	public static bool Save<T>(this T data, out Exception? error) where T : ISerializableSettings
	{
		error = null;
		try
		{
			var directory = data.GetDirectory();
			if(directory.IsValid())
			{
				Directory.CreateDirectory(directory);
				if(directory.IsExistingDirectory())
				{
					var filePath = Path.Join(directory, data.FileName);
					data.ModManagerVersion = Locator.Current.GetService<IEnvironmentService>()?.AppVersion;
					var contents = JsonSerializer.Serialize(data, data.GetType(), JsonUtils.DefaultSerializerSettings);
					if(!data.SkipEmpty)
					{
						File.WriteAllText(filePath, contents);
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(contents) && !contents.Trim().Equals("{}"))
						{
							File.WriteAllText(filePath, contents);
						}
						else
						{
							DivinityApp.Log("Output file would be empty, so we're skipping writing it.");
							File.Delete(filePath);
						}
					}
					return true;
				}
			}
			throw new DirectoryNotFoundException($"Failed to find settings ({data.FileName}) output directory at '{directory}'");
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error saving {data.FileName}:\n{ex}");
			error = ex;
		}
		return false;
	}

	public static bool Load<T>(this T data, out Exception? error, bool saveIfNotFound = true) where T : ISerializableSettings
	{
		error = null;
		try
		{
			var directory = data.GetDirectory();
			if(saveIfNotFound && !directory.IsExistingDirectory() && directory.IsValid())
			{
				Directory.CreateDirectory(directory);
			}
			if (directory.IsExistingDirectory())
			{
				var filePath = Path.Join(directory, data.FileName);
				if (filePath.IsExistingFile())
				{
					var outputType = data.GetType();
					var text = File.ReadAllText(filePath);
					var settings = JsonSerializer.Deserialize(text, outputType, JsonUtils.DefaultSerializerSettings);
					if (settings != null)
					{
						var props = GetDataMemberProperties(outputType);
						if (props != null)
						{
							foreach(var prop in props)
							{
								var value = prop.GetValue(settings);
								var existing = prop.GetValue(data);
								if (value != null && existing != value)
								{
									data.RaisePropertyChanging(prop.Name);
									if(TryGetPropertyUpdater(outputType, prop.Name, out var action))
									{
										action.Invoke(data, existing, value);
									}
									else
									{
										prop.SetValue(data, value);
									}
									data.RaisePropertyChanged(prop.Name);
								}
							}
						}
						/*var props = TypeDescriptor.GetProperties(outputType);
						foreach (PropertyDescriptor pr in props)
						{
							var value = pr.GetValue(settings);
							if (value != null)
							{
								pr.SetValue(data, value);
								data.RaisePropertyChanged(pr.Name);
							}
						}*/
						return true;
					}
				}
				else if (saveIfNotFound)
				{
					if (!Save(data, out var saveError))
					{
						error = saveError;
						return false;
					}
					return true;
				}
			}
			else
			{
				throw new DirectoryNotFoundException($"Failed to find settings ({data.FileName}) output directory at '{directory}'");
			}
		}
		catch (Exception ex)
		{
			error = ex;
			DivinityApp.Log($"Error saving {data.FileName}:\n{ex}");
		}
		return false;
	}

	public static IEnumerable<T2> ForEachNested<T, T2>(this INested<T, T2> nested) where T : IList<T2>
	{
		foreach(var entry in nested.Children)
		{
			yield return entry;
			if(entry is INested<T, T2> subNested)
			{
				foreach(var subEntry in subNested.ForEachNested())
				{
					yield return subEntry;
				}
			}
		}
	}
}
