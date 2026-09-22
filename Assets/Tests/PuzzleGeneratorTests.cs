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
            Assert.IsNotNull(puzzle, "Generated puzzle should not be null");
            Assert.IsTrue(puzzle.IsValid(), "Generated puzzle should be valid");

            var solver = new PuzzleSolver();
            Assert.AreEqual(1, solver.CountSolutions(puzzle, 2), "Puzzle should have exactly one solution");
            Assert.AreEqual(5, puzzle.solution.Count, "Solution should have 5 positions");

            // Rows and columns unique, no touching
            for (int i = 0; i < 5; i++)
            {
                for (int j = i + 1; j < 5; j++)
                {
                    var a = puzzle.solution[i];
                    var b = puzzle.solution[j];
                    Assert.AreNotEqual(a.row, b.row, "Rows should be unique");
                    Assert.AreNotEqual(a.column, b.column, "Columns should be unique");
                    bool touching = System.Math.Abs(a.row - b.row) <= 1
                                 && System.Math.Abs(a.column - b.column) <= 1;
                    Assert.IsFalse(touching, "Solution positions should not touch diagonally or adjacently");
                }
            }

            // Solution cells must all have different colors
            var seen = new HashSet<string>();
            foreach (var pos in puzzle.solution)
            {
                Assert.IsTrue(seen.Add(puzzle.GetCellColor(pos.row, pos.column)), 
                    "Solution cells should have unique colors");
            }
        }

        [Test]
        public void Generate_EdgeCases_MinimumGridSize()
        {
            var generator = new PuzzleGenerator();
            
            // Test minimum viable grid size (3x3)
            var puzzle = generator.Generate(new GenerationSettings { gridSize = 3, randomSeed = 42 });
            Assert.IsNotNull(puzzle, "Should generate valid 3x3 puzzle");
            Assert.IsTrue(puzzle.IsValid(), "3x3 puzzle should be valid");
        }

        [Test]
        public void Generate_EdgeCases_LargerGridSize()
        {
            var generator = new PuzzleGenerator();
            
            // Test larger grid size (7x7)
            var puzzle = generator.Generate(new GenerationSettings { gridSize = 7, randomSeed = 42, maxAttempts = 3000 });
            Assert.IsNotNull(puzzle, "Should generate valid 7x7 puzzle");
            Assert.IsTrue(puzzle.IsValid(), "7x7 puzzle should be valid");
            Assert.AreEqual(7, puzzle.solution.Count, "7x7 puzzle should have 7 solution positions");
        }

        [Test]
        public void Generate_EmptyColorsList_UsesDefaultPalette()
        {
            var settings = new GenerationSettings { gridSize = 5, randomSeed = 999, colors = new List<string>() };
            var generator = new PuzzleGenerator();
            
            var puzzle = generator.Generate(settings);
            Assert.IsNotNull(puzzle, "Should generate puzzle with empty color list");
            Assert.AreEqual(5, puzzle.colors.Count, "Should have default color palette");
        }

        [Test]
        public void Generate_CustomPalette_UsesProvidedColors()
        {
            var customColors = new List<string> { "red", "blue", "green", "yellow", "purple" };
            var settings = new GenerationSettings 
            { 
                gridSize = 5, 
                randomSeed = 999, 
                colors = customColors 
            };
            var generator = new PuzzleGenerator();
            
            var puzzle = generator.Generate(settings);
            Assert.IsNotNull(puzzle, "Should generate puzzle with custom colors");
            
            // Verify all custom colors are used
            var puzzleColors = new HashSet<string>(puzzle.colors);
            foreach (var color in customColors)
            {
                Assert.IsTrue(puzzleColors.Contains(color), $"Custom color '{color}' should be in puzzle");
            }
        }

        [Test]
        public void Generate_SameSeed_SamePuzzle()
        {
            var generator = new PuzzleGenerator();
            var a = generator.Generate(new GenerationSettings { gridSize = 5, randomSeed = 777 });
            var b = generator.Generate(new GenerationSettings { gridSize = 5, randomSeed = 777 });

            Assert.IsNotNull(a, "First generation should succeed");
            Assert.IsNotNull(b, "Second generation should succeed");
            Assert.AreEqual(a.cellColors.Count, b.cellColors.Count, "Cell counts should match");
            for (int i = 0; i < a.cellColors.Count; i++)
            {
                Assert.AreEqual(a.cellColors[i], b.cellColors[i], $"Cell color at index {i} should match");
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
        
        [Test]
        public void Generate_WithDifficultyConfig_AppliesSettings()
        {
            // Create mock difficulty config
            var difficultyConfig = ScriptableObject.CreateInstance<DifficultyConfigSO>();
            difficultyConfig.maxGenerationAttempts = 500;
            difficultyConfig.maxRepairIterations = 50;
            difficultyConfig.baseGridSize = 6;
            
            var settings = new GenerationSettings 
            { 
                gridSize = 0, // Should be overridden by config
                randomSeed = 123,
                difficultyConfig = difficultyConfig
            };
            
            var generator = new PuzzleGenerator();
            var puzzle = generator.Generate(settings);
            
            Assert.IsNotNull(puzzle, "Should generate puzzle with difficulty config");
            Assert.IsTrue(puzzle.IsValid(), "Puzzle should be valid");
        }
    }
}