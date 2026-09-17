using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// Validates puzzle moves according to the 4 core rules.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public class PuzzleValidator
    {
        private int gridSize;
        
        public void Initialize(int gridSize)
        {
            this.gridSize = gridSize;
        }
        
        /// <summary>
        /// Checks if placing an animal at (row, col) violates any rules.
        /// Returns true if the placement is VALID.
        /// </summary>
        public bool IsValidPlacement(PuzzleState state, int row, int col, string colorId)
        {
            // Check all 4 rules
            if (!CheckRowRule(state, row, col)) return false;
            if (!CheckColumnRule(state, row, col)) return false;
            if (!CheckColorRule(state, col, colorId)) return false;
            if (!CheckAdjacencyRule(state, row, col)) return false;
            
            return true;
        }
        
        /// <summary>
        /// Rule 1: Only one selected animal per row
        /// </summary>
        private bool CheckRowRule(PuzzleState state, int row, int col)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (c == col) continue; // Skip the cell we're checking
                
                var cell = state.GetCell(row, c);
                if (cell != null && cell.state == CellState.Selected)
                {
                    return false; // Another animal already in this row
                }
            }
            return true;
        }
        
        /// <summary>
        /// Rule 2: Only one selected animal per column
        /// </summary>
        private bool CheckColumnRule(PuzzleState state, int row, int col)
        {
            for (int r = 0; r < gridSize; r++)
            {
                if (r == row) continue; // Skip the cell we're checking
                
                var cell = state.GetCell(r, col);
                if (cell != null && cell.state == CellState.Selected)
                {
                    return false; // Another animal already in this column
                }
            }
            return true;
        }
        
        /// <summary>
        /// Rule 3: Only one selected animal per color
        /// </summary>
        private bool CheckColorRule(PuzzleState state, int col, string colorId)
        {
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    var cell = state.GetCell(r, c);
                    if (cell != null && 
                        cell.state == CellState.Selected && 
                        cell.colorId == colorId)
                    {
                        return false; // This color already used
                    }
                }
            }
            return true;
        }
        
        /// <summary>
        /// Rule 4: No touching (including diagonal)
        /// </summary>
        private bool CheckAdjacencyRule(PuzzleState state, int row, int col)
        {
            // Check all 8 surrounding cells
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    // Skip out-of-bounds
                    if (r < 0 || r >= gridSize || c < 0 || c >= gridSize)
                        continue;
                    
                    // Skip the cell itself
                    if (r == row && c == col)
                        continue;
                    
                    var cell = state.GetCell(r, c);
                    if (cell != null && cell.state == CellState.Selected)
                    {
                        return false; // Touching another animal!
                    }
                }
            }
            return true;
        }
        
        /// <summary>
        /// Validates the entire puzzle solution.
        /// </summary>
        public bool ValidateCompleteSolution(PuzzleState state)
        {
            // Count selected animals
            int selectedCount = 0;
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    var cell = state.GetCell(r, c);
                    if (cell != null && cell.state == CellState.Selected)
                    {
                        selectedCount++;
                    }
                }
            }
            
            // Must have exactly N animals placed
            if (selectedCount != gridSize)
                return false;
            
            return true;
        }
    }
}