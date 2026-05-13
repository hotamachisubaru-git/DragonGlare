using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class VersionDisplay : MonoBehaviour
    {
        [SerializeField] private Text versionText;

        private void Start()
        {
            versionText.text = $"v{Application.version}";
        }
    }
}
