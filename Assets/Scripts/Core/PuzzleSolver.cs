using System.Collections.Generic;

namespace AnimalGrid.Core
{
    public interface IPuzzleSolver
    {
        SolveResult Solve(PuzzleDefinition puzzle);
        int CountSolutions(PuzzleDefinition puzzle, int maxCount = 2);
    }

    /// <summary>
    /// The result of a solver attempt.
    /// </summary>
    public class SolveResult
    {
        public bool success;
        public List<SolutionPosition> solution;

        public SolveResult()
        {
            success = false;
            solution = new List<SolutionPosition>();
        }
    }

    /// <summary>
    /// Solves and counts solutions using backtracking, row by row.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public class PuzzleSolver : IPuzzleSolver
    {
        private PuzzleDefinition puzzle;
        private int gridSize;
        private bool[] usedColumns;
        private HashSet<string> usedColors;
        private bool[,] occupied;
        private List<SolutionPosition> placed;

        public SolveResult Solve(PuzzleDefinition puzzle)
        {
            Setup(puzzle);
            var result = new SolveResult();
            var found = new List<SolutionPosition>();
            Search(0, 1, found, null);

            if (found.Count > 0)
            {
                result.success = true;
                result.solution = found;
            }
            return result;
        }

        public int CountSolutions(PuzzleDefinition puzzle, int maxCount = 2)
        {
            return CountSolutions(puzzle, maxCount, null);
        }

        /// <summary>
        /// Counts solutions and (optionally) collects each one found, up to maxCount.
        /// </summary>
        public int CountSolutions(PuzzleDefinition puzzle, int maxCount, List<List<SolutionPosition>> collect)
        {
            Setup(puzzle);
            return Search(0, maxCount, null, collect);
        }

        private void Setup(PuzzleDefinition puzzle)
        {
            this.puzzle = puzzle;
            gridSize = puzzle.gridSize;
            usedColumns = new bool[gridSize];
            usedColors = new HashSet<string>();
            occupied = new bool[gridSize, gridSize];
            placed = new List<SolutionPosition>();
        }

        private int Search(int row, int maxCount, List<SolutionPosition> outSolution, List<List<SolutionPosition>> collect)
        {
            if (maxCount <= 0) return 0;

            if (row == gridSize)
            {
                if (outSolution != null && outSolution.Count == 0)
                {
                    outSolution.AddRange(placed);
                }
                if (collect != null)
                {
                    collect.Add(new List<SolutionPosition>(placed));
                }
                return 1;
            }

            int count = 0;
            for (int col = 0; col < gridSize; col++)
            {
                if (usedColumns[col]) continue;

                string color = puzzle.GetCellColor(row, col);
                if (usedColors.Contains(color)) continue;

                if (!IsAdjacentFree(row, col)) continue;

                usedColumns[col] = true;
                usedColors.Add(color);
                occupied[row, col] = true;
                placed.Add(new SolutionPosition(row, col));

                count += Search(row + 1, maxCount - count, outSolution, collect);

                placed.RemoveAt(placed.Count - 1);
                occupied[row, col] = false;
                usedColors.Remove(color);
                usedColumns[col] = false;

                if (count >= maxCount) break;
            }
            return count;
        }

        private bool IsAdjacentFree(int row, int col)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r < 0 || r >= gridSize || c < 0 || c >= gridSize) continue;
                    if (occupied[r, c]) return false;
                }
            }
            return true;
        }
    }
}