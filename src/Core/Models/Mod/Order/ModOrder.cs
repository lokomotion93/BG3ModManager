using ModManager.Models.Mod.Game;
using ModManager.Util;

namespace ModManager.Models.Mod.Order;

[DataContract]
public class ModOrder : ReactiveObject, IJsonOnDeserialized
{
	[Reactive] public string? Name { get; set; }
	[Reactive] public string? FilePath { get; set; }
	[Reactive] public DateTimeOffset LastModifiedDate { get; set; }
	[Reactive] public bool IsLoaded { get; set; }

	[Reactive] public bool IsModSettings { get; set; }

	/// <summary>
	/// This is an order from a non-standard order file (info .json, .txt, .tsv).
	/// </summary>
	[Reactive] public bool IsDecipheredOrder { get; set; }

	[ObservableAsProperty] public string? LastModified { get; }

	[DataMember] public List<IModOrderEntry> Entries { get; set; } = [];
	[DataMember] public Version? ModManagerVersion { get; set; }

	private static int FindNestedIndex(ModOrderContainer container, int indexModifier, string id)
	{
		for (var i = 0; i < container.Children.Count; i++)
		{
			var entry = container.Children[i];

			if (entry.Id == id)
			{
				return indexModifier + i;
			}
			if (entry.Type == ModEntryType.Container && entry is ModOrderContainer subContainer)
			{
				var index = FindNestedIndex(subContainer, indexModifier + i, id);
				if (index > -1)
				{
					return index;
				}
			}
		}
		return -1;
	}

	public int GetIndex(string id)
	{
		for(var i = 0; i < Entries.Count; i++)
		{
			var entry = Entries[i];

			if (entry.Id == id)
			{
				return i;
			}
			if(entry.Type == ModEntryType.Container && entry is ModOrderContainer container)
			{
				var index = FindNestedIndex(container, i, id);
				if(index > -1)
				{
					return index;
				}
			}
		}
		return -1;
	}

	private static void AddNestedEntries(ModOrderContainer container, ref List<IModOrderEntry> target)
	{
		target.Add(container);
		foreach (var entry in container.Children)
		{
			if (entry.Type == ModEntryType.Mod)
			{
				target.Add(entry);
			}
			else if (entry.Type == ModEntryType.Container && entry is ModOrderContainer subContainer)
			{
				AddNestedEntries(subContainer, ref target);
			}
		}
	}

	//Get all entries in a flattened order (nested containers/mods are included in order)
	public List<IModOrderEntry> GetFlattenedEntries()
	{
		var entries = new List<IModOrderEntry>();
		foreach(var entry in Entries)
		{
			if(entry.Type == ModEntryType.Mod)
			{
				entries.Add(entry);
			}
			else if(entry.Type == ModEntryType.Container && entry is ModOrderContainer container)
			{
				AddNestedEntries(container, ref entries);
			}
		}
		return entries;
	}

	public HashSet<string> GetModIds()
	{
		var ids = new HashSet<string>();
		foreach (var entry in Entries)
		{
			if (entry.Type == ModEntryType.Mod)
			{
				ids.Add(entry.Id);
			}
			else if (entry.Type == ModEntryType.Container && entry is ModOrderContainer container)
			{
				foreach(var subEntry in container.ForEachNested())
				{
					if(subEntry.Type == ModEntryType.Mod)
					{
						ids.Add(subEntry.Id);
					}
				}
			}
		}
		return ids;
	}

