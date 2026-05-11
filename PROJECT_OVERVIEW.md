# DragonGlareAlpha プロジェクト概要

更新日: 2026-05-10

## 概要

DragonGlareAlpha は、Windows 向けの MonoGame / DesktopGL 製 RPG 風プロトタイプです。
メインプロジェクトは `DragonGlareAlpha.csproj` に統一されており、アプリ表示名は `DragonGlare Alpha`、現在の `AppVersion` は `1.4.1` です。

プレイヤーは起動設定を選択したあと、モード選択、言語選択、名前入力、フィールド探索へ進みます。
フィールドからはランダムエンカウントによるバトル、ショップ、銀行、NPC 会話、宝箱、マップ遷移などの基本的な RPG 体験を確認できます。

## 技術スタック

- 言語: C#
- 対象フレームワーク: `net10.0-windows10.0.17763.0`
- ランタイム: `win-x64`
- ゲームフレームワーク: `MonoGame.Framework.DesktopGL`
- 音声: `NAudio`
- フォント: `JF-Dot-ShinonomeMin14.ttf` を 14px で直接使用
- テスト: xUnit

## 起動フロー

`Program.cs` が起動時の入口です。

1. Windows の AppUserModelId と MuiCache 表示名を同期します。
2. `PlatformSupportService` でサポート外プラットフォームを検出します。
3. `AntiCheatService` でデバッガや不正ツールの起動時検出を行います。
4. `LaunchSettingsService` で表示設定を読み込みます。
5. `DragonGlareAlpha` ゲーム本体を生成して `Run()` します。

表示設定は `%LOCALAPPDATA%\DragonGlareAlpha\launch_settings.json` に保存されます。

## ゲームの主な状態

ゲーム本体は `GameState` を中心に画面と入力を切り替えます。

- `StartupOptions`: 起動時の表示設定
- `ModeSelect`: 新規開始、ロード、コピー、削除の選択
- `LanguageSelection`: オープニング演出と日本語 / 英語選択
- `NameInput`: プレイヤー名入力
- `SaveSlotSelection`: 3 スロットのセーブ操作
- `Field`: フィールド探索
- `EncounterTransition`: エンカウント遷移
- `Battle`: バトル
- `ShopBuy`: ショップ
- `Bank`: 銀行

## 実装済み機能

- フィールド探索
  - ハブ、城、フィールドの 3 マップ
  - マップ間遷移
  - NPC 会話、回復、銀行イベント
  - 草むらを含むフィールドでのランダムエンカウント
- バトル
  - 攻撃、呪文、防御、道具、装備変更、逃走
  - マップとプレイヤーレベルに応じた敵プール
  - 毒、睡眠などの状態異常
  - 勝利時の経験値、ゴールド、ドロップ処理
- 成長と装備
  - レベル、HP、MP、攻撃、防御、所持金
  - 武器、むねあて、あたま、こて、レギンス、ブーツの装備スロット
  - 装備補正を反映した戦闘計算
- ショップ
  - 消耗品、武器、防具の購入
  - 所持品の売却
  - 現在装備より強い装備の自動装備
- 銀行
  - 預け入れ、引き出し、借入
  - 借入残高と歩数 / バトルによる利息
- セーブ
  - 3 スロットのセーブ / ロード / コピー / 削除
  - 旧 `save1.sav` からスロット形式への移行
  - DPAPI と署名による保存データ保護
- 入力
  - キーボード: 矢印キー / WASD、Enter / Z、Esc / X、B、V
  - ゲームパッド: 十字キー / 左スティック、A / Start、B / Back、X、Y、LB、RB

## ディレクトリ構成

| パス | 役割 |
| --- | --- |
| `Program.cs` | アプリ起動処理 |
| `DragonGlare.cs` | MonoGame のメインゲームクラス |
| `UI/` | 入力、描画、画面状態、音声、起動表示など |
| `Data/` | ゲーム内テキスト、アイテム、敵、イベント、遷移定義 |
| `Domain/` | ゲーム状態、プレイヤー、バトル、フィールドなどのモデル |
| `Services/` | バトル、ショップ、銀行、マップ、遷移などのロジック |
| `Persistence/` | セーブデータ、スロット管理、保存データ変換 |
| `Security/` | 不正検出、改ざん検出用の保護値 |
| `Assets/` | 実行時に参照する画像、音声、フォント、マップ素材 |
| `Content/` | MonoGame Content 用の素材配置 |
| `DragonGlareAlpha.Tests/` | xUnit テスト |

## 主要データ

`Data/GameContent.cs` に、ゲーム内データの大半が集約されています。

- 名前入力テーブル
- バトルコマンド表示
- 呪文カタログ
- 武器、防具、消耗品カタログ
- ショップ商品リスト
- 敵カタログ
- フィールド遷移定義
- フィールドイベント定義

マップ生成は `Services/MapFactory.cs` が担当します。
城マップは `Assets/map(mycas2).txt` を優先して読み込み、読み込めない場合は埋め込み定義を使用します。

## 保存先

- セーブデータ: `%APPDATA%\DragonGlareAlpha\slot1.sav` から `slot3.sav`
- 起動設定: `%LOCALAPPDATA%\DragonGlareAlpha\launch_settings.json`

## ビルドとテスト

リポジトリルートで実行します。

```powershell
dotnet build DragonGlareAlpha.sln
```

```powershell
dotnet build DragonGlareAlpha.csproj
```

```powershell
dotnet test DragonGlareAlpha.Tests\DragonGlareAlpha.Tests.csproj
```

## 開発時の注意

- メインプロジェクトは `DragonGlareAlpha.csproj` を基準に扱います。
- 対象フレームワークは Windows 向けの `net10.0-windows10.0.17763.0` です。
- アセットを追加・変更する場合は `Assets/README.md` と `Content/README.md` も確認します。
- バージョン変更時は `DragonGlareAlpha.csproj` の `AppVersion` と `CHANGELOG.md` を合わせて更新します。
- 進行中の実装状況や残タスクは `PROGRESS.md` を参照します。
