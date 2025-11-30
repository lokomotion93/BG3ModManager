using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModManager.ViewModels.Main;
public partial class FooterViewModel : ReactiveObject
{
	[Reactive] public partial bool IsNexusModsSectionExpanded { get; set; }
	[ObservableAsProperty] public partial string? NexusModsLimitsText { get; }
	[ObservableAsProperty] public partial Uri? NexusModsProfileBitmapUri { get; }
	[ObservableAsProperty] public partial bool HasNexusModsProfileAvatar { get; }

	public FooterViewModel(INexusModsService nexusModsService)
	{
		IsNexusModsSectionExpanded = true;

		_nexusModsLimitsTextHelper = nexusModsService.WhenLimitsChange.Throttle(TimeSpan.FromMilliseconds(50)).Select(x => x?.ToString() ?? string.Empty).ToUIProperty(this, x => x.NexusModsLimitsText, nexusModsService.ApiLimits.ToString());
		var whenNexusModsAvatar = nexusModsService.WhenAnyValue(x => x.ProfileAvatarUrl);
		_hasNexusModsProfileAvatarHelper = whenNexusModsAvatar.Select(Validators.IsValid).ToUIProperty(this, x => x.HasNexusModsProfileAvatar);
		_nexusModsProfileBitmapUriHelper = whenNexusModsAvatar.ToUIProperty(this, x => x.NexusModsProfileBitmapUri);
	}
}
