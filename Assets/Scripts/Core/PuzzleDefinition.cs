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
        
        /// <summary>
        /// Generation metrics for debugging and analytics.
        /// </summary>
        public int generationAttempts;
        public int repairIterations;
        public int randomSeed;

        public PuzzleDefinition()
        {
            colors = new List<string>();
            animals = new List<AnimalDefinition>();
            solution = new List<SolutionPosition>();
            cellColors = new List<string>();
            generationAttempts = 0;
            repairIterations = 0;
            randomSeed = 0;
        }

        /// <summary>
        /// Returns the color region of the cell at (row, column).
        /// </summary>
        public string GetCellColor(int row, int column)
        {
            if (row < 0 || row >= gridSize || column < 0 || column >= gridSize)
            {
                Debug.LogWarning($"[PuzzleDefinition] GetCellColor called with invalid coordinates: ({row}, {column})");
                return null;
            }
            return cellColors[row * gridSize + column];
        }

        /// <summary>
        /// A quick check to make sure the puzzle data makes sense.
        /// Now includes more thorough validation.
        /// </summary>
        public bool IsValid()
        {
            if (gridSize < 3) // Allow smaller grids for testing
            {
                Debug.LogWarning($"[PuzzleDefinition] Grid size {gridSize} is too small (min: 3)");
                return false;
            }
            if (colors.Count != gridSize)
            {
                Debug.LogWarning($"[PuzzleDefinition] Colors count ({colors.Count}) doesn't match grid size ({gridSize})");
                return false;
            }
            if (animals.Count != gridSize)
            {
                Debug.LogWarning($"[PuzzleDefinition] Animals count ({animals.Count}) doesn't match grid size ({gridSize})");
                return false;
            }
            if (solution.Count != gridSize)
            {
                Debug.LogWarning($"[PuzzleDefinition] Solution count ({solution.Count}) doesn't match grid size ({gridSize})");
                return false;
            }
            if (cellColors.Count != gridSize * gridSize)
            {
                Debug.LogWarning($"[PuzzleDefinition] Cell colors count ({cellColors.Count}) doesn't match expected ({gridSize * gridSize})");
                return false;
            }
            
            // Validate no empty colors
            foreach (var color in cellColors)
            {
                if (string.IsNullOrEmpty(color))
                {
                    Debug.LogWarning("[PuzzleDefinition] Found empty color in cellColors");
                    return false;
                }
            }
            
            // Validate solution positions are within bounds
            foreach (var pos in solution)
            {
                if (pos.row < 0 || pos.row >= gridSize || pos.column < 0 || pos.column >= gridSize)
                {
                    Debug.LogWarning($"[PuzzleDefinition] Solution position out of bounds: ({pos.row}, {pos.column})");
                    return false;
                }
            }
            
            return true;
        }
    }
}