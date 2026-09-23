using System.Collections.Generic;
using UnityEngine;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Object pool for grid cells. Pre-creates cells for the maximum grid size
    /// (10x10 = 100) and reuses them across levels instead of creating/destroying
    /// GameObjects every time. This eliminates GC spikes on mobile.
    /// </summary>
    public class CellPool
    {
        private readonly List<CellView> allCells = new List<CellView>();
        private readonly Stack<CellView> availableCells = new Stack<CellView>();
        private readonly System.Func<Transform, CellView> createCell;
        private readonly System.Action<CellView> resetCell;

        public CellPool(int maxCells,
            System.Func<Transform, CellView> createCell,
            System.Action<CellView> resetCell)
        {
            this.createCell = createCell;
            this.resetCell = resetCell;

            for (int i = 0; i < maxCells; i++)
            {
                CellView cell = createCell(null); // parent set by caller
                allCells.Add(cell);
                availableCells.Push(cell);
            }
        }

        /// <summary>
        /// Gets an inactive cell from the pool, activates it, and parents it.
        /// </summary>
        public CellView GetCell(Transform parent)
        {
            if (availableCells.Count == 0)
            {
                Debug.LogWarning("CellPool exhausted! Consider increasing maxCells.");
                return null;
            }

            CellView cell = availableCells.Pop();
            cell.transform.SetParent(parent, false);
            cell.gameObject.SetActive(true);
            return cell;
        }

        /// <summary>
        /// Returns ALL cells back to the pool. Call when loading a new board.
        /// </summary>
        public void ReturnAll()
        {
            availableCells.Clear();
            foreach (var cell in allCells)
            {
                if (cell == null) continue;
                resetCell(cell);
                cell.gameObject.SetActive(false);
                availableCells.Push(cell);
            }
        }

        public int AvailableCount => availableCells.Count;
        public int TotalCount => allCells.Count;
    }
}