using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class InventoryItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text countText;
        [SerializeField] private Image highlightImage;

        public void SetItem(Sprite icon, string name, int count)
        {
            iconImage.sprite = icon;
            nameText.text = name;
            countText.text = count > 1 ? $"x{count}" : string.Empty;
        }

        public void SetSelected(bool selected)
        {
            highlightImage.gameObject.SetActive(selected);
        }
    }
}
