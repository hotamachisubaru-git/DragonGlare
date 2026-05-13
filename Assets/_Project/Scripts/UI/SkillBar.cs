using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class SkillBar : MonoBehaviour
    {
        [SerializeField] private Transform skillRoot;
        [SerializeField] private GameObject skillIconPrefab;

        public void SetSkills(Sprite[] skillIcons, float[] cooldowns, float[] maxCooldowns)
        {
            ClearSkills();
            for (int i = 0; i < skillIcons.Length; i++)
            {
                var go = Instantiate(skillIconPrefab, skillRoot);
                var skillIcon = go.GetComponent<SkillIcon>();
                skillIcon.SetSkill(skillIcons[i], cooldowns[i], maxCooldowns[i]);
            }
        }

        public void ClearSkills()
        {
            foreach (Transform child in skillRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
