using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class RetroWindow : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private Color backgroundColor = Color.black;
        [SerializeField] private Color borderColor = Color.white;
        [SerializeField] private int borderThickness = 1;

        private void Awake()
        {
            if (background != null)
                background.color = backgroundColor;
            if (border != null)
                border.color = borderColor;
        }

        public void SetColors(Color bg, Color bd)
        {
            if (background != null)
                background.color = bg;
            if (border != null)
                border.color = bd;
        }
    }
}
