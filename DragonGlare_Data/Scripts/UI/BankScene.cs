using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain.Player;
using System.Collections.Generic;

namespace DragonGlare
{
    public class BankScene : MonoBehaviour
    {
        [SerializeField] private Text helpText;
        [SerializeField] private Transform listRoot;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private Text listTitle;
        [SerializeField] private Text cashText;
        [SerializeField] private Text bankText;
        [SerializeField] private Text loanText;
        [SerializeField] private Text creditText;
        [SerializeField] private Text messageText;
        [SerializeField] private RectTransform cursor;

        private List<BankListItem> listItems = new();

        public void Show(PlayerProgress player, BankPhase phase, int promptCursor, int itemCursor, string message, UiLanguage language)
        {
            gameObject.SetActive(true);
            messageText.text = message;
            cashText.text = language == UiLanguage.English ? $"CASH: {player.Gold}G" : $"てもち: {player.Gold}G";
            bankText.text = language == UiLanguage.English ? $"BANK: {player.BankGold}G" : $"よきん: {player.BankGold}G";
            loanText.text = language == UiLanguage.English ? $"LOAN: {player.LoanBalance}G" : $"しゃっきん: {player.LoanBalance}G";
            var bankService = new DragonGlare.Services.BankService();
            creditText.text = language == UiLanguage.English ? $"CREDIT: {bankService.GetAvailableCredit(player)}G" : $"しんよう: {bankService.GetAvailableCredit(player)}G";

            if (phase == BankPhase.Welcome)
            {
                ShowWelcome(language, promptCursor);
                return;
            }

            ShowList(phase, itemCursor, player, language);
        }

        private void ShowWelcome(UiLanguage language, int promptCursor)
        {
            listRoot.gameObject.SetActive(false);
            helpText.gameObject.SetActive(true);

            var options = language == UiLanguage.English
                ? new[] { "DEPOSIT", "WITHDRAW", "BORROW", "LEAVE" }
                : new[] { "あずける", "ひきだす", "かりる", "やめる" };

            helpText.text = string.Join("\n", options.Select((o, i) => i == promptCursor ? $"> {o}" : $"  {o}"));
        }

        private void ShowList(BankPhase phase, int itemCursor, PlayerProgress player, UiLanguage language)
        {
            listRoot.gameObject.SetActive(true);
            helpText.gameObject.SetActive(false);

            listTitle.text = phase switch
            {
                BankPhase.DepositList => language == UiLanguage.English ? "DEPOSIT" : "あずける",
                BankPhase.WithdrawList => language == UiLanguage.English ? "WITHDRAW" : "ひきだす",
                BankPhase.BorrowList => language == UiLanguage.English ? "BORROW" : "かりる",
                _ => language == UiLanguage.English ? "BANK" : "ぎんこう"
            };

            var options = GetBankAmountOptions(language);
            EnsureListItems(options.Count);

            for (int i = 0; i < options.Count; i++)
            {
                var amount = options[i].Amount > 0 ? options[i].Amount : ResolveAmount(options[i], phase, player);
                listItems[i].Show(options[i], language, i == itemCursor, amount);
            }

            if (itemCursor >= 0 && itemCursor < listItems.Count)
            {
                cursor.position = listItems[itemCursor].transform.position;
            }
        }

        private void EnsureListItems(int count)
        {
            while (listItems.Count < count)
            {
                var go = Instantiate(listItemPrefab, listRoot);
                listItems.Add(go.GetComponent<BankListItem>());
            }
            for (int i = 0; i < listItems.Count; i++)
            {
                listItems[i].gameObject.SetActive(i < count);
            }
        }

        private static List<BankOption> GetBankAmountOptions(UiLanguage language)
        {
            return new List<BankOption>
            {
                new() { Label = language == UiLanguage.English ? "100G" : "100G", Amount = 100 },
                new() { Label = language == UiLanguage.English ? "1000G" : "1000G", Amount = 1000 },
                new() { Label = language == UiLanguage.English ? "10000G" : "10000G", Amount = 10000 },
                new() { Label = language == UiLanguage.English ? "All" : "ぜんぶ", Amount = -1 },
                new() { Label = language == UiLanguage.English ? "Back" : "もどる", Quit = true }
            };
        }

        private static int ResolveAmount(BankOption option, BankPhase phase, PlayerProgress player)
        {
            if (option.Amount > 0) return option.Amount;
            var bankService = new DragonGlare.Services.BankService();
            return phase switch
            {
                BankPhase.DepositList => player.Gold,
                BankPhase.WithdrawList => player.BankGold,
                BankPhase.BorrowList => bankService.GetAvailableCredit(player),
                _ => 0
            };
        }
    }
}
