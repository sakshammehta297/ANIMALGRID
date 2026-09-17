using System;
using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class PuzzleSolverTests
    {
        private PuzzleDefinition MakePuzzle(int size, Func<int, int, string> colorAt)
        {
            var puzzle = new PuzzleDefinition();
            puzzle.levelId = 1;
            puzzle.gridSize = size;

            for (int i = 0; i < size; i++)
            {
                puzzle.colors.Add("color" + i);
            }

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    puzzle.cellColors.Add(colorAt(r, c));
                }
            }
            return puzzle;
        }

        // Every cell same color => impossible to place 5 different colors
        [Test]
        public void Solver_AllCellsSameColor_NoSolution()
        {
            var puzzle = MakePuzzle(5, (r, c) => "color0");
            var solver = new PuzzleSolver();

            Assert.AreEqual(0, solver.CountSolutions(puzzle, 2));
            Assert.IsFalse(solver.Solve(puzzle).success);
        }

        // Each row one color => many solutions => counter must stop at max
        [Test]
        public void Solver_RowColorLayout_StopsAtMaxCount()
        {
            var puzzle = MakePuzzle(5, (r, c) => "color" + r);
            var solver = new PuzzleSolver();

            Assert.AreEqual(2, solver.CountSolutions(puzzle, 2));
        }

        // Each column one color => solvable; verify the found solution obeys "no touching"
        [Test]
        public void Solver_ColumnColorLayout_FindsValidSolution()
        {
            var puzzle = MakePuzzle(5, (r, c) => "color" + c);
            var solver = new PuzzleSolver();

            var result = solver.Solve(puzzle);
            Assert.IsTrue(result.success);
            Assert.AreEqual(5, result.solution.Count);

            for (int i = 0; i < result.solution.Count; i++)
            {
                for (int j = i + 1; j < result.solution.Count; j++)
                {
                    var a = result.solution[i];
                    var b = result.solution[j];
                    bool touching = Math.Abs(a.row - b.row) <= 1 && Math.Abs(a.column - b.column) <= 1;
                    Assert.IsFalse(touching);
                }
            }
        }
    }
}