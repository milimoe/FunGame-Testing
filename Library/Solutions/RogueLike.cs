using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;
using Milimoe.FunGame.Testing.Tests;
using Oshima.FunGame.OshimaModules.Characters;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Solutions
{
    /// <summary>
    /// 客户端逻辑层
    /// </summary>
    /// <param name="dispatcher"></param>
    public class RogueLike(RogueLikeDispatcher dispatcher)
    {
        public RogueLikeDispatcher Dispatcher { get; set; } = dispatcher;

        public Dictionary<string, Func<string[], Task>> Commands { get; set; } = [];
        public Dictionary<string, string> CommandAlias { get; set; } = [];
        public User User { get; set; } = General.UnknownUserInstance;
        public bool Running { get; set; } = false;
        public bool InGame { get; set; } = false;
        public Lock Lock { get; } = new();

        /// <summary>
        /// 为空表示没有正在进行的询问
        /// </summary>
        private Guid _currentRequestGuid = Guid.Empty;
        /// <summary>
        /// 询问结果，和上面的字段配合使用
        /// </summary>
        private TaskCompletionSource<DataRequestArgs> _requestTcs = new();

        public async Task StartGame()
        {
            Running = true;
            Dispatcher.StartServer();
            AddDialog("？？？", "探索者，欢迎入职【永恒方舟计划】，我是您的专属 AI，协助您前往指定任务地点开展勘测工作。请问您的名字是？");
            string username;
            do
            {
                username = await GetTextResultsAsync([]);
                if (username == "")
                {
                    WriteLine("请输入不为空的字符串。");
                }
            }
            while (username == "");
            DataRequestArgs response = await DataRequest(new("createuser")
            {
                Data = new()
                {
                    { "username", username }
                }
            });
            if (response.Data.TryGetValue("user", out object? value) && value is User user)
            {
                User = user;
            }
            // 初始化菜单指令
            Commands["quit"] = async (args) =>
            {
                Running = false;
                WriteLine("游戏结束！");
            };
            AddCommandAlias("quit", "退出", "exit", "!q");
            Commands["test"] = async (args) =>
            {
                WriteLine("打木桩测试");
                Character enemy = new CustomCharacter(0, "木桩")
                {
                    Level = User.Inventory.MainCharacter.Level,
                    CharacterState = CharacterState.NotActionable
                };
                enemy.Recovery();
                List<string> strings = (await FunGameActionQueue.NewAndStartGame([User.Inventory.MainCharacter, enemy], showAllRound: true)).Result;
                foreach (string str in strings)
                {
                    WriteLine(str);
                }
            };
            Commands["help"] = Help;
            Commands["start"] = async (args) =>
            {
                if (InGame)
                {
                    return;
                }

                InGame = true;
                await Start();
                InGame = false;
            };
            AddCommandAlias("help", "菜单", "h", "帮助");
            AddCommandAlias("start", "开局");
            AddDialog("柔哥", $"让我再次欢迎您，{username}。入职手续已办理完毕，接下来，您可以使用菜单了。请您记住我的名字：【柔哥】。");
            await Help([]);
            while (Running)
            {
                if (InGame)
                {
                    await Task.Delay(1000);
                    continue;
                }
                string input = await GetTextResultsAsync([]);
                string[] strings = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length == 0)
                {
                    continue;
                }
                string command = strings[0];
                string[] args = [.. strings.Skip(1)];
                if (Commands.TryGetValue(command.ToLower(), out Func<string[], Task>? func) && func != null)
                {
                    await func.Invoke(args);
                }
                else if (CommandAlias.TryGetValue(command.ToLower(), out string? actualCommand) && actualCommand != null)
                {
                    if (Commands.TryGetValue(actualCommand, out Func<string[], Task>? func2) && func2 != null)
                    {
                        await func2.Invoke(args);
                    }
                }
                else
                {
                    WriteLine("未知指令");
                }
            }
        }

        public Action<string> WriteLineHandler
        {
            get => field ?? Console.WriteLine;
            set => field = value;
        }

        public delegate Task ReadInput<T>(Dictionary<string, object> args, List<T> results);
        public delegate Task<InquiryResponse> ReadInputInGameResponse(InquiryOptions options);
        public event ReadInput<string>? ReadInputStringHandler;
        public event ReadInput<double>? ReadInputNumberHandler;
        public event ReadInputInGameResponse? ReadInputInGameResponseHandler;

        public async Task<List<string>> GetChoiceResultsAsync(InquiryOptions options, Dictionary<string, object> args)
        {
            int index = 0;
            Dictionary<int, string> indexToChoice = [];
            foreach (string choice in options.Choices.Keys)
            {
                index++;
                indexToChoice[index] = choice;
                WriteLine($"{index}. {choice}：{options.Choices[choice]}");
            }
            WriteLine($"--- {options.Topic} ---");
            List<string> results = [];
            if (ReadInputStringHandler != null)
            {
                await ReadInputStringHandler.Invoke(args, results);
            }
            return [.. indexToChoice.Where(kv => results.Contains(kv.Key.ToString())).Select(kv => kv.Value)];
        }

        public async Task<List<double>> GetNumberResultsAsync(Dictionary<string, object> args)
        {
            List<double> results = [];
            if (ReadInputNumberHandler != null)
            {
                await ReadInputNumberHandler.Invoke(args, results);
            }
            return results;
        }

        public async Task<string> GetTextResultsAsync(Dictionary<string, object> args)
        {
            List<string> results = [];
            if (ReadInputStringHandler != null)
            {
                await ReadInputStringHandler.Invoke(args, results);
            }
            return results.FirstOrDefault() ?? "";
        }

        public async Task<DataRequestArgs> DataRequest(DataRequestArgs args)
        {
            _requestTcs.TrySetCanceled();
            _currentRequestGuid = Guid.NewGuid();

            _requestTcs = new TaskCompletionSource<DataRequestArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

            await Dispatcher.DataRequest(_currentRequestGuid, args);

            return await _requestTcs.Task;
        }

        public async Task DataRequestComplete(Guid guid, DataRequestArgs response)
        {
            if (guid == _currentRequestGuid)
            {
                _currentRequestGuid = Guid.Empty;
                _requestTcs.TrySetResult(response);
            }
        }

        public async Task<InquiryResponse> GetInGameResponse(InquiryOptions options)
        {
            if (ReadInputInGameResponseHandler is null) return new(options);
            return await ReadInputInGameResponseHandler.Invoke(options);
        }

        public void WriteLine(string message = "")
        {
            WriteLineHandler(message);
        }

        public void AddDialog(string speaker, string message)
        {
            WriteLine(BuildSpeakerDialog(speaker, message));
        }

        public static string BuildSpeakerDialog(string speaker, string message)
        {
            return $"【{speaker}】{message}";
        }

        private void AddCommandAlias(string command, params string[] aliases)
        {
            foreach (string alias in aliases)
            {
                CommandAlias[alias] = command;
            }
        }

        public string GetCommandAliases(string command)
        {
            string[] alias = [.. CommandAlias.Where(kv => kv.Value == command).Select(kv => kv.Key)];
            return alias.Length > 0 ? $"（替代：{string.Join("，", alias)}）" : "";
        }

        private async Task Help(string[] args)
        {
            WriteLine($"可用指令：{string.Join("，", Commands.Keys.Select(c => $"{c}{GetCommandAliases(c)}"))}");
        }

        private async Task Start()
        {
            await Dispatcher.CreateGameLoop(User.Username);
        }
    }
}
