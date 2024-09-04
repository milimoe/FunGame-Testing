using Milimoe.FunGame.Core.Entity;

namespace Milimoe.FunGame.Testing.Solutions;

public class ActionQueue
{
    private readonly List<Character> _Queue = [];
    private readonly Dictionary<Character, double> _HardnessTimes = [];

    public void AddCharacter(Character character, double hardnessTime)
    {
        // 插队机制：按硬直时间排序
        int insertIndex = _Queue.FindIndex(c => _HardnessTimes[c] > hardnessTime);
        if (insertIndex == -1)
        {
            _Queue.Add(character);
        }
        else
        {
            _Queue.Insert(insertIndex, character);
        }
        _HardnessTimes[character] = hardnessTime;
    }

    public void CalculateInitialOrder(List<Character> characters)
    {
        // 排序时，时间会流逝
        int nowTime = 1;

        // 初始排序：按速度排序
        List<IGrouping<double, Character>> groupedBySpeed = [.. characters
            .GroupBy(c => c.SPD)
            .OrderByDescending(g => g.Key)];

        Random random = new();

        foreach (IGrouping<double, Character> group in groupedBySpeed)
        {
            if (group.Count() == 1)
            {
                // 如果只有一个角色，直接加入队列
                AddCharacter(group.First(), _Queue.Count + nowTime);
            }
            else
            {
                // 如果有多个角色，进行先行决定
                List<Character> sortedList = [.. group];

                while (sortedList.Count > 0)
                {
                    Character? selectedCharacter = null;
                    bool decided = false;
                    if (sortedList.Count == 1)
                    {
                        selectedCharacter = sortedList[0];
                        decided = true;
                    }

                    while (!decided)
                    {
                        // 每个角色进行两次随机数抽取
                        var randomNumbers = sortedList.Select(c => new
                        {
                            Character = c,
                            FirstRoll = random.Next(1, 21),
                            SecondRoll = random.Next(1, 21)
                        }).ToList();

                        randomNumbers.ForEach(a => Console.WriteLine(a.Character.Name + ": " + a.FirstRoll + " / " + a.SecondRoll));

                        nowTime++;

                        // 找到两次都大于其他角色的角色
                        int maxFirstRoll = randomNumbers.Max(r => r.FirstRoll);
                        int maxSecondRoll = randomNumbers.Max(r => r.SecondRoll);

                        var candidates = randomNumbers
                            .Where(r => r.FirstRoll == maxFirstRoll && r.SecondRoll == maxSecondRoll)
                            .ToList();

                        if (candidates.Count == 1)
                        {
                            selectedCharacter = candidates.First().Character;
                            decided = true;
                        }
                    }

                    // 将决定好的角色加入顺序表
                    if (selectedCharacter != null)
                    {
                        AddCharacter(selectedCharacter, _Queue.Count + nowTime);
                        Console.WriteLine("decided: " + selectedCharacter.Name + "\r\n");
                        sortedList.Remove(selectedCharacter);
                    }
                }
            }
        }
    }

    public Character? NextCharacter()
    {
        if (_Queue.Count == 0) return null;

        // 硬直时间为0的角色将执行行动
        Character? character = _Queue.FirstOrDefault(c => _HardnessTimes[c] == 0);
        if (character != null)
        {
            _Queue.Remove(character);
            return character;
        }

        return null;
    }

    public void ProcessTurn(Character character)
    {
        double baseTime = 15; // 假设基础硬直时间为15
        if (character.Name == "A")
        {
            baseTime = 10; // A的硬直
        }
        double newHardnessTime = Math.Round(baseTime * (1 - character.ActionCoefficient), 2, MidpointRounding.AwayFromZero);

        AddCharacter(character, newHardnessTime);
    }

    public void ReduceHardnessTimes()
    {
        if (_Queue.Count == 0) return;

        // 获取第一个角色的硬直时间
        double timeToReduce = _HardnessTimes[_Queue[0]];

        Console.WriteLine("Time Lapse: " + timeToReduce);

        // 减少所有角色的硬直时间
        foreach (Character character in _Queue)
        {
            _HardnessTimes[character] = Math.Round(_HardnessTimes[character] - timeToReduce, 2, MidpointRounding.AwayFromZero);

            // 回血回蓝
            double douHP = Math.Round(character.HR * timeToReduce, 2, MidpointRounding.AwayFromZero);
            double douMP = Math.Round(character.MR * timeToReduce, 2, MidpointRounding.AwayFromZero);
            Console.WriteLine("角色 " + character.Name + " 回血：" + douHP + " / " + "回蓝：" + douMP);
        }
        Console.WriteLine();
    }

    public void DisplayQueue()
    {
        Console.WriteLine("Current ActionQueue:");
        foreach (var character in _Queue)
        {
            Console.WriteLine($"{character.Name}: Hardness Time {_HardnessTimes[character]}");
        }
    }
}