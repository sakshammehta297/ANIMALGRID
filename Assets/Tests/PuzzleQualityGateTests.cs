using System.Collections.Generic;
using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class PuzzleQualityGateTests
    {
        // Helper: every complete puzzle needs N animals!
        private void AddAnimals(PuzzleDefinition puzzle, int count)
        {
            for (int i = 0; i < count; i++)
            {
                puzzle.animals.Add(new AnimalDefinition("animal" + i, "Animal " + i, "color" + i));
            }
        }

        // PROPERTY TEST (spec 37): mass-generate puzzles and inspect every one
        [Test]
        public void MassGeneration_AllPuzzlesPassQualityGate()
        {
            var generator = new PuzzleGenerator();
            var gate = new PuzzleQualityGate();
            int[] sizes = { 5, 6, 7 };

            foreach (int size in sizes)
            {
                for (int seed = 1; seed <= 15; seed++)
                {
                    var puzzle = generator.Generate(new GenerationSettings
                    {
                        gridSize = size,
                        randomSeed = size * 1000 + seed
                    });

                    Assert.IsNotNull(puzzle, $"Size {size} seed {seed}: generation returned null");

                    var result = gate.Check(puzzle);
                    Assert.IsTrue(result.passed,
                        $"Size {size} seed {seed} failed gate: {result.failureReason}");
                }
            }
        }

        // The gate must REJECT puzzles with more than one solution
        [Test]
        public void Gate_RejectsMultipleSolutions()
        {
            var puzzle = new PuzzleDefinition();
            puzzle.gridSize = 5;
            for (int i = 0; i < 5; i++) puzzle.colors.Add("color" + i);
            AddAnimals(puzzle, 5);
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    puzzle.cellColors.Add("color" + r); // whole row same color => many solutions
                }
            }
            puzzle.solution = new List<SolutionPosition>
            {
                new SolutionPosition(0, 0),
                new SolutionPosition(1, 2),
                new SolutionPosition(2, 4),
                new SolutionPosition(3, 1),
                new SolutionPosition(4, 3)
            };

            var result = new PuzzleQualityGate().Check(puzzle);
            Assert.IsFalse(result.passed);
            Assert.AreEqual("Multiple solutions", result.failureReason);
        }

        // The gate must REJECT solutions where animals touch
        [Test]
        public void Gate_RejectsTouchingSolution()
        {
            var puzzle = new PuzzleDefinition();
            puzzle.gridSize = 5;
            for (int i = 0; i < 5; i++) puzzle.colors.Add("color" + i);
            AddAnimals(puzzle, 5);
            for (int i = 0; i < 25; i++) puzzle.cellColors.Add("color0");

            // Diagonal touch between first two positions
            puzzle.solution = new List<SolutionPosition>
            {
                new SolutionPosition(0, 0),
                new SolutionPosition(1, 1),
                new SolutionPosition(2, 3),
                new SolutionPosition(3, 4),
                new SolutionPosition(4, 2)
            };
            // Give the solution cells distinct colors so only the touch check fails
            puzzle.cellColors[0 * 5 + 0] = "color0";
            puzzle.cellColors[1 * 5 + 1] = "color1";
            puzzle.cellColors[2 * 5 + 3] = "color2";
            puzzle.cellColors[3 * 5 + 4] = "color3";
            puzzle.cellColors[4 * 5 + 2] = "color4";

            var result = new PuzzleQualityGate().Check(puzzle);
            Assert.IsFalse(result.passed);
            Assert.AreEqual("Animals touch in solution", result.failureReason);
        }
    }
}