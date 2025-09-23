using ModManager.Locale;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ModManager.Enums.Extender;

[JsonConverter(typeof(JsonStringEnumConverter<ExtenderUpdateChannel>))]
public enum ExtenderUpdateChannel
{
	[Display(Name = nameof(Resources.ExtenderUpdateChannel_Release), Description = nameof(Resources.ExtenderUpdateChannel_Release_ToolTip))]
	[Description("Release")]
	Release,
	[Display(Name = nameof(Resources.ExtenderUpdateChannel_Devel), Description = nameof(Resources.ExtenderUpdateChannel_Devel_ToolTip))]
	[Description("Devel")]
	Devel,
	[Display(Name = nameof(Resources.ExtenderUpdateChannel_Nightly), Description = nameof(Resources.ExtenderUpdateChannel_Nightly_ToolTip))]
	[Description("Nightly")]
	Nightly
}