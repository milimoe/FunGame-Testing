using System.Text;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Model;
using MilimoeFunGame.Testing.Characters;

namespace Milimoe.FunGame.Testing.Solutions
{
    public class Novels
    {
        public static Dictionary<Character, int> Likability { get; } = [];
        public static Dictionary<string, Func<bool>> Conditions { get; } = [];

        public static bool 攻击力大于20(Character character)
        {
            return character.ATK > 20;
        }

        public static bool 好感度低于50(Character character)
        {
            return Likability[character] > 50;
        }

        public Novels()
        {
            Character main = OshimaCharacters.Oshima;
            Character character = OshimaCharacters.马猴烧酒;
            Likability.Add(character, 100);

            Conditions.Add("马猴烧酒的好感度低于50", () => 好感度低于50(character));
            Conditions.Add("主角攻击力大于20", () => 攻击力大于20(main));
            Conditions.Add("马猴烧酒攻击力大于20", () => 攻击力大于20(character));
            NovelNode node1 = new()
            {
                Key = "node1",
                Name = "声音",
                Content = "听说你在等我？我来了！"
            };
            NovelNode node2 = new()
            {
                Key = "node2",
                Name = main.NickName,
                Content = "什么人！"
            };
            node1.NextNodes.Add(node2);
            node2.Previous = node1;
            NovelOption option1 = new()
            {
                Key = "option1",
                Name = "你好。"
            };
            NovelOption option2 = new()
            {
                Key = "option2",
                Name = "我不认识你。",
                AndPredicates = new()
                {
                    { "主角攻击力大于20", Conditions["主角攻击力大于20"] }
                }
            };
            NovelNode node3 = new()
            {
                Key = "node3",
                Name = character.NickName,
                Content = "你好，我叫【马猴烧酒】！",
                Options = [option1, option2]
            };
            NovelNode node4 = new()
            {
                Key = "node4",
                Name = character.NickName,
                Content = "你的名字是？"
            };
            NovelNode node5 = new()
            {
                Key = "node5",
                Name = character.NickName,
                Content = "滚，谁要认识你？"
            };
            node2.NextNodes.Add(node3);
            option1.Targets.Add(node4);
            option2.Targets.Add(node5);
            NovelNode node6 = new()
            {
                Key = "node6",
                Content = "旁白：示例结束。"
            };
            NovelNode node7 = new()
            {
                Key = "node7",
                Priority = 2,
                Content = "旁白：示例结束，你被马猴烧酒吃掉了。",
                AndPredicates = new()
                {
                    { "主角攻击力大于20", Conditions["主角攻击力大于20"] }
                },
                OrPredicates = new()
                {
                    { "马猴烧酒的好感度低于50", Conditions["马猴烧酒的好感度低于50"] },
                    { "马猴烧酒攻击力大于20", Conditions["马猴烧酒攻击力大于20"] }
                }
            };
            node4.NextNodes.Add(node6);
            node5.NextNodes.Add(node6);
            node5.NextNodes.Add(node7);

            NovelConfig config = new("novel1", "chapter1")
            {
                { node1.Key, node1 },
                { node2.Key, node2 },
                { node3.Key, node3 },
                { node4.Key, node4 },
                { node5.Key, node5 },
                { node6.Key, node6 },
                { node7.Key, node7 }
            };
            config.SaveConfig();

            NovelConfig config2 = new("novel1", "chapter1");
            config2.LoadConfig(Conditions);

            foreach (NovelNode node in config2.Values)
            {
                StringBuilder builder = new();
                builder.AppendLine("== 节点：" + node.Key + " ==");

                if (node.AndPredicates.Union(node.OrPredicates).Any())
                {
                    builder.AppendLine("对话触发条件（需满足以下所有条件）：");
                    int count = 0;
                    if (node.AndPredicates.Count > 0)
                    {
                        count++;
                        builder.AppendLine(count + ". " + "满足以下所有子条件：");
                        int subCount = 0;
                        foreach (string ap in node.AndPredicates.Keys)
                        {
                            subCount++;
                            builder.AppendLine("(" + subCount + ") " + ap);
                        }
                    }
                    if (node.OrPredicates.Count > 0)
                    {
                        count++;
                        builder.AppendLine(count + ". " + "满足以下任意一个子条件：");
                        int subCount = 0;
                        foreach (string op in node.OrPredicates.Keys)
                        {
                            subCount++;
                            builder.AppendLine("(" + subCount + ") " + op);
                        }
                    }
                }

                if (node.Name != "")
                {
                    builder.Append(node.Name + "说：");
                }
                builder.AppendLine(node.Content);

                if (node.Options.Count > 0)
                {
                    builder.AppendLine("选项：");
                    int count = 0;
                    foreach (NovelOption option in node.Options)
                    {
                        count++;
                        builder.AppendLine(count + ". " + option.Name + "【可跳转：" + string.Join("，", option.Targets.Select(t => t.Key)) + "】");
                        if (option.AndPredicates.Union(option.OrPredicates).Any())
                        {
                            builder.AppendLine("选项显示条件（需满足以下所有条件）：");
                            int optionCount = 0;
                            if (option.AndPredicates.Count > 0)
                            {
                                optionCount++;
                                builder.AppendLine(optionCount + ". " + "满足以下所有子条件：");
                                int subCount = 0;
                                foreach (string ap in option.AndPredicates.Keys)
                                {
                                    subCount++;
                                    builder.AppendLine("(" + subCount + ") " + ap);
                                }
                            }
                            if (option.OrPredicates.Count > 0)
                            {
                                optionCount++;
                                builder.AppendLine(optionCount + ". " + "满足以下任意一个子条件：");
                                int subCount = 0;
                                foreach (string op in option.OrPredicates.Keys)
                                {
                                    subCount++;
                                    builder.AppendLine("(" + subCount + ") " + op);
                                }
                            }
                        }
                    }
                }

                if (node.NextNodes.Count > 0)
                {
                    builder.AppendLine("下一句对话：");
                    int count = 0;
                    foreach (NovelNode next in node.NextNodes)
                    {
                        count++;
                        builder.AppendLine(count + ". " + next.Key);
                    }
                }

                Console.WriteLine(builder.ToString());
            }
        }
    }
}
