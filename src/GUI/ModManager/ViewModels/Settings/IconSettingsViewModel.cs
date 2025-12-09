using Avalonia.Platform.Storage;

using DynamicData;
using DynamicData.Binding;

using Material.Icons;

using ModManager.Models.Mod;
using ModManager.Models.View;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.VisualTree;

using DynamicData;
using DynamicData.Binding;

using ModManager.Controls.TreeDataGrid;
using ModManager.Models;
using ModManager.Models.App;
using ModManager.Models.Mod;
using ModManager.Models.Mod.Game;
using ModManager.Models.Mod.Order;
using ModManager.Models.Settings;
using ModManager.Services;
using ModManager.Util;
using ModManager.ViewModels.Mods;
using ModManager.Windows;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization.DataContracts;
using Avalonia.Media;
using ModManager.Styling;


namespace ModManager.ViewModels.Settings;

public partial class IconSettingsViewModel : ReactiveObject
{
	[Reactive] public partial ModContainerIconSettings Settings { get; private set; }
	[Reactive] public partial ModContainerIconSettings? Target { get; private set; }
	[Reactive] public partial bool IsExpanded { get; set; }
	[Reactive] public partial bool IsPathIconType { get; set; }
	[Reactive] public partial bool IsMaterialIconType { get; set; }
	[Reactive] public partial int SelectedMaterialIconIndex { get; set; }

	private readonly SourceCache<MaterialIconEntry, string> _materialIconsSource = new(x => x.Name);


	private readonly ReadOnlyObservableCollection<MaterialIconEntry> _materialIcons;
	public ReadOnlyObservableCollection<MaterialIconEntry> MaterialIcons => _materialIcons;

	public RxCommandUnit RenderImageCommand { get; }
	public RxCommandUnit ClearImageCommand { get; }

	[ObservableAsProperty] private IBrush? _foregroundColorBrush;
	[ObservableAsProperty] private IBrush? _backgroundColorBrush;
	[ObservableAsProperty] private IBrush? _borderColorBrush;
	[ObservableAsProperty] private Thickness _borderThickness;

	[ReactiveCommand]
	public async Task BrowseForImage()
	{
		var dialog = AppServices.Dialog;
		var result = await dialog.OpenFileAsync(new OpenFileBrowserDialogRequest(
			"Open Icon Image...",
			dialog.GetInitialStartingDirectory(AppServices.Settings.ManagerSettings.LastImportDirectoryPath),
			CommonFileTypes.ImageFileTypes,
			multiSelect: false
		));
		if(result.Success)
		{
			Settings.Path = result.File;
			AppServices.Settings.ManagerSettings.LastImportDirectoryPath = AppServices.FS.Path.GetDirectoryName(result.File);
		}
	}

	public void Open(ModContainerIconSettings target)
	{
		Target = target;
		Settings.SetFromDataMember(Target);

		IsMaterialIconType = Settings.Kind.IsValid() && !Settings.Path.IsValid();
		IsPathIconType = !IsMaterialIconType;

		Settings.IsDirty = true;
	}

	public void Apply()
	{
		if(Target != null)
		{
			if(IsMaterialIconType)
			{
				Settings.Path = null;
			}
			else
			{
				Settings.Kind = null;
			}
			Target.SetFromDataMember(Settings);
		}
	}

	public void Clear()
	{
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			Target = null;
			Settings.SetToDefault();
			ClearImageCommand.Execute().Subscribe();
		});
	}

	private int KindStringToEntryIndex(string? kind)
	{
		if(kind.IsValid())
		{
			var entry = _materialIconsSource.Lookup(kind);
			if(entry.HasValue)
			{
				return _materialIcons.IndexOf(entry.Value);
			}
		}
		return 0;
	}

	private string? IndexToKindStr(int index)
	{
		if(index > 0 && index < _materialIcons.Count)
		{
			return _materialIcons[index].Name;
		}
		return null;
	}

	public IconSettingsViewModel()
	{
		Settings = new();
		RenderImageCommand = ReactiveCommand.Create(() => { Settings.IsDirty = false; });
		ClearImageCommand = ReactiveCommand.Create(() => { });

		IsPathIconType = true;

		_materialIconsSource.AddOrUpdate(new MaterialIconEntry(MaterialIconKind.BorderNone, "None"));
		var entries = Enum.GetValues<MaterialIconKind>()
			.Select(x => new MaterialIconEntry(x))
			.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);
		_materialIconsSource.AddOrUpdate(entries);
		_materialIconsSource.Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(out _materialIcons).Subscribe();

		var clearIsExecuting = ClearImageCommand.IsExecuting.Where(x => !x);

		var shouldRenderIcon = this.WhenAnyValue(x => x.IsPathIconType, x => x.IsMaterialIconType, x => x.SelectedMaterialIconIndex);
		var isDirty = Settings.WhenAnyValue(x => x.IsDirty)
			.Where(b => b == true)
			.SkipUntil(clearIsExecuting);
		isDirty.CombineLatest(shouldRenderIcon)
			.Throttle(TimeSpan.FromTicks(50))
			.ObserveOn(RxApp.MainThreadScheduler)
			.Select(_ => Unit.Default)
			.InvokeCommand(RenderImageCommand);

		RenderImageCommand.ThrownExceptions.Subscribe(ex =>
		{
			DivinityApp.Log($"Error rendering icon:\n{ex}");
		});
		this.WhenAnyValue(x => x.Settings.Kind, KindStringToEntryIndex).BindTo(this, x => x.SelectedMaterialIconIndex, 0);
		this.WhenAnyValue(x => x.SelectedMaterialIconIndex, IndexToKindStr).BindTo(this, x => x.Settings.Kind);

		var defaultForegroundBrush = ColorBrushCache.GetResourceBrush("SukiText");
		var defaultThickness = new Thickness(0);

		_foregroundColorBrushHelper = Settings.WhenAnyValue(x => x.ForegroundColor, x => ColorBrushCache.GetBrush(x, defaultForegroundBrush))
			.ToUIProperty(this, x => x.ForegroundColorBrush, defaultForegroundBrush);

		_backgroundColorBrushHelper = Settings.WhenAnyValue(x => x.BackgroundColor, x => ColorBrushCache.GetBrush(x, defaultForegroundBrush))
			.ToUIProperty(this, x => x.BackgroundColorBrush, defaultForegroundBrush);

		_borderColorBrushHelper = Settings.WhenAnyValue(x => x.BorderColor, x => ColorBrushCache.GetBrush(x, defaultForegroundBrush))
			.ToUIProperty(this, x => x.BorderColorBrush, defaultForegroundBrush);

		_borderThicknessHelper = Settings.WhenAnyValue(x => x.BorderThickness)
			.WhereNotNull()
			.ValueOrFallback(Thickness.Parse, defaultThickness)
			.ToUIProperty(this, x => x.BorderThickness, defaultThickness);
	}
}

public class DesignIconSettingsViewModel : IconSettingsViewModel
{
	public DesignIconSettingsViewModel() : base()
	{
		Settings.Path = "avares://ModManager/Assets/Icons/DivinityEngine2_64x.png";
	}
}