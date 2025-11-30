namespace ModManager.ViewModels;

public partial class DownloadActivityBarViewModel : ReactiveObject, IClosableViewModel
{
	#region IClosableViewModel
	[Reactive] public partial bool IsVisible { get; set; }
	public RxCommandUnit CloseCommand { get; }
	#endregion

	[Reactive] private double ProgressValue { get; set; }
	[Reactive] private string? ProgressText { get; set; }
	[Reactive] public partial bool IsActive { get; private set; }
	[Reactive] public partial Action? CancelAction { get; set; }

	[ObservableAsProperty] public partial double CurrentValue { get; }
	[ObservableAsProperty] public partial string? CurrentText { get; }
	[ObservableAsProperty] public partial bool IsAnimating { get; }

	public void UpdateProgress(double value, string? text = null)
	{
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			ProgressValue = value;
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
				ProgressValue = 0d;
				ProgressText = "";
				IsActive = false;
			});
		}
	}

	private double Clamp(double value)
	{
		return Math.Min(100, Math.Max(0, value));
	}

	public DownloadActivityBarViewModel()
	{
		CloseCommand = this.CreateCloseCommand(invokeAction: Cancel);

		_currentValueHelper = this.WhenAnyValue(x => x.ProgressValue).Select(Clamp).ToUIProperty(this, x => x.CurrentValue, 0d);
		_currentTextHelper = this.WhenAnyValue(x => x.ProgressText).ToUIProperty(this, x => x.CurrentText, "");
		_isAnimatingHelper = this.WhenAnyValue(x => x.CurrentValue, x => x < 100).ToUIProperty(this, x => x.IsAnimating, true);

		this.WhenAnyValue(x => x.CurrentText, x => x.CurrentValue).Select(x => x.Item1.IsValid() || x.Item2 > 0).BindTo(this, x => x.IsActive);
	}
}
