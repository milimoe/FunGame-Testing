using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Oshima.FunGame.OshimaServers.Model;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Tests
{
    public class HorseTest
    {
        public static void HorseTest1()
        {
            List<string> msgs = [];
            while (true)
            {
                Room room = Factory.GetRoom(1, "1", gameMap: "1");
                room.Name = "赛马房间";
                for (int i = 0; i < 5; i++)
                {
                    User user = Factory.GetUser();
                    user.Username = FunGameService.GenerateRandomChineseUserName();
                    room.UserAndIsReady.Add(user, true);
                    if (i == 0) room.RoomMaster = user;
                }
                HorseRacing.RunHorseRacing(msgs, room);
                Console.WriteLine(string.Join("\r\n", msgs));
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }
    }
}
