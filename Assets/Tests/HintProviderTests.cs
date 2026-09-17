using System;
using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class HintProviderTests
    {
        private PuzzleDefinition Generate(int seed)
        {
            var generator = new PuzzleGenerator();
            return generator.Generate(new GenerationSettings { gridSize = 5, randomSeed = seed });
        }

        private PuzzleState BuildState(PuzzleDefinition puzzle)
        {
            var state = new PuzzleState(puzzle.gridSize);
            for (int r = 0; r < puzzle.gridSize; r++)
            {
                for (int c = 0; c < puzzle.gridSize; c++)
                {
                    state.SetCellColor(r, c, puzzle.GetCellColor(r, c));
                }
            }
            return state;
        }

        [Test]
        public void Hint_AfterPlacement_SuggestsElimination()
        {
            var puzzle = Generate(11);
            var state = BuildState(puzzle);
            var first = puzzle.solution[0];
            state.PlaceAnimal(first.row, first.column, "animal");

            var hint = new HintProvider().GetHint(puzzle, state);
            Assert.IsTrue(hint.found);
            Assert.AreEqual(HintKind.Elimination, hint.kind);

            var cell = state.GetCell(hint.row, hint.column);
            Assert.AreEqual(CellState.Empty, cell.state);

            bool conflict =
                hint.row == first.row ||
                hint.column == first.column ||
                cell.colorId == puzzle.GetCellColor(first.row, first.column) ||
                (Math.Abs(hint.row - first.row) <= 1 && Math.Abs(hint.column - first.column) <= 1);
            Assert.IsTrue(conflict);
        }

        [Test]
        public void Hint_FreshBoard_RevealsSolutionCell()
        {
            var puzzle = Generate(12);
            var state = BuildState(puzzle);

            var hint = new HintProvider().GetHint(puzzle, state);
            Assert.IsTrue(hint.found);
            Assert.AreEqual(HintKind.Reveal, hint.kind);

            bool inSolution = false;
            foreach (var pos in puzzle.solution)
            {
                if (pos.row == hint.row && pos.column == hint.column) inSolution = true;
            }
            Assert.IsTrue(inSolution);
        }

        [Test]
        public void Hint_RespectsPlayerState_RevealsOnlyUnplacedColor()
        {
            var puzzle = Generate(13);
            var state = BuildState(puzzle);
            int n = puzzle.gridSize;

            // Place all but the last solution animal
            for (int i = 0; i < n - 1; i++)
            {
                var pos = puzzle.solution[i];
                state.PlaceAnimal(pos.row, pos.column, "animal" + i);
            }

            // Player has X-marked every remaining non-solution empty cell
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    var cell = state.GetCell(r, c);
                    if (cell.state != CellState.Empty) continue;
                    bool isSolutionCell = false;
                    foreach (var pos in puzzle.solution)
                    {
                        if (pos.row == r && pos.column == c) isSolutionCell = true;
                    }
                    if (!isSolutionCell) state.MarkEliminated(r, c);
                }
            }

            var hint = new HintProvider().GetHint(puzzle, state);
            Assert.IsTrue(hint.found);
            Assert.AreEqual(HintKind.Reveal, hint.kind);

            var last = puzzle.solution[n - 1];
            Assert.AreEqual(last.row, hint.row);
            Assert.AreEqual(last.column, hint.column);
        }
    }
}