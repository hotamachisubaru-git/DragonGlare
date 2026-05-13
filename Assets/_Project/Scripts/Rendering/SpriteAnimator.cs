using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameRate = 8f;
        [SerializeField] private bool loop = true;

        private float timer;
        private int currentFrame;

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            timer += Time.deltaTime;
            if (timer >= 1f / frameRate)
            {
                timer = 0f;
                currentFrame++;
                if (currentFrame >= frames.Length)
                {
                    if (loop)
                        currentFrame = 0;
                    else
                        currentFrame = frames.Length - 1;
                }
                targetImage.sprite = frames[currentFrame];
            }
        }

        public void SetFrames(Sprite[] newFrames)
        {
            frames = newFrames;
            currentFrame = 0;
            if (frames.Length > 0)
                targetImage.sprite = frames[0];
        }
    }
}
