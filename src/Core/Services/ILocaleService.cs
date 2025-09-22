using System.Globalization;
using System.Reactive.Subjects;

namespace ModManager.Services;
public interface ILocaleService
{
	CultureInfo Culture { get; set; }
	string? GetEntry(string key, string? fallback = null);
	IObservable<string?> EntryToObservable(string key, string? fallback = null);
}
