using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class TitleScene : MonoBehaviour
    {
        [SerializeField] private Image titleImage;
        [SerializeField] private Text pressStartText;
        [SerializeField] private float blinkInterval = 0.8f;

        private float timer;
        private bool textVisible = true;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= blinkInterval)
            {
                timer = 0f;
                textVisible = !textVisible;
                pressStartText.enabled = textVisible;
            }

            if (Input.anyKeyDown)
            {
                GameManager.Instance.SceneUI.ChangeGameState(GameState.ModeSelect);
            }
        }
    }
}