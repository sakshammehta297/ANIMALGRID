using System;
using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// The result of a quality gate inspection.
    /// </summary>
    public class QualityGateResult
    {
        public bool passed;
        public string failureReason;
    }

    /// <summary>
    /// Final inspector: a puzzle may only ship if it passes ALL checks.
    /// Spec section 38. Pure C# - no Unity dependencies!
    /// </summary>
    public class PuzzleQualityGate
    {
        private readonly PuzzleSolver solver = new PuzzleSolver();
        private readonly DifficultyAnalyzer analyzer = new DifficultyAnalyzer();

        public QualityGateResult Check(PuzzleDefinition puzzle)
        {
            if (puzzle == null) return Fail("Puzzle is null");
            if (!puzzle.IsValid()) return Fail("Invalid structure (grid/colors/animals/solution/cellColors)");

            int n = puzzle.gridSize;

            // --- Solution rule checks ---
            var rows = new HashSet<int>();
            var cols = new HashSet<int>();
            var colors = new HashSet<string>();

            foreach (var pos in puzzle.solution)
            {
                if (pos.row < 0 || pos.row >= n || pos.column < 0 || pos.column >= n)
                    return Fail("Solution position out of bounds");
                if (!rows.Add(pos.row)) return Fail("Duplicate row in solution");
                if (!cols.Add(pos.column)) return Fail("Duplicate column in solution");
                if (!colors.Add(puzzle.GetCellColor(pos.row, pos.column)))
                    return Fail("Duplicate color in solution");
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    var a = puzzle.solution[i];
                    var b = puzzle.solution[j];
                    if (Math.Abs(a.row - b.row) <= 1 && Math.Abs(a.column - b.column) <= 1)
                        return Fail("Animals touch in solution");
                }
            }

            // --- Uniqueness check ---
            int count = solver.CountSolutions(puzzle, 2);
            if (count == 0) return Fail("No solution exists");
            if (count > 1) return Fail("Multiple solutions");

            // --- Difficulty check ---
            var report = analyzer.Analyze(puzzle);
            if (report.score < 1 || report.score > 10) return Fail("Difficulty out of range");

            return new QualityGateResult { passed = true, failureReason = null };
        }

        private QualityGateResult Fail(string reason)
        {
            return new QualityGateResult { passed = false, failureReason = reason };
        }
    }
}