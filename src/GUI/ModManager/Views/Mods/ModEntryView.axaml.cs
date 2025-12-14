using Avalonia.Labs.Controls;
using Avalonia.Media;

using Material.Icons;
using Material.Icons.Avalonia;

using ModManager.Models.Mod;

namespace ModManager.Views.Mods;

public class DesignModEntry() : ModEntry(new ModData("d23b7409-f085-4946-aacb-aed4e434cb2d") {
	Name = "Test Mod",
	Description = "This is a test mod for the design view",
	Author = "LaughingLeader",
	FilePath = @"%LOCALAPPADATA%\Larian Studios\Baldur's Gate 3\Mods\TestMod.pak",
	IsToolkitProject = false,
	IsLooseMod = true,
	IsUserMod = true,
	HasOverrideFiles = true,
	LastModified = DateTimeOffset.Now,
	OsirisModStatus = DivinityOsirisModStatus.MODFIXER,
	ExtenderModStatus = ModExtenderStatus.Fulfilled,
	DisplayExtraIcons = true,
	CurrentExtenderVersion = 24,
	ScriptExtenderData = new() { ModTable = "TestMod", RequiredVersion = 24 },
})
{

}

public partial class ModEntryView : ReactiveUserControl<ModEntry>
{
	private static IBrush? ExtenderIconToForeground(ScriptExtenderIconType iconType) => iconType switch
	{
		ScriptExtenderIconType.Warning => Brushes.Yellow,
		ScriptExtenderIconType.Missing => Brushes.Red,
		ScriptExtenderIconType.FulfilledRequired => Brushes.DodgerBlue,
		ScriptExtenderIconType.FulfilledSupports => Brushes.DodgerBlue,
		_ => null,
	};

	private static MaterialIconKind ExtenderIconToKind(ScriptExtenderIconType iconType) => iconType switch
	{
		ScriptExtenderIconType.Warning => MaterialIconKind.Warning,
		ScriptExtenderIconType.Missing => MaterialIconKind.CallMissed,
		ScriptExtenderIconType.FulfilledRequired => MaterialIconKind.ScriptText,
		ScriptExtenderIconType.FulfilledSupports => MaterialIconKind.Script,
		_ => MaterialIconKind.BorderNone,
	};

	private static Control? GetExtenderStatusContent(ScriptExtenderIconType iconType)
	{
		if (iconType == ScriptExtenderIconType.FulfilledRequired || iconType == ScriptExtenderIconType.FulfilledSupports)
		{
			var icon = new AsyncImage() { Source = new Uri("avares://ModManager/Assets/Icons/DivinityEngine2_64x.png"), Width = 16, Height = 16 };
			if (iconType == ScriptExtenderIconType.FulfilledSupports)
			{
				icon.Opacity = 0.5d;
			}
			return icon;
		}
		else if (iconType != ScriptExtenderIconType.None)
		{
			var icon = new MaterialIcon() { Kind = ExtenderIconToKind(iconType), Foreground = ExtenderIconToForeground(iconType) };
			return icon;
		}
		return null;
	}

	public ModEntryView()
	{
		InitializeComponent();

		this.WhenActivated(d =>
		{
			if(ViewModel != null)
			{
				//ExtenderStatusContentControl[!ContentProperty] = ViewModel.WhenAnyValue(x => x.ExtenderIcon, GetExtenderStatusContent).ToBinding();
				d(ViewModel.WhenAnyValue(x => x.ExtenderIcon).Subscribe(icon =>
				{
					ExtenderStatusContentControl.Content = GetExtenderStatusContent(icon);
				}));
				//d(whenExtenderIcon.Select(ExtenderIconToKind).BindTo(this, x => x.ExtenderStatusImage.Kind));
				//d(whenExtenderIcon.Select(ExtenderIconToForeground).BindTo(this, x => x.ExtenderStatusImage.Foreground));
			}
		});
	}
}