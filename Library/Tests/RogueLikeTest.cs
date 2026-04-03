using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Model;
using Milimoe.FunGame.Testing.Solutions;
using Oshima.FunGame.OshimaModules.Regions;

namespace Milimoe.FunGame.Testing.Tests
{
    /// <summary>
    /// 肉鸽客户端原型测试
    /// </summary>
    public class RogueLikeTest
    {
        public RogueLike RogueLike { get; set; }
        public RogueLikeServer RogueLikeServer { get; set; }

        public RogueLikeTest()
        {
            RogueLikeDispatcher dispatcher = new();
            RogueLike = new(dispatcher);
            RogueLikeServer = new(dispatcher);
            dispatcher.RogueLikeInstance = RogueLike;
            dispatcher.RogueLikeServer = RogueLikeServer;
            RogueLike.ReadInputStringHandler += RogueLike_ReadInputStringHandler;
            RogueLike.ReadInputNumberHandler += RogueLike_ReadInputNumberHandler;
            RogueLike.ReadInputInGameResponseHandler += RogueLike_ReadInputInGameResponseHandler;
        }

        /// <summary>
        /// 此方法完全自主处理options并输入
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private async Task<InquiryResponse> RogueLike_ReadInputInGameResponseHandler(InquiryOptions options)
        {
            InquiryResponse response = new(options);

            switch (options.InquiryType)
            {
                case Core.Library.Constant.InquiryType.Choice:
                case Core.Library.Constant.InquiryType.BinaryChoice:
                    {
                        int index = 0;
                        Dictionary<int, string> indexToChoice = [];
                        foreach (string choice in options.Choices.Keys)
                        {
                            index++;
                            indexToChoice[index] = choice;
                            RogueLike.WriteLine($"{index}. {choice}：{options.Choices[choice]}");
                        }
                        RogueLike.WriteLine($"--- {options.Topic} ---");
                        bool resolve = false;
                        while (!resolve)
                        {
                            RogueLike.WriteLine($"选择一个选项（输入序号，输入 !c 表示取消）：");
                            string result = (await Console.In.ReadLineAsync())?.Trim() ?? "";
                            if (result == "!c")
                            {
                                response.Cancel = true;
                                resolve = true;
                            }
                            else if (int.TryParse(result, out int inputIndex) && indexToChoice.TryGetValue(inputIndex, out string? value))
                            {
                                response.Choices = [value];
                                resolve = true;
                            }
                        }
                        break;
                    }
                case Core.Library.Constant.InquiryType.MultipleChoice:
                    {
                        int index = 0;
                        Dictionary<int, string> indexToChoice = [];
                        foreach (string choice in options.Choices.Keys)
                        {
                            index++;
                            indexToChoice[index] = choice;
                            RogueLike.WriteLine($"{index}. {choice}：{options.Choices[choice]}");
                        }
                        RogueLike.WriteLine($"--- {options.Topic} ---");
                        bool resolve = false;
                        while (!resolve)
                        {
                            RogueLike.WriteLine($"选择多个选项（输入序号，空格分隔，包含 !c 时表示取消）：");
                            string result = (await Console.In.ReadLineAsync())?.Trim() ?? "";
                            if (result.Contains("!c"))
                            {
                                response.Cancel = true;
                                break;
                            }
                            string[] strings = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            foreach (string str in strings)
                            {
                                if (int.TryParse(str, out int inputIndex) && indexToChoice.TryGetValue(inputIndex, out string? value))
                                {
                                    response.Choices.Add(value);
                                }
                            }
                            if (response.Choices.Count > 0)
                            {
                                resolve = true;
                            }
                        }
                        break;
                    }
                case Core.Library.Constant.InquiryType.TextInput:
                    {
                        RogueLike.WriteLine($"--- {options.Topic} ---");
                        bool resolve = false;
                        while (!resolve)
                        {
                            RogueLike.WriteLine("输入回应（输入 !c 表示取消）：");
                            string result = (await Console.In.ReadLineAsync())?.Trim() ?? "";
                            if (result == "!c")
                            {
                                response.Cancel = true;
                                resolve = true;
                            }
                            else if (result != "")
                            {
                                response.TextResult = result;
                                resolve = true;
                            }
                        }
                        break;
                    }
                case Core.Library.Constant.InquiryType.NumberInput:
                    {
                        RogueLike.WriteLine($"--- {options.Topic} ---");
                        bool resolve = false;
                        while (!resolve)
                        {
                            RogueLike.WriteLine("输入结果（输入 !c 表示取消）：");
                            string result = await Console.In.ReadLineAsync() ?? "";
                            if (result.Trim() == "!c")
                            {
                                response.Cancel = true;
                                resolve = true;
                            }
                            else if (double.TryParse(result, out double doubleResult))
                            {
                                response.NumberResult = doubleResult;
                                resolve = true;
                            }
                        }
                        break;
                    }
                case Core.Library.Constant.InquiryType.Custom:
                    break;
                default:
                    break;
            }

            return response;
        }

