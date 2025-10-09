using ModManager.Services;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModManager;

public static class EnumExtensions
{
	/// <summary>
	/// Get an enum's Description attribute value.
	/// </summary>
	public static string GetDescription(this Enum enumValue)
	{
		var member = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
		if (member != null)
		{
			if(member.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute descriptionAttribute)
			{
				return descriptionAttribute.Description ?? string.Empty;
			}
			else if(member.GetCustomAttribute<DisplayAttribute>() is DisplayAttribute displayAttribute && displayAttribute.Description.IsValid())
			{
				return displayAttribute.Description;
			}
		}
		return string.Empty;
	}

	/// <summary>
	/// Get an enum's Description attribute value.
	/// </summary>
	public static string GetName(this Enum enumValue)
	{
		var valueStr = enumValue.ToString();
		var member = enumValue.GetType().GetMember(valueStr).FirstOrDefault();
		if (member != null && member.GetCustomAttribute<DisplayAttribute>() is DisplayAttribute displayAttribute && displayAttribute.Name.IsValid())
		{
			return displayAttribute.Name;
		}
		return valueStr;
	}

	public static bool IsConfirmation(this InteractionMessageBoxType messageBoxType)
	{
		return messageBoxType.HasFlag(InteractionMessageBoxType.YesNo) || messageBoxType.HasFlag(InteractionMessageBoxType.Input);
	}

	public static T[] IndexToEnumArray<T>() where T : Enum
	{
		var enumType = typeof(T);
		var names = Enum.GetNames(enumType).ToList();
		T[] result = new T[names.Count];
		var i = 0;
		foreach(string name in names)
		{
			var value = (T)Enum.Parse(enumType, name, true);
			result[i] = value;
			i++;
		}
		return result;
	}

	public static Dictionary<T, int> EnumToIndexDict<T>() where T : Enum
	{
		var enumType = typeof(T);
		var names = Enum.GetNames(enumType).ToList();
		Dictionary<T, int> result = [];
		var i = 0;
		foreach (string name in names)
		{
			var value = (T)Enum.Parse(enumType, name, true);
			result.Add(value, i);
			i++;
		}
		return result;
	}
}
