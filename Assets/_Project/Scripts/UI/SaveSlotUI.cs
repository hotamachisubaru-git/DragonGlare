using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class SaveSlotUI : MonoBehaviour
    {
        [SerializeField] private Text slotNumberText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text dateText;
        [SerializeField] private Text badgeText;
        [SerializeField] private Image cursorImage;

        public void Show(SaveSlotSummary summary, UiLanguage language, bool isSelected, int sourceSlot)
        {
            slotNumberText.text = language == UiLanguage.English ? $"FILE {summary.SlotNumber}" : $"ぼうけんのしょ {summary.SlotNumber}";
            cursorImage.gameObject.SetActive(isSelected);

            if (summary.SlotNumber == sourceSlot)
            {
                badgeText.gameObject.SetActive(true);
                badgeText.text = sourceSlot > 0 ? (language == UiLanguage.English ? "SOURCE" : "うつすもと") : string.Empty;
            }
            else
            {
                badgeText.gameObject.SetActive(false);
            }

            switch (summary.State)
            {
                case SaveSlotState.Occupied:
                    statusText.text = $"{summary.Name}   LV {summary.Level}   G {summary.Gold}";
                    detailText.text = $"{GetMapName(summary.CurrentFieldMap, language)}  {summary.SavedAtLocal:yyyy/MM/dd HH:mm}";
                    break;
                case SaveSlotState.Corrupted:
                    statusText.text = language == UiLanguage.English ? "BROKEN DATA" : "BROKEN DATA / よみこめません";
                    detailText.text = string.Empty;
                    break;
                default:
                    statusText.text = language == UiLanguage.English ? "NO DATA" : "NO DATA / まだ きろくがありません";
                    detailText.text = string.Empty;
                    break;
            }
        }

        private static string GetMapName(FieldMapId mapId, UiLanguage language)
        {
            return mapId switch
            {
                FieldMapId.Hub => language == UiLanguage.English ? "Hub" : "拠点",
                FieldMapId.Castle => language == UiLanguage.English ? "Castle" : "城",
                FieldMapId.Dungeon => language == UiLanguage.English ? "Dungeon" : "ダンジョン",
                FieldMapId.Field => language == UiLanguage.English ? "Field" : "野外",
                _ => string.Empty
            };
        }
    }
}
