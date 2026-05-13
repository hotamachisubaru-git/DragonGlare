using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private Text floatingText;
        [SerializeField] private float floatSpeed = 30f;
        [SerializeField] private float lifetime = 1.5f;

        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            var alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
            floatingText.color = new Color(floatingText.color.r, floatingText.color.g, floatingText.color.b, alpha);

            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        public void Show(string text, Color color)
        {
            floatingText.text = text;
            floatingText.color = color;
        }
    }
}
