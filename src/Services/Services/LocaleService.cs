using DynamicData;

using ModManager.Locale;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModManager.Services;
public class LocaleService : ReactiveObject, ILocaleService
{
	[Reactive] public CultureInfo Culture { get; set; }

	private readonly Dictionary<string, Subject<string?>> _subjects = [];

	private IDisposable? _initialNotify = null;

	public string? GetEntry(string key, string? fallback = null)
	{
		return Resources.ResourceManager.GetString(key, Resources.Culture) ?? fallback;
	}

	private void NotifySubscribers()
	{
		foreach(var entry in _subjects)
		{
			entry.Value.OnNext(GetEntry(entry.Key));
		}
	}

	private void StartNotify()
	{
		_initialNotify?.Dispose();
		_initialNotify = RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(50), NotifySubscribers);
	}

	public IObservable<string?> EntryToObservable(string key, string? fallback = null)
	{
		if(_subjects.TryGetValue(key, out var existing))
		{
			StartNotify();
			return existing;
		}
		var subject = new Subject<string?>();
		_subjects.Add(key, subject);
		StartNotify();
		return subject;
	}

	public LocaleService()
	{
		Culture = Resources.Culture;
		this.WhenAnyValue(x => x.Culture).WhereNotNull().Subscribe(lang =>
		{
			Resources.Culture = lang;
			StartNotify();
		});
	}
}
