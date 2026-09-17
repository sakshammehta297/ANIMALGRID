using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// Measured difficulty metrics for a puzzle.
    /// </summary>
    public class DifficultyReport
    {
        public int gridSize;
        public int score;               // 1 (easy) .. 10 (brutal)
        public int forcedMoves;         // placements logic alone decides
        public int deadEnds;            // wrong paths explored before the solution
        public float averageCandidates; // average legal choices per row on the solution path
    }

    public interface IDifficultyAnalyzer
    {
        DifficultyReport Analyze(PuzzleDefinition puzzle);
    }

    /// <summary>
    /// Rates difficulty by walking the solution path:
    /// many forced moves = easy; many dead ends / choices = hard.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public class DifficultyAnalyzer : IDifficultyAnalyzer
    {
        // Tunable weights (spec section 10: the formula must be tunable)
        private const float WeightSize = 1.2f;
        private const float WeightDeadEnds = 0.15f;
        private const float WeightCandidates = 0.6f;
        private const float WeightForcedRatio = 3.0f;

        public DifficultyReport Analyze(PuzzleDefinition puzzle)
        {
            int n = puzzle.gridSize;
            var report = new DifficultyReport();
            report.gridSize = n;

            var usedCols = new bool[n];
            var usedColors = new HashSet<string>();
            var occupied = new bool[n, n];
            var pathCandidates = new List<int>();
            var pathForced = new List<bool>();
            int deadEnds = 0;

            Search(puzzle, 0, n, usedCols, usedColors, occupied,
                   pathCandidates, pathForced, ref deadEnds);

            report.deadEnds = deadEnds;
            report.averageCandidates = Average(pathCandidates);

            int forced = 0;
            foreach (var f in pathForced)
            {
                if (f) forced++;
            }
            report.forcedMoves = forced;

            float forcedRatio = n > 0 ? (float)forced / n : 0f;
            float raw = (n - 4) * WeightSize
                      + deadEnds * WeightDeadEnds
                      + report.averageCandidates * WeightCandidates
                      - forcedRatio * WeightForcedRatio;

            int score = (int)System.Math.Round(raw);
            if (score < 1) score = 1;
            if (score > 10) score = 10;
            report.score = score;
            return report;
        }

        private bool Search(
            PuzzleDefinition puzzle, int row, int n,
            bool[] usedCols, HashSet<string> usedColors, bool[,] occupied,
            List<int> pathCandidates, List<bool> pathForced, ref int deadEnds)
        {
            if (row == n) return true;

            int legal = 0;
            for (int col = 0; col < n; col++)
            {
                if (IsLegal(puzzle, row, col, n, usedCols, usedColors, occupied)) legal++;
            }
            pathCandidates.Add(legal);
            pathForced.Add(legal == 1);

            for (int col = 0; col < n; col++)
            {
                if (!IsLegal(puzzle, row, col, n, usedCols, usedColors, occupied)) continue;

                string color = puzzle.GetCellColor(row, col);
                usedCols[col] = true;
                usedColors.Add(color);
                occupied[row, col] = true;

                if (Search(puzzle, row + 1, n, usedCols, usedColors, occupied,
                           pathCandidates, pathForced, ref deadEnds))
                {
                    return true;
                }

                usedCols[col] = false;
                usedColors.Remove(color);
                occupied[row, col] = false;
                deadEnds++;
            }

            pathCandidates.RemoveAt(pathCandidates.Count - 1);
            pathForced.RemoveAt(pathForced.Count - 1);
            return false;
        }

        private bool IsLegal(
            PuzzleDefinition puzzle, int row, int col, int n,
            bool[] usedCols, HashSet<string> usedColors, bool[,] occupied)
        {
            if (usedCols[col]) return false;

            string color = puzzle.GetCellColor(row, col);
            if (usedColors.Contains(color)) return false;

            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r < 0 || r >= n || c < 0 || c >= n) continue;
                    if (occupied[r, c]) return false;
                }
            }
            return true;
        }

        private float Average(List<int> values)
        {
            if (values.Count == 0) return 0f;
            int sum = 0;
            foreach (var v in values) sum += v;
            return (float)sum / values.Count;
        }
    }
}