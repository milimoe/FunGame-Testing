using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public class ItemManager
    {
        public string ModuleName { get; set; } = "EntityEditor";
        public string ConfigName { get; set; } = "items";
        public Dictionary<string, Item> LoadedItems { get; set; } = [];

        public void Load()
        {
            EntityModuleConfig<Item> config = new(ModuleName, ConfigName);
            config.LoadConfig();
            LoadedItems = new(config);
        }

        public bool Add(string name, Item item)
        {
            return LoadedItems.TryAdd(name, item);
        }

        public bool Remove(string name)
        {
            return LoadedItems.Remove(name);
        }

        public void Save()
        {
            EntityModuleConfig<Item> config = new(ModuleName, ConfigName);
            foreach (string key in LoadedItems.Keys)
            {
                config.Add(key, LoadedItems[key]);
            }
            config.SaveConfig();
        }

        public void OpenCreator(Item? item = null)
        {
            ItemCreator manager = new(this, item);
            manager.ShowDialog();
        }
    }
}
