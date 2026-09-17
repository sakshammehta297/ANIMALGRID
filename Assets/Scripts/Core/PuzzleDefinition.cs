using System;
using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// Represents the state of a single cell on the board.
    /// </summary>
    public enum CellState
    {
        Empty,          // Nothing here yet
        Eliminated,     // Player marked this with an X
        Selected        // Player placed an animal here
    }

    /// <summary>
    /// What defines an Animal?
    /// </summary>
    [Serializable]
    public class AnimalDefinition
    {
        public string id;          // Unique ID, e.g., "cat"
        public string displayName; // Display name, e.g., "Cat"
        public string colorName;   // Associated color, e.g., "orange"

        public AnimalDefinition(string id, string displayName, string colorName)
        {
            this.id = id;
            this.displayName = displayName;
            this.colorName = colorName;
        }
    }

    /// <summary>
    /// Where is an animal placed in the solution? (Row and Column)
    /// </summary>
    [Serializable]
    public class SolutionPosition
    {
        public int row;
        public int column;

        public SolutionPosition(int row, int column)
        {
            this.row = row;
            this.column = column;
        }
    }

    /// <summary>
    /// The Master Puzzle Data
    /// </summary>
    [Serializable]
    public class PuzzleDefinition
    {
        public int levelId;
        public int gridSize;       // N (e.g., 5 for a 5x5 board)
        public bool isBoss;
        public int difficulty;

        public List<string> colors;
        public List<AnimalDefinition> animals;
        public List<SolutionPosition> solution;
        public List<string> cellColors; // Row-major color layout: index = row * gridSize + column

        public PuzzleDefinition()
        {
            colors = new List<string>();
            animals = new List<AnimalDefinition>();
            solution = new List<SolutionPosition>();
            cellColors = new List<string>();
        }

        /// <summary>
        /// Returns the color region of the cell at (row, column).
        /// </summary>
        public string GetCellColor(int row, int column)
        {
            return cellColors[row * gridSize + column];
        }

        /// <summary>
        /// A quick check to make sure the puzzle data makes sense.
        /// </summary>
        public bool IsValid()
        {
            if (gridSize < 5) return false;
            if (colors.Count != gridSize) return false;
            if (animals.Count != gridSize) return false;
            if (solution.Count != gridSize) return false;
            if (cellColors.Count != gridSize * gridSize) return false;
            return true;
        }
    }
}