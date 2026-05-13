using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonGlare
{
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float handleRange = 1f;

        private Vector2 inputVector;
        private bool isDragging;

        public Vector2 InputVector => inputVector;
        public bool IsDragging => isDragging;

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 direction = eventData.position - (Vector2)joystickBackground.position;
            inputVector = direction.magnitude > joystickBackground.sizeDelta.x / 2f
                ? direction.normalized
                : direction / (joystickBackground.sizeDelta.x / 2f);

            joystickHandle.anchoredPosition = inputVector * joystickBackground.sizeDelta.x / 2f * handleRange;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            inputVector = Vector2.zero;
            joystickHandle.anchoredPosition = Vector2.zero;
        }
    }
}
