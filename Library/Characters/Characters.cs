using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace FunGame.Testing.Characters
{
    public class Characters
    {
        public static Character Oshima
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Oshima";
                c.FirstName = "Shiya";
                c.NickName = "大島シヤ";
                c.PrimaryAttribute = PrimaryAttribute.STR;
                c.InitialATK = 25;
                c.InitialHP = 85;
                c.InitialMP = 10;
                c.InitialSTR = 35;
                c.STRGrowth = 3.5;
                c.InitialAGI = 0;
                c.AGIGrowth = 0;
                c.InitialINT = 0;
                c.INTGrowth = 0;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character Xinyin
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Xiyue";
                c.FirstName = "XinYin";
                c.NickName = "心音";
                c.PrimaryAttribute = PrimaryAttribute.AGI;
                c.InitialATK = 22;
                c.InitialHP = 80;
                c.InitialMP = 60;
                c.InitialSTR = 8;
                c.STRGrowth = 0.9;
                c.InitialAGI = 19;
                c.AGIGrowth = 1.7;
                c.InitialINT = 3;
                c.INTGrowth = 0.4;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character Yang
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Ya";
                c.FirstName = "Yang";
                c.NickName = "吖养";
                c.PrimaryAttribute = PrimaryAttribute.STR;
                c.InitialATK = 23;
                c.InitialHP = 105;
                c.InitialMP = 55;
                c.InitialSTR = 11;
                c.STRGrowth = 1.8;
                c.InitialAGI = 9;
                c.AGIGrowth = 0.5;
                c.InitialINT = 10;
                c.INTGrowth = 0.7;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character NanGanyu
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Nan";
                c.FirstName = "Ganyu";
                c.NickName = "男甘雨";
                c.PrimaryAttribute = PrimaryAttribute.INT;
                c.InitialATK = 17;
                c.InitialHP = 115;
                c.InitialMP = 80;
                c.InitialSTR = 6;
                c.STRGrowth = 0.6;
                c.InitialAGI = 7;
                c.AGIGrowth = 0.7;
                c.InitialINT = 17;
                c.INTGrowth = 1.7;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character NiuNan
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Niu";
                c.FirstName = "Nan";
                c.NickName = "牛腩";
                c.PrimaryAttribute = PrimaryAttribute.INT;
                c.InitialATK = 16;
                c.InitialHP = 75;
                c.InitialMP = 90;
                c.InitialSTR = 0;
                c.STRGrowth = 0;
                c.InitialAGI = 0;
                c.AGIGrowth = 0;
                c.InitialINT = 30;
                c.INTGrowth = 3;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character Mayor
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Dokyo";
                c.FirstName = "Mayor";
                c.NickName = "铎京市长";
                c.PrimaryAttribute = PrimaryAttribute.AGI;
                c.InitialATK = 21;
                c.InitialHP = 120;
                c.InitialMP = 20;
                c.InitialSTR = 7;
                c.STRGrowth = 1;
                c.InitialAGI = 21;
                c.AGIGrowth = 1.8;
                c.InitialINT = 2;
                c.INTGrowth = 0.2;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character 马猴烧酒
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Magical";
                c.FirstName = "Girl";
                c.NickName = "魔法少女";
                c.PrimaryAttribute = PrimaryAttribute.AGI;
                c.InitialATK = 20;
                c.InitialHP = 95;
                c.InitialMP = 35;
                c.InitialSTR = 7;
                c.STRGrowth = 0.3;
                c.InitialAGI = 15;
                c.AGIGrowth = 2.3;
                c.InitialINT = 8;
                c.INTGrowth = 0.4;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character QingXiang
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Qing";
                c.FirstName = "Xiang";
                c.NickName = "清香";
                c.PrimaryAttribute = PrimaryAttribute.INT;
                c.InitialATK = 26;
                c.InitialHP = 110;
                c.InitialMP = 80;
                c.InitialSTR = 6;
                c.STRGrowth = 0.5;
                c.InitialAGI = 4;
                c.AGIGrowth = 0.5;
                c.InitialINT = 20;
                c.INTGrowth = 2;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character QWQAQW
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "QWQ";
                c.FirstName = "AQW";
                c.NickName = "LUOLI66";
                c.PrimaryAttribute = PrimaryAttribute.INT;
                c.InitialATK = 18;
                c.InitialHP = 125;
                c.InitialMP = 45;
                c.InitialSTR = 0;
                c.STRGrowth = 0;
                c.InitialAGI = 15;
                c.AGIGrowth = 1.5;
                c.InitialINT = 15;
                c.INTGrowth = 1.5;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character ColdBlue
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Cold";
                c.FirstName = "Blue";
                c.NickName = "冷蓝";
                c.PrimaryAttribute = PrimaryAttribute.STR;
                c.InitialATK = 28;
                c.InitialHP = 95;
                c.InitialMP = 25;
                c.InitialSTR = 22;
                c.STRGrowth = 1.9;
                c.InitialAGI = 4;
                c.AGIGrowth = 0.6;
                c.InitialINT = 4;
                c.INTGrowth = 0.6;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character 绿拱门
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "ddd";
                c.FirstName = "ovo";
                c.NickName = "绿拱门";
                c.PrimaryAttribute = PrimaryAttribute.AGI;
                c.InitialATK = 22;
                c.InitialHP = 65;
                c.InitialMP = 22;
                c.InitialSTR = 10;
                c.STRGrowth = 1;
                c.InitialAGI = 20;
                c.AGIGrowth = 2;
                c.InitialINT = 0;
                c.INTGrowth = 0;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }

        public static Character QuDuoduo
        {
            get
            {
                Character c = Factory.GetCharacter();
                c.Name = "Qu";
                c.FirstName = "Duoduo";
                c.NickName = "趣多多";
                c.PrimaryAttribute = PrimaryAttribute.STR;
                c.InitialATK = 19;
                c.InitialHP = 90;
                c.InitialMP = 40;
                c.InitialSTR = 13;
                c.STRGrowth = 1.5;
                c.InitialAGI = 13;
                c.AGIGrowth = 1.2;
                c.InitialINT = 4;
                c.INTGrowth = 0.3;
                c.InitialSPD = 300;
                c.InitialHR = 4;
                c.InitialMR = 2;
                return c;
            }
        }
    }
}
