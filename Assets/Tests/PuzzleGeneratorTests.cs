using System.Collections.Generic;
using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class PuzzleGeneratorTests
    {
        [Test]
        public void Generate_5x5_ProducesUniqueValidPuzzle()
        {
            var settings = new GenerationSettings { gridSize = 5, randomSeed = 12345 };
            var generator = new PuzzleGenerator();

            var puzzle = generator.Generate(settings);
            Assert.IsNotNull(puzzle);
            Assert.IsTrue(puzzle.IsValid());

            var solver = new PuzzleSolver();
            Assert.AreEqual(1, solver.CountSolutions(puzzle, 2));
            Assert.AreEqual(5, puzzle.solution.Count);

            // Rows and columns unique, no touching
            for (int i = 0; i < 5; i++)
            {
                for (int j = i + 1; j < 5; j++)
                {
                    var a = puzzle.solution[i];
                    var b = puzzle.solution[j];
                    Assert.AreNotEqual(a.row, b.row);
                    Assert.AreNotEqual(a.column, b.column);
                    bool touching = System.Math.Abs(a.row - b.row) <= 1
                                 && System.Math.Abs(a.column - b.column) <= 1;
                    Assert.IsFalse(touching);
                }
            }

            // Solution cells must all have different colors
            var seen = new HashSet<string>();
            foreach (var pos in puzzle.solution)
            {
                Assert.IsTrue(seen.Add(puzzle.GetCellColor(pos.row, pos.column)));
            }
        }

        [Test]
        public void Generate_SameSeed_SamePuzzle()
        {
            var generator = new PuzzleGenerator();
            var a = generator.Generate(new GenerationSettings { gridSize = 5, randomSeed = 777 });
            var b = generator.Generate(new GenerationSettings { gridSize = 5, randomSeed = 777 });

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.cellColors.Count, b.cellColors.Count);
            for (int i = 0; i < a.cellColors.Count; i++)
            {
                Assert.AreEqual(a.cellColors[i], b.cellColors[i]);
            }
        }

        [Test]
        public void Generate_ManySeeds_AllUniqueAndValid()
        {
            var generator = new PuzzleGenerator();
            var solver = new PuzzleSolver();

            for (int seed = 1; seed <= 20; seed++)
            {
                var puzzle = generator.Generate(new GenerationSettings { gridSize = 5, randomSeed = seed });
                Assert.IsNotNull(puzzle, "Generation failed for seed " + seed);
                Assert.AreEqual(1, solver.CountSolutions(puzzle, 2), "Seed " + seed + " is not unique");
            }
        }
    }
}