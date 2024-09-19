using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Testing.Items;
using Milimoe.FunGame.Testing.Skills;

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
                Character c = Factory.GetCharacter();
                List<Skill> list = [];
                list.Add(new 冰霜攻击(c));
                list.Add(new 疾风步(c));
                list.Add(new META马(c));
                list.Add(new 力量爆发(c));
                list.Add(new 心灵之火(c));
                list.Add(new 天赐之力(c));
                list.Add(new 魔法震荡(c));
                list.Add(new 魔法涌流(c));
                list.Add(new 灵能反射(c));
                list.Add(new 三重叠加(c));
                list.Add(new 智慧与力量(c));
                list.Add(new 变幻之心(c));
                list.Add(new 致命打击(c));
                list.Add(new 精准打击(c));
                list.Add(new 毁灭之势(c));
                list.Add(new 绝对领域(c));
                list.Add(new 枯竭打击(c));
                list.Add(new 能量毁灭(c));
                list.Add(new 玻璃大炮(c));
                list.Add(new 迅捷之势(c));
                list.Add(new 累积之压(c));
                list.Add(new 嗜血本能(c));
                list.Add(new 敏捷之刃(c));
                list.Add(new 平衡强化(c));
                list.Add(new 弱者猎手(c));
                list.Add(new 血之狂欢(c));
                list.Add(new 冰霜攻击(c));
                list.Add(new 疾风步(c));
                return list;
            }
        }

        public override Skill? GetSkill(long id, string name)
        {
            string str = id + "." + name;
            if (Skills.Where(s => s.GetIdName() == str).FirstOrDefault() is Skill s)
            {
                return s;
            }
            return null;
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
                list.Add(new 攻击之爪50());
                return list;
            }
        }

        public override Item? GetItem(long id, string name)
        {
            string str = id + "." + name;
            if (Items.Where(s => s.GetIdName() == str).FirstOrDefault() is Item i)
            {
                return i;
            }
            return null;
        }
    }
}
