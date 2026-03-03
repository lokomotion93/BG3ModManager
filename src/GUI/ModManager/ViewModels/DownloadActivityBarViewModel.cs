namespace ModManager.ViewModels;

public partial class DownloadActivityBarViewModel : ReactiveObject, IClosableViewModel
{
	#region IClosableViewModel
	[Reactive] public partial bool IsVisible { get; set; }
	public RxCommandUnit CloseCommand { get; }
	#endregion

	[Reactive] internal partial int CurrentStep { get; set; }
	[Reactive] internal partial int TotalSteps { get; set; }
	[Reactive] internal partial string? ProgressText { get; set; }

	[Reactive] public partial bool IsActive { get; private set; }
	[Reactive] public partial Action? CancelAction { get; set; }

	[ObservableAsProperty] public partial double Value { get; }
	[ObservableAsProperty] public partial double Maximum { get; }
	[ObservableAsProperty] public partial string? CurrentText { get; }
	[ObservableAsProperty] public partial bool IsAnimating { get; }

	public void SetProgress(int totalSteps, int currentStep = 0)
	{
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			CurrentStep = currentStep;
			TotalSteps = totalSteps;
		});
	}

	public void UpdateProgress(int stepAmount = 1, string? text = null)
	{
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			CurrentStep += stepAmount;
			if (text.IsValid())
			{
				ProgressText = text;
			}
		});
	}

	public void Cancel()
	{
		if (CancelAction != null)
		{
			CancelAction.Invoke();
		}
		else if (ProgressText.IsValid())
		{
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				CurrentStep = 0;
				TotalSteps = 0;
				ProgressText = "";
				IsActive = false;
			});
		}
	}

	private static double Clamp(int value, int max)
	{
		return Math.Min(max, Math.Max(0d, value));
	}

	public DownloadActivityBarViewModel(INexusModsService nexusModsService)
	{
		CloseCommand = this.CreateCloseCommand(invokeAction: Cancel);

		_valueHelper = this.WhenAnyValue(x => x.CurrentStep, x => x.TotalSteps, Clamp).ToUIProperty(this, x => x.Value, 0d);
		_maximumHelper = this.WhenAnyValue(x => x.TotalSteps, x => (double)x).ToUIProperty(this, x => x.Maximum, 0d);
		_currentTextHelper = this.WhenAnyValue(x => x.ProgressText).ToUIProperty(this, x => x.CurrentText, "");
		_isAnimatingHelper = this.WhenAnyValue(x => x.Value, x => x.Maximum, (a,b) => a < b).ToUIProperty(this, x => x.IsAnimating, true);

		this.WhenAnyValue(x => x.CurrentText, x => x.Value).Select(x => x.Item1.IsValid() || x.Item2 > 0).BindTo(this, x => x.IsActive);

		nexusModsService.WhenAnyValue(x => x.DownloadProgressMax, x => x.CanCancel)
		.ObserveOn(RxApp.MainThreadScheduler)
		.Subscribe(x =>
		{
			SetProgress(x.Item1);
			if (x.Item2)
			{
				CancelAction = () => nexusModsService.CancelDownloads();
			}
			else
			{
				CancelAction = null;
			}
		});

		nexusModsService.WhenAnyValue(x => x.DownloadProgressCurrent, x => x.DownloadProgressText)
		.ObserveOn(RxApp.MainThreadScheduler)
		.Subscribe(x =>
		{
			UpdateProgress(x.Item1, x.Item2);
		});
	}
}

public class DesignDownloadActivityBarViewModel : DownloadActivityBarViewModel
{
	public DesignDownloadActivityBarViewModel() : base(AppServices.NexusMods)
	{
		//SetProgress(100, 50);
		CurrentStep = 50;
		TotalSteps = 100;
	}
}