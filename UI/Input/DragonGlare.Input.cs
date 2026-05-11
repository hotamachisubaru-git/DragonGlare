using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Services;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void MoveNameCursor(int deltaX, int deltaY)
    {
        var previousRow = nameCursorRow;
        var previousColumn = nameCursorColumn;
        var table = GameContent.GetNameTable(selectedLanguage);
        nameCursorRow = Math.Clamp(nameCursorRow + deltaY, 0, table.Length - 1);
        var maxColumn = table[nameCursorRow].Length - 1;
        nameCursorColumn = Math.Clamp(nameCursorColumn + deltaX, 0, maxColumn);
        if (previousRow != nameCursorRow || previousColumn != nameCursorColumn)
        {
            PlaySe(SoundEffect.Cursor);
        }
    }

    private void AddSelectedCharacter()
    {
        var table = GameContent.GetNameTable(selectedLanguage);
        var selected = table[nameCursorRow][nameCursorColumn];

        var deleteToken = selectedLanguage == UiLanguage.Japanese ? "けす" : "DEL";
        var endToken = selectedLanguage == UiLanguage.Japanese ? "おわり" : "END";

        if (selected == deleteToken)
        {
            RemoveLastCharacter();
            return;
        }

        if (selected == endToken)
        {
            if (playerName.Length > 0)
            {
                player.Name = TrimPlayerName(playerName.ToString());
                OpenSaveSlotSelection(SaveSlotSelectionMode.Save);
            }

            return;
        }

        if (playerName.Length < MaxPlayerNameLength)
        {
            playerName.Append(selected);
        }
    }

    private void RemoveLastCharacter()
    {
        if (playerName.Length > 0)
        {
            playerName.Remove(playerName.Length - 1, 1);
            PlayCancelSe();
        }
    }

    private bool IsWalkableTile(Point tile)
    {
        return IsWalkableTile(map, tile);
    }

    private static bool IsWalkableTile(int[,] fieldMap, Point tile)
    {
        if (tile.X < 0 || tile.Y < 0 || tile.X >= fieldMap.GetLength(1) || tile.Y >= fieldMap.GetLength(0))
        {
            return false;
        }

        return MapFactory.IsWalkableTileId(fieldMap[tile.Y, tile.X]);
    }

    private bool TryMovePlayer(Point movement)
    {
        var target = new Point(player.TilePosition.X + movement.X, player.TilePosition.Y + movement.Y);
        if (TryTransitionFromTile(target))
        {
            bankService.AccrueStepInterest(player);
            return true;
        }

        if (!IsWalkableTile(target) || IsBlockedByFieldEvent(target))
        {
            return false;
        }

        player.TilePosition = target;
        bankService.AccrueStepInterest(player);
        StartFieldMovementAnimation(movement);

        if (TryTriggerRandomEncounter())
        {
            PersistProgress();
            return true;
        }

        PersistProgress();
        return true;
    }

    private void SetPlayerFacingDirection(Point movement)
    {
        if (movement.X < 0)
        {
            playerFacingDirection = PlayerFacingDirection.Left;
            return;
        }

        if (movement.X > 0)
        {
            playerFacingDirection = PlayerFacingDirection.Right;
            return;
        }

        if (movement.Y < 0)
        {
            playerFacingDirection = PlayerFacingDirection.Up;
            return;
        }

        if (movement.Y > 0)
        {
            playerFacingDirection = PlayerFacingDirection.Down;
        }
    }

    private bool WasPressed(Keys key) => pressedKeys.Contains(key);

    private bool WasPrimaryConfirmPressed()
    {
        return WasPressed(Keys.Enter) || WasPressed(Keys.Z);
    }

    private bool WasConfirmPressed()
    {
        return WasPrimaryConfirmPressed() || WasPressed(Keys.X);
    }

    private bool WasShopConfirmPressed()
    {
        return WasPrimaryConfirmPressed();
    }

    private bool WasShopBackPressed()
    {
        return WasPressed(Keys.Escape) || WasPressed(Keys.X);
    }

    private bool WasBattleSubmenuConfirmPressed()
    {
        return WasPrimaryConfirmPressed();
    }

    private bool WasBattleSubmenuBackPressed()
    {
        return WasPressed(Keys.Escape) || WasPressed(Keys.X);
    }

    private bool WasFieldInteractPressed()
    {
        return WasPrimaryConfirmPressed();
    }

    private void PlayCursorSeIfChanged(int previousValue, int currentValue)
    {
        if (previousValue != currentValue)
        {
            PlaySe(SoundEffect.Cursor);
        }
    }

    private void PlayCancelSe()
    {
        PlaySe(SoundEffect.Cancel);
    }
}
