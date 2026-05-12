using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class SelectionListItem : MonoBehaviour
    {
        [SerializeField] private Text labelText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text badgeText;
        [SerializeField] private Image highlightImage;

        public void Show(string label, string detail, string badge)
        {
            labelText.text = label;
            detailText.text = detail;
            badgeText.text = badge;
        }

        public void SetSelected(bool selected)
        {
            highlightImage.gameObject.SetActive(selected);
        }
    }
}
