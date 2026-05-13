using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class ComboCounter : MonoBehaviour
    {
        [SerializeField] private Text comboText;
        [SerializeField] private Animator animator;
        [SerializeField] private float displayDuration = 2f;

        private int currentCombo;
        private float timer;
        private bool isShowing;

        private void Update()
        {
            if (!isShowing) return;

            timer += Time.deltaTime;
            if (timer >= displayDuration)
            {
                isShowing = false;
                currentCombo = 0;
                comboText.gameObject.SetActive(false);
            }
        }

        public void AddCombo()
        {
            currentCombo++;
            timer = 0f;
            isShowing = true;
            comboText.gameObject.SetActive(true);
            comboText.text = $"{currentCombo} Combo!";
            animator?.SetTrigger("Pulse");
        }

        public void ResetCombo()
        {
            currentCombo = 0;
            isShowing = false;
            comboText.gameObject.SetActive(false);
        }
    }
}
