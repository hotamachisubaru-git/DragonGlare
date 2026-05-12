using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private float countSpeed = 100f;

        private int targetScore;
        private int currentScore;

        private void Update()
        {
            if (currentScore != targetScore)
            {
                int diff = targetScore - currentScore;
                int change = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(diff) * Time.deltaTime * countSpeed));
                currentScore += Mathf.Clamp(diff, -change, change);
                scoreText.text = currentScore.ToString("N0");
            }
        }

        public void SetScore(int score)
        {
            targetScore = score;
        }

        public void AddScore(int amount)
        {
            targetScore += amount;
        }
    }
}
