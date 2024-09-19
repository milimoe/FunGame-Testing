using System.Collections.Generic;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Common.Event;
using Milimoe.FunGame.Testing.Items;
using Milimoe.FunGame.Testing.Skills;
using MilimoeFunGame.Testing.Characters;

namespace Addons
{
    public class TestPlugin : Plugin, ILoginEvent
    {
        public override string Name => "fungame.example.plugin";

        public override string Description => "My First Plugin";

        public override string Version => "1.0.0";

        public override string Author => "FunGamer";

        protected override bool BeforeLoad()
        {
            EntityModuleConfig<Character> config = new(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleCharacter)
            {
                { "Oshima", OshimaCharacters.Oshima },
                { "Xinyin", OshimaCharacters.Xinyin },
                { "Yang", OshimaCharacters.Yang },
                { "NanGanyu", OshimaCharacters.NanGanyu },
                { "NiuNan", OshimaCharacters.NiuNan },
                { "Mayor", OshimaCharacters.Mayor },
                { "马猴烧酒", OshimaCharacters.马猴烧酒 },
                { "QingXiang", OshimaCharacters.QingXiang },
                { "QWQAQW", OshimaCharacters.QWQAQW },
                { "ColdBlue", OshimaCharacters.ColdBlue },
                { "绿拱门", OshimaCharacters.绿拱门 },
                { "QuDuoduo", OshimaCharacters.QuDuoduo }
            };
            config.SaveConfig();

            EntityModuleConfig<Skill> config2 = new(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleSkill);
            Character c = Factory.GetCharacter();
            List<Skill> listSkill = [];
            listSkill.Add(new 冰霜攻击(c));
            listSkill.Add(new 疾风步(c));
            listSkill.Add(new META马(c));
            listSkill.Add(new 力量爆发(c));
            listSkill.Add(new 心灵之火(c));
            listSkill.Add(new 天赐之力(c));
            listSkill.Add(new 魔法震荡(c));
            listSkill.Add(new 魔法涌流(c));
            listSkill.Add(new 灵能反射(c));
            listSkill.Add(new 三重叠加(c));
            listSkill.Add(new 智慧与力量(c));
            listSkill.Add(new 变幻之心(c));
            listSkill.Add(new 致命打击(c));
            listSkill.Add(new 精准打击(c));
            listSkill.Add(new 毁灭之势(c));
            listSkill.Add(new 绝对领域(c));
            listSkill.Add(new 枯竭打击(c));
            listSkill.Add(new 能量毁灭(c));
            listSkill.Add(new 玻璃大炮(c));
            listSkill.Add(new 迅捷之势(c));
            listSkill.Add(new 累积之压(c));
            listSkill.Add(new 嗜血本能(c));
            listSkill.Add(new 敏捷之刃(c));
            listSkill.Add(new 平衡强化(c));
            listSkill.Add(new 弱者猎手(c));
            listSkill.Add(new 血之狂欢(c));
            listSkill.Add(new 冰霜攻击(c));
            listSkill.Add(new 疾风步(c));
            foreach (Skill s in listSkill)
            {
                config2.Add(s.Name, s);
            }
            config2.SaveConfig();

            EntityModuleConfig<Item> config3 = new(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleItem)
            {
                { "攻击之爪50", new 攻击之爪50() }
            };
            config3.SaveConfig();

            PluginConfig config4 = new(Name, "config")
            {
                { "flush", 10000 },
                { "oshima", "呵呵了" }
            };
            config4.SaveConfig();
            return true;
        }

        public void AfterLoginEvent(object sender, LoginEventArgs e)
        {
            Console.WriteLine("after");
        }

        public void BeforeLoginEvent(object sender, LoginEventArgs e)
        {
            Console.WriteLine("before");
            // 如果这里设置Cancel = true，将终止登录
            e.Cancel = true;
        }

        public void FailedLoginEvent(object sender, LoginEventArgs e)
        {
            Console.WriteLine("failed");
        }

        public void SucceedLoginEvent(object sender, LoginEventArgs e)
        {
            Console.WriteLine("succeed");
        }
    }
}
