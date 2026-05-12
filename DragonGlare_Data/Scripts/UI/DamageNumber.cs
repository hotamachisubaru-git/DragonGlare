using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private Text damageText;
        [SerializeField] private float floatSpeed = 50f;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private float lifetime = 1f;

        private float timer;
        private Color originalColor;

        private void Awake()
        {
            originalColor = damageText.color;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            var alpha = Mathf.Lerp(originalColor.a, 0f, timer / lifetime);
            damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        public void Show(int damage, bool isCritical = false)
        {
            damageText.text = damage.ToString();
            if (isCritical)
            {
                damageText.color = Color.red;
                damageText.fontSize = Mathf.RoundToInt(damageText.fontSize * 1.5f);
            }
        }
    }
}
