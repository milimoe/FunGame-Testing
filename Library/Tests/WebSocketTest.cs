using System.Collections;
using Milimoe.FunGame.Core.Api.Transmittal;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Controller;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Network;
using Milimoe.FunGame.Core.Library.Constant;
using Oshima.FunGame.OshimaModules;

namespace Milimoe.FunGame.Testing.Tests
{
    public class WebSocketTestRunTime : RunTimeController
    {
        public Guid LoginKey { get; set; } = Guid.Empty;
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
            if (HTTPClient is null || Quit)
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
            if (str == "wstest")
            {
                await HTTPClient.Send(SocketMessageType.AnonymousGameServer, OshimaGameModuleConstant.Anonymous);
            }
            if (str == "scadd")
            {
                Dictionary<string, object> data = [];
                data.Add("command", "scadd");
                data.Add("qq", 1);
                data.Add("sc", 1);
                await HTTPClient.Send(SocketMessageType.AnonymousGameServer, OshimaGameModuleConstant.Anonymous, data);
            }
            if (str == "sclist")
            {
                Dictionary<string, object> data = [];
                data.Add("command", "sclist");
                await HTTPClient.Send(SocketMessageType.AnonymousGameServer, OshimaGameModuleConstant.Anonymous, data);
            }
            if (str == "wsclose")
            {
                await HTTPClient.Send(SocketMessageType.EndGame);
            }
            if (str == "fungametest")
            {
                Console.WriteLine(string.Join("\r\n", await HTTPClient.HttpGet<List<string>>("http://localhost:5000/fungame/test") ?? []));
            }
            if (str == "quit")
            {
                Quit = true;
            }
            if (str == "disc")
            {
                if (LoginKey != Guid.Empty) await LogOut();
                await HTTPClient.Send(SocketMessageType.Disconnect);
            }
            if (str == "login")
            {
                string username = "test";
                string password = "123123";
                await Login(username, password);
            }
            if (str == "reg")
            {
                string username = "test2";
                string password = "123123";
                string email = "1231232@qq.com";
                await Reg(username, password, email);
            }
            if (str == "logout")
            {
                await LogOut();
            }
        }

        public async Task Reg(string username, string password, string email)
        {
            DataRequest request = NewDataRequest(DataRequestType.Reg_Reg);
            request.AddRequestData("username", username);
            request.AddRequestData("password", Encryption.HmacSha512(password, username));
            request.AddRequestData("email", email);
            request.AddRequestData("verifycode", ""); // 初始验证码为空，请求发送验证码
            await request.SendRequestAsync();

            if (request.Result == RequestResult.Success)
            {
                string msg = request.GetResult<string>("msg") ?? "";
                RegInvokeType type = request.GetResult<RegInvokeType>("type");
                if (msg != "") Console.WriteLine(msg);

                if (type == RegInvokeType.InputVerifyCode)
                {
                    bool success = false;
                    do
                    {
                        Console.Write("请输入收到的验证码：");
                        string verifycode = Console.ReadLine() ?? "";
                        if (verifycode == "q!")
                        {
                            Console.WriteLine("取消注册操作。");
                            break;
                        }
                        request = NewDataRequest(DataRequestType.Reg_Reg);
                        request.AddRequestData("username", username);
                        request.AddRequestData("password", Encryption.HmacSha512(password, username));
                        request.AddRequestData("email", email);
                        request.AddRequestData("verifycode", verifycode);
                        await request.SendRequestAsync();

                        if (request.Result == RequestResult.Success)
                        {
                            msg = request.GetResult<string>("msg") ?? "";
                            success = request.GetResult<bool>("success");
                            if (msg != "") Console.WriteLine(msg);
                        }
                        else
                        {
                            Console.WriteLine("请求服务器失败！");
                        }
                    } while (!success);
                }
                else if (type == RegInvokeType.DuplicateUserName)
                {
                    Console.WriteLine("用户名已被注册！");
                }
                else if (type == RegInvokeType.DuplicateEmail)
                {
                    Console.WriteLine("邮箱已被注册！");
                }
            }
            else
            {
                Console.WriteLine("注册请求失败！");
            }
        }

        public async Task Login(string username, string password)
        {
            DataRequest request = NewDataRequest(DataRequestType.Login_Login);
            request.AddRequestData("username", username);
            request.AddRequestData("password", Encryption.HmacSha512(password, username));
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
                                    LoginKey = loginKey;
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task LogOut()
        {
            DataRequest request = NewDataRequest(DataRequestType.RunTime_Logout);
            request.AddRequestData("key", LoginKey);
            await request.SendRequestAsync();

            if (request.Result == RequestResult.Success)
            {
                string msg = request.GetResult<string>("msg") ?? "";
                Guid key = request.GetResult<Guid>("key");
                Console.WriteLine(msg);
                if (key != Guid.Empty)
                {
                    LoginKey = Guid.Empty;
                    Console.WriteLine("退出登录成功！");
                }
                else
                {
                    Console.WriteLine("退出登录失败！");
                }
            }
            else
            {
                Console.WriteLine("退出登录请求失败！");
            }
        }

        public override void Error(Exception e)
        {
            Console.WriteLine(e.ToString());
            TaskUtility.NewTask(async () =>
            {
                if (HTTPClient != null)
                {
                    await HTTPClient.Send(SocketMessageType.Disconnect);
                    Close_WebSocket();
                }
            });
        }

        public override void WritelnSystemInfo(string msg, LogLevel level, bool useLevel)
        {
            Console.WriteLine(msg);
        }

        protected override void SocketHandler_Disconnect(SocketObject ServerMessage)
        {
            Console.WriteLine("断开服务器连接成功");
        }

        protected override void SocketHandler_HeartBeat(SocketObject ServerMessage)
        {
            Console.WriteLine("服务器连接成功");
        }

        protected override void SocketHandler_AnonymousGameServer(SocketObject ServerMessage)
        {
            Dictionary<string, object> data = ServerMessage.GetParam<Dictionary<string, object>>(0) ?? [];
            if (data.Count > 0)
            {
                string msg = NetworkUtility.JsonDeserializeFromDictionary<string>(data, "msg") ?? "";
                if (msg != "") Console.WriteLine(msg);
            }
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
                        Console.WriteLine("退出ws测试！");
                        Console.ReadLine();
                    }
                });
            }
        }
    }
}
