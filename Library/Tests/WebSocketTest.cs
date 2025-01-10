using System.Collections;
using Milimoe.FunGame.Core.Api.Transmittal;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Controller;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Network;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Tests
{
    public class WebSocketTestRunTime : RunTimeController
    {
        public bool Quit { get; set; } = false;

        public override bool BeforeConnect(ref string addr, ref int port, ArrayList args)
        {
            string[] strings = ["oshima.fungame.fastauto"];
            args.Add(strings);
            args.Add(false);
            return true;
        }

        public override void AfterConnect(ArrayList ConnectArgs)
        {
            string msg = ConnectArgs[1]?.ToString() ?? "";
            string serverName = ConnectArgs[2]?.ToString() ?? "";
            string notice = ConnectArgs[3]?.ToString() ?? "";
            if (msg != "") Console.WriteLine(msg);
            if (serverName != "") Console.WriteLine(serverName);
            if (notice != "") Console.WriteLine(notice);
        }

        public async Task StartTest()
        {
            await ConnectAsync(TransmittalType.WebSocket, "localhost", 5000, false, "ws");
        }

        public async Task CheckInput(string str)
        {
            if (WebSocket is null || Quit)
            {
                if (str == "retry")
                {
                    if (await ConnectAsync(TransmittalType.WebSocket, "localhost", 5000, false, "ws") == ConnectResult.Success)
                    {
                        Console.WriteLine("重连成功！");
                    }
                    else
                    {
                        Console.WriteLine("重连失败！");
                    }
                }
                return;
            }
            if (str == "quit")
            {
                await WebSocket.Send(SocketMessageType.Disconnect);
            }
            if (str == "login")
            {
                DataRequest request = NewDataRequest(DataRequestType.Login_Login);
                request.AddRequestData("username", "mili");
                request.AddRequestData("password", Encryption.HmacSha512("123123", "Mili"));
                request.AddRequestData("autokey", "");
                request.AddRequestData("key", Guid.Empty);
                await request.SendRequestAsync();
                if (request.Result == RequestResult.Success)
                {
                    string msg = request.GetResult<string>("msg") ?? "";
                    if (msg != "")
                    {
                        Console.WriteLine(msg);
                    }
                    else
                    {
                        Guid loginKey = request.GetResult<Guid>("key");
                        if (loginKey != Guid.Empty)
                        {
                            request = NewDataRequest(DataRequestType.Login_Login);
                            request.AddRequestData("username", "username");
                            request.AddRequestData("password", Encryption.HmacSha512("password", "username"));
                            request.AddRequestData("autokey", "");
                            request.AddRequestData("key", loginKey);
                            await request.SendRequestAsync();
                            if (request.Result == RequestResult.Success)
                            {
                                msg = request.GetResult<string>("msg") ?? "";
                                if (msg != "")
                                {
                                    Console.WriteLine(msg);
                                }
                                else
                                {
                                    User? user = request.GetResult<User>("user");
                                    if (user != null)
                                    {
                                        Console.WriteLine("登录用户完成：" + user.Username);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Error(Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        public override void WritelnSystemInfo(string msg)
        {
            Console.WriteLine(msg);
        }

        protected override void SocketHandler_Disconnect(SocketObject ServerMessage)
        {
            Quit = true;
        }

        protected override void SocketHandler_HeartBeat(SocketObject ServerMessage)
        {
            Console.WriteLine("服务器连接成功");
        }
    }

    public class WebSocketTest
    {
        public WebSocketTest()
        {
            WebSocketTestRunTime runtime = new();
            TaskUtility.NewTask(runtime.StartTest).OnError(Console.WriteLine);
            while (!runtime.Quit)
            {
                string str = Console.ReadLine() ?? "";
                TaskUtility.NewTask(async () => await runtime.CheckInput(str)).OnError(Console.WriteLine).OnCompleted(() =>
                {
                    if (str == "quit")
                    {
                        Console.WriteLine("断开服务器连接成功！");
                        Console.ReadLine();
                    }
                });
            }
        }
    }
}
