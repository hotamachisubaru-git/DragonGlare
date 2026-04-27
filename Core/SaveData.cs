using DragonGlareAlpha.Domain;
using XnaPoint = Microsoft.Xna.Framework.Point;

namespace DragonGlareAlpha.Data
{
    /// <summary>
    /// ゲームのセーブデータを表すクラス。
    /// </summary>
    public class SaveData
    {
        public FieldMapId MapId { get; set; }
        public XnaPoint PlayerTile { get; set; }
        public PlayerFacingDirection PlayerFacingDirection { get; set; }
        public int PlayerCurrentHP { get; set; }
        public int PlayerMaxHP { get; set; }
        public int PlayerCurrentMP { get; set; }
        public int PlayerMaxMP { get; set; }
        public int PlayerGold { get; set; }
        public int PlayerExperience { get; set; }
        // 必要に応じて、さらにセーブしたいデータをここに追加
    }
}
