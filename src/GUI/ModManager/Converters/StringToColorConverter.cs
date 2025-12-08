using Avalonia.Data.Converters;
using Avalonia.Media;

using System.Globalization;
using System.Reflection;

namespace ModManager.Converters;

public class StringToColorConverter : IValueConverter
{
	private static readonly Dictionary<string, Color> _nameToColor;
	private static readonly Dictionary<Color, string> _colorToName;

	static StringToColorConverter()
	{
		_nameToColor = [];
		_colorToName = [];
		foreach(var prop in typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
		{
			var name = prop.Name;
			if(prop.GetValue(null) is Color color)
			{
				_nameToColor[name] = color;
				_colorToName[color] = name;
			}
		}
	}

	public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if(value is string colorStr)
		{
			if(_nameToColor.TryGetValue(colorStr, out var color))
			{
				return color;
			}
			else if(Color.TryParse(colorStr, out var parsedColor))
			{
				return parsedColor;
			}
		}
		return Colors.White;
	}

	public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if(value is Color color)
		{
			if (_colorToName.TryGetValue(color, out var str))
			{
				return str;
			}
			else
			{
				return color.ToString();
			}
		}
		return string.Empty;
	}
}