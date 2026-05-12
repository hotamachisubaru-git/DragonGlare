using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class CommandGrid : MonoBehaviour
    {
        [SerializeField] private Transform cellRoot;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private RectTransform cursor;

        private Text[,] cells;
        private int rows;
        private int cols;

        public void Initialize(int rowCount, int colCount)
        {
            rows = rowCount;
            cols = colCount;
            cells = new Text[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var go = Instantiate(cellPrefab, cellRoot);
                    cells[r, c] = go.GetComponent<Text>();
                }
            }
        }

        public void SetCellText(int row, int col, string text)
        {
            if (row >= 0 && row < rows && col >= 0 && col < cols)
            {
                cells[row, col].text = text;
            }
        }

        public void SetCursorPosition(int row, int col)
        {
            if (row >= 0 && row < rows && col >= 0 && col < cols)
            {
                cursor.position = cells[row, col].rectTransform.position;
            }
        }
    }
}
