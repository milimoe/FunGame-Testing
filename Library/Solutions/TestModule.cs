using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;

namespace Addons
{
    public class ExampleGameModuleConstant
    {
        public const string Example = "fungame.example.gamemode";
        public const string ExampleCharacter = "fungame.example.character";
        public const string ExampleSkill = "fungame.example.skill";
        public const string ExampleItem = "fungame.example.item";
    }

    public class ExampleCharacterModule : CharacterModule
    {
        public override string Name => ExampleGameModuleConstant.ExampleCharacter;

        public override string Description => "My First CharacterModule";

        public override string Version => "1.0.0";

        public override string Author => "FunGamer";

        public override List<Character> Characters
        {
            get
            {
                EntityModuleConfig<Character> config = new(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleCharacter);
                config.LoadConfig();
                return [.. config.Values];
            }
        }
    }

    public class ExampleSkillModule : SkillModule
    {
        public override string Name => ExampleGameModuleConstant.ExampleSkill;

        public override string Description => "My First SkillModule";

        public override string Version => "1.0.0";

        public override string Author => "FunGamer";

        public override List<Skill> Skills
        {
            get
            {
                List<Skill> list = [];
                Skill s = Factory.GetSkill();
                s.Name = "Example Skill";
                list.Add(s);
                return list;
            }
        }
    }

    public class ExampleItemModule : ItemModule
    {
        public override string Name => ExampleGameModuleConstant.ExampleItem;

        public override string Description => "My First ItemModule";

        public override string Version => "1.0.0";

        public override string Author => "FunGamer";

        public override List<Item> Items
        {
            get
            {
                List<Item> list = [];
                Item i = Factory.GetItem();
                i.Name = "Example Item";
                i.Price = 20;
                list.Add(i);
                return list;
            }
        }
    }
}
