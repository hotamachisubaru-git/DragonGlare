using UnityEngine;

namespace DragonGlare
{
    public class GameContent
    {
        public static readonly BattleActionType[,] BattleCommandGrid = new BattleActionType[2, 3]
        {
            { BattleActionType.Attack, BattleActionType.Spell, BattleActionType.Item },
            { BattleActionType.Equip, BattleActionType.Defend, BattleActionType.Run }
        };

        private static readonly string[,] BattleActionNamesJp = new string[2, 3]
        {
            { "たたかう", "まほう", "どうぐ" },
            { "そうび", "ぼうぎょ", "にげる" }
        };

        private static readonly string[,] BattleActionNamesEn = new string[2, 3]
        {
            { "Attack", "Spell", "Item" },
            { "Equip", "Defend", "Run" }
        };

        private static readonly string[][] NameTableJp = new string[][]
        {
            new string[] { "あ", "い", "う", "え", "お" },
            new string[] { "か", "き", "く", "け", "こ" },
            new string[] { "さ", "し", "す", "せ", "そ" },
            new string[] { "た", "ち", "つ", "て", "と" },
            new string[] { "な", "に", "ぬ", "ね", "の" },
            new string[] { "は", "ひ", "ふ", "へ", "ほ" },
            new string[] { "ま", "み", "む", "め", "も" },
            new string[] { "や", "ゆ", "よ", "わ", "を" },
            new string[] { "ら", "り", "る", "れ", "ろ" },
            new string[] { "が", "ぎ", "ぐ", "げ", "ご" },
            new string[] { "ざ", "じ", "ず", "ぜ", "ぞ" },
            new string[] { "だ", "ぢ", "づ", "で", "ど" },
            new string[] { "ば", "び", "ぶ", "べ", "ぼ" },
            new string[] { "ぱ", "ぴ", "ぷ", "ぺ", "ぽ" },
            new string[] { "ぁ", "ぃ", "ぅ", "ぇ", "ぉ" },
            new string[] { "ゃ", "ゅ", "ょ", "っ", "ー" },
            new string[] { "A", "B", "C", "D", "E" },
            new string[] { "F", "G", "H", "I", "J" },
            new string[] { "K", "L", "M", "N", "O" },
            new string[] { "P", "Q", "R", "S", "T" },
            new string[] { "U", "V", "W", "X", "Y" },
            new string[] { "Z", "0", "1", "2", "3" },
            new string[] { "4", "5", "6", "7", "8" },
            new string[] { "9", "けす", "おわり", "", "" }
        };

        private static readonly string[][] NameTableEn = new string[][]
        {
            new string[] { "A", "B", "C", "D", "E" },
            new string[] { "F", "G", "H", "I", "J" },
            new string[] { "K", "L", "M", "N", "O" },
            new string[] { "P", "Q", "R", "S", "T" },
            new string[] { "U", "V", "W", "X", "Y" },
            new string[] { "Z", "a", "b", "c", "d" },
            new string[] { "e", "f", "g", "h", "i" },
            new string[] { "j", "k", "l", "m", "n" },
            new string[] { "o", "p", "q", "r", "s" },
            new string[] { "t", "u", "v", "w", "x" },
            new string[] { "y", "z", "0", "1", "2" },
            new string[] { "3", "4", "5", "6", "7" },
            new string[] { "8", "9", "!", "?", "." },
            new string[] { "-", "_", " ", "DEL", "END" }
        };

        public static string GetBattleActionName(BattleActionType action, UiLanguage language)
        {
            for (int r = 0; r < BattleCommandGrid.GetLength(0); r++)
            {
                for (int c = 0; c < BattleCommandGrid.GetLength(1); c++)
                {
                    if (BattleCommandGrid[r, c] == action)
                    {
                        return language == UiLanguage.English ? BattleActionNamesEn[r, c] : BattleActionNamesJp[r, c];
                    }
                }
            }
            return string.Empty;
        }

        public static string[][] GetNameTable(UiLanguage language)
        {
            return language == UiLanguage.English ? NameTableEn : NameTableJp;
        }

        public static string GetEnemyName(DragonGlare.Domain.Battle.EnemyDefinition enemy, UiLanguage language)
        {
            return language == UiLanguage.English ? enemy.Name : enemy.NameJp;
        }

        public static readonly OpeningNarrationLine[] LanguageOpeningScript = new OpeningNarrationLine[]
        {
            new("遠い昔。", 138, 31),
            new("世界には五つの大地があった。", 138, 31),
            new("その時代では、争いがなく。\n皆が平和に満ちていた。", 199, 38),
            new("そして、それぞれの大地で\n違う神が崇められていたという。", 245, 38),
            new("しかし", 69, 92),
            new("平和は長くは続かなかった。", 192, 38),
            new("争いによって、日の大地は\n跡形もなく崩れ落ちてしまった。", 222, 38),
            new("そして、世界には暗い月しか\n上らなくなってしまった。", 214, 38),
            new("次から次へと世界は\n闇に満ちていった。", 191, 38),
            new("ついには、世界の中心となる光の大地が\n愚かな争いにより、闇に沈んでいった。", 260, 46),
            new("やがて、光の神は\n闇に飲み込まれ", 169, 38),
            new("世界にある万物が\n人々を襲うようにしてしまった。", 207, 38),
            new("世界は、いつしか光を失い\n闇が世界を司るようになった。", 230, 0)
        };
    }
}
