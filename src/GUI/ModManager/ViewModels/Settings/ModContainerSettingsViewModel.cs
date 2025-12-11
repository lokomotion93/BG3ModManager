using Avalonia.Media;

using Humanizer;

using Material.Icons;

using ModManager.Models.Mod;
using ModManager.Styling;

using System;
using System.Collections.Generic;
using System.Text;

namespace ModManager.ViewModels.Settings;

public partial class ModContainerSettingsViewModel : ReactiveObject, IClosableViewModel
{
	public ModContainerSettings Settings { get; set; }

	[Reactive] public partial bool IsVisible { get; set; }
	[Reactive] public partial double BorderThicknessPicker { get; set; }
	[Reactive] public partial ModContainerSettings? Target { get; set; }


	[ObservableAsProperty] private bool _hasTarget;

	[ObservableAsProperty] private IBrush? _foregroundColorBrush;
	[ObservableAsProperty] private IBrush? _backgroundColorBrush;
	[ObservableAsProperty] private IBrush? _borderColorBrush;
	[ObservableAsProperty] private Thickness _borderThickness;

	public IconSettingsViewModel Icon { get; }

	public RxCommandUnit CloseCommand { get; }

	private CompositeDisposable? _openDisp;

	private ModContainer? _targetContainer;

	public void Open(ModContainer target)
	{
		_targetContainer = target;
		Target = target.Settings;

		if(Target.Icon != null)
		{
			Icon.Open(Target.Icon);
			if (!Icon.IsExpanded) Icon.IsExpanded = true;
		}
		else
		{
			Icon.Clear();
		}

		Settings.SetFromDataMember(Target);
		Settings.DisplayName = Target.DisplayName;

		_openDisp = [];

		this.WhenAnyValue(x => x.BorderThicknessPicker).BindTo(Settings, x => x.BorderThickness).DisposeWith(_openDisp);

		IsVisible = true;
	}

	private readonly IObservable<bool> _hasTargetObs;

	[ReactiveCommand(CanExecute = nameof(_hasTargetObs), OutputScheduler = "RxApp.MainThreadScheduler")]
	public void Apply()
	{
		if(Target != null)
		{
			Target.SetFromDataMember(Settings);
			Icon.Apply(Target);
		}
		
		if(_targetContainer != null && !_targetContainer.IsActive)
		{
			ViewModelLocator.ModOrder.UpdateInactiveModsConfig();
		}
		CloseCommand.Execute().Subscribe();
	}

	private void OnClose()
	{
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			Target = null;
			Icon.ClearImageCommand.Execute().Subscribe();
			_openDisp?.Dispose();
		});
	}

	public ModContainerSettingsViewModel()
	{
		CloseCommand = this.CreateCloseCommand(invokeAction: OnClose);

		Settings = new();
		Icon = new();

		Settings.WhenAnyValue(x => x.BorderThickness).BindTo(this, x => x.BorderThicknessPicker);

		_hasTargetObs = this.WhenAnyValue(x => x.Target).Select(x => x != null);
		_hasTargetHelper = _hasTargetObs.ToUIProperty(this, x => x.HasTarget);

		var defaultForegroundBrush = ColorBrushCache.GetResourceBrush("SukiText");
		var defaultThickness = new Thickness(0);

		_foregroundColorBrushHelper = Settings.WhenAnyValue(x => x.ForegroundColor, x => ColorBrushCache.GetBrush(x, defaultForegroundBrush))
			.ToUIProperty(this, x => x.ForegroundColorBrush, defaultForegroundBrush);

		_backgroundColorBrushHelper = Settings.WhenAnyValue(x => x.BackgroundColor, x => ColorBrushCache.GetBrush(x, defaultForegroundBrush))
			.ToUIProperty(this, x => x.BackgroundColorBrush, defaultForegroundBrush);

		_borderColorBrushHelper = Settings.WhenAnyValue(x => x.BorderColor, x => ColorBrushCache.GetBrush(x, defaultForegroundBrush))
			.ToUIProperty(this, x => x.BorderColorBrush, defaultForegroundBrush);

		_borderThicknessHelper = Settings.WhenAnyValue(x => x.BorderThickness)
			.WhereNotNull()
			.ValueOrFallback(Thickness.Parse, defaultThickness)
			.ToUIProperty(this, x => x.BorderThickness, defaultThickness);
	}
}

public class DesignModContainerSettingsViewModel : ModContainerSettingsViewModel
{
	public DesignModContainerSettingsViewModel() : base()
	{
		Settings.DisplayName = "Test Mods";
		Settings.Description = "This is a container with various test mods";
		Settings.ForegroundColor = "#00FF00";
		Settings.BackgroundColor = "#9900CCCC";
		Settings.BorderColor = "#44FF0000";
		Settings.BorderThickness = "2";
		Icon.IsExpanded = true;
		Icon.Open(new DesignIconSettingsViewModel().Settings);
	}
}