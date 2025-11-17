using System.Globalization;
using System.Windows.Data;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.GameMapTesting
{
    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 如果值为 null，返回空字符串，否则返回其 ToString() 结果
            return value?.ToString()?.Trim() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FirstCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && s.Length > 0)
            {
                return s[0].ToString().ToUpper();
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CharacterToStringWithLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Character character)
            {
                string name = character.ToString();
                if (character.CharacterState == CharacterState.Casting)
                {
                    name += " - 吟唱";
                }
                else if (character.CharacterState == CharacterState.PreCastSuperSkill)
                {
                    name += " - 爆发技";
                }
                return name;
            }
            return "[未知角色]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SkillItemFormatterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = "";
            if (value is Skill skill)
            {
                Character? character = skill.Character;
                name = $"【{(skill.IsSuperSkill ? "爆发技" : (skill.IsMagic ? "魔法" : "战技"))}】{skill.Name}";
                if (skill.CurrentCD > 0)
                {
                    name += $" - 冷却剩余 {skill.CurrentCD:0.##} 秒";
                }
                else if (skill.RealEPCost > 0 && skill.RealMPCost > 0 && character != null && character.EP < skill.RealEPCost && character.MP < skill.RealMPCost)
                {
                    name += $" - 能量/魔法要求 {skill.RealEPCost:0.##} / {skill.RealMPCost:0.##} 点";
                }
                else if (skill.RealEPCost > 0 && character != null && character.EP < skill.RealEPCost)
                {
                    name += $" - 能量不足，要求 {skill.RealEPCost:0.##} 点";
                }
                else if (skill.RealMPCost > 0 && character != null && character.MP < skill.RealMPCost)
                {
                    name += $" - 魔法不足，要求 {skill.RealMPCost:0.##} 点";
                }
            }
            else if (value is Item item)
            {
                name = item.Name;
            }
            else
            {
                name = value?.ToString() ?? name;
            }
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 组合转换器：判断技能或物品是否可用。
    /// 接收 Skill/Item 对象和当前 Character 对象。
    /// </summary>
    public class SkillUsabilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] 应该是 Skill 或 Item 对象
            // values[1] 应该是 PlayerCharacter 对象
            if (values.Length < 2 || values[1] is not Character character)
            {
                return false;
            }

            if (values[0] is Skill s)
            {
                return s.Level > 0 && s.SkillType != SkillType.Passive && s.Enable && !s.IsInEffect && s.CurrentCD == 0 &&
                    ((s.SkillType == SkillType.SuperSkill || s.SkillType == SkillType.Skill) && s.RealEPCost <= character.EP || s.SkillType == SkillType.Magic && s.RealMPCost <= character.MP);
            }
            else if (values[0] is NormalAttack a)
            {
                return a.Level > 0 && a.Enable;
            }
            else if (values[0] is Item i)
            {
                return i.IsActive && i.Skills.Active != null && i.Enable && i.IsInGameItem &&
                    i.Skills.Active.SkillType == SkillType.Item && i.Skills.Active.Enable && !i.Skills.Active.IsInEffect && i.Skills.Active.CurrentCD == 0 &&
                    i.Skills.Active.RealMPCost <= character.MP && i.Skills.Active.RealEPCost <= character.EP;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
