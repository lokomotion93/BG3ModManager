using Avalonia.Controls.Models.TreeDataGrid;

using DynamicData.Binding;

using ModManager.Models;
using ModManager.Models.Mod;
using ModManager.ViewModels.Main;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModManager
{
	public static class ModelGlobals
	{
		public static ObservableCollectionExtended<IModEntry> TestMods { get; }

		private static bool _addedDataFolderPak = false;

		private static string GetModFilePath(Random ran, bool isToolkitProject, string modFolder)
		{
			if(isToolkitProject)
			{
				return $"C:\\Games\\BG3\\Data\\Mods\\{modFolder}\\meta.lsx";
			}
			if(!_addedDataFolderPak || ran.Next(10) <= 1)
			{
				_addedDataFolderPak = true;
				return $"C:\\Games\\BG3\\Data\\{modFolder}.pak";
			}
			return $"C:\\Users\\TestUser\\AppData\\Local\\Larian Studios\\Baldur's Gate 3\\Mods\\{modFolder}.pak";
		}

		private static void RecalculateIndexes(IModEntry entry, ref int index)
		{
			entry.Index = index;
			index += 1;
			if (entry.EntryType == ModEntryType.Container && entry is ModContainer modContainer && modContainer.Children != null)
			{
				foreach(var child in modContainer.Children)
				{
					RecalculateIndexes(child, ref index);
				}
			}
		}

		public static DesignModOrderViewModel ModOrderViewModel { get; }

		static ModelGlobals()
		{
			TestMods = [];

			ModOrderViewModel = new();

			var ran = new Random(1337);

			var containers = new List<ModContainer>();

			for(var i = 0; i < 4; i++)
			{
				var container = new ModContainer(Guid.NewGuid().ToString(), $"Container{i}");
				containers.Add(container);
				TestMods.Add(container);
			}

			for (var i = 0; i < 30; i++)
			{
				var isToolkitProject = ran.Next(10) <= 3;
				var modNum = i + 1;
				var modName = $"Mod {modNum}";
				var uuid = Guid.NewGuid().ToString();
				var modFolder = $"Mod{modNum}_{uuid}";

				var mod = new ModEntry(new ModData(uuid)
				{
					Name = modName,
					Folder = modFolder,
					Description = $"Random mod {modNum}",
					Author = i % 2 <= 0 ? "LaughingLeader" : "Rando",
					FilePath = GetModFilePath(ran, isToolkitProject, modFolder),
					IsToolkitProject = isToolkitProject,
					IsLooseMod = isToolkitProject,
					LastModified = DateTimeOffset.Now,
					Version = new LarianVersion((ulong)ran.Next(0, 24), (ulong)ran.Next(0, 48), (ulong)ran.Next(0, 128), (ulong)ran.Next(0, 256))
				});

				if (ran.Next(100) <= 60)
				{
					containers[ran.Next(0, containers.Count-1)].Children!.Add(mod);
				}
				else
				{
					TestMods.Add(mod);
				}

				var index = 0;
				foreach(var entry in TestMods)
				{
					RecalculateIndexes(entry, ref index);
				}
			}
		}
	}
}
