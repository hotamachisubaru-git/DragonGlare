# Changelog

## Alpha 1.3 - 2026-05-06

前バージョン: `v1.2.5`

### 変更点

- 城マップを仮実装から `Assets/map(mycas2).txt` ベースの文字マップ読み込みに変更。
- 城の描画を `mapTile_Assets_SFC` 系タイルシートから文字ごとにチップを切り出す方式に変更。
- 赤い城床は周囲のタイルを見て縁チップを選択するように変更。
- 城マップ用の通行判定を追加し、壁・柱・装飾と床・出口を判別。
- ハブと城の遷移先座標を新しい城マップに合わせて更新。
- 城マップ読み込みと通行判定のテストを追加。

### 検証

- `dotnet test DragonGlareAlpha.Tests\DragonGlareAlpha.Tests.csproj -p:UseAppHost=false`
