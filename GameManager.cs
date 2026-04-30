using DragonGlare.Core.Security;
using DragonGlare.Core.Persistence;
using DragonGlareAlpha.Persistence;

namespace DragonGlare.Core;

/// <summary>
/// ゲームの全体的な状態を定義
/// </summary>
public enum GameState
{
    StartupOptions,
    ModeSelect,
    Field,
    Battle,
    // 必要に応じて既存のものを追加
}

/// <summary>
/// ゲームの純粋なロジックを担当するクラス
/// </summary>
public class GameManager
{
    private readonly AntiCheatService _antiCheat = new();
    private readonly SaveService _saveService;

    // プレイヤーの座標（Coreプロジェクトは計算のみ担当）
    public float PlayerX { get; set; } = 100f;
    public float PlayerY { get; set; } = 100f;
    public GameState CurrentState { get; private set; } = GameState.ModeSelect;

    public GameManager()
    {
        var keyProvider = new SecureKeyProvider();
        _saveService = new SaveService(keyProvider);
    }

    /// <summary>
    /// ロジックの更新
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間（秒）</param>
    /// <param name="isMovingUp">上入力があるか</param>
    public void Update(float deltaTime, bool isMovingUp)
    {
        float speed = 200f * deltaTime;
        if (isMovingUp) PlayerY -= speed;
    }

    public void LoadGame(int slot)
    {
        var data = _saveService.Load(slot);
        if (data != null)
        {
            PlayerX = (float)data.PlayerX;
            PlayerY = (float)data.PlayerY;
        }
    }

    public void SaveGame(int slot)
    {
        var data = new SaveData
        {
            PlayerX = (int)PlayerX,
            PlayerY = (int)PlayerY,
            SlotNumber = slot
        };
        _saveService.Save(slot, data);
    }

    /// <summary>
    /// セキュリティチェックの実行
    /// </summary>
    public bool CheckSecurityViolation(out string message)
    {
        return _antiCheat.TryDetectViolation(out message);
    }
}