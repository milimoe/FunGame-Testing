using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Solutions
{
    public enum ActivityState
    {
        Future,
        Upcoming,
        InProgress,
        Ended
    }
    
    public enum RedeemResult
    {
        Success,
        StockNotEnough,
        PointsNotEnough
    }

    public class Activity(long id, string name, DateTime startTime, DateTime endTime)
    {
        public long Id { get; set; } = id;
        public string Name { get; set; } = name;
        public DateTime StartTime { get; set; } = startTime;
        public DateTime EndTime { get; set; } = endTime;
        public ActivityState State { get; private set; } = ActivityState.Future;
        public List<Quest> Quests { get; set; } = [];
        public Store Store { get; set; } = new Store();

        // 事件
        public event EventHandler<ActivityStateChangedEventArgs>? ActivityStateChanged;
        public event EventHandler<UserAccessEventArgs>? UserAccessCheck;

        public void UpdateState()
        {
            ActivityState newState;
            DateTime now = DateTime.Now;

            if (now < StartTime)
            {
                newState = ActivityState.Future;
            }
            else if (now >= StartTime && now < StartTime.AddHours(1))
            {
                newState = ActivityState.Upcoming;
            }
            else if (now >= StartTime && now < EndTime)
            {
                newState = ActivityState.InProgress;
            }
            else
            {
                newState = ActivityState.Ended;
            }

            if (State != newState)
            {
                State = newState;
                OnActivityStateChanged(new ActivityStateChangedEventArgs(State));
            }
        }

        protected virtual void OnActivityStateChanged(ActivityStateChangedEventArgs e)
        {
            ActivityStateChanged?.Invoke(this, e);
        }

        public bool AllowUserAccess(long userId)
        {
            UserAccessEventArgs args = new(userId, State, StartTime, EndTime);
            UserAccessCheck?.Invoke(this, args);
            return args.AllowAccess;
        }

        public void AddQuest(Quest quest)
        {
            Quests.Add(quest);
        }

        public void RemoveQuest(Quest quest)
        {
            Quests.Remove(quest);
        }

        public void UpdateQuestStatus(long questId, QuestState newStatus)
        {
            var quest = Quests.FirstOrDefault(q => q.Id == questId);
            if (quest != null)
            {
                quest.Status = newStatus;
                // 可选：触发任务状态更新事件
            }
        }
    }

    public class ActivityStateChangedEventArgs(ActivityState newState) : EventArgs
    {
        public ActivityState NewState { get; } = newState;
    }

    public class UserAccessEventArgs(long userId, ActivityState activityState, DateTime startTime, DateTime endTime) : EventArgs
    {
        public long UserId { get; } = userId;
        public ActivityState ActivityState { get; } = activityState;
        public DateTime StartTime { get; } = startTime;
        public DateTime EndTime { get; } = endTime;
        public bool AllowAccess { get; set; } = false; // 默认不允许访问
    }

    public class Points(long userId, int amount = 0)
    {
        public long UserId { get; set; } = userId;
        public int Amount { get; set; } = amount;

        public void AddPoints(int amount)
        {
            Amount += amount;
        }

        public void RemovePoints(int amount)
        {
            if (Amount >= amount)
            {
                Amount -= amount;
            }
            else
            {
                throw new InvalidOperationException("积分不足！");
            }
        }
    }

    public class Store
    {
        public List<Item> Items { get; set; } = [];
        public Dictionary<long, int> ItemPrices { get; set; } = []; // ItemId, Price
        public Dictionary<long, int> ItemStocks { get; set; } = []; // ItemId, Stock

        /// <summary>
        /// 添加物品到商店
        /// </summary>
        /// <param name="item">物品</param>
        /// <param name="price">价格</param>
        /// <param name="stock">库存</param>
        public void AddItem(Item item, int price, int stock)
        {
            Items.Add(item);
            ItemPrices[item.Id] = price;
            ItemStocks[item.Id] = stock;
        }

        public void RemoveItem(Item item)
        {
            Items.Remove(item);
            ItemPrices.Remove(item.Id);
            ItemStocks.Remove(item.Id);
        }

        public RedeemResult TryRedeemItem(Points points, long itemId)
        {
            if (!ItemPrices.TryGetValue(itemId, out int price))
            {
                throw new ArgumentException($"商品 {itemId} 不存在！");
            }

            if (!ItemStocks.TryGetValue(itemId, out int stock))
            {
                throw new ArgumentException($"商品 {itemId} 不存在！");
            }

            if (stock <= 0)
            {
                return RedeemResult.StockNotEnough; // 库存不足
            }

            if (points.Amount < price)
            {
                return RedeemResult.PointsNotEnough; // 积分不足
            }

            points.RemovePoints(price);
            ItemStocks[itemId]--; // 减少库存
            // 可选：记录兑换记录
            return RedeemResult.Success; // 兑换成功
        }
    }

}
