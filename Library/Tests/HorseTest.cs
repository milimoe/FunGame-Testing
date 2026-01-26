using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Oshima.FunGame.OshimaModules.Models;
using Oshima.FunGame.OshimaServers.Model;

namespace Milimoe.FunGame.Testing.Tests
{
    public class HorseTest
    {
        public static void HorseTest1()
        {
            List<string> msgs = [];
            Room room = Factory.GetRoom(1, "1", gameMap: "1");
            room.Name = "赛马房间";
            Dictionary<User, int> points = [];
            for (int i = 0; i < 9; i++)
            {
                User user = Factory.GetUser();
                user.Id = i;
                user.Username = FunGameConstant.GenerateRandomChineseUserName();
                room.UserAndIsReady.Add(user, true);
                points[user] = 0;
                if (i == 0) room.RoomMaster = user;
            }
            int plays = 0;
            while (true)
            {
                plays++;
                Dictionary<long, int> racingPoints = HorseRacing.RunHorseRacing(msgs, room);
                foreach (long userId in racingPoints.Keys)
                {
                    if (points.Keys.FirstOrDefault(u => u.Id == userId) is User user)
                    {
                        points[user] += racingPoints[userId];
                    }
                }
                Console.WriteLine(string.Join("\r\n", msgs));
                Console.WriteLine($"\r\n====赛马积分排行榜====\r\n比赛场次：{plays} 场\r\n" + string.Join("\r\n", points.OrderByDescending(kv => kv.Value).Select((kv, index) => (index + 1) + ". " + kv.Key + "：" + kv.Value + " 分")));
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }
    }
}
