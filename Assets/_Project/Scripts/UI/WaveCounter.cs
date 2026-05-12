using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class WaveCounter : MonoBehaviour
    {
        [SerializeField] private Text waveText;
        [SerializeField] private Text enemyCountText;

        public void SetWave(int currentWave, int totalWaves)
        {
            waveText.text = $"Wave {currentWave}/{totalWaves}";
        }

        public void SetEnemyCount(int current, int total)
        {
            enemyCountText.text = $"Enemies: {current}/{total}";
        }
    }
}
