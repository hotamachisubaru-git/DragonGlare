using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class EquipmentSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text slotNameText;
        [SerializeField] private Text itemNameText;
        [SerializeField] private Image highlightImage;

        public void SetEquipment(Sprite icon, string itemName)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
            itemNameText.text = itemName;
        }

        public void SetSelected(bool selected)
        {
            highlightImage.gameObject.SetActive(selected);
        }

        public void SetSlotName(string name)
        {
            slotNameText.text = name;
        }
    }
}
