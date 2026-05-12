using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class EquipmentPanel : MonoBehaviour
    {
        [SerializeField] private Transform slotRoot;
        [SerializeField] private GameObject slotPrefab;

        public void SetEquipment(Sprite[] equipmentIcons, string[] equipmentNames)
        {
            ClearSlots();
            for (int i = 0; i < equipmentIcons.Length; i++)
            {
                var go = Instantiate(slotPrefab, slotRoot);
                var slot = go.GetComponent<EquipmentSlotUI>();
                slot.SetEquipment(equipmentIcons[i], equipmentNames[i]);
            }
        }

        public void ClearSlots()
        {
            foreach (Transform child in slotRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
