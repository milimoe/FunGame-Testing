using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 血之狂欢 : Skill
    {
        public override long Id => 4012;
        public override string Name => "血之狂欢";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 血之狂欢(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 血之狂欢特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 血之狂欢特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"30秒内，获得50%吸血。";
        public override bool TargetSelf => true;

        private double 交换前的额外智力 = 0;
        private double 交换前的额外力量 = 0;

        public override void OnAttributeChanged(Character character)
        {
            if (Skill.Character != null)
            {
                if (Skill.Character.PrimaryAttribute == PrimaryAttribute.INT)
                {
                    double diff = character.ExSTR - 交换前的额外力量;
                    character.ExINT = 交换前的额外力量 + character.BaseSTR + diff;
                }
                else if (Skill.Character.PrimaryAttribute == PrimaryAttribute.STR)
                {
                    double diff = character.ExINT - 交换前的额外智力;
                    character.ExSTR = 交换前的额外智力 + character.BaseINT + diff;
                }
            }
        }

        public override void OnTimeElapsed(Character character, double elapsed)
        {
            if (Skill.Character != null)
            {
                Character c = Skill.Character;
                if (c.HP < c.MaxHP * 0.3)
                {
                    if (c.PrimaryAttribute == PrimaryAttribute.INT)
                    {
                        double pastHP = c.HP;
                        double pastMaxHP = c.MaxHP;
                        double pastMP = c.MP;
                        double pastMaxMP = c.MaxMP;
                        c.PrimaryAttribute = PrimaryAttribute.STR;
                        交换前的额外智力 = c.ExINT;
                        交换前的额外力量 = c.ExSTR;
                        c.ExINT = -c.BaseINT;
                        c.ExSTR = 交换前的额外智力 + c.BaseINT + 交换前的额外力量;
                        c.Recovery(pastHP, pastMP, pastMaxHP, pastMaxMP);
                    }
                }
                else
                {
                    if (c.PrimaryAttribute == PrimaryAttribute.STR)
                    {
                        double pastHP = c.HP;
                        double pastMaxHP = c.MaxHP;
                        double pastMP = c.MP;
                        double pastMaxMP = c.MaxMP;
                        c.PrimaryAttribute = PrimaryAttribute.INT;
                        交换前的额外智力 = c.ExINT;
                        交换前的额外力量 = c.ExSTR;
                        c.ExINT = 交换前的额外力量 + c.BaseSTR + 交换前的额外智力;
                        c.ExSTR = -c.BaseSTR;
                        c.Recovery(pastHP, pastMP, pastMaxHP, pastMaxMP);
                    }
                }
            }
        }
    }
}
