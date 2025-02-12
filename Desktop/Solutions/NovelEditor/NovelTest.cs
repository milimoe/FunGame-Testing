using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Model;

namespace Milimoe.FunGame.Testing.Desktop.Solutions.NovelEditor
{
    internal class NovelTest
    {
        internal static void CreateNovels()
        {
            NovelNode node1 = new()
            {
                Key = "node1",
                Name = "声音",
                Content = "听说你在等我？我来了！"
            };
            NovelNode node2 = new()
            {
                Key = "node2",
                Name = NovelEditor.MainCharacter.NickName,
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
                    { "主角攻击力大于20", NovelEditor.Conditions["主角攻击力大于20"] }
                }
            };
            NovelNode node3 = new()
            {
                Key = "node3",
                Name = NovelEditor.马猴烧酒.NickName,
                Content = "你好，我叫【马猴烧酒】！",
                Options = [option1, option2]
            };
            NovelNode node4 = new()
            {
                Key = "node4",
                Name = NovelEditor.马猴烧酒.NickName,
                Content = "你的名字是？"
            };
            NovelNode node5 = new()
            {
                Key = "node5",
                Name = NovelEditor.马猴烧酒.NickName,
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
            NovelOption option3 = new()
            {
                Key = "option3",
                Name = "重新开始游戏",
                Targets = [node1]
            };
            NovelNode node7 = new()
            {
                Key = "node7",
                Priority = 2,
                Content = "旁白：示例结束，你被马猴烧酒吃掉了。",
                Options = [option3],
                AndPredicates = new()
                {
                    { "主角攻击力大于20", NovelEditor.Conditions["主角攻击力大于20"] }
                },
                OrPredicates = new()
                {
                    { "马猴烧酒的好感度低于50", NovelEditor.Conditions["马猴烧酒的好感度低于50"] },
                    { "马猴烧酒攻击力大于20", NovelEditor.Conditions["马猴烧酒攻击力大于20"] }
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
        }
    }
}
