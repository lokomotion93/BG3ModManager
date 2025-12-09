using System;
using System.Collections.Generic;
using System.Text;

namespace ModManager.Services;

public interface IColorResourceService
{
	string? GetColorHex(string name, string? fallback = null);
}
