﻿using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 致命打击 : Skill
    {
        public override long Id => 4006;
        public override string Name => "致命打击";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 致命打击(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 致命打击特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 致命打击特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"暴击伤害提升 30%。";
        public override bool TargetSelf => true;

        public override void OnEffectGained(Character character)
        {
            character.ExCritDMG += 0.3;
        }
    }
}