	public void Add(IModOrderEntry entry, bool force = false)
	{
		try
		{
			if (Entries != null && entry != null)
			{
				if (force)
				{
					Entries.Add(entry);
				}
				else
				{
					foreach (var x in Entries)
					{
						if (x.Id.Equals(entry.Id))
						{
							return;
						}
					}
					Entries.Add(entry);
				}
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error adding entry to order:\n{ex}");
		}
	}

	public void Add(ModData entry, bool force = false) => Add(new ModOrderMod(entry.UUID) { Name = entry.DisplayName }, force);
	public void Add(ModuleShortDesc entry, bool force = false) => Add(new ModOrderMod(entry.UUID) { Name = entry.Name }, force);

	public void Add(IModEntry entry, bool force = false)
	{
		if(entry.EntryType == ModEntryType.Mod && entry is ModEntry mod)
		{
			Add(mod.ToSerialized(), force);
		}
		else if(entry.EntryType == ModEntryType.Container && entry is ModContainer container)
		{
			Add(container.ToSerialized(), force);
		}
	}

	public void AddRange(IEnumerable<IModOrderEntry> entries, bool replace = false)
	{
		if(replace)
		{
			Entries.Clear();
		}
		foreach (var entry in entries)
		{
			Add(entry, replace);
		}
	}

	public void AddRange(IEnumerable<IModEntry> entries, bool replace = false)
	{
		if (replace)
		{
			Entries.Clear();
		}
		foreach (var entry in entries)
		{
			Add(entry, replace);
		}
	}

	public void Remove(string id)
	{
		try
		{
			if (Entries != null && Entries.Count > 0)
			{
				Entries.RemoveAll(x => x.Id == id);
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error removing mod from order:\n{ex}");
		}
	}

	public void RemoveRange(IEnumerable<IModOrderEntry> entries)
	{
		if (Entries.Count > 0 && entries != null)
		{
			var uuids = entries.Select(x => x.Id).ToHashSet();
			Entries.RemoveAll(x => uuids.Contains(x.Id));
		}
	}

	public void Update(IModOrderEntry entry)
	{
		if (Entries != null && Entries.Count > 0)
		{
			var existing = Entries.FirstOrDefault(x => x.Id.Equals(entry.Id) && x.Type == entry.Type);
			if(existing != null)
			{
				if(entry.Type == ModEntryType.Mod)
				{
					var mod = (ModOrderMod)entry;
					var existingMod = (ModOrderMod)existing;
					existingMod.Name = mod.Name;
				}
				else if (entry.Type == ModEntryType.Container)
				{
					var container = (ModOrderContainer)entry;
					var existingContainer = (ModOrderContainer)existing;

					if (existingContainer.Settings != null && container.Settings != null)
					{
						existingContainer.Settings.SetFrom(container.Settings);
					}
					existingContainer.Name = container.Name;
					existingContainer.Children.Clear();
					existingContainer.Children.AddRange(container.Children);
				}
			}
		}
	}

	public void Update(ModData entry)
	{
		Update(new ModOrderMod(entry.UUID) { Name = entry.DisplayName });
	}

	public void Sort(Comparison<IModOrderEntry> comparison)
	{
		try
		{
			if (Entries.Count > 1)
			{
				Entries.Sort(comparison);
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error sorting order:\n{ex}");
		}
	}

	public void CopyOrder(ModOrder nextOrder)
	{
		Entries.Clear();
		Entries.AddRange(nextOrder.Entries);
	}

	private static void AddContainerIds(ref List<string> targetList, ModOrderContainer container)
	{
		foreach(var entry in container.Children)
		{
			if (entry.Type == ModEntryType.Mod && entry is ModOrderMod mod)
			{
				targetList.Add(mod.Id);
			}
			else if (entry.Type == ModEntryType.Container && entry is ModOrderContainer childContainer)
			{
				AddContainerIds(ref targetList, childContainer);
			}
		}
	}

	public bool OrderEquals(IEnumerable<string> orderList)
	{
		if (Entries.Count > 0)
		{
			var ids = new List<string>();
			foreach (var entry in Entries)
			{
				if (entry.Type == ModEntryType.Mod && entry is ModOrderMod mod)
				{
					ids.Add(mod.Id);
				}
				else if (entry.Type == ModEntryType.Container && entry is ModOrderContainer container)
				{
					AddContainerIds(ref ids, container);
				}
			}
			return ids.SequenceEqual(orderList);
		}
		return false;
	}

	public ModOrder Clone()
	{
		return new ModOrder()
		{
			Name = Name,
			Entries = [.. Entries],
			LastModifiedDate = LastModifiedDate
		};
	}

	[JsonExtensionData]
	private Dictionary<string, object>? Extras { get; set; }

	void IJsonOnDeserialized.OnDeserialized()
	{
		if (Extras?.Count > 0)
		{
			//Renamed Order to "Entries" so it isn't serialized As Order.Order
			if (JsonUtils.TryGetExtraProperty(Extras, "Order", out List<IModOrderEntry>? order) && order != null)
			{
				AddRange(order);
				Extras.Remove("Order");
			}
		}
	}

	public ModOrder()
	{
		this.WhenAnyValue(x => x.LastModifiedDate).Select(x => x.ToString("g")).ToUIProperty(this, x => x.LastModified, "");
	}
}
