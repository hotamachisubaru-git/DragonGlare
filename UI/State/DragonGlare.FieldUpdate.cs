using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Services;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void UpdateField()
    {
        if (isFieldDialogOpen)
        {
            if (WasFieldInteractPressed())
            {
                AdvanceFieldDialog();
            }
            else if (WasPressed(Keys.Escape))
            {
                CloseFieldDialog();
            }

            return;
        }

        if (WasPressed(Keys.B))
        {
            EnterBattle();
            return;
        }

        if (WasPressed(Keys.V))
        {
            EnterShopBuy();
            return;
        }

        if (WasPressed(Keys.X))
        {
            isFieldStatusVisible = !isFieldStatusVisible;
            return;
        }

        if (movementCooldown > 0)
        {
            movementCooldown--;
        }

        var movement = Point.Empty;
        if (heldKeys.Contains(Keys.Up) || heldKeys.Contains(Keys.W))
        {
            movement = new Point(0, -1);
        }
        else if (heldKeys.Contains(Keys.Down) || heldKeys.Contains(Keys.S))
        {
            movement = new Point(0, 1);
        }
        else if (heldKeys.Contains(Keys.Left) || heldKeys.Contains(Keys.A))
        {
            movement = new Point(-1, 0);
        }
        else if (heldKeys.Contains(Keys.Right) || heldKeys.Contains(Keys.D))
        {
            movement = new Point(1, 0);
        }

        if (movement != Point.Empty && movementCooldown == 0)
        {
            SetPlayerFacingDirection(movement);
            var moved = TryMovePlayer(movement);
            if (!moved)
            {
                PlaySe(SoundEffect.Collision);
            }

            movementCooldown = FieldMovementAnimationDuration;
            if (gameState != GameState.Field)
            {
                return;
            }
        }

        if (WasFieldInteractPressed())
        {
            var fieldEvent = GetInteractableFieldEvent();
            if (fieldEvent is not null)
            {
                OpenFieldDialog(fieldEvent);
            }
        }
    }

    private void EnterBattle()
    {
        StartEncounterTransition(battleService.CreateEncounter(random, currentFieldMap, player.Level));
    }

    private bool TryTriggerRandomEncounter()
    {
        if (currentFieldMap != FieldMapId.Field)
        {
            return false;
        }

        var tileId = map[player.TilePosition.Y, player.TilePosition.X];
        if (MapFactory.IsFieldGateTileId(tileId))
        {
            return false;
        }

        fieldEncounterStepsRemaining -= MapFactory.IsGrassTileId(tileId) ? 2 : 1;
        if (fieldEncounterStepsRemaining > 0)
        {
            return false;
        }

        StartEncounterTransition(battleService.CreateEncounter(random, currentFieldMap, player.Level));
        return true;
    }

    private void StartEncounterTransition(BattleEncounter encounter)
    {
        pendingEncounter = encounter;
        encounterTransitionFrames = EncounterTransitionDuration;
        ResetBattleSelectionState();
        ResetEncounterCounter();
        ChangeGameState(GameState.EncounterTransition);
        PlaySe(SoundEffect.Dialog);
    }
}
