using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class TypewriterText : MonoBehaviour
    {
        [SerializeField] private Text targetText;
        [SerializeField] private float typeSpeed = 0.05f;
        [SerializeField] private AudioSource typeSound;

        private string fullText;
        private int currentIndex;
        private float timer;
        private bool isTyping;

        private void Update()
        {
            if (!isTyping) return;

            timer += Time.deltaTime;
            if (timer >= typeSpeed)
            {
                timer = 0f;
                currentIndex++;
                if (currentIndex >= fullText.Length)
                {
                    currentIndex = fullText.Length;
                    isTyping = false;
                }
                targetText.text = fullText[..currentIndex];
                if (typeSound != null && currentIndex < fullText.Length)
                    typeSound.Play();
            }
        }

        public void StartTyping(string text)
        {
            fullText = text;
            currentIndex = 0;
            isTyping = true;
            targetText.text = string.Empty;
        }

        public void SkipTyping()
        {
            isTyping = false;
            targetText.text = fullText;
        }

        public bool IsTyping => isTyping;
    }
}
