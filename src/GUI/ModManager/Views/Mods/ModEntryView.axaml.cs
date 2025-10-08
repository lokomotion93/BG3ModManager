using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

using Material.Icons;

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
	IsForceLoaded = true,
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

	public ModEntryView()
	{
		InitializeComponent();

		this.WhenActivated(d =>
		{
			if(ViewModel != null)
			{
				var whenExtenderIcon = ViewModel.WhenAnyValue(x => x.Data, x => x.Data.ExtenderIcon).Where(x => x.Item1 != null);
				d(whenExtenderIcon.Select(x => ExtenderIconToKind(x.Item2)).BindTo(this, x => x.ExtenderStatusImage.Kind));
				d(whenExtenderIcon.Select(x => ExtenderIconToForeground(x.Item2)).BindTo(this, x => x.ExtenderStatusImage.Foreground));
			}
		});
	}
}