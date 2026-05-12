using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform tooltipPanel;
        [SerializeField] private Text tooltipText;
        [SerializeField] private Vector2 offset = new(10f, 10f);

        private void Update()
        {
            if (tooltipPanel.gameObject.activeSelf)
            {
                tooltipPanel.position = Input.mousePosition + (Vector3)offset;
            }
        }

        public void Show(string text)
        {
            tooltipPanel.gameObject.SetActive(true);
            tooltipText.text = text;
        }

        public void Hide()
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }
}
