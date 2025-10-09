using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModManager.Services;
public class ControlFactoryService(ILocaleService localeService)
{
	private readonly ILocaleService _locale = localeService;

	public TextBlock LocalizedTextBlock(string key, string fallback)
	{
		var tb = new TextBlock();
		tb[!TextBlock.TextProperty] = _locale.EntryToObservable(key, fallback).ToBinding();
		return tb;
	}
}
