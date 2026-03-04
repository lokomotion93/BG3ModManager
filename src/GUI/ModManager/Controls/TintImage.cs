using Avalonia.Media;

using System;
using System.Collections.Generic;
using System.Text;

namespace ModManager.Controls;

public class TintImage : ContentControl
{
	public static readonly StyledProperty<IImage?> SourceProperty = Image.SourceProperty.AddOwner<TintImage>();

	public IImage? Source
	{
		get => GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}
}
