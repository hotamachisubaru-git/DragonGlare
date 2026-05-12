using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class BuffBar : MonoBehaviour
    {
        [SerializeField] private Transform buffRoot;
        [SerializeField] private GameObject buffIconPrefab;

        public void UpdateBuffs(Sprite[] icons, float[] durations, float[] maxDurations)
        {
            ClearBuffs();
            for (int i = 0; i < icons.Length; i++)
            {
                var go = Instantiate(buffIconPrefab, buffRoot);
                var buffIcon = go.GetComponent<BuffIcon>();
                buffIcon.SetBuff(icons[i], durations[i], maxDurations[i]);
            }
        }

        public void ClearBuffs()
        {
            foreach (Transform child in buffRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
