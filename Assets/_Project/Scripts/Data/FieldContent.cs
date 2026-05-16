using DragonGlare.Domain;
using DragonGlare.Domain.Field;

namespace DragonGlare.Data;

public static class FieldContent
{
    public static readonly FieldTransitionDefinition[] FieldTransitions = new FieldTransitionDefinition[]
    {
        new(FieldMapId.Hub, new Rectangle(9, 0, 2, 1), FieldMapId.Castle, new Point(14, 20)),
        new(FieldMapId.Hub, new Rectangle(19, 7, 1, 1), FieldMapId.Field, new Point(2, 6)),
        new(FieldMapId.Hub, new Rectangle(19, 8, 1, 1), FieldMapId.Field, new Point(2, 7)),
        new(FieldMapId.Castle, new Rectangle(14, 21, 1, 1), FieldMapId.Hub, new Point(9, 2)),
        new(FieldMapId.Field, new Rectangle(1, 6, 1, 1), FieldMapId.Hub, new Point(18, 7)),
        new(FieldMapId.Field, new Rectangle(1, 7, 1, 1), FieldMapId.Hub, new Point(18, 8)),
        new(FieldMapId.Field, new Rectangle(15, 1, 1, 1), FieldMapId.Dungeon, new Point(14, 20)),
        new(FieldMapId.Dungeon, new Rectangle(14, 21, 1, 1), FieldMapId.Field, new Point(15, 2))
    };

    public static readonly FieldEventDefinition[] FieldEvents = new FieldEventDefinition[]
    {
        new(
            "guide_npc",
            FieldMapId.Hub,
            new Point(12, 7),
            Color.Cyan,
            true,
            FieldEventActionType.Dialogue,
            new[]
            {
                "{player}、ようこそ。\nけんをみがき たびのしたくをしよう。".Replace("、", "、"),
                "やくそうは HPを なおし、\nひのたまは どうぐで なげられるぞ。"
            },
            new[]
            {
                "Welcome, {player}.\nSharpen your blade and prepare.",
                "Herbs heal you.\nFire orbs can be thrown from ITEMS."
            },
            SpriteAssetName: "guide_npc.png",
            PortraitAssetName: "guide-4.png"),
        new(
            "town_child",
            FieldMapId.Hub,
            new Point(4, 4),
            Color.FromArgb(120, 255, 180),
            true,
            FieldEventActionType.Dialogue,
            new[]
            {
                "まちの こどもだ。\n「おしろの へいしって すごく まじめだよ！」",
                "「フィールドの くさむらは\n　まものが でやすいから きをつけてね。」"
            },
            new[]
            {
                "A village child grins.\n\"The castle guard is super serious!\"",
                "\"Watch the tall grass out in the field.\nMonsters jump out fast there.\""
            },
            SpriteAssetName: "town_child.png",
            PortraitAssetName: "young-5.png"),
        new(
            "castle_guard",
            FieldMapId.Castle,
            new Point(12, 11),
            Color.FromArgb(255, 180, 120),
            true,
            FieldEventActionType.Dialogue,
            new[]
            {
                "おしろの へいしだ。\n「りゅうの ひかりを おうものよ、あわてるな。」",
                "「レベルが あがったら そうびも みなおせ。\n　ちからだけでは かてぬぞ。」"
            },
            new[]
            {
                "A castle guard stands firm.\n\"Hunter of dragonlight, do not rush.\"",
                "\"When you grow stronger, review your gear.\nPower alone will not carry you.\""
            },
            SpriteAssetName: "castle_guard.png",
            PortraitAssetName: "castle-guard-4.png"),
        new(
            "field_scout",
            FieldMapId.Field,
            new Point(11, 11),
            Color.FromArgb(255, 228, 120),
            true,
            FieldEventActionType.Dialogue,
            new[]
            {
                "みはりの ぼうけんしゃだ。\n「このさきは ぬかるみが おおい。」",
                "「HPが へったら いったん もどれ。\n　むりやり すすむと いたいめを みるぞ。」"
            },
            new[]
            {
                "A field scout watches the road.\n\"The ground ahead gets rough.\"",
                "\"If your HP drops, fall back first.\nPushing through carelessly will cost you.\""
            },
            SpriteAssetName: "field_scout.png",
            PortraitAssetName: "mihari-3.png"),
        new(
            "field_notice_sign",
            FieldMapId.Field,
            new Point(5, 3),
            Color.Gold,
            true,
            FieldEventActionType.Dialogue,
            new[]
            {
                "古い たてふだだ。\n北東の いわやまに 地下への入口がある。",
                "くさむらの おくには まものが ひそむ。\n宝箱を みつけたら A/Y/Zで しらべよう。"
            },
            new[]
            {
                "An old sign reads:\nA dungeon entrance lies to the northeast.",
                "Monsters lurk in the tall grass.\nInspect treasure chests with A/Y/Z."
            },
            RenderOnMap: false),
        new(
            "field_treasure_chest",
            FieldMapId.Field,
            new Point(13, 7),
            Color.Gold,
            true,
            FieldEventActionType.Treasure,
            new[]
            {
                "宝箱を あけた！"
            },
            new[]
            {
                "Opened the treasure chest!"
            },
            Reward: new FieldEventReward(ItemId: "bronze_sword", ItemQuantity: 1),
            JapaneseCompletedPages:
            new[]
            {
                "宝箱は からっぽだ。"
            },
            EnglishCompletedPages:
            new[]
            {
                "The treasure chest is empty."
            },
            RenderOnMap: false),
        new(
            "banker_npc",
            FieldMapId.Hub,
            new Point(7, 12),
            Color.Gold,
            true,
            FieldEventActionType.Bank,
            new[]
            {
                "ぎんこういんだ。\n「あずける・ひきだす・かりるを あつかうよ。」"
            },
            new[]
            {
                "A banker nods.\n\"Deposit, withdraw, or borrow here.\""
            }),
        new(
            "field_sign",
            FieldMapId.Hub,
            new Point(2, 12),
            Color.Gold,
            true,
            FieldEventActionType.Dialogue,
            new[]
            {
                "たてふだだ。\nXで ステータスをひらける。",
                "Bキー/LBで バトル、Vキー/RBで ショップ。\nZやAで イベントを よめる。"
            },
            new[]
            {
                "A sign reads:\nPress X to open STATUS.",
                "B key/LB starts battle, V key/RB opens shop.\nPress Z or A to inspect events."
            }),
        new(
            "healing_spring",
            FieldMapId.Hub,
            new Point(16, 12),
            Color.MediumSpringGreen,
            true,
            FieldEventActionType.Recover,
            new[]
            {
                "きらめく いずみだ。",
                "みずの ちからが からだに しみこんだ。"
            },
            new[]
            {
                "A shining spring bubbles here.",
                "The water restores your strength."
            },
            RecoverHp: 999,
            RecoverMp: 999)
    };

    private static readonly Dictionary<string, FieldEventDefinition> FieldEventById =
        FieldEvents.ToDictionary(e => e.Id, StringComparer.Ordinal);

    public static FieldEventDefinition? GetFieldEventById(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return null;
        }

        return FieldEventById.TryGetValue(eventId, out var fieldEvent) ? fieldEvent : null;
    }
}
