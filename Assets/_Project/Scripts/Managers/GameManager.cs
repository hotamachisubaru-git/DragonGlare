using UnityEngine;

namespace DragonGlare
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private InputManager inputManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private SpriteManager spriteManager;
        [SerializeField] private SceneDirector sceneDirector;

        public InputManager Input => inputManager;
        public AudioManager Audio => audioManager;
        public SaveManager Save => saveManager;
        public SpriteManager Sprites => spriteManager;
        public SceneDirector SceneDirector => sceneDirector;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeManagers();
        }

        private void InitializeManagers()
        {
            if (inputManager == null) inputManager = gameObject.AddComponent<InputManager>();
            if (audioManager == null) audioManager = gameObject.AddComponent<AudioManager>();
            if (saveManager == null) saveManager = gameObject.AddComponent<SaveManager>();
            if (spriteManager == null) spriteManager = gameObject.AddComponent<SpriteManager>();
        }

        private void Update()
        {
            inputManager?.PollInput();
            audioManager?.UpdateBgm();
        }

        private void OnApplicationQuit()
        {
            GameSession.Instance?.FlushSave();
        }
    }
}
