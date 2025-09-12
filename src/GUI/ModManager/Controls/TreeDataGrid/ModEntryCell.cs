using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;

using ModManager.Models.Mod;

namespace ModManager.Controls.TreeDataGrid;
public class ModEntryCell(IModEntry value) : ITemplateCell, ICell
{
	private readonly IModEntry _value = value;

	public object? Value => _value;

	public bool CanEdit => _value.EntryType == ModEntryType.Container;

	public BeginEditGestures EditGestures { get; }

	private static readonly Dictionary<string, IDataTemplate> _cachedTemplates = [];

	private static IDataTemplate? GetDataTemplate(string name)
	{
		if(_cachedTemplates.TryGetValue(name, out var template))
		{
			return template;
		}
		var resource = App.Current.FindResource(name);
		if(resource is IDataTemplate dataTemplate)
		{
			_cachedTemplates[name] = dataTemplate;
			return dataTemplate;
		}
		return null;
	}

	private static IDataTemplate? GetDisplayTemplate(ModEntryType modEntryType, bool isEditing = false)
	{
		object? template = null;

		if(!isEditing)
		{
			switch (modEntryType)
			{
				case ModEntryType.Container:
					template = GetDataTemplate("ModContainerEntryTemplate");
					break;
				case ModEntryType.Mod:
				default:
					template = GetDataTemplate("ModEntryTemplate");
					break;
			}
		}
		else if(modEntryType == ModEntryType.Container)
		{
			template = GetDataTemplate("ModContainerEntryEditingTemplate");
		}

		return template as IDataTemplate;
	}

	public IDataTemplate GetCellTemplate(Control control)
	{
		return GetDisplayTemplate(_value.EntryType)!;
	}

	public IDataTemplate? GetCellEditingTemplate(Control control)
	{
		return GetDisplayTemplate(_value.EntryType, true);
	}
}