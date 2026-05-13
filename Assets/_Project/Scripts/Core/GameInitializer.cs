using UnityEngine;

namespace DragonGlare
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] private GameManager gameManagerPrefab;

        private void Awake()
        {
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
        }
    }
}
