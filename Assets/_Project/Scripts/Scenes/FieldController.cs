using UnityEngine;
using System.Linq;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class FieldController : SceneControllerBase
    {
        [SerializeField] private FieldScene scene;

        public override void OnEnter()
        {
            scene?.Show(Session.Player, Session.CurrentEncounter, Session.IsFieldStatusVisible, Session.IsFieldDialogOpen,
                Session.ActiveFieldDialogPages, Session.ActiveFieldDialogPageIndex, Session.ActiveFieldDialogPortraitAssetName,
                Session.SelectedLanguage, Session.PlayerFacingDirection, Session.FieldMovementAnimationFramesRemaining,
                Session.FieldMovementAnimationDirection);
        }

        public override void OnUpdate()
        {
            if (Session.IsFieldDialogOpen)
            {
                UpdateDialog();
                return;
            }

            if (Input.WasPressed(KeyCode.B))
            {
                EnterBattle();
                return;
            }

            if (Input.WasPressed(KeyCode.V))
            {
                EnterShop();
                return;
            }

            if (Input.WasPressed(KeyCode.X))
            {
                Session.IsFieldStatusVisible = !Session.IsFieldStatusVisible;
                return;
            }

            if (Session.MovementCooldown > 0)
                Session.MovementCooldown--;

            var movement = GetMovementInput();
            if (movement != Vector2Int.zero && Session.MovementCooldown == 0)
            {
                SetPlayerFacingDirection(movement);
                var moved = TryMovePlayer(movement);
                if (!moved)
                    Audio.PlaySe(SoundEffect.Collision);
                Session.MovementCooldown = GameConstants.FieldMovementAnimationDuration;
                if (Session.CurrentGameState != GameState.Field)
                    return;
            }

            if (Input.WasFieldInteractPressed())
            {
                var fieldEvent = GetInteractableFieldEvent();
                if (fieldEvent != null)
                    OpenFieldDialog(fieldEvent);
            }
        }

        private Vector2Int GetMovementInput()
        {
            if (Input.HeldKeys.Contains(KeyCode.Up) || Input.HeldKeys.Contains(KeyCode.W))
                return new Vector2Int(0, -1);
            if (Input.HeldKeys.Contains(KeyCode.Down) || Input.HeldKeys.Contains(KeyCode.S))
                return new Vector2Int(0, 1);
            if (Input.HeldKeys.Contains(KeyCode.Left) || Input.HeldKeys.Contains(KeyCode.A))
                return new Vector2Int(-1, 0);
            if (Input.HeldKeys.Contains(KeyCode.Right) || Input.HeldKeys.Contains(KeyCode.D))
                return new Vector2Int(1, 0);
            return Vector2Int.zero;
        }

        private void UpdateDialog()
        {
            if (Input.WasFieldInteractPressed())
                AdvanceFieldDialog();
            else if (Input.WasPressed(KeyCode.Escape))
            {
                PlayCancelSe();
                CloseFieldDialog();
            }
        }

        private void SetPlayerFacingDirection(Vector2Int movement)
        {
            if (movement.x < 0) Session.PlayerFacingDirection = PlayerFacingDirection.Left;
            else if (movement.x > 0) Session.PlayerFacingDirection = PlayerFacingDirection.Right;
            else if (movement.y < 0) Session.PlayerFacingDirection = PlayerFacingDirection.Up;
            else if (movement.y > 0) Session.PlayerFacingDirection = PlayerFacingDirection.Down;
        }

        private bool TryMovePlayer(Vector2Int movement)
        {
            var target = new Vector2Int(Session.Player.TilePosition.X + movement.x, Session.Player.TilePosition.Y + movement.y);
            if (TryTransitionFromTile(target))
            {
                Session.BankService.AccrueStepInterest(Session.Player);
                return true;
            }

            if (!IsWalkableTile(target) || IsBlockedByFieldEvent(target))
                return false;

            Session.Player.TilePosition = target;
            Session.BankService.AccrueStepInterest(Session.Player);
            StartFieldMovementAnimation(movement);

            if (TryTriggerRandomEncounter())
            {
                Session.PersistProgress();
                return true;
            }

            Session.PersistProgress();
            return true;
        }

        private bool IsWalkableTile(Vector2Int tile)
        {
            if (tile.x < 0 || tile.y < 0 || tile.x >= Session.Map.GetLength(1) || tile.y >= Session.Map.GetLength(0))
                return false;
            return MapFactory.IsWalkableTileId(Session.Map[tile.y, tile.x]);
        }

        private bool IsBlockedByFieldEvent(Vector2Int tile)
        {
            var events = Session.FieldEventService.GetEventsForMap(Session.CurrentFieldMap);
            return events.Any(e => e.TilePosition.x == tile.x && e.TilePosition.y == tile.y && e.IsBlocking);
        }

        private bool TryTransitionFromTile(Vector2Int tile)
        {
            var transition = Session.FieldTransitionService.GetTransition(Session.CurrentFieldMap, tile);
            if (transition == null) return false;
            Session.SetFieldMap(transition.TargetMap);
            Session.Player.TilePosition = transition.TargetPosition;
            Session.ResetFieldUiState();
            return true;
        }

        private bool TryTriggerRandomEncounter()
        {
            if (Session.CurrentFieldMap != FieldMapId.Field)
                return false;
            var tileId = Session.Map[Session.Player.TilePosition.Y, Session.Player.TilePosition.X];
            if (MapFactory.IsFieldGateTileId(tileId))
                return false;
            Session.FieldEncounterStepsRemaining -= MapFactory.IsGrassTileId(tileId) ? 2 : 1;
            if (Session.FieldEncounterStepsRemaining > 0)
                return false;
            StartEncounterTransition(Session.BattleService.CreateEncounter(Session.Random, Session.CurrentFieldMap, Session.Player.Level));
            return true;
        }

        private void StartEncounterTransition(BattleEncounter encounter)
        {
            Session.PendingEncounter = encounter;
            Session.EncounterTransitionFrames = GameConstants.EncounterTransitionDuration;
            Session.ResetBattleSelectionState();
            Session.ResetEncounterCounter();
            Session.ChangeGameState(GameState.EncounterTransition);
            Audio.PlaySe(SoundEffect.Dialog);
        }

        private void StartFieldMovementAnimation(Vector2Int direction)
        {
            Session.FieldMovementAnimationDirection = direction;
            Session.FieldMovementAnimationFramesRemaining = GameConstants.FieldMovementAnimationDuration;
        }

        private void OpenFieldDialog(DragonGlare.Domain.Field.FieldEventDefinition fieldEvent)
        {
            Session.IsFieldDialogOpen = true;
            Session.ActiveFieldDialogPages = fieldEvent.DialogPages;
            Session.ActiveFieldDialogPageIndex = 0;
            Session.ActiveFieldDialogPortraitAssetName = fieldEvent.PortraitAssetName;
        }

        private void AdvanceFieldDialog()
        {
            Session.ActiveFieldDialogPageIndex++;
            if (Session.ActiveFieldDialogPageIndex >= Session.ActiveFieldDialogPages.Count)
            {
                CloseFieldDialog();
                var fieldEvent = GetInteractableFieldEvent();
                if (fieldEvent != null)
                {
                    var result = Session.FieldEventService.Interact(fieldEvent, Session.Player);
                    if (result.TransitionToBattle)
                        StartEncounterTransition(Session.BattleService.CreateEncounter(Session.Random, Session.CurrentFieldMap, Session.Player.Level));
                    else if (result.TransitionToShop)
                        EnterShop();
                    else if (result.TransitionToBank)
                        EnterBank();
                }
            }
        }

        private void CloseFieldDialog()
        {
            Session.IsFieldDialogOpen = false;
            Session.ActiveFieldDialogPages = System.Array.Empty<string>();
            Session.ActiveFieldDialogPageIndex = 0;
            Session.ActiveFieldDialogPortraitAssetName = null;
        }

        private DragonGlare.Domain.Field.FieldEventDefinition GetInteractableFieldEvent()
        {
            var events = Session.FieldEventService.GetEventsForMap(Session.CurrentFieldMap);
            return events.FirstOrDefault(e => e.TilePosition.x == Session.Player.TilePosition.X && e.TilePosition.y == Session.Player.TilePosition.Y && e.IsInteractable);
        }

        private void EnterBattle()
        {
            StartEncounterTransition(Session.BattleService.CreateEncounter(Session.Random, Session.CurrentFieldMap, Session.Player.Level));
        }

        private void EnterShop()
        {
            Session.ResetShopState();
            Session.ChangeGameState(GameState.ShopBuy);
            Audio.PlaySe(SoundEffect.Dialog);
        }

        private void EnterBank()
        {
            Session.ResetBankState();
            Session.ChangeGameState(GameState.Bank);
            Audio.PlaySe(SoundEffect.Dialog);
        }
    }
}