using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Testing.Effects;
using Milimoe.FunGame.Testing.Items;
using Milimoe.FunGame.Testing.Skills;
using MilimoeFunGame.Testing.Characters;

namespace Milimoe.FunGame.Testing.Tests
{
    internal class SkillJSONTest
    {
        public SkillJSONTest()
        {
            //EntityModuleConfig<Character> config = new(nameof(SkillJSONTest), nameof(Character))
            //{
            //    { "Oshima", OshimaCharacters.Oshima },
            //    { "Xinyin", OshimaCharacters.Xinyin },
            //    { "Yang", OshimaCharacters.Yang },
            //    { "NanGanyu", OshimaCharacters.NanGanyu },
            //    { "NiuNan", OshimaCharacters.NiuNan },
            //    { "Mayor", OshimaCharacters.Mayor },
            //    { "马猴烧酒", OshimaCharacters.马猴烧酒 },
            //    { "QingXiang", OshimaCharacters.QingXiang },
            //    { "QWQAQW", OshimaCharacters.QWQAQW },
            //    { "ColdBlue", OshimaCharacters.ColdBlue },
            //    { "绿拱门", OshimaCharacters.绿拱门 },
            //    { "QuDuoduo", OshimaCharacters.QuDuoduo }
            //};
            //config.SaveConfig();

            EntityModuleConfig<Skill> config2 = new(nameof(SkillJSONTest), nameof(Skill));
            Character c = Factory.GetCharacter();
            List<Skill> listSkill = [];
            listSkill.Add(new 精准打击(c));
            foreach (Skill s in listSkill)
            {
                config2.Add(s.Name, s);
            }
            config2.SaveConfig();

            config2.LoadConfig();
            foreach (string key in config2.Keys)
            {
                Skill prev = config2[key];
                Skill? next = GetSkill(prev.Id, prev.Name, prev.SkillType);
                if (next != null)
                {
                    config2[key] = next;
                }
                Skill skill = config2[key];
                foreach (Effect effect in skill.Effects.ToList())
                {
                    Effect? newEffect = GetEffect(effect.Id, effect.Name, skill);
                    if (newEffect != null)
                    {
                        skill.Effects.Remove(effect);
                        skill.Effects.Add(newEffect);
                    }
                }
            }
            Console.WriteLine(string.Join("\r\n", config2.Values));

            EntityModuleConfig<Item> config3 = new(nameof(SkillJSONTest), nameof(Item)); ;
            //EntityModuleConfig<Item> config3 = new(nameof(SkillJSONTest), nameof(Item))
            //{
            //    { "攻击之爪10", new 攻击之爪10() }
            //};
            //config3.SaveConfig();
            config3.LoadConfig();
            foreach (string key in config3.Keys)
            {
                Item prev = config3[key];
                Item? next = GetItem(prev.Id, prev.Name, prev.ItemType);
                if (next != null)
                {
                    prev.SetPropertyToItemModuleNew(next);
                    config3[key] = next;
                }
                Item item = config3[key];
                HashSet<Skill> skills = item.Skills.Passives;
                if (item.Skills.Active != null) skills.Add(item.Skills.Active);
                foreach (Skill skill in skills.ToList())
                {
                    Skill? newSkill = GetSkill(skill.Id, skill.Name, skill.SkillType);
                    if (newSkill != null)
                    {
                        if (newSkill.IsActive)
                        {
                            item.Skills.Active = newSkill;
                        }
                        else
                        {
                            item.Skills.Passives.Remove(skill);
                            item.Skills.Passives.Add(newSkill);
                        }
                    }
                    Skill s = newSkill ?? skill;
                    foreach (Effect effect in s.Effects.ToList())
                    {
                        Effect? newEffect = GetEffect(effect.Id, effect.Name, skill);
                        if (newEffect != null)
                        {
                            skill.Effects.Remove(effect);
                            skill.Effects.Add(newEffect);
                        }
                    }
                }
            }
            Console.WriteLine(string.Join("\r\n", config3.Values));

        }

        public static Item? GetItem(long id, string name, ItemType type)
        {
            if (type == ItemType.Accessory)
            {
                switch ((AccessoryID)id)
                {
                    case AccessoryID.攻击之爪10:
                        return new 攻击之爪10();
                    case AccessoryID.攻击之爪30:
                        return new 攻击之爪30();
                    case AccessoryID.攻击之爪50:
                        return new 攻击之爪50();
                }
            }

            return null;
        }

        public static Skill? GetSkill(long id, string name, SkillType type)
        {
            if (type == SkillType.Magic)
            {
                switch ((MagicID)id)
                {
                    case MagicID.冰霜攻击:
                        return new 冰霜攻击();
                }
            }

            if (type == SkillType.Skill)
            {
                switch ((SkillID)id)
                {
                    case SkillID.疾风步:
                        return new 疾风步();
                }
            }

            if (type == SkillType.SuperSkill)
            {
                switch ((SuperSkillID)id)
                {
                    case SuperSkillID.力量爆发:
                        return new 力量爆发();
                    case SuperSkillID.天赐之力:
                        return new 天赐之力();
                    case SuperSkillID.魔法涌流:
                        return new 魔法涌流();
                    case SuperSkillID.三重叠加:
                        return new 三重叠加();
                    case SuperSkillID.变幻之心:
                        return new 变幻之心();
                    case SuperSkillID.精准打击:
                        return new 精准打击();
                    case SuperSkillID.绝对领域:
                        return new 绝对领域();
                    case SuperSkillID.能量毁灭:
                        return new 能量毁灭();
                    case SuperSkillID.迅捷之势:
                        return new 迅捷之势();
                    case SuperSkillID.嗜血本能:
                        return new 嗜血本能();
                    case SuperSkillID.平衡强化:
                        return new 平衡强化();
                    case SuperSkillID.血之狂欢:
                        return new 血之狂欢();
                }
            }

            if (type == SkillType.Passive)
            {
                switch ((PassiveID)id)
                {
                    case PassiveID.META马:
                        return new META马();
                    case PassiveID.心灵之火:
                        return new 心灵之火();
                    case PassiveID.魔法震荡:
                        return new 魔法震荡();
                    case PassiveID.灵能反射:
                        return new 灵能反射();
                    case PassiveID.智慧与力量:
                        return new 智慧与力量();
                    case PassiveID.致命打击:
                        return new 致命打击();
                    case PassiveID.毁灭之势:
                        return new 毁灭之势();
                    case PassiveID.枯竭打击:
                        return new 枯竭打击();
                    case PassiveID.玻璃大炮:
                        return new 玻璃大炮();
                    case PassiveID.累积之压:
                        return new 累积之压();
                    case PassiveID.敏捷之刃:
                        return new 敏捷之刃();
                    case PassiveID.弱者猎手:
                        return new 弱者猎手();
                }
                switch ((ItemPassiveID)id)
                {
                    case ItemPassiveID.攻击之爪:
                        return new 攻击之爪技能();
                }
            }

            return null;
        }

        public static Effect? GetEffect(long id, string name, Skill skill)
        {
            switch (id)
            {
                case 8001:
                    return new ExATK(skill, null, null);
                case 8002:
                    return new ExDEF(skill, null, null);
                case 8003:
                    return new ExSTR(skill, null, null);
                default:
                    break;
            }

            return null;
        }
    }
}
