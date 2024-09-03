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
            c.BaseHP = 30;
            c.BaseSTR = 20;
            c.BaseAGI = 10;
            c.BaseINT = 5;
            c.BaseATK = 100;
            c.BaseDEF = 10;
            c.SPD = 250;
            c.Init();
            config.Add("OSM", c);
            c = Factory.GetCharacter();
            c.Name = "A";
            c.FirstName = "测试1";
            c.NickName = "A";
            c.MagicType = MagicType.Particle;
            c.BaseHP = 25;
            c.BaseSTR = 15;
            c.BaseAGI = 5;
            c.BaseINT = 10;
            c.BaseATK = 80;
            c.BaseDEF = 15;
            c.SPD = 290;
            c.Init();
            config.Add("A", c);
            c = Factory.GetCharacter();
            c.Name = "B";
            c.FirstName = "测试2";
            c.NickName = "B";
            c.MagicType = MagicType.Fleabane;
            c.BaseHP = 355;
            c.BaseSTR = 5;
            c.BaseAGI = 5;
            c.BaseINT = 25;
            c.BaseATK = 75;
            c.BaseDEF = 20;
            c.SPD = 320;
            c.Init();
            config.Add("B", c);
            c = Factory.GetCharacter();
            c.Name = "C";
            c.FirstName = "测试3";
            c.NickName = "B的复制人";
            c.MagicType = MagicType.Fleabane;
            c.BaseHP = 355;
            c.BaseSTR = 5;
            c.BaseAGI = 5;
            c.BaseINT = 25;
            c.BaseATK = 75;
            c.BaseDEF = 20;
            c.SPD = 320;
            c.Init();
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