        private async Task RogueLike_ReadInputStringHandler(Dictionary<string, object> args, List<string> results)
        {
            string input = await Console.In.ReadLineAsync() ?? "";
            if (input.Trim().Equals("!c", StringComparison.CurrentCultureIgnoreCase))
            {
                args["cancel"] = true;
            }
            results.Add(input.Trim());
        }

        private async Task RogueLike_ReadInputNumberHandler(Dictionary<string, object> args, List<double> results)
        {
            string input = await Console.In.ReadLineAsync() ?? "";
            if (input.Trim().Equals("!c", StringComparison.CurrentCultureIgnoreCase))
            {
                args["cancel"] = true;
            }
            if (double.TryParse(input.Trim(), out double result))
            {
                results.Add(result);
            }
        }
    }

    /// <summary>
    /// 仅本地测试原型用，实际使用时需替换为网络层
    /// </summary>
    public class RogueLikeDispatcher
    {
        public RogueLike? RogueLikeInstance { get; set; } = null;
        public RogueLikeServer? RogueLikeServer { get; set; } = null;

        public bool Running => RogueLikeInstance?.Running ?? false;

        public void WriteLine(string str)
        {
            if (RogueLikeInstance is null) return;
            RogueLikeInstance.WriteLine(str);
        }

        public async Task<List<string>> GetChoiceResultsAsync(InquiryOptions options, Dictionary<string, object> args)
        {
            if (RogueLikeInstance is null) return [];
            return await RogueLikeInstance.GetChoiceResultsAsync(options, args);
        }

        public async Task<List<double>> GetNumberResultsAsync(Dictionary<string, object> args)
        {
            if (RogueLikeInstance is null) return [];
            return await RogueLikeInstance.GetNumberResultsAsync(args);
        }

        public async Task<string> GetTextResultsAsync(Dictionary<string, object> args)
        {
            if (RogueLikeInstance is null) return "";
            return await RogueLikeInstance.GetTextResultsAsync(args);
        }

        public async Task<InquiryResponse> GetInGameResponse(InquiryOptions options)
        {
            if (RogueLikeInstance is null) return new(options);
            return await RogueLikeInstance.GetInGameResponse(options);
        }

        public async Task DataRequestComplete(Guid guid, DataRequestArgs response)
        {
            if (RogueLikeInstance is null) return;
            await RogueLikeInstance.DataRequestComplete(guid, response);
        }

        public async Task DataRequest(Guid guid, DataRequestArgs args)
        {
            if (RogueLikeServer is null) return;
            RogueLikeServer.ReceiveDataRequest(guid, args);
            await Task.CompletedTask;
        }

        public void StartServer()
        {
            RogueLikeServer?.Guard ??= Task.Run(RogueLikeServer.DataRequestGuard);
        }

        public async Task CreateGameLoop(string username)
        {
            if (RogueLikeServer is null) return;
            await RogueLikeServer.CreateGameLoop(username);
        }
    }

    public class RogueLikeGameData(Character character)
    {
        public RogueState RogueState { get; set; } = RogueState.Init;
        public Character Character { get; set; } = character;
        public int Chapter { get; set; } = 1;
        public OshimaRegion? CurrentRegion { get; set; } = null;
        public string CurrentArea { get; set; } = "";
        public int RoomId { get; set; } = -1;
        public OshimaRegion? Chapter1Region { get; set; } = null;
        public OshimaRegion? Chapter2Region { get; set; } = null;
        public OshimaRegion? Chapter3Region { get; set; } = null;
    }

    public class DataRequestArgs(string type)
    {
        public string RequestType { get; } = type;
        public bool Success { get; set; } = true;
        public Dictionary<string, object> Data { get; set; } = [];
    }
}
