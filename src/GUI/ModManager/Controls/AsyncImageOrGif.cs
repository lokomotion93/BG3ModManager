namespace ModManager.Controls;

public class AsyncImageOrGif : ContentControl
{
	public static readonly DirectProperty<AsyncImageOrGif, object?> SourceProperty = AvaloniaProperty.RegisterDirect<AsyncImageOrGif, object?>(nameof(Source), o => o.Source, (o,v) => o.Source = v);

	private object? _source;

	public object? Source
	{
		get { return _source; }
		private set
		{
			if(SetAndRaise(SourceProperty, ref _source, value))
			{
				StartRealizeImage(value);
			}
		}
	}

	private string? _actualPath;

	private IDisposable? _loadTask;

	private async Task RealizeImageAsync(IScheduler sch, CancellationToken token)
	{
		if(_actualPath.IsValid())
		{
			var image = await AppServices.ControlFactory.ImageFromPathAsync(_actualPath, null, token);
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				Content = image;
			});
		}
	}

	private void StartRealizeImage(object? path)
	{
		_loadTask?.Dispose();
		if (path != null)
		{
			if (path is string pathStr && pathStr.IsValid())
			{
				_actualPath = pathStr;
				_loadTask = RxApp.TaskpoolScheduler.ScheduleAsync(RealizeImageAsync);
				return;
			}
			else if (path is Uri pathUri && pathUri.IsValid())
			{
				_actualPath = pathUri.ToString();
				_loadTask = RxApp.TaskpoolScheduler.ScheduleAsync(RealizeImageAsync);
				return;
			}
			Content = null;
		}
	}
}
