using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain.Commerce;
using DragonGlare.Domain.Items;
using DragonGlare.Domain.Player;

namespace DragonGlare
{
    public class ShopListItem : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text atkText;
        [SerializeField] private Text defText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text ownText;
        [SerializeField] private Image highlightImage;

        public void Show(ShopMenuEntry entry, UiLanguage language, bool isSelected)
        {
            highlightImage.gameObject.SetActive(isSelected);

            if (entry.Type == ShopMenuEntryType.Product && entry.Product != null)
            {
                var item = entry.Product;
                nameText.text = item.Name;
                atkText.text = item.AttackBonus > 0 ? $"+{item.AttackBonus}" : "-";
                defText.text = item.DefenseBonus > 0 ? $"+{item.DefenseBonus}" : "-";
                priceText.text = item.Price.ToString();
                ownText.text = "0";
            }
            else if (entry.Type == ShopMenuEntryType.InventoryItem && entry.InventoryItem != null)
            {
                var item = entry.InventoryItem.Value;
                nameText.text = item.Name;
                atkText.text = item.AttackBonus > 0 ? $"+{item.AttackBonus}" : "-";
                defText.text = item.DefenseBonus > 0 ? $"+{item.DefenseBonus}" : "-";
                priceText.text = item.Price.ToString();
                ownText.text = item.Count.ToString();
            }
            else
            {
                nameText.text = entry.Label;
                atkText.text = string.Empty;
                defText.text = string.Empty;
                priceText.text = string.Empty;
                ownText.text = string.Empty;
            }
        }
    }
}
