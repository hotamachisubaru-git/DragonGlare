using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class InventoryGrid : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private int columns = 4;

        private void Awake()
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
        }

        public void SetItems(Sprite[] itemIcons, string[] itemNames, int[] itemCounts)
        {
            ClearItems();
            for (int i = 0; i < itemIcons.Length; i++)
            {
                var go = Instantiate(itemPrefab, grid.transform);
                var itemUI = go.GetComponent<InventoryItemUI>();
                itemUI.SetItem(itemIcons[i], itemNames[i], itemCounts[i]);
            }
        }

        public void ClearItems()
        {
            foreach (Transform child in grid.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
