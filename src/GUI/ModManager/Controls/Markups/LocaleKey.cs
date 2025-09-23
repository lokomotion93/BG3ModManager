using Avalonia.Markup.Xaml;

namespace ModManager;
public class LocaleKey(string key) : MarkupExtension
{
	public string Key => key;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return AppServices.Locale.EntryToObservable(Key).ToBinding();
	}
}
