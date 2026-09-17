namespace AnimalGrid.Core
{
    /// <summary>
    /// Holds the current state of every cell on the board.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public class PuzzleState
    {
        public int gridSize { get; private set; }
        private PuzzleCell[,] cells;

        public PuzzleState(int gridSize)
        {
            this.gridSize = gridSize;
            cells = new PuzzleCell[gridSize, gridSize];

            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    cells[r, c] = new PuzzleCell(r, c);
                }
            }
        }

        /// <summary>Returns the cell at (row, column), or null if out of bounds.</summary>
        public PuzzleCell GetCell(int row, int column)
        {
            if (row < 0 || row >= gridSize || column < 0 || column >= gridSize)
                return null;
            return cells[row, column];
        }

        /// <summary>Assigns a color region to a cell.</summary>
        public void SetCellColor(int row, int column, string colorId)
        {
            PuzzleCell cell = GetCell(row, column);
            if (cell != null) cell.colorId = colorId;
        }

        /// <summary>Player marks this cell with an X (eliminated).</summary>
        public bool MarkEliminated(int row, int column)
        {
            PuzzleCell cell = GetCell(row, column);
            if (cell == null || cell.state == CellState.Selected) return false;
            cell.state = CellState.Eliminated;
            return true;
        }

        /// <summary>Player places an animal on this cell.</summary>
        public bool PlaceAnimal(int row, int column, string animalId)
        {
            PuzzleCell cell = GetCell(row, column);
            if (cell == null) return false;
            cell.state = CellState.Selected;
            cell.placedAnimalId = animalId;
            return true;
        }

        /// <summary>Clears a cell back to empty.</summary>
        public void ClearCell(int row, int column)
        {
            PuzzleCell cell = GetCell(row, column);
            if (cell == null) return;
            cell.state = CellState.Empty;
            cell.placedAnimalId = null;
        }

        /// <summary>Counts how many animals are currently placed.</summary>
        public int CountSelected()
        {
            int count = 0;
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    if (cells[r, c].state == CellState.Selected) count++;
                }
            }
            return count;
        }

        /// <summary>True when exactly N animals are placed.</summary>
        public bool IsComplete()
        {
            return CountSelected() == gridSize;
        }
    }
}