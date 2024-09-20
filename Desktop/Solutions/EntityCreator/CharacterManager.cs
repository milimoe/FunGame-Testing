using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public class CharacterManager
    {
        public Dictionary<string, Character> LoadedCharacters { get; set; } = [];

        public void Load()
        {
            EntityModuleConfig<Character> config = new("EntityEditor", "characters");
            config.LoadConfig();
            LoadedCharacters = new(config);
            foreach (Character c in LoadedCharacters.Values)
            {
                c.Recovery();
            }
        }

        public bool Add(string name, Character character)
        {
            return LoadedCharacters.TryAdd(name, character);
        }

        public bool Remove(string name)
        {
            return LoadedCharacters.Remove(name);
        }

        public void Save()
        {
            EntityModuleConfig<Character> config = new("EntityEditor", "characters");
            foreach (string key in LoadedCharacters.Keys)
            {
                config.Add(key, LoadedCharacters[key]);
            }
            config.SaveConfig();
        }

        public void OpenCreator(Character? character = null)
        {
            CharacterCreator manager = new(this, character);
            manager.ShowDialog();
        }
    }
}
