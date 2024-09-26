using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Library.Common.Network;

namespace Milimoe.FunGame.Testing.Tests
{
    public class WebSocketTest
    {
        public WebSocketTest()
        {
            bool quit = false;
            HTTPClient? client = null;
            TaskUtility.NewTask(async () =>
            {
                string[] strings = ["oshima.fungame.fastauto"];
                client = await HTTPClient.Connect("localhost", 5000, false, "ws", strings, false);
                if (client.Connected)
                {
                    client.AddSocketObjectHandler(obj =>
                    {
                        Console.WriteLine(NetworkUtility.JsonSerialize(obj));
                        if (obj.SocketType == Core.Library.Constant.SocketMessageType.Connect)
                        {
                            client.Token = obj.GetParam<Guid>(2);
                        }
                        if (obj.SocketType == Core.Library.Constant.SocketMessageType.Disconnect)
                        {
                            quit = true;
                            client.Close();
                        }
                    });
                }
            }).OnError(Console.WriteLine);
            while (!quit)
            {
                string str = Console.ReadLine() ?? "";
                if (str == "quit")
                {
                    client?.Send(Core.Library.Constant.SocketMessageType.Disconnect);
                }
            }
        }
    }
}
