using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Common.Event;
using MilimoeFunGame.Testing.Characters;

namespace Addons
{
    public class TestPlugin : Plugin, ILoginEvent
    {
        public override string Name => "fungame.example.plugin";

        public override string Description => "My First Plugin";

        public override string Version => "1.0.0";

        public override string Author => "FunGamer";

        protected override bool BeforeLoad(params object[] objs)
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
            //Character c = Factory.GetCharacter();
            List<Skill> listSkill = [];
            foreach (Skill s in listSkill)
            {
                config2.Add(s.Name, s);
            }
            config2.SaveConfig();

            EntityModuleConfig<Item> config3 = new(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleItem)
            {

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
    }
}
