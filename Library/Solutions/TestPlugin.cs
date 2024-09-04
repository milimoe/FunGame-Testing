using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Common.Event;
using Milimoe.FunGame.Core.Library.Constant;

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
            EntityModuleConfig<Character> config = new(ExampleGameModuleConstant.Example, ExampleGameModuleConstant.ExampleCharacter);
            // 构建一个你想要的角色
            Character c = Factory.GetCharacter();
            c.Name = "Oshima";
            c.FirstName = "Shiya";
            c.NickName = "OSM";
            c.MagicType = MagicType.PurityNatural;
            c.InitialHP = 30;
            c.InitialSTR = 20;
            c.InitialAGI = 10;
            c.InitialINT = 5;
            c.InitialATK = 100;
            c.InitialDEF = 10;
            c.InitialSPD = 250;
            config.Add("OSM", c);
            c = Factory.GetCharacter();
            c.Name = "A";
            c.FirstName = "测试1";
            c.NickName = "A";
            c.MagicType = MagicType.Particle;
            c.InitialHP = 25;
            c.InitialSTR = 15;
            c.InitialAGI = 5;
            c.InitialINT = 10;
            c.InitialATK = 80;
            c.InitialDEF = 15;
            c.InitialSPD = 290;
            config.Add("A", c);
            c = Factory.GetCharacter();
            c.Name = "B";
            c.FirstName = "测试2";
            c.NickName = "B";
            c.MagicType = MagicType.Fleabane;
            c.InitialHP = 355;
            c.InitialSTR = 5;
            c.InitialAGI = 5;
            c.InitialINT = 25;
            c.InitialATK = 75;
            c.InitialDEF = 20;
            c.InitialSPD = 320;
            config.Add("B", c);
            c = Factory.GetCharacter();
            c.Name = "C";
            c.FirstName = "测试3";
            c.NickName = "B的复制人";
            c.MagicType = MagicType.Fleabane;
            c.InitialHP = 355;
            c.InitialSTR = 5;
            c.InitialAGI = 5;
            c.InitialINT = 25;
            c.InitialATK = 75;
            c.InitialDEF = 20;
            c.InitialSPD = 320;
            config.Add("C", c);
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
