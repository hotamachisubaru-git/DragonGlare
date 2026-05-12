using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class QuestTracker : MonoBehaviour
    {
        [SerializeField] private Transform questRoot;
        [SerializeField] private GameObject questItemPrefab;
        [SerializeField] private Text questTitle;

        public void SetQuests(string[] questNames, string[] questProgress)
        {
            ClearQuests();
            for (int i = 0; i < questNames.Length; i++)
            {
                var go = Instantiate(questItemPrefab, questRoot);
                var questItem = go.GetComponent<QuestItemUI>();
                questItem.SetQuest(questNames[i], questProgress[i]);
            }
        }

        public void ClearQuests()
        {
            foreach (Transform child in questRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
