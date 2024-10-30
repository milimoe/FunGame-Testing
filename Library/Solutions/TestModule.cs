using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Testing.Effects.OpenEffects;
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
                    (long)MagicID.冰霜攻击 => new 冰霜攻击(),
                    (long)SkillID.疾风步 => new 疾风步(),
                    (long)SuperSkillID.力量爆发 => new 力量爆发(),
                    (long)SuperSkillID.天赐之力 => new 天赐之力(),
                    (long)SuperSkillID.魔法涌流 => new 魔法涌流(),
                    (long)SuperSkillID.三重叠加 => new 三重叠加(),
                    (long)SuperSkillID.变幻之心 => new 变幻之心(),
                    (long)SuperSkillID.精准打击 => new 精准打击(),
                    (long)SuperSkillID.绝对领域 => new 绝对领域(),
                    (long)SuperSkillID.能量毁灭 => new 能量毁灭(),
                    (long)SuperSkillID.迅捷之势 => new 迅捷之势(),
                    (long)SuperSkillID.嗜血本能 => new 嗜血本能(),
                    (long)SuperSkillID.平衡强化 => new 平衡强化(),
                    (long)SuperSkillID.血之狂欢 => new 血之狂欢(),
                    (long)PassiveID.META马 => new META马(),
                    (long)PassiveID.心灵之火 => new 心灵之火(),
                    (long)PassiveID.魔法震荡 => new 魔法震荡(),
                    (long)PassiveID.灵能反射 => new 灵能反射(),
                    (long)PassiveID.智慧与力量 => new 智慧与力量(),
                    (long)PassiveID.致命打击 => new 致命打击(),
                    (long)PassiveID.毁灭之势 => new 毁灭之势(),
                    (long)PassiveID.枯竭打击 => new 枯竭打击(),
                    (long)PassiveID.玻璃大炮 => new 玻璃大炮(),
                    (long)PassiveID.累积之压 => new 累积之压(),
                    (long)PassiveID.敏捷之刃 => new 敏捷之刃(),
                    (long)PassiveID.弱者猎手 => new 弱者猎手(),
                    (long)ItemPassiveID.攻击之爪 => new 攻击之爪技能(),
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
                    return (EffectID)id switch
                    {
                        EffectID.ExATK => new ExATK(skill, dict),
                        EffectID.ExDEF => new ExDEF(skill, dict),
                        EffectID.ExSTR => new ExSTR(skill, dict),
                        EffectID.ExAGI => new ExAGI(skill, dict),
                        EffectID.ExINT => new ExINT(skill, dict),
                        EffectID.SkillHardTimeReduce => new SkillHardTimeReduce(skill, dict),
                        EffectID.NormalAttackHardTimeReduce => new NormalAttackHardTimeReduce(skill, dict),
                        EffectID.AccelerationCoefficient => new AccelerationCoefficient(skill, dict),
                        EffectID.ExSPD => new ExSPD(skill, dict),
                        EffectID.ExActionCoefficient => new ExActionCoefficient(skill, dict),
                        EffectID.ExCDR => new ExCDR(skill, dict),
                        EffectID.ExMaxHP => new ExMaxHP(skill, dict),
                        EffectID.ExMaxMP => new ExMaxMP(skill, dict),
                        EffectID.ExCritRate => new ExCritRate(skill, dict),
                        EffectID.ExCritDMG => new ExCritDMG(skill, dict),
                        EffectID.ExEvadeRate => new ExEvadeRate(skill, dict),
                        EffectID.PhysicalPenetration => new PhysicalPenetration(skill, dict),
                        EffectID.MagicalPenetration => new MagicalPenetration(skill, dict),
                        EffectID.ExPDR => new ExPDR(skill, dict),
                        EffectID.ExMDF => new ExMDF(skill, dict),
                        EffectID.ExHR => new ExHR(skill, dict),
                        EffectID.ExMR => new ExMR(skill, dict),
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
                    (long)AccessoryID.攻击之爪10 => new 攻击之爪10(),
                    (long)AccessoryID.攻击之爪30 => new 攻击之爪30(),
                    (long)AccessoryID.攻击之爪50 => new 攻击之爪50(),
                    _ => null,
                };
            };
        }
    }
}
