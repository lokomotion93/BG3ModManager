using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Humanizer;

using ModManager.ViewModels;
using ModManager.ViewModels.Main;

using ReactiveUI;

using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;

using TextCopy;

namespace ModManager.Windows;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
	private readonly SukiToastManager _toastManager;
	private readonly SukiDialogManager _dialogManager;

	private ISukiDialog? _lastDialog = null;

	private void DismissLastDialog()
	{
		if (_lastDialog is not null)
		{
			_dialogManager.TryDismissDialog(_lastDialog);
			_lastDialog = null;
		}
	}

	public void ShowSukiDialog(object? content, bool showCardBehind = true, bool allowBackgroundClose = false)
	{
		DismissLastDialog();

		RxApp.MainThreadScheduler.Schedule(() =>
		{
			if (content is string message)
			{
				var dialogContent = new TextBlock { Text = message };
				var dialog = _dialogManager.CreateDialog().WithContent(dialogContent);
				_lastDialog = dialog.Dialog;
				dialog.SetCanDismissWithBackgroundClick(allowBackgroundClose);
				dialog.TryShow();
			}
			else if (content is ReactiveObject viewModel)
			{
				var dialog = _dialogManager.CreateDialog().WithViewModel(x =>
				{
					x.CanDismissWithBackgroundClick = allowBackgroundClose;
					x.ViewModel = content;
					if (content is IDialogViewModel dialogVM)
					{
						dialogVM.Dialog = x;
					}
					return content;
				});
				_lastDialog = dialog.Dialog;
				dialog.TryShow();
			}

			var borderDialog = DialogHost.GetTemplateChildren().FirstOrDefault(x => x.GetType() == typeof(Border));
			borderDialog?.Opacity = (showCardBehind ? 1 : 0);
		});
	}

	public void BringToFront()
	{
		Dispatcher.UIThread.Post(Activate);
	}

	private async Task HandleShowMessageBox(IInteractionContext<ShowMessageBoxRequest, MessageBoxResult> context)
	{
		var result = await Observable.StartAsync(async () =>
		{
			DismissLastDialog();
			var data = context.Input;
			var dialogVM = ViewModelLocator.MessageBox;
			dialogVM.Open(data);

			var isConfirmation = data.MessageBoxType.IsConfirmation();

			ShowSukiDialog(dialogVM, true, !isConfirmation);
			if (!isConfirmation)
			{
				return new(true, null);
			}
			else
			{
				return await dialogVM.WaitForResult();
			}
		}, RxApp.MainThreadScheduler);
		context.SetOutput(result);
	}

	public MainWindow()
	{
		InitializeComponent();

		_toastManager = new();
		_dialogManager = new();

		this.WhenActivated(d =>
		{
			DialogHost.Manager = _dialogManager;
			DialogHost.GetObservable(SukiDialogHost.IsDialogOpenProperty).BindTo(ViewModelLocator.MessageBox, x => x.IsVisible);
			ToastHost.Manager = _toastManager;

			this.OneWayBind(ViewModel, x => x.Router, x => x.ViewHost.Router);

			Dispatcher.UIThread.InvokeAsync(() => ViewModel.OnViewActivated(this), DispatcherPriority.Background);

			var interactions = AppServices.Interactions;

			interactions.ShowMessageBox.RegisterHandler(HandleShowMessageBox);

			interactions.PickMods.RegisterHandler(context =>
			{
				return Observable.StartAsync(async () =>
				{
					DismissLastDialog();
					var data = context.Input;
					var dialogVM = ViewModelLocator.ModPicker;
					dialogVM.Open(data);

					ShowSukiDialog(dialogVM, true, false);
					var result = await dialogVM.WaitForResult();
					context.SetOutput(result);
				}, RxApp.MainThreadScheduler);
			});

			interactions.ShowAlert.RegisterHandler(async context =>
			{
				var data = context.Input;
				var title = data.Title;
				var duration = data.Timeout <= 0 ? TimeSpan.FromSeconds(10) : TimeSpan.FromSeconds(data.Timeout);
				if (!title.IsValid())
				{
					title = data.AlertType switch
					{
						AlertType.Danger => "Error",
						AlertType.Warning => "Warning",
						AlertType.Success => "Success",
						AlertType.Info => "Info",
						_ => "Info",
					};
				}
				await Observable.Start(() =>
				{
					if (ViewModel?.Settings.DebugModeEnabled == true) DivinityApp.Log(data.Message);
					var shortenedMessage = data.Message;
					if(shortenedMessage.Length > 255)
					{
						shortenedMessage = string.Concat(shortenedMessage.AsSpan(0, 255), "...");
					}
					var content = new TextBlock() {
						Text = shortenedMessage,
						TextWrapping = TextWrapping.Wrap,
					};
					var tooltipElement = new TextBlock()
					{
						Text = data.Message,
						TextWrapping = TextWrapping.Wrap,
						MaxWidth = 400
					};
					void OnKeyDown(object? sender, KeyEventArgs e)
					{
						if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.C)
						{
							ClipboardService.SetText(data.Message);
						}
					};

					tooltipElement.AttachedToVisualTree += (o, e) =>
					{
						AddHandler(KeyDownEvent, OnKeyDown);
					};

					tooltipElement.DetachedFromVisualTree += (o, e) =>
					{
						RemoveHandler(KeyDownEvent, OnKeyDown);
					};

					ToolTip.SetTip(content, tooltipElement);
					var toastBuilder = _toastManager.CreateToast().WithTitle(title).WithContent(content);
					toastBuilder.SetCanDismissByClicking(true);
					toastBuilder.SetDismissAfter(duration);
					toastBuilder.SetType(data.AlertType.ToNotificationType());
					toastBuilder.Queue();
				}, RxApp.MainThreadScheduler);
				context.SetOutput(true);
			});
		});
	}
}