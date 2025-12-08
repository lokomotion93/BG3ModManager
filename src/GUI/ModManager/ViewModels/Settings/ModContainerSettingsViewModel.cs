using Avalonia.Media;

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

	[Reactive] public partial double BorderThicknessPicker { get; set; }


	[ObservableAsProperty] private bool _hasTarget;

	public IconSettingsViewModel Icon { get; }

	public RxCommandUnit CloseCommand { get; }

	private CompositeDisposable? _openDisp;

	public void Open(ModContainer target)
	{
		Name = Loca.ModContainerSettingsView_Title.SafeFormat("{0} Settings", target.DisplayName ?? "Container");
		Target = target.Settings;

		if(Target.Icon != null)
		{
			Icon.Open(Target.Icon);
		}
		else
		{
			Icon.Clear();
		}

		Settings.SetFrom(Target);

		_openDisp = [];

		this.WhenAnyValue(x => x.BorderThicknessPicker).BindTo(Settings, x => x.BorderThickness).DisposeWith(_openDisp);

		IsVisible = true;
	}

	private readonly IObservable<bool> _hasTargetObs;

	[ReactiveCommand(CanExecute = nameof(_hasTargetObs))]
	public void Apply()
	{
		Target?.SetFrom(Settings);
		CloseCommand.Execute().Subscribe();
	}

	private void OnClose()
	{
		Target = null;
		_openDisp?.Dispose();
	}

	public ModContainerSettingsViewModel()
	{
		Name = "Container Settings";
		CloseCommand = this.CreateCloseCommand(invokeAction: OnClose);

		Settings = new();
		Icon = new();

		Settings.WhenAnyValue(x => x.BorderThickness).BindTo(this, x => x.BorderThicknessPicker);

		_hasTargetObs = this.WhenAnyValue(x => x.Target).Select(x => x != null);
		_hasTargetHelper = _hasTargetObs.ToUIProperty(this, x => x.HasTarget);
	}
}
