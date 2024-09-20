using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public class ItemCreator
    {
        public Dictionary<string, Item> LoadedItems { get; set; } = [];

        public void Load()
        {
            EntityModuleConfig<Item> config = new("EntityCreator", "item.creator");
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
            EntityModuleConfig<Item> config = new("EntityCreator", "item.creator");
            foreach (string key in LoadedItems.Keys)
            {
                config.Add(key, LoadedItems[key]);
            }
            config.SaveConfig();
        }

        public void OpenCreator(Item? item = null)
        {
            CreateItem creator = new(this, item);
            creator.ShowDialog();
        }
    }
}
