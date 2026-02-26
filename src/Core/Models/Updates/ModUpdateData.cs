using ModManager.Models.Mod;
using ModManager.Services;

using System.Globalization;

namespace ModManager.Models.Updates;

public partial class ModUpdateData : ReactiveObject, ISelectable
{
	[Reactive] public partial ModData Mod { get; set; }
	[Reactive] public partial ModDownloadData DownloadData { get; set; }
	[Reactive] public partial bool IsSelected { get; set; }
	[Reactive] public partial bool CanDrag { get; set; }
	[Reactive] public partial bool IsHidden { get; set; }
	public bool IsDraggable => false;
	[Reactive] public partial string? SourceText { get; private set; }

	[ObservableAsProperty] public partial ModSourceType Source { get; }
	[ObservableAsProperty] public partial bool IsEditorMod { get; }
	[ObservableAsProperty] public partial string? DisplayName { get; }
	[ObservableAsProperty] public partial string? Author { get; }
	[ObservableAsProperty] public partial string? CurrentVersion { get; }
	[ObservableAsProperty] public partial string? UpdateVersion { get; }
	[ObservableAsProperty] public partial Uri? UpdateLink { get; }
	[ObservableAsProperty] public partial string? LocalFilePath { get; }
	[ObservableAsProperty] public partial string? LocalFileDateText { get; }
	[ObservableAsProperty] public partial string? UpdateFilePath { get; }
	[ObservableAsProperty] public partial string? UpdateDateText { get; }
	[ObservableAsProperty] public partial string? UpdateToolTip { get; }

	private Uri? SourceToLink(ValueTuple<ModData, ModSourceType> data)
	{
		if (data.Item1 != null)
		{
			var url = data.Item1.GetURL(data.Item2);
			if (url.IsValid())
			{
				return new Uri(url);
			}
		}
		return null;
	}

	private string DateToString(DateTimeOffset? date)
	{
		if (date.HasValue)
		{
			return date.Value.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture);
		}
		return "";
	}

	private string GetUpdateTooltip(string? description, Uri? url)
	{
		var result = "";
		if (description.IsValid())
		{
			result = description;
		}
		if (url?.AbsoluteUri.IsValid() == true)
		{
			if (result.IsValid()) result += Environment.NewLine;
			result += url.AbsoluteUri;
		}
		return result;
	}

	private IDisposable? _sourceTextObs = null;

	public ModUpdateData(ModData mod, ModDownloadData downloadData)
	{
		IsSelected = true;
		CanDrag = true;

		Mod = mod;
		DownloadData = downloadData;

		_displayNameHelper = this.WhenAnyValue(x => x.Mod.DisplayName).ToUIProperty(this, x => x.DisplayName);
		_isEditorModHelper = this.WhenAnyValue(x => x.Mod.IsLooseMod).ToUIProperty(this, x => x.IsEditorMod);
		_authorHelper = this.WhenAnyValue(x => x.Mod.AuthorDisplayName).ToUIProperty(this, x => x.Author);
		_currentVersionHelper = this.WhenAnyValue(x => x.Mod.Version.Version).ToUIProperty(this, x => x.CurrentVersion);
		_localFilePathHelper = this.WhenAnyValue(x => x.Mod.FilePath).ToUIProperty(this, x => x.LocalFilePath);
		_localFileDateTextHelper = this.WhenAnyValue(x => x.Mod.LastModified).Select(DateToString).ToUIProperty(this, x => x.LocalFileDateText);

		var whenSource = this.WhenAnyValue(x => x.DownloadData.DownloadSourceType);
		_sourceHelper = whenSource.ToUIProperty(this, x => x.Source);
		whenSource.Select(x => x.GetName()).Subscribe(key =>
		{
			_sourceTextObs?.Dispose();
			if (key.IsValid())
			{
				var locaService = AppLocator.Current.GetService<ILocaleService>();
				if(locaService != null)
				{
					_sourceTextObs = locaService.EntryToObservable(key).BindTo(this, x => x.SourceText);
				}
			}
		});

		_updateFilePathHelper = this.WhenAnyValue(x => x.DownloadData.DownloadPath).ToUIProperty(this, x => x.UpdateFilePath);
		_updateDateTextHelper = this.WhenAnyValue(x => x.DownloadData.Date).Select(DateToString).ToUIProperty(this, x => x.UpdateDateText);
		_updateVersionHelper = this.WhenAnyValue(x => x.DownloadData.Version).ToUIProperty(this, x => x.UpdateVersion);
		_updateLinkHelper = this.WhenAnyValue(x => x.Mod, x => x.Source).Select(SourceToLink).ToUIProperty(this, x => x.UpdateLink);
		_updateToolTipHelper = this.WhenAnyValue(x => x.DownloadData.Description, x => x.UpdateLink, GetUpdateTooltip).ToUIProperty(this, x => x.UpdateToolTip);
	}
}
