using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class StatusPanel : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text hpText;
        [SerializeField] private Text mpText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text atkDefText;
        [SerializeField] private Text expText;
        [SerializeField] private Transform equipmentList;
        [SerializeField] private GameObject equipmentItemPrefab;

        public void Show(string name, int level, int currentHp, int maxHp, int currentMp, int maxMp, int gold, int atk, int def, int exp, string[] equipmentNames)
        {
            gameObject.SetActive(true);
            nameText.text = name;
            levelText.text = $"Lv.{level}";
            hpText.text = $"HP {currentHp}/{maxHp}";
            mpText.text = $"MP {currentMp}/{maxMp}";
            goldText.text = $"G {gold}";
            atkDefText.text = $"ATK {atk}  DEF {def}";
            expText.text = $"EXP {exp}";

            for (int i = 0; i < equipmentNames.Length; i++)
            {
                if (i >= equipmentList.childCount)
                {
                    Instantiate(equipmentItemPrefab, equipmentList);
                }
                equipmentList.GetChild(i).GetComponent<Text>().text = equipmentNames[i];
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
