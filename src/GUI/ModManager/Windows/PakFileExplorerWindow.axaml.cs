using Avalonia.Controls.Templates;
using Avalonia.Media;

using ModManager.Controls;
using ModManager.ViewModels;
using ModManager.ViewModels.Window;

using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace ModManager.Windows;

public partial class PakFileExplorerWindow : HideWindowBase<PakFileExplorerWindowViewModel>
{
	private readonly SukiToastManager _toastManager = new();
	private readonly SukiDialogManager _dialogManager = new();

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

		if (content is string message)
		{
			var dialogContent = new TextBlock { Text = message };
			var dialog = _dialogManager.CreateDialog().WithContent(dialogContent);
			_lastDialog = dialog.Dialog;
			dialog.SetCanDismissWithBackgroundClick(allowBackgroundClose);
			ViewModelLocator.MessageBox.IsVisible = dialog.TryShow();
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
			ViewModelLocator.MessageBox.IsVisible = dialog.TryShow();
		}

		var borderDialog = DialogHost.GetTemplateChildren().FirstOrDefault(x => x.GetType() == typeof(Border));
		if (borderDialog != null)
		{
			borderDialog.Opacity = (showCardBehind ? 1 : 0);
		}
	}

	private IObservable<Unit> ShowAlert(ShowAlertRequest data)
	{
		return Observable.Start(() =>
		{
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
			var toastBuilder = _toastManager.CreateToast().WithTitle(title).WithContent(data.Message);
			toastBuilder.SetCanDismissByClicking(true);
			toastBuilder.SetDismissAfter(duration);
			toastBuilder.SetType(data.AlertType.ToNotificationType());
			toastBuilder.Queue();
		}, RxApp.MainThreadScheduler);
	}

	public PakFileExplorerWindow()
	{
		InitializeComponent();

		ViewModel ??= AppServices.Get<PakFileExplorerWindowViewModel>();

		var whenVisible = ViewModel.WhenAnyValue(x => x.IsVisible);
		ViewModel.AddModCommand = ReactiveCommand.CreateFromTask(async token =>
		{
			DismissLastDialog();
			var dialogVM = ViewModelLocator.ModPicker;
			dialogVM.Open(Loca.Window_PakFileExplorer_Picker_AddMod_Title);
			ShowSukiDialog(dialogVM, true, false);
			var result = await dialogVM.WaitForResult();

			ViewModelLocator.MessageBox.IsVisible = false;

			if (result.Confirmed)
			{
				await ViewModel.LoadModsAsync(result.Mods, token);
				var modFileNames = string.Join(";", result.Mods.Select(x => x.FileName));
				var msg = Loca.Alert_Success_PakFileExplorer_LoadedMods.SafeFormat($"Loaded {modFileNames}", modFileNames);
				ViewModel.ShowAlertCommand?.Execute(new ShowAlertRequest(msg, AlertType.Success)).Subscribe();
			}
		}, whenVisible);

		ViewModel.ShowAlertCommand = ReactiveCommand.CreateFromObservable<ShowAlertRequest, Unit>(ShowAlert, whenVisible);

		this.WhenActivated(d =>
		{
			DialogHost.Manager = _dialogManager;
			ToastHost.Manager = _toastManager;

			if (ViewModel != null)
			{
				d(this.GetObservable(IsVisibleProperty).BindTo(ViewModel, x => x.IsVisible));
				//Binding via axaml doesn't seem to work when the command starts off null, but this works
				d(this.BindCommand(ViewModel, x => x.AddModCommand, x => x.AddModButton));
			}
		});
	}
}