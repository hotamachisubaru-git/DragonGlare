using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class QuestItemUI : MonoBehaviour
    {
        [SerializeField] private Text questNameText;
        [SerializeField] private Text progressText;
        [SerializeField] private Image completeIcon;

        public void SetQuest(string name, string progress)
        {
            questNameText.text = name;
            progressText.text = progress;
            completeIcon.gameObject.SetActive(progress == "Complete");
        }
    }
}
