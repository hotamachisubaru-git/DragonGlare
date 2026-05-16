using UnityEngine;
using System.Collections;

namespace DragonGlare
{
    public class ScreenShake : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float defaultDuration = 0.5f;
        [SerializeField] private float defaultMagnitude = 0.1f;

        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;
        }

        public void Shake(float duration = -1f, float magnitude = -1f)
        {
            float dur = duration > 0 ? duration : defaultDuration;
            float mag = magnitude > 0 ? magnitude : defaultMagnitude;

            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            shakeCoroutine = StartCoroutine(ShakeCoroutine(dur, mag));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            originalPosition = cameraTransform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
                float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
                cameraTransform.localPosition = originalPosition + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cameraTransform.localPosition = originalPosition;
        }
    }
}
