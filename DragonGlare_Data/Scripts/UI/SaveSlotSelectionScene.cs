using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class SaveSlotSelectionScene : MonoBehaviour
    {
        [SerializeField] private Transform slotsRoot;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Text titleText;
        [SerializeField] private Text helpText;
        [SerializeField] private Text noticeText;

        private SaveSlotUI[] slotUIs;

        public void Show(UiLanguage language, SaveSlotSelectionMode mode, int cursor, SaveSlotSummary[] summaries, int sourceSlot, string menuNotice)
        {
            gameObject.SetActive(true);
            if (slotUIs == null)
                InitializeSlots();

            titleText.text = GetTitle(mode, language);
            helpText.text = GetHelpText(mode, language);

            for (int i = 0; i < slotUIs.Length && i < summaries.Length; i++)
            {
                slotUIs[i].Show(summaries[i], language, cursor == i, sourceSlot);
            }

            noticeText.gameObject.SetActive(!string.IsNullOrWhiteSpace(menuNotice));
            noticeText.text = menuNotice;
        }

        private void InitializeSlots()
        {
            slotUIs = new SaveSlotUI[SaveManager.SlotCount];
            for (int i = 0; i < SaveManager.SlotCount; i++)
            {
                var go = Instantiate(slotPrefab, slotsRoot);
                slotUIs[i] = go.GetComponent<SaveSlotUI>();
            }
        }

        private static string GetTitle(SaveSlotSelectionMode mode, UiLanguage language)
        {
            if (language == UiLanguage.English)
            {
                return mode switch
                {
                    SaveSlotSelectionMode.Save => "CHOOSE A SAVE SLOT",
                    SaveSlotSelectionMode.Load => "CHOOSE A FILE TO LOAD",
                    SaveSlotSelectionMode.CopySource => "CHOOSE A FILE TO COPY",
                    SaveSlotSelectionMode.CopyDestination => "CHOOSE A DESTINATION",
                    SaveSlotSelectionMode.DeleteSelect => "CHOOSE A FILE TO DELETE",
                    SaveSlotSelectionMode.DeleteConfirm => "CONFIRM DELETE",
                    _ => string.Empty
                };
            }
            return mode switch
            {
                SaveSlotSelectionMode.Save => "ぼうけんのしょを えらんでください",
                SaveSlotSelectionMode.Load => "よみこむ ぼうけんのしょを えらんでください",
                SaveSlotSelectionMode.CopySource => "うつす ぼうけんのしょを えらんでください",
                SaveSlotSelectionMode.CopyDestination => "うつすさきを えらんでください",
                SaveSlotSelectionMode.DeleteSelect => "けす ぼうけんのしょを えらんでください",
                SaveSlotSelectionMode.DeleteConfirm => "ほんとうに けしますか？",
                _ => string.Empty
            };
        }

        private static string GetHelpText(SaveSlotSelectionMode mode, UiLanguage language)
        {
            if (language == UiLanguage.English)
            {
                return mode switch
                {
                    SaveSlotSelectionMode.Save => "A/Enter/Z: SAVE  B/Esc: NAME",
                    SaveSlotSelectionMode.Load => "A/Enter/Z: LOAD  B/Esc: MODE",
                    SaveSlotSelectionMode.CopySource => "A/Enter/Z: SOURCE  B/Esc: MODE",
                    SaveSlotSelectionMode.CopyDestination => "A/Enter/Z: COPY  B/Esc: BACK",
                    SaveSlotSelectionMode.DeleteSelect => "A/Enter/Z: DELETE  B/Esc: MODE",
                    SaveSlotSelectionMode.DeleteConfirm => "A/Enter/Z: DELETE  B/Esc: CANCEL",
                    _ => string.Empty
                };
            }
            return mode switch
            {
                SaveSlotSelectionMode.Save => "A/Enter/Z: きろく  B/Esc: なまえにもどる",
                SaveSlotSelectionMode.Load => "A/Enter/Z: よみこむ  B/Esc: モードにもどる",
                SaveSlotSelectionMode.CopySource => "A/Enter/Z: うつすもと  B/Esc: モードにもどる",
                SaveSlotSelectionMode.CopyDestination => "A/Enter/Z: うつす  B/Esc: もどる",
                SaveSlotSelectionMode.DeleteSelect => "A/Enter/Z: けすデータ  B/Esc: モードにもどる",
                SaveSlotSelectionMode.DeleteConfirm => "A/Enter/Z: けす  B/Esc: やめる",
                _ => string.Empty
            };
        }
    }
}
