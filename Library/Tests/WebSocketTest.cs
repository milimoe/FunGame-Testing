using System.Collections;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Library.Common.Network;

namespace Milimoe.FunGame.Testing.Tests
{
    public class WebSocketTest
    {
        public WebSocketTest()
        {
            TaskUtility.NewTask(async () =>
            {
                HTTPClient client = await HTTPClient.Connect("localhost", 22223, false, "ws");
                ArrayList list = [];
                list.Add(new string[] { "oshima.fungame.fastauto" });
                list.Add(false);
                Console.WriteLine(NetworkUtility.JsonSerialize(new SocketObject(Core.Library.Constant.SocketMessageType.Connect, Guid.NewGuid(), list)));
                await client.Send(Core.Library.Constant.SocketMessageType.Connect, list);
                while (true)
                {
                    await Task.Delay(1000);
                    await client.Send(Core.Library.Constant.SocketMessageType.HeartBeat);
                }
            }).OnError(Console.WriteLine);
            while (true)
            {
                string str = Console.ReadLine() ?? "";
            }
        }
    }
}
