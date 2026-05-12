using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class SelectionList : MonoBehaviour
    {
        [SerializeField] private Transform itemRoot;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private RectTransform cursor;
        [SerializeField] private Text titleText;
        [SerializeField] private Text counterText;

        private List<SelectionListItem> items = new();

        public void Show(string title, string counter, int selectedIndex)
        {
            gameObject.SetActive(true);
            titleText.text = title;
            counterText.text = counter;
            UpdateCursor(selectedIndex);
        }

        public void SetItems(string[] labels, string[] details, string[] badges)
        {
            EnsureItemCount(labels.Length);
            for (int i = 0; i < labels.Length; i++)
            {
                items[i].Show(labels[i], details[i], badges[i]);
            }
        }

        public void UpdateCursor(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                cursor.position = items[index].transform.position;
            }
        }

        private void EnsureItemCount(int count)
        {
            while (items.Count < count)
            {
                var go = Instantiate(itemPrefab, itemRoot);
                items.Add(go.GetComponent<SelectionListItem>());
            }
            for (int i = 0; i < items.Count; i++)
            {
                items[i].gameObject.SetActive(i < count);
            }
        }
    }
}
