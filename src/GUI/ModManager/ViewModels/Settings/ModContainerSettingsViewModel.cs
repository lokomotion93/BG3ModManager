using Humanizer;

using Material.Icons;

using ModManager.Models.Mod;

using System;
using System.Collections.Generic;
using System.Text;

namespace ModManager.ViewModels.Settings;

public partial class ModContainerSettingsViewModel : ReactiveObject, IClosableViewModel
{
	public ModContainerSettings Settings { get; set; }
	public ModContainerSettings? Target { get; set; }

	[Reactive] public partial string Name { get; set; }
	[Reactive] public partial bool IsVisible { get; set; }

	[Reactive] public partial string? IconKind { get; set; }
	[Reactive] public partial string? IconPath { get; set; }
	[Reactive] public partial string? IconForegroundColor { get; set; }
	[Reactive] public partial string? IconBackgroundColor { get; set; }
	[Reactive] public partial string? IconBorderColor { get; set; }
	[Reactive] public partial string? IconBorderThickness { get; set; }


	[ObservableAsProperty] private bool _hasTarget;

	public RxCommandUnit CloseCommand { get; }

	public void Open(ModContainer target)
	{
		Name = Loca.ModContainerSettingsView_Title.SafeFormat("{0} Settings", target.DisplayName ?? "Container");
		Target = target.Settings;

		if(Target.Icon != null)
		{
			IconKind = target.Icon.Kind;
			IconPath = target.Icon.Path;
			IconForegroundColor = target.Icon.ForegroundColor;
			IconBackgroundColor = target.Icon.BackgroundColor;
			IconBorderColor = target.Icon.BorderColor;
			IconBorderThickness = target.Icon.BorderThickness;
		}

		Settings.SetFrom(Target);
		IsVisible = true;
	}

	private readonly IObservable<bool> _hasTargetObs;

	[ReactiveCommand(CanExecute = nameof(_hasTargetObs))]
	public void Apply()
	{
		Target?.SetFrom(Settings);
		CloseCommand.Execute().Subscribe();
	}

	public ModContainerSettingsViewModel()
	{
		Name = "Container Settings";
		CloseCommand = this.CreateCloseCommand(invokeAction: () => Target = null);

		Settings = new();

		_hasTargetObs = this.WhenAnyValue(x => x.Target).Select(x => x != null);
		_hasTargetHelper = _hasTargetObs.ToUIProperty(this, x => x.HasTarget);
	}
}
