using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// Settings that control puzzle generation.
    /// </summary>
    public class GenerationSettings
    {
        public int levelId = 0;
        public int gridSize = 5;
        public bool isBoss = false;
        public int difficulty = 0;
        public int randomSeed = 0;           // 0 = different every time
        public int maxAttempts = 2000;
        public int maxRepairs = 100;
        public List<string> colors = null;      // optional palette
        public List<string> animalIds = null;   // optional animal ids
    }

    public interface IPuzzleGenerator
    {
        PuzzleDefinition Generate(GenerationSettings settings);
    }

    /// <summary>
    /// Generates puzzles with exactly ONE solution.
    /// Pipeline: random solution -> connected color regions -> uniqueness check -> repair.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public class PuzzleGenerator : IPuzzleGenerator
    {
        private System.Random random;
        private PuzzleSolver solver;

        public PuzzleDefinition Generate(GenerationSettings settings)
        {
            random = settings.randomSeed != 0
                ? new System.Random(settings.randomSeed)
                : new System.Random();
            solver = new PuzzleSolver();

            for (int attempt = 0; attempt < settings.maxAttempts; attempt++)
            {
                var solution = RandomSolution(settings.gridSize);
                if (solution == null) continue;

                var puzzle = BuildPuzzle(settings);
                AssignColors(puzzle, solution);

                var collected = new List<List<SolutionPosition>>();
                int count = solver.CountSolutions(puzzle, 2, collected);

                int repairs = 0;
                while (count == 2 && repairs < settings.maxRepairs)
                {
                    KillExtraSolution(puzzle, solution, collected[1]);
                    collected = new List<List<SolutionPosition>>();
                    count = solver.CountSolutions(puzzle, 2, collected);
                    repairs++;
                }

                if (count == 1)
                {
                    puzzle.solution = solution;
                    return puzzle;
                }
            }
            return null;
        }

        private PuzzleDefinition BuildPuzzle(GenerationSettings settings)
        {
            int n = settings.gridSize;
            var puzzle = new PuzzleDefinition();
            puzzle.levelId = settings.levelId;
            puzzle.gridSize = n;
            puzzle.isBoss = settings.isBoss;
            puzzle.difficulty = settings.difficulty;

            if (settings.colors != null && settings.colors.Count == n)
            {
                puzzle.colors.AddRange(settings.colors);
            }
            else
            {
                for (int i = 0; i < n; i++) puzzle.colors.Add("color" + i);
            }

            if (settings.animalIds != null && settings.animalIds.Count == n)
            {
                for (int i = 0; i < n; i++)
                {
                    string id = settings.animalIds[i];
                    puzzle.animals.Add(new AnimalDefinition(id, id, puzzle.colors[i]));
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    string id = "animal" + i;
                    puzzle.animals.Add(new AnimalDefinition(id, id, puzzle.colors[i]));
                }
            }

            for (int i = 0; i < n * n; i++) puzzle.cellColors.Add("");
            return puzzle;
        }

        /// <summary>
        /// Grows N connected color regions, one seeded at each solution cell.
        /// This produces clean readable color blocks instead of random confetti.
        /// </summary>
        private void AssignColors(PuzzleDefinition puzzle, List<SolutionPosition> solution)
        {
            int n = puzzle.gridSize;
            int[,] region = new int[n, n];
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++) region[r, c] = -1;
            }

            for (int i = 0; i < solution.Count; i++)
            {
                region[solution[i].row, solution[i].column] = i;
            }

            var frontier = new List<int[]>();
            while (true)
            {
                frontier.Clear();
                for (int r = 0; r < n; r++)
                {
                    for (int c = 0; c < n; c++)
                    {
                        if (region[r, c] < 0) continue;
                        AddFrontier(frontier, region, n, r, c, r - 1, c);
                        AddFrontier(frontier, region, n, r, c, r + 1, c);
                        AddFrontier(frontier, region, n, r, c, r, c - 1);
                        AddFrontier(frontier, region, n, r, c, r, c + 1);
                    }
                }
                if (frontier.Count == 0) break;
                int[] pick = frontier[random.Next(frontier.Count)];
                region[pick[0], pick[1]] = pick[2];
            }

            var palette = new List<string>(puzzle.colors);
            Shuffle(palette);

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    puzzle.cellColors[r * n + c] = palette[region[r, c]];
                }
            }
        }

        private void AddFrontier(List<int[]> frontier, int[,] region, int n, int r, int c, int nr, int nc)
        {
            if (nr < 0 || nr >= n || nc < 0 || nc >= n) return;
            if (region[nr, nc] >= 0) return;
            frontier.Add(new int[] { nr, nc, region[r, c] });
        }

        /// <summary>
        /// Recolors one cell of the extra solution so that extra solution becomes invalid.
        /// </summary>
        private void KillExtraSolution(PuzzleDefinition puzzle, List<SolutionPosition> solution, List<SolutionPosition> extra)
        {
            int n = puzzle.gridSize;
            SolutionPosition target = null;
            foreach (var pos in extra)
            {
                if (!ContainsPosition(solution, pos)) { target = pos; break; }
            }
            if (target == null) return;

            SolutionPosition donor = null;
            foreach (var pos in extra)
            {
                if (pos.row != target.row || pos.column != target.column) { donor = pos; break; }
            }
            if (donor == null) return;

            puzzle.cellColors[target.row * n + target.column] =
                puzzle.cellColors[donor.row * n + donor.column];
        }

        private bool ContainsPosition(List<SolutionPosition> list, SolutionPosition pos)
        {
            foreach (var p in list)
            {
                if (p.row == pos.row && p.column == pos.column) return true;
            }
            return false;
        }

        private List<SolutionPosition> RandomSolution(int n)
        {
            var placed = new List<SolutionPosition>();
            var usedCols = new bool[n];
            var occupied = new bool[n, n];
            if (FillRow(0, n, placed, usedCols, occupied)) return placed;
            return null;
        }

        private bool FillRow(int row, int n, List<SolutionPosition> placed, bool[] usedCols, bool[,] occupied)
        {
            if (row == n) return true;

            int[] cols = new int[n];
            for (int i = 0; i < n; i++) cols[i] = i;
            Shuffle(cols);

            foreach (int col in cols)
            {
                if (usedCols[col]) continue;
                if (!AdjacentFree(occupied, n, row, col)) continue;

                usedCols[col] = true;
                occupied[row, col] = true;
                placed.Add(new SolutionPosition(row, col));

                if (FillRow(row + 1, n, placed, usedCols, occupied)) return true;

                placed.RemoveAt(placed.Count - 1);
                occupied[row, col] = false;
                usedCols[col] = false;
            }
            return false;
        }

        private bool AdjacentFree(bool[,] occupied, int n, int row, int col)
        {
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

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }

        private void Shuffle(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int tmp = array[i]; array[i] = array[j]; array[j] = tmp;
            }
        }
    }
}