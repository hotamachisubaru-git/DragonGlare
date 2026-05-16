using UnityEngine;
using System.Collections.Generic;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class BankController : SceneControllerBase
    {
        [SerializeField] private BankScene scene;

        public override void OnEnter()
        {
            scene?.Show(Session.Player, Session.BankPhase, Session.BankPromptCursor, Session.BankItemCursor,
                Session.BankMessage, Session.SelectedLanguage);
        }

        public override void OnUpdate()
        {
            if (Session.BankPhase == BankPhase.Welcome)
            {
                UpdateWelcome();
                return;
            }

            UpdateList();
        }

        private void UpdateWelcome()
        {
            var previousCursor = Session.BankPromptCursor;
            if (Input.WasPressed(KeyCode.UpArrow) || Input.WasPressed(KeyCode.W))
                Session.BankPromptCursor = Mathf.Max(0, Session.BankPromptCursor - 1);
            else if (Input.WasPressed(KeyCode.DownArrow) || Input.WasPressed(KeyCode.S))
                Session.BankPromptCursor = Mathf.Min(3, Session.BankPromptCursor + 1);

            if (previousCursor != Session.BankPromptCursor)
                PlayCursorSe();

            if (Input.WasShopBackPressed())
            {
                PlayCancelSe();
                Session.ChangeGameState(GameState.Field);
                return;
            }

            if (!Input.WasShopConfirmPressed())
                return;

            switch (Session.BankPromptCursor)
            {
                case 0:
                    OpenBankList(BankPhase.DepositList);
                    break;
                case 1:
                    OpenBankList(BankPhase.WithdrawList);
                    break;
                case 2:
                    OpenBankList(BankPhase.BorrowList);
                    break;
                default:
                    PlayCancelSe();
                    Session.ChangeGameState(GameState.Field);
                    break;
            }
        }

        private void UpdateList()
        {
            var options = Session.GetBankAmountOptions();
            var previousItemCursor = Session.BankItemCursor;
            if (Input.WasPressed(KeyCode.UpArrow) || Input.WasPressed(KeyCode.W))
                Session.BankItemCursor = Mathf.Max(0, Session.BankItemCursor - 1);
            else if (Input.WasPressed(KeyCode.DownArrow) || Input.WasPressed(KeyCode.S))
                Session.BankItemCursor = Mathf.Min(options.Count - 1, Session.BankItemCursor + 1);

            if (previousItemCursor != Session.BankItemCursor)
                PlayCursorSe();

            if (Input.WasShopBackPressed())
            {
                PlayCancelSe();
                ReturnToBankPrompt(Session.GetBankReturnMessage());
                return;
            }

            if (!Input.WasShopConfirmPressed())
                return;

            var selectedOption = options[Session.BankItemCursor];
            if (selectedOption.Quit)
            {
                PlayCancelSe();
                ReturnToBankPrompt(Session.GetBankReturnMessage());
                return;
            }

            var amount = Session.ResolveBankTransactionAmount(selectedOption);
            var result = Session.BankPhase switch
            {
                BankPhase.DepositList => Session.BankService.Deposit(Session.Player, amount),
                BankPhase.WithdrawList => Session.BankService.Withdraw(Session.Player, amount),
                BankPhase.BorrowList => Session.BankService.Borrow(Session.Player, amount),
                _ => new BankTransactionResult(false, 0, 0, Session.GetBankReturnMessage())
            };

            Session.BankMessage = result.Message;
            if (result.Success)
                Session.PersistProgress();
        }

        private void OpenBankList(BankPhase nextPhase)
        {
            Session.BankPhase = nextPhase;
            Session.BankItemCursor = 0;
            Session.BankMessage = nextPhase switch
            {
                BankPhase.DepositList => Session.GetBankDepositMessage(),
                BankPhase.WithdrawList => Session.GetBankWithdrawMessage(),
                BankPhase.BorrowList => Session.GetBankBorrowMessage(),
                _ => Session.GetBankWelcomeMessage()
            };
        }

        private void ReturnToBankPrompt(string message)
        {
            Session.BankPhase = BankPhase.Welcome;
            Session.BankPromptCursor = 0;
            Session.BankItemCursor = 0;
            Session.BankMessage = message;
        }
    }
}