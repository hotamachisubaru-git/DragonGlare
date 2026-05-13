using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class NameInputScene : MonoBehaviour
    {
        [SerializeField] private Transform nameTableRoot;
        [SerializeField] private GameObject nameCellPrefab;
        [SerializeField] private Text namePreview;
        [SerializeField] private Text titleText;
        [SerializeField] private Text helpText;
        [SerializeField] private RectTransform cursor;

        private Text[,] cells;

        public void Show(UiLanguage language, int cursorRow, int cursorColumn, string currentName)
        {
            gameObject.SetActive(true);
            var table = GameContent.GetNameTable(language);
            if (cells == null)
                BuildTable(table);

            for (int r = 0; r < table.Length; r++)
            {
                for (int c = 0; c < table[r].Length; c++)
                {
                    cells[r, c].text = table[r][c];
                }
            }

            if (cursorRow < cells.GetLength(0) && cursorColumn < cells.GetLength(1))
            {
                cursor.position = cells[cursorRow, cursorColumn].rectTransform.position;
            }

            namePreview.text = string.IsNullOrEmpty(currentName) ? "..." : currentName;
            titleText.text = language == UiLanguage.English ? "CHOOSE A NAME" : "なまえをきめてください";
            helpText.text = language == UiLanguage.Japanese
                ? "A/Y/Enter/Z: 入力  X/Back: けす  B/Esc: もどる"
                : "A/Y/Enter/Z: INPUT  X/Back: DEL  B/Esc: BACK";
        }

        private void BuildTable(string[][] table)
        {
            int rows = table.Length;
            int cols = 0;
            for (int i = 0; i < rows; i++)
                cols = Mathf.Max(cols, table[i].Length);

            cells = new Text[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < table[r].Length; c++)
                {
                    var go = Instantiate(nameCellPrefab, nameTableRoot);
                    var txt = go.GetComponent<Text>();
                    cells[r, c] = txt;
                }
            }
        }
    }
}
