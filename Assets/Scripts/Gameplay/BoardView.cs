using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AnimalGrid.Core;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Builds the N x N grid visually at runtime. Fully dynamic - no hard-coded sizes!
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        public float spacing = 10f;
        public float boardPixels = 900f;

        public PuzzleDefinition puzzle { get; private set; }
        public Action<int, int> OnCellClicked;

        public static readonly Color32[] Palette =
        {
            new Color32(244, 162, 97, 255),   // orange
            new Color32(91, 141, 239, 255),   // blue
            new Color32(123, 201, 111, 255),  // green
            new Color32(242, 126, 178, 255),  // pink
            new Color32(155, 126, 222, 255),  // purple
            new Color32(247, 209, 84, 255),   // yellow
            new Color32(78, 205, 196, 255),   // teal
            new Color32(231, 111, 81, 255),   // red
            new Color32(169, 132, 103, 255),  // brown
            new Color32(141, 153, 174, 255),  // gray
            new Color32(197, 224, 99, 255),   // lime
            new Color32(100, 223, 223, 255)   // cyan
        };

        private readonly List<CellView> cells = new List<CellView>();

        public CellView GetCellView(int row, int column)
        {
            return cells[row * puzzle.gridSize + column];
        }

        public Sprite AnimalSpriteForCell(int row, int column)
        {
            return AnimalSpriteForColor(puzzle.GetCellColor(row, column));
        }

        public Sprite AnimalSpriteForColor(string colorId)
        {
            if (puzzle == null) return null;
            foreach (var animal in puzzle.animals)
            {
                if (animal.colorName == colorId) return ArtLoader.GetAnimal(animal.id);
            }
            return null;
        }

        public void RenderBoard(PuzzleDefinition definition)
        {
            puzzle = definition;
            ClearCells();

            var rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(boardPixels, boardPixels);

            int n = definition.gridSize;
            float cellSize = (boardPixels - spacing * (n - 1)) / n;

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    var go = new GameObject("Cell_" + r + "_" + c, typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(transform, false);

                    var view = go.AddComponent<CellView>();
                    var cellRect = go.transform as RectTransform;
                    float x = -boardPixels / 2f + cellSize / 2f + c * (cellSize + spacing);
                    float y = boardPixels / 2f - cellSize / 2f - r * (cellSize + spacing);
                    cellRect.anchoredPosition = new Vector2(x, y);

                    view.Setup(r, c, cellSize, TintFor(definition, r, c));

                    int rr = r, cc = c;
                    var button = go.AddComponent<Button>();
                    button.onClick.AddListener(() => OnCellClicked?.Invoke(rr, cc));

                    cells.Add(view);
                }
            }
        }

        private Color TintFor(PuzzleDefinition definition, int r, int c)
        {
            string colorId = definition.GetCellColor(r, c);
            int index = definition.colors.IndexOf(colorId);
            if (index < 0) index = 0;
            Color baseColor = Palette[index % Palette.Length];
            return Color.Lerp(Color.white, baseColor, 0.45f);
        }

        private void ClearCells()
        {
            foreach (var cell in cells)
            {
                if (cell != null) Destroy(cell.gameObject);
            }
            cells.Clear();
        }
    }
}