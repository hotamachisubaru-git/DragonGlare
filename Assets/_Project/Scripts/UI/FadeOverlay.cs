using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class FadeOverlay : MonoBehaviour
    {
        [SerializeField] private Image overlayImage;

        public void SetAlpha(float alpha)
        {
            if (overlayImage != null)
            {
                var color = overlayImage.color;
                color.a = Mathf.Clamp01(alpha);
                overlayImage.color = color;
            }
        }
    }
}
