using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    [RequireComponent(typeof(Canvas))]
    public class PixelPerfectCanvas : MonoBehaviour
    {
        [SerializeField] private int referenceWidth = 640;
        [SerializeField] private int referenceHeight = 480;
        [SerializeField] private CanvasScaler scaler;

        private void Awake()
        {
            if (scaler == null)
                scaler = GetComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
        }
    }
}
