using Milimoe.FunGame.Core.Interface.Base;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Model;

namespace Milimoe.FunGame.Testing.Desktop.GameMapTesting
{
    public class TestMap : GameMap
    {
        public override string Name => "TestMap";

        public override string Description => "TestMap";

        public override string Version => "1.0.0";

        public override string Author => "TestUser";

        public override int Length => 12;

        public override int Width => 12;

        public override int Height => 1;

        public override float Size => 32;

        public override GameMap InitGamingQueue(IGamingQueue queue)
        {
            GameMap map = new TestMap();
            map.Load();

            if (queue is GamingQueue gq)
            {
                gq.WriteLine($"地图 {map.Name} 已加载。");
            }

            return map;
        }

        protected override void AfterTimeElapsed(ref double timeToReduce)
        {

        }
    }
}
