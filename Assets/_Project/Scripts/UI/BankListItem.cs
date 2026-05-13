using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class BankListItem : MonoBehaviour
    {
        [SerializeField] private Text labelText;
        [SerializeField] private Text amountText;
        [SerializeField] private Image highlightImage;

        public void Show(BankOption option, UiLanguage language, bool isSelected, int resolvedAmount)
        {
            highlightImage.gameObject.SetActive(isSelected);
            labelText.text = option.Label;
            amountText.text = option.Quit ? string.Empty : $"{resolvedAmount}G";
        }
    }
}
