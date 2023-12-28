using System.Collections;
using System.Data;
using ConverterExample;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.JsonConverter;

DataSet ds = new();
DataTable table = new("SampleTable1");
table.Columns.Add("Id", typeof(int));
table.Columns.Add("Name", typeof(string));
table.Columns.Add("Age", typeof(int));
table.Rows.Add(1, "John", 30);
table.Rows.Add(2, "Jane", 25);
table.Rows.Add(3, "Bob", 40);
ds.Tables.Add(table);

table = new("SampleTable2");
table.Columns.Add("Id", typeof(int));
table.Columns.Add("Name", typeof(string));
table.Columns.Add("Age", typeof(int));
table.Rows.Add(1, "John", 30);
table.Rows.Add(2, "Jane", 25);
table.Rows.Add(3, "Bob", 40);
ds.Tables.Add(table);

JsonTool JsonTool = new();
JsonTool.AddConverters(new System.Text.Json.Serialization.JsonConverter[] { new UserConverter(), new RoomConverter(), new PersonConverter(), new AddressConverter() });

Room r = Factory.GetRoom(1294367, "w5rtvh8".ToUpper(), DateTime.Now, Factory.GetUser(), Milimoe.FunGame.Core.Library.Constant.RoomType.Mix, "", "", Milimoe.FunGame.Core.Library.Constant.RoomState.Created);
User u = Factory.GetUser(1, "LUOLI", DateTime.Now, DateTime.Now, "LUOLI@66.COM", "QWQAQW");

Hashtable hashtable = new()
{
    { "table", table },
    { "room", r },
    { "user", u }
};

string json = JsonTool.GetString(hashtable);

Hashtable hashtable2 = JsonTool.GetObject<Hashtable>(json) ?? new();

DataTable table2 = JsonTool.GetObject<DataTable>(json) ?? new();
User u2 = JsonTool.GetObject<User>(hashtable2, "user") ?? Factory.GetUser();
Room r2 = JsonTool.GetObject<Room>(hashtable2, "room") ?? Factory.GetRoom();

table2.AsEnumerable().ToList().ForEach(row =>
{
    Console.WriteLine("Id: " + row["Id"] + ", Name: "+ row["Name"] + ", Age: " + row["Age"]);
});

Console.WriteLine(u2.Username + " 进入了 " + r2.Roomid + " 房间");

Person p = new()
{
    Age = (int)r2.Id,
    Name = u2.Username,
    Address = new()
    {
        State = "呵呵州(Hehe State)",
        City = "哈哈市(Haha City)"
    }
};

json = JsonTool.GetString(p);

Person p2 = JsonTool.GetObject<Person>(json) ?? new();

Console.WriteLine("My name is " + p2.Name + ", I am " + p2.Age + "-year-old. I live at " + p2.Address.State + " " + p2.Address.City);
Console.WriteLine("摆烂了54");

// 生成一对公钥秘钥
//TwoFactorAuthenticator.CreateSecretKey();