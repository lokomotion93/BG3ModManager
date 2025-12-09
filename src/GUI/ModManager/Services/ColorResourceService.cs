using Avalonia.Media;

using System;
using System.Collections.Generic;
using System.Text;

namespace ModManager.Services;

public class ColorResourceService : IColorResourceService
{
	private static readonly string _defaultFallback = "#FFFFFF";

	public string? GetColorHex(string name, string? fallback = null)
	{
		if(Application.Current?.TryFindResource(name, out var colorObj) == true && colorObj is Color color)
		{
			return color.ToString();
		}
		return fallback ?? _defaultFallback;
	}
}
