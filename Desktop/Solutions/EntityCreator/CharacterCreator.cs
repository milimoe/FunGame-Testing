using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public class CharacterCreator
    {
        public Dictionary<string, Character> LoadedCharacters { get; set; } = [];

        public void Load()
        {
            EntityModuleConfig<Character> config = new("EntityCreator", "character.creator");
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
            EntityModuleConfig<Character> config = new("EntityCreator", "character.creator");
            foreach (string key in LoadedCharacters.Keys)
            {
                config.Add(key, LoadedCharacters[key]);
            }
            config.SaveConfig();
        }

        public void OpenCreator(Character? character = null)
        {
            CreateCharacter creator = new(this, character);
            creator.ShowDialog();
        }
    }
}
