using UnityEngine;

namespace DragonGlare
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private int referenceWidth = 640;
        [SerializeField] private int referenceHeight = 480;
        [SerializeField] private float pixelsPerUnit = 32f;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            mainCamera.orthographic = true;
            UpdateOrthographicSize();
        }

        private void Update()
        {
            UpdateOrthographicSize();
        }

        private void UpdateOrthographicSize()
        {
            float targetHeight = referenceHeight / pixelsPerUnit;
            mainCamera.orthographicSize = targetHeight / 2f;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }
}
