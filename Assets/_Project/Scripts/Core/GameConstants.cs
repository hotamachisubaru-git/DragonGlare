using UnityEngine;

namespace DragonGlare
{
    public static class GameConstants
    {
        public const int TileSize = 32;
        public const int ShopItemsPerPage = 6;
        public const int CompactFieldViewportWidthTiles = 13;
        public const int ExpandedFieldViewportWidthTiles = 17;
        public const int CompactFieldViewportHeightTiles = 15;
        public const int ExpandedFieldViewportHeightTiles = 15;
        public const int FieldSidebarEquipmentHeightTiles = 6;
        public const int ExpandedFieldViewportHorizontalMargin = 19;
        public const int ExpandedFieldViewportVerticalTrim = 16;
        public const int FieldMovementAnimationDuration = 9;
        public const int EncounterTransitionDuration = 26;
        public const int BattleSelectionVisibleRows = 4;
        public const int MaxPlayerNameLength = 6;
        public const int OpeningSourceViewportWidth = 256;
        public const int OpeningSourceViewportHeight = 240;
        public const int LanguageOpeningAudioFrames = 2978;
        public const int SceneFadeOutDuration = 40;
        public const int ProgressSaveDelayFrames = 45;
        public const int ProgressSaveMaxDelayFrames = 180;
        public const float GamepadThumbStickThreshold = 0.5f;
        public static readonly Vector2Int PlayerStartTile = new Vector2Int(3, 12);
        public const int BattleIntroDurationFrames = 90;
        public const int BattleStepMinimumFrames = 8;
        public const int BattleStepMessageHoldFrames = 34;
        public const int BattleResolutionVisibleLines = 7;
        public const int BgmFadeInDurationFrames = 28;
        public const int BgmFadeOutDurationFrames = 18;
        public const float BgmPlaybackVolume = 0.85f;
        public const float SePlaybackVolume = 0.9f;

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
