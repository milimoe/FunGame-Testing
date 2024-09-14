using FunGame.Testing.Characters;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Common.Event;

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
                { "Oshima", Characters.Oshima },
                { "Xinyin", Characters.Xinyin },
                { "Yang", Characters.Yang },
                { "NanGanyu", Characters.NanGanyu },
                { "NiuNan", Characters.NiuNan },
                { "Mayor", Characters.Mayor },
                { "马猴烧酒", Characters.马猴烧酒 },
                { "QingXiang", Characters.QingXiang },
                { "QWQAQW", Characters.QWQAQW },
                { "ColdBlue", Characters.ColdBlue },
                { "绿拱门", Characters.绿拱门 },
                { "QuDuoduo", Characters.QuDuoduo }
            };
            
            config.SaveConfig();
            PluginConfig config2 = new(Name, "config")
            {
                { "flush", 10000 },
                { "oshima", "呵呵了" }
            };
            config2.SaveConfig();
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
