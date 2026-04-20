using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Field;
using DragonGlare.Entities;
using DragonGlare.Managers;
using DragonGlareAlpha.Services;
using DrawingColor = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaPoint = Microsoft.Xna.Framework.Point;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace DragonGlare.Scenes
{
    public sealed class GameScene : IScene, IDisposable
    {
        private const string PlayerName = "のりたま";
        private const int InteractionRangeTiles = 1;
        private const int MovementCooldownFrames = Constants.FieldMovementAnimationDuration;
        private const int CompactFieldViewportWidthTiles = 13;
        private const int CompactFieldViewportHeightTiles = 9;
        private const int ExpandedFieldViewportWidthTiles = 17;
        private const int ExpandedFieldViewportHeightTiles = 11;

        private readonly Field _field;
        private readonly Player _player;
        private readonly FieldTransitionService _fieldTransitionService = new();
        private XnaPoint _playerTile;
        private PlayerFacingDirection _playerFacingDirection = PlayerFacingDirection.Down;
        private int _movementCooldown;
        private int _currentMp = 8;
        private int _maxMp = 8;
        private int _gold = 120;
        private int _experience;
        private bool _statusVisible = true;
        private string _notice = "Z / Enter: はなす・しらべる";
        private IReadOnlyList<string> _dialogPages = [];
        private int _dialogPageIndex;
        private bool _isDialogOpen;
        private string? _activeDialogPortraitTextureName;
        private bool _disposed;

        public GameScene(ContentManager content)
            : this(content, FieldMapId.Hub)
        {
        }

        public GameScene(ContentManager content, FieldMapId initialMap)
        {
            _field = new Field(content, initialMap);
            _player = new Player(XnaVector2.Zero);
            SetPlayerTile(ToXnaPoint(Constants.PlayerStartTile));
        }

        public FieldMapId CurrentMapId => _field.MapId;

        public void Update(GameTime gameTime)
        {
            _field.Update(gameTime);

            if (_isDialogOpen)
            {
                if (InputManager.WasPressed(Keys.Z) || InputManager.WasPressed(Keys.Enter))
                {
                    AdvanceDialog();
                }
                else if (InputManager.WasPressed(Keys.Escape))
                {
                    CloseDialog();
                }

                return;
            }

            if (_movementCooldown > 0)
            {
                _movementCooldown--;
            }

            if (InputManager.WasPressed(Keys.X))
            {
                _statusVisible = !_statusVisible;
                return;
            }

            if (InputManager.WasPressed(Keys.Z) || InputManager.WasPressed(Keys.Enter))
            {
                Interact();
                return;
            }

            if (_movementCooldown == 0)
            {
                var movement = GetMovement();
                if (movement != XnaPoint.Zero)
                {
                    SetPlayerFacingDirection(movement);
                    TryMovePlayer(movement);
                    _movementCooldown = MovementCooldownFrames;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawBackdrop(spriteBatch);

            var viewport = GetFieldViewport();
            var cameraOrigin = GetFieldCameraOrigin();
            _field.Draw(spriteBatch, viewport, cameraOrigin);
            DrawFieldEvents(spriteBatch, viewport, cameraOrigin);
            DrawPlayer(spriteBatch, viewport, cameraOrigin);
            DrawFieldViewportFrame(spriteBatch, viewport);

            DrawHelpWindow(spriteBatch);

            if (_statusVisible)
            {
                DrawStatusWindows(spriteBatch, CreateStatus());
            }

            if (_isDialogOpen)
            {
                DrawDialogWindow(spriteBatch);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _field.Dispose();
            _disposed = true;
        }

        private void TryMovePlayer(XnaPoint movement)
        {
            var target = new XnaPoint(_playerTile.X + movement.X, _playerTile.Y + movement.Y);
            if (!_field.IsWalkable(target) || IsBlockedByFieldEvent(target))
            {
                _notice = "そこへは すすめない。";
                return;
            }

            SetPlayerTile(target);
            _experience++;

            if (_fieldTransitionService.TryGetTransition(_field.MapId, ToDrawingPoint(target), out var transition))
            {
                _field.SetMap(transition.ToMapId);
                SetPlayerTile(ToXnaPoint(transition.DestinationTile));
                _notice = $"{GetMapName(_field.MapId)}へ いどうした。";
                return;
            }

            _notice = GetTileMessage(_field.GetTileId(target));
        }

        private void Interact()
        {
            var fieldEvent = GetInteractableFieldEvent();
            if (fieldEvent is null)
            {
                _notice = "なにも みつからない。";
                return;
            }

            if (fieldEvent.ActionType == FieldEventActionType.Bank)
            {
                _dialogPages = ["＊「ぎんこうへ ようこそ。\n　ごようけんは？」"];
            }
            else if (fieldEvent.ActionType == FieldEventActionType.Recover)
            {
                var recoveredHp = Math.Min(fieldEvent.RecoverHp, _player.MaxHP - _player.CurrentHP);
                var recoveredMp = Math.Min(fieldEvent.RecoverMp, _maxMp - _currentMp);
                _player.CurrentHP += recoveredHp;
                _currentMp += recoveredMp;

                _dialogPages = fieldEvent
                    .GetPages(UiLanguage.Japanese)
                    .Append($"HP+{recoveredHp}  MP+{recoveredMp}\nからだが かるくなった。")
                    .Where(page => !string.IsNullOrWhiteSpace(page))
                    .ToArray();
            }
            else
            {
                _dialogPages = fieldEvent
                    .GetPages(UiLanguage.Japanese)
                    .Select(page => page.Replace("{player}", PlayerName, StringComparison.Ordinal))
                    .Where(page => !string.IsNullOrWhiteSpace(page))
                    .ToArray();
            }

            _dialogPageIndex = 0;
            _activeDialogPortraitTextureName = Path.GetFileNameWithoutExtension(fieldEvent.PortraitAssetName ?? string.Empty);
            _isDialogOpen = _dialogPages.Count > 0;
            _notice = _isDialogOpen ? "Z / Enter: つぎへ   ESC: とじる" : "……";
        }

        private XnaPoint GetMovement()
        {
            if (InputManager.IsKeyDown(Keys.Up) || InputManager.IsKeyDown(Keys.W))
            {
                return new XnaPoint(0, -1);
            }

            if (InputManager.IsKeyDown(Keys.Down) || InputManager.IsKeyDown(Keys.S))
            {
                return new XnaPoint(0, 1);
            }

            if (InputManager.IsKeyDown(Keys.Left) || InputManager.IsKeyDown(Keys.A))
            {
                return new XnaPoint(-1, 0);
            }

            if (InputManager.IsKeyDown(Keys.Right) || InputManager.IsKeyDown(Keys.D))
            {
                return new XnaPoint(1, 0);
            }

            return XnaPoint.Zero;
        }

        private void SetPlayerTile(XnaPoint tile)
        {
            _playerTile = tile;
            _player.Position = new XnaVector2(tile.X * Constants.TileSize, tile.Y * Constants.TileSize);
        }

        private void SetPlayerFacingDirection(XnaPoint movement)
        {
            if (movement.X < 0)
            {
                _playerFacingDirection = PlayerFacingDirection.Left;
                return;
            }

            if (movement.X > 0)
            {
                _playerFacingDirection = PlayerFacingDirection.Right;
                return;
            }

            if (movement.Y < 0)
            {
                _playerFacingDirection = PlayerFacingDirection.Up;
                return;
            }

            if (movement.Y > 0)
            {
                _playerFacingDirection = PlayerFacingDirection.Down;
            }
        }

        private void AdvanceDialog()
        {
            if (_dialogPageIndex < _dialogPages.Count - 1)
            {
                _dialogPageIndex++;
                return;
            }

            CloseDialog();
        }

        private void CloseDialog()
        {
            _isDialogOpen = false;
            _dialogPages = [];
            _dialogPageIndex = 0;
            _activeDialogPortraitTextureName = null;
            _notice = "Z / Enter: はなす・しらべる";
        }

        private bool IsBlockedByFieldEvent(XnaPoint tile)
        {
            return GameContent.FieldEvents.Any(fieldEvent =>
                fieldEvent.MapId == _field.MapId &&
                fieldEvent.BlocksMovement &&
                ToXnaPoint(fieldEvent.TilePosition) == tile);
        }

        private FieldEventDefinition? GetInteractableFieldEvent()
        {
            // 向いている方向の1マス先を計算
            var targetTile = _playerFacingDirection switch
            {
                PlayerFacingDirection.Up => new XnaPoint(_playerTile.X, _playerTile.Y - 1),
                PlayerFacingDirection.Down => new XnaPoint(_playerTile.X, _playerTile.Y + 1),
                PlayerFacingDirection.Left => new XnaPoint(_playerTile.X - 1, _playerTile.Y),
                PlayerFacingDirection.Right => new XnaPoint(_playerTile.X + 1, _playerTile.Y),
                _ => _playerTile
            };

            // 目の前にあるイベントを探す
            var eventAtTarget = GameContent.FieldEvents
                .FirstOrDefault(e => e.MapId == _field.MapId && ToXnaPoint(e.TilePosition) == targetTile);

            if (eventAtTarget != null) return eventAtTarget;

            // 足元のイベント（階段など）をフォールバックとしてチェック
            return GameContent.FieldEvents
                .FirstOrDefault(e => e.MapId == _field.MapId && ToXnaPoint(e.TilePosition) == _playerTile);
        }

        private IEnumerable<FieldEventDefinition> GetCurrentFieldEvents()
        {
            return GameContent.FieldEvents.Where(fieldEvent => fieldEvent.MapId == _field.MapId);
        }

        private void DrawFieldEvents(SpriteBatch spriteBatch, XnaRectangle viewport, XnaPoint cameraOrigin)
        {
            foreach (var fieldEvent in GetCurrentFieldEvents())
            {
                var tile = ToXnaPoint(fieldEvent.TilePosition);
                if (!IsTileVisible(tile, cameraOrigin))
                {
                    continue;
                }

                var bounds = GetFieldTileRectangle(viewport, cameraOrigin, tile);
                var textureName = Path.GetFileNameWithoutExtension(fieldEvent.SpriteAssetName ?? string.Empty);
                var texture = string.IsNullOrWhiteSpace(textureName)
                    ? null
                    : AssetManager.GetTexture(textureName);

                if (texture is not null)
                {
                    spriteBatch.Draw(texture, bounds, XnaColor.White);
                    continue;
                }

                DrawNpcMarker(spriteBatch, bounds, ToXnaColor(fieldEvent.DisplayColor), GetNpcMarkerLabel(fieldEvent));
            }
        }

        private void DrawPlayer(SpriteBatch spriteBatch, XnaRectangle viewport, XnaPoint cameraOrigin)
        {
            var tileRect = GetFieldTileRectangle(viewport, cameraOrigin, _playerTile);
            var texture = GetPlayerTexture();
            if (texture is not null)
            {
                var destination = new XnaRectangle(
                    tileRect.X + ((tileRect.Width - texture.Width) / 2),
                    tileRect.Bottom - texture.Height,
                    texture.Width,
                    texture.Height);
                spriteBatch.Draw(texture, destination, XnaColor.White);
                return;
            }

            DrawPlayerMarker(spriteBatch, tileRect);
        }

        private Texture2D? GetPlayerTexture()
        {
            var baseKey = _playerFacingDirection switch
            {
                PlayerFacingDirection.Left => "player_left",
                PlayerFacingDirection.Right => "player_right",
                PlayerFacingDirection.Up => "player_up",
                PlayerFacingDirection.Down => "player_down",
                _ => null
            };

            if (baseKey == null) return AssetManager.GetTexture("player") ?? _player.Texture;

            return AssetManager.GetTexture(baseKey)
                ?? AssetManager.GetTexture("player")
                ?? _player.Texture;
        }

        private void DrawNpcMarker(SpriteBatch spriteBatch, XnaRectangle tileRect, XnaColor color, string label)
        {
            if (AssetManager.Pixel is not null)
            {
                spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 8, tileRect.Y + 5, 16, 5), color);
                spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 6, tileRect.Y + 10, 20, 16), color);
                spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 10, tileRect.Y + 26, 4, 4), color);
                spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 18, tileRect.Y + 26, 4, 4), color);
                DrawRectangle(spriteBatch, new XnaRectangle(tileRect.X + 6, tileRect.Y + 5, 20, 25), XnaColor.White, 1);
            }

            DrawText(spriteBatch, label, new XnaVector2(tileRect.X + 16, tileRect.Y - 14), XnaColor.White, alignCenter: true);
        }

        private void DrawPlayerMarker(SpriteBatch spriteBatch, XnaRectangle tileRect)
        {
            if (AssetManager.Pixel is null)
            {
                return;
            }

            var capeColor = new XnaColor(48, 92, 255);
            var bodyColor = XnaColor.White;
            var accentColor = new XnaColor(255, 224, 96);

            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 13, tileRect.Y + 4, 6, 6), accentColor);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 9, tileRect.Y + 10, 14, 15), capeColor);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 12, tileRect.Y + 10, 8, 16), bodyColor);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 10, tileRect.Y + 26, 4, 4), bodyColor);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(tileRect.X + 18, tileRect.Y + 26, 4, 4), bodyColor);
            DrawRectangle(spriteBatch, new XnaRectangle(tileRect.X + 8, tileRect.Y + 3, 16, 28), XnaColor.Black, 1);
        }

        private void DrawBackdrop(SpriteBatch spriteBatch)
        {
            if (AssetManager.Pixel is null)
            {
                return;
            }

            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(0, 0, Constants.VirtualWidth, Constants.VirtualHeight), new XnaColor(0, 4, 12));
            for (var y = 0; y < Constants.VirtualHeight; y += 4)
            {
                spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(0, y, Constants.VirtualWidth, 1), new XnaColor(24, 38, 80));
            }
        }

        private void DrawHelpWindow(SpriteBatch spriteBatch)
        {
            var rect = _statusVisible
                ? new XnaRectangle(8, 8, 430, 96)
                : new XnaRectangle(8, 8, 624, 96);
            DrawWindow(spriteBatch, rect);
            DrawText(spriteBatch, "やじるし / WASD: いどう", new XnaVector2(rect.X + 18, rect.Y + 14));
            DrawText(spriteBatch, "Z: はなす・しらべる   X: ステータス", new XnaVector2(rect.X + 18, rect.Y + 42));
            DrawText(spriteBatch, $"{_notice}   いま: {GetMapName(_field.MapId)}", new XnaVector2(rect.X + 18, rect.Y + 70));
        }

        private void DrawStatusWindows(SpriteBatch spriteBatch, FieldUiStatus status)
        {
            var statusRect = new XnaRectangle(446, 8, 186, 116);
            DrawWindow(spriteBatch, statusRect);
            DrawText(spriteBatch, $"{status.PlayerName}  Lv.{status.Level}", new XnaVector2(458, 22));
            DrawText(spriteBatch, $"HP {status.CurrentHp}/{status.MaxHp}", new XnaVector2(458, 48));
            DrawText(spriteBatch, $"MP {status.CurrentMp}/{status.MaxMp}", new XnaVector2(458, 72));
            DrawText(spriteBatch, $"G {status.Gold}", new XnaVector2(458, 96));

            var detailRect = new XnaRectangle(446, 132, 186, 148);
            DrawWindow(spriteBatch, detailRect);
            DrawText(spriteBatch, $"ATK {status.Attack}  DEF {status.Defense}", new XnaVector2(458, 146));
            DrawText(spriteBatch, $"EXP {status.Experience}", new XnaVector2(458, 176));
            DrawText(spriteBatch, $"ぶき {status.WeaponName}", new XnaVector2(458, 206));
            DrawText(spriteBatch, $"ぼうぐ {status.ArmorName}", new XnaVector2(458, 234));
        }

        private void DrawDialogWindow(SpriteBatch spriteBatch)
        {
            var rect = new XnaRectangle(46, 320, 548, 138);
            DrawWindow(spriteBatch, rect);
            var page = _dialogPages.Count == 0
                ? string.Empty
                : _dialogPages[Math.Clamp(_dialogPageIndex, 0, _dialogPages.Count - 1)];
            var portrait = string.IsNullOrWhiteSpace(_activeDialogPortraitTextureName)
                ? null
                : AssetManager.GetTexture(_activeDialogPortraitTextureName);

            var textRect = new XnaRectangle(rect.X + 26, rect.Y + 22, rect.Width - 52, 68);
            var footerX = rect.Right - 28;
            if (portrait is not null)
            {
                var portraitFrame = new XnaRectangle(rect.X + 16, rect.Y + 16, 96, 96);
                DrawWindow(spriteBatch, portraitFrame);
                DrawTextureCover(spriteBatch, portrait, new XnaRectangle(portraitFrame.X + 6, portraitFrame.Y + 6, 84, 84));
                textRect = new XnaRectangle(portraitFrame.Right + 18, rect.Y + 26, rect.Right - portraitFrame.Right - 44, 68);
                footerX = rect.Right - 28;
            }

            DrawWrappedText(spriteBatch, page, textRect);

            var footer = _dialogPageIndex < _dialogPages.Count - 1
                ? "Z: つぎへ"
                : "Z / ESC: とじる";
            DrawText(spriteBatch, footer, new XnaVector2(footerX, rect.Bottom - 34), XnaColor.White, alignRight: true);
        }

        private void DrawTextureCover(SpriteBatch spriteBatch, Texture2D texture, XnaRectangle bounds)
        {
            var scale = Math.Max(bounds.Width / (float)texture.Width, bounds.Height / (float)texture.Height);
            var sourceWidth = Math.Max(1, (int)Math.Round(bounds.Width / scale));
            var sourceHeight = Math.Max(1, (int)Math.Round(bounds.Height / scale));
            var source = new XnaRectangle(
                Math.Max(0, (texture.Width - sourceWidth) / 2),
                Math.Max(0, (texture.Height - sourceHeight) / 2),
                Math.Min(sourceWidth, texture.Width),
                Math.Min(sourceHeight, texture.Height));

            spriteBatch.Draw(texture, bounds, source, XnaColor.White);
        }

        private void DrawFieldViewportFrame(SpriteBatch spriteBatch, XnaRectangle rect)
        {
            if (AssetManager.Pixel is null)
            {
                return;
            }

            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rect.X + 6, rect.Y + 6, rect.Width, rect.Height), XnaColor.Black * 0.35f);
            DrawRectangle(spriteBatch, rect, new XnaColor(0, 120, 255), thickness: 2);
            DrawRectangle(spriteBatch, new XnaRectangle(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10), new XnaColor(132, 206, 255), thickness: 1);
        }

        private void DrawWindow(SpriteBatch spriteBatch, XnaRectangle rect)
        {
            if (AssetManager.Pixel is null)
            {
                return;
            }

            spriteBatch.Draw(AssetManager.Pixel, rect, XnaColor.Black * 0.88f);
            DrawRectangle(spriteBatch, rect, XnaColor.White, thickness: 1);
            DrawRectangle(spriteBatch, new XnaRectangle(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8), new XnaColor(80, 140, 220), thickness: 1);
        }

        private void DrawRectangle(SpriteBatch spriteBatch, XnaRectangle rect, XnaColor color, int thickness)
        {
            if (AssetManager.Pixel is null)
            {
                return;
            }

            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rect.Left, rect.Top, rect.Width, thickness), color);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rect.Left, rect.Top, thickness, rect.Height), color);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
        }

        private void DrawText(SpriteBatch spriteBatch, string text, XnaVector2 position)
        {
            DrawText(spriteBatch, text, position, XnaColor.White);
        }

        private void DrawText(
            SpriteBatch spriteBatch,
            string text,
            XnaVector2 position,
            XnaColor color,
            bool alignRight = false,
            bool alignCenter = false)
        {
            if (AssetManager.MainFont is null || string.IsNullOrEmpty(text))
            {
                return;
            }

            var drawPosition = position;
            var size = AssetManager.MainFont.MeasureString(text);
            if (alignCenter)
            {
                drawPosition.X -= size.X / 2f;
            }
            else if (alignRight)
            {
                drawPosition.X -= size.X;
            }

            spriteBatch.DrawString(AssetManager.MainFont, text, drawPosition + new XnaVector2(2, 2), XnaColor.Black);
            spriteBatch.DrawString(AssetManager.MainFont, text, drawPosition, color);
        }

        private void DrawWrappedText(SpriteBatch spriteBatch, string text, XnaRectangle bounds)
        {
            if (AssetManager.MainFont is null || string.IsNullOrEmpty(text))
            {
                return;
            }

            var lines = WrapText(text, bounds.Width, Math.Max(1, bounds.Height / 18));
            for (var index = 0; index < lines.Count; index++)
            {
                DrawText(spriteBatch, lines[index], new XnaVector2(bounds.X, bounds.Y + (index * 24)));
            }
        }

        private static IReadOnlyList<string> WrapText(string text, int maxWidth, int maxLines)
        {
            if (AssetManager.MainFont is null)
            {
                return [];
            }

            var output = new List<string>();
            foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
            {
                var line = string.Empty;
                foreach (var character in rawLine)
                {
                    var candidate = line + character;
                    if (line.Length > 0 && AssetManager.MainFont.MeasureString(candidate).X > maxWidth)
                    {
                        output.Add(line);
                        line = character.ToString();
                        if (output.Count >= maxLines)
                        {
                            return output;
                        }
                    }
                    else
                    {
                        line = candidate;
                    }
                }

                if (output.Count < maxLines)
                {
                    output.Add(line);
                }

                if (output.Count >= maxLines)
                {
                    break;
                }
            }

            return output;
        }

        private XnaRectangle GetFieldViewport()
        {
            if (_statusVisible)
            {
                return new XnaRectangle(
                    16,
                    112,
                    CompactFieldViewportWidthTiles * Constants.TileSize,
                    CompactFieldViewportHeightTiles * Constants.TileSize);
            }

            return new XnaRectangle(
                48,
                112,
                ExpandedFieldViewportWidthTiles * Constants.TileSize,
                ExpandedFieldViewportHeightTiles * Constants.TileSize);
        }

        private XnaPoint GetFieldCameraOrigin()
        {
            var widthTiles = GetFieldViewportWidthTiles();
            var heightTiles = GetFieldViewportHeightTiles();
            var maxCameraX = Math.Max(0, _field.WidthTiles - widthTiles);
            var maxCameraY = Math.Max(0, _field.HeightTiles - heightTiles);

            return new XnaPoint(
                Math.Clamp(_playerTile.X - (widthTiles / 2), 0, maxCameraX),
                Math.Clamp(_playerTile.Y - (heightTiles / 2), 0, maxCameraY));
        }

        private int GetFieldViewportWidthTiles()
        {
            return _statusVisible ? CompactFieldViewportWidthTiles : ExpandedFieldViewportWidthTiles;
        }

        private int GetFieldViewportHeightTiles()
        {
            return _statusVisible ? CompactFieldViewportHeightTiles : ExpandedFieldViewportHeightTiles;
        }

        private static XnaRectangle GetFieldTileRectangle(XnaRectangle viewport, XnaPoint cameraOrigin, XnaPoint tile)
        {
            return new XnaRectangle(
                viewport.X + ((tile.X - cameraOrigin.X) * Constants.TileSize),
                viewport.Y + ((tile.Y - cameraOrigin.Y) * Constants.TileSize),
                Constants.TileSize,
                Constants.TileSize);
        }

        private bool IsTileVisible(XnaPoint tile, XnaPoint cameraOrigin)
        {
            return tile.X >= cameraOrigin.X &&
                tile.Y >= cameraOrigin.Y &&
                tile.X < cameraOrigin.X + GetFieldViewportWidthTiles() &&
                tile.Y < cameraOrigin.Y + GetFieldViewportHeightTiles();
        }

        private FieldUiStatus CreateStatus()
        {
            return new FieldUiStatus(
                PlayerName,
                1,
                _player.CurrentHP,
                _player.MaxHP,
                _currentMp,
                _maxMp,
                _gold,
                _experience,
                4,
                2,
                "ぼう",
                "ぬののふく",
                GetMapName(_field.MapId));
        }

        private static string GetMapName(FieldMapId mapId)
        {
            return mapId switch
            {
                FieldMapId.Castle => "しろ",
                FieldMapId.Field => "フィールド",
                _ => "ハブ"
            };
        }

        private static string GetTileMessage(int tileId)
        {
            return tileId switch
            {
                MapFactory.GrassTile => "くさむらを すすんだ。",
                MapFactory.CastleFloorTile => "しろの ゆかを すすんだ。",
                MapFactory.CastleGateTile or MapFactory.FieldGateTile => "みちが つながっている。",
                _ => "フィールドを すすんだ。"
            };
        }

        private static int GetManhattanDistance(XnaPoint a, XnaPoint b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static string GetNpcMarkerLabel(FieldEventDefinition fieldEvent)
        {
            return fieldEvent.ActionType switch
            {
                FieldEventActionType.Recover => "泉",
                FieldEventActionType.Bank => "銀",
                _ when fieldEvent.Id.Contains("sign", StringComparison.OrdinalIgnoreCase) => "札",
                _ => "人"
            };
        }

        private static XnaPoint ToXnaPoint(DrawingPoint point)
        {
            return new XnaPoint(point.X, point.Y);
        }

        private static DrawingPoint ToDrawingPoint(XnaPoint point)
        {
            return new DrawingPoint(point.X, point.Y);
        }

        private static XnaColor ToXnaColor(DrawingColor color)
        {
            return new XnaColor(color.R, color.G, color.B, color.A);
        }

        private enum PlayerFacingDirection
        {
            Left,
            Right,
            Up,
            Down
        }
    }
}
