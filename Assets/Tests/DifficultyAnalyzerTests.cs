using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class DifficultyAnalyzerTests
    {
        private PuzzleDefinition Generate(int size, int seed)
        {
            var generator = new PuzzleGenerator();
            return generator.Generate(new GenerationSettings
            {
                gridSize = size,
                randomSeed = seed
            });
        }

        [Test]
        public void Analyze_ScoreIsWithinValidRange()
        {
            var puzzle = Generate(5, 42);
            var report = new DifficultyAnalyzer().Analyze(puzzle);

            Assert.GreaterOrEqual(report.score, 1);
            Assert.LessOrEqual(report.score, 10);
            Assert.GreaterOrEqual(report.forcedMoves, 0);
            Assert.GreaterOrEqual(report.deadEnds, 0);
            Assert.GreaterOrEqual(report.averageCandidates, 1f);
        }

        [Test]
        public void Analyze_IsDeterministic()
        {
            var puzzle = Generate(5, 42);
            var a = new DifficultyAnalyzer().Analyze(puzzle);
            var b = new DifficultyAnalyzer().Analyze(puzzle);

            Assert.AreEqual(a.score, b.score);
            Assert.AreEqual(a.forcedMoves, b.forcedMoves);
            Assert.AreEqual(a.deadEnds, b.deadEnds);
            Assert.AreEqual(a.averageCandidates, b.averageCandidates);
        }

        [Test]
        public void Analyze_LargerGrids_StillValidRange()
        {
            var analyzer = new DifficultyAnalyzer();

            var six = Generate(6, 99);
            var seven = Generate(7, 123);
            Assert.IsNotNull(six);
            Assert.IsNotNull(seven);

            var reportSix = analyzer.Analyze(six);
            var reportSeven = analyzer.Analyze(seven);

            Assert.GreaterOrEqual(reportSix.score, 1);
            Assert.LessOrEqual(reportSix.score, 10);
            Assert.GreaterOrEqual(reportSeven.score, 1);
            Assert.LessOrEqual(reportSeven.score, 10);
        }
    }
}