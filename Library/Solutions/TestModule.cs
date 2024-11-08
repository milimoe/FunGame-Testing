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

        public override Dictionary<string, Character> Characters
        {
            get
            {
                EntityModuleConfig<Character> config = new(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleCharacter);
                config.LoadConfig();
                return config;
            }
        }
    }

    public class ExampleSkillModule : SkillModule
    {
        public override string Name => ExampleGameModuleConstant.ExampleSkill;

        public override string Description => "My First SkillModule";

        public override string Version => "1.0.0";

        public override string Author => "FunGamer";

        public override Dictionary<string, Skill> Skills
        {
            get
            {
                return Factory.GetGameModuleInstances<Skill>(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleSkill);
            }
        }

        protected override Factory.EntityFactoryDelegate<Skill> SkillFactory()
        {
            return (id, name, args) =>
            {
                Skill skill = id switch
                {
                    _ => new OpenSkill(id, name, args)
                };

                if (skill is OpenSkill && args.TryGetValue("values", out object? value) && value is Dictionary<string, object> dict)
                {
                    foreach (string key in dict.Keys)
                    {
                        skill.Values[key] = dict[key];
                    }
                }

                return skill;
            };
        }

        protected override Factory.EntityFactoryDelegate<Effect> EffectFactory()
        {
            return (id, name, args) =>
            {
                if (args.TryGetValue("skill", out object? value) && value is Skill skill && args.TryGetValue("values", out value) && value is Dictionary<string, object> dict)
                {
                    return id switch
                    {
                        _ => null
                    };
                }
                return null;
            };
        }
    }

    public class ExampleItemModule : ItemModule
    {
        public override string Name => ExampleGameModuleConstant.ExampleItem;

        public override string Description => "My First ItemModule";

        public override string Version => "1.0.0";

        public override string Author => "FunGamer";

        public override Dictionary<string, Item> Items
        {
            get
            {
                return Factory.GetGameModuleInstances<Item>(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleItem);
            }
        }

        protected override Factory.EntityFactoryDelegate<Item> ItemFactory()
        {
            return (id, name, args) =>
            {
                return id switch
                {
                    _ => null,
                };
            };
        }
    }
}
