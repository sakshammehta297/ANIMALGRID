using System;

namespace AnimalGrid.Core
{
    public enum HintKind { Elimination, Reveal }

    public class HintResult
    {
        public bool found;
        public HintKind kind;
        public int row;
        public int column;
    }

    /// <summary>
    /// Logical hints that respect the current player state (spec 17).
    /// Never reveals randomly: elimination first, then the most constrained color.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public class HintProvider
    {
        public HintResult GetHint(PuzzleDefinition puzzle, PuzzleState state)
        {
            int n = puzzle.gridSize;

            // 1) Elimination: an empty cell that conflicts with a placed animal
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    var cell = state.GetCell(r, c);
                    if (cell.state != CellState.Empty) continue;
                    if (ConflictsWithPlaced(state, n, r, c))
                    {
                        return new HintResult
                        {
                            found = true,
                            kind = HintKind.Elimination,
                            row = r,
                            column = c
                        };
                    }
                }
            }

            // 2) Reveal: solution cell of the most constrained unplaced color
            string bestColor = null;
            int bestCount = int.MaxValue;
            foreach (var color in puzzle.colors)
            {
                if (IsColorPlaced(state, n, color)) continue;
                int count = CountEmptyCellsOfColor(state, n, color);
                if (count < bestCount)
                {
                    bestCount = count;
                    bestColor = color;
                }
            }

            if (bestColor != null)
            {
                foreach (var pos in puzzle.solution)
                {
                    if (puzzle.GetCellColor(pos.row, pos.column) == bestColor)
                    {
                        return new HintResult
                        {
                            found = true,
                            kind = HintKind.Reveal,
                            row = pos.row,
                            column = pos.column
                        };
                    }
                }
            }

            return new HintResult { found = false };
        }

        private bool ConflictsWithPlaced(PuzzleState state, int n, int r, int c)
        {
            var cell = state.GetCell(r, c);
            for (int rr = 0; rr < n; rr++)
            {
                for (int cc = 0; cc < n; cc++)
                {
                    var other = state.GetCell(rr, cc);
                    if (other.state != CellState.Selected) continue;
                    if (rr == r || cc == c || other.colorId == cell.colorId ||
                        (Math.Abs(rr - r) <= 1 && Math.Abs(cc - c) <= 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsColorPlaced(PuzzleState state, int n, string color)
        {
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    var cell = state.GetCell(r, c);
                    if (cell.state == CellState.Selected && cell.colorId == color) return true;
                }
            }
            return false;
        }

        private int CountEmptyCellsOfColor(PuzzleState state, int n, string color)
        {
            int count = 0;
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    var cell = state.GetCell(r, c);
                    if (cell.state == CellState.Empty && cell.colorId == color) count++;
                }
            }
            return count;
        }
    }
}