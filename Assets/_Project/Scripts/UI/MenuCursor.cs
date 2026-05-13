using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class MenuCursor : MonoBehaviour
    {
        [SerializeField] private Image cursorImage;
        [SerializeField] private float blinkInterval = 0.3f;

        private float timer;
        private bool visible = true;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= blinkInterval)
            {
                timer = 0f;
                visible = !visible;
                if (cursorImage != null)
                    cursorImage.enabled = visible;
            }
        }

        public void SetPosition(Vector2 position)
        {
            if (cursorImage != null)
                cursorImage.rectTransform.anchoredPosition = position;
        }
    }
}
