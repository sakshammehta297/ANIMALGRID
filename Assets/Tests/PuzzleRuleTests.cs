using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class PuzzleRuleTests
    {
        private PuzzleState MakeState(int size)
        {
            return new PuzzleState(size);
        }

        private PuzzleValidator MakeValidator(int size)
        {
            var validator = new PuzzleValidator();
            validator.Initialize(size);
            return validator;
        }

        // RULE 1: One animal per row
        [Test]
        public void RowRule_SecondAnimalInSameRow_IsInvalid()
        {
            var state = MakeState(5);
            var validator = MakeValidator(5);

            state.PlaceAnimal(0, 0, "cat");
            bool valid = validator.IsValidPlacement(state, 0, 2, "blue");
            Assert.IsFalse(valid);
        }

        [Test]
        public void RowRule_DifferentRow_IsValid()
        {
            var state = MakeState(5);
            var validator = MakeValidator(5);

            state.PlaceAnimal(0, 0, "cat");
            bool valid = validator.IsValidPlacement(state, 2, 2, "blue");
            Assert.IsTrue(valid);
        }

        // RULE 2: One animal per column
        [Test]
        public void ColumnRule_SecondAnimalInSameColumn_IsInvalid()
        {
            var state = MakeState(5);
            var validator = MakeValidator(5);

            state.PlaceAnimal(0, 0, "cat");
            bool valid = validator.IsValidPlacement(state, 3, 0, "blue");
            Assert.IsFalse(valid);
        }

        // RULE 3: One animal per color
        [Test]
        public void ColorRule_SameColorTwice_IsInvalid()
        {
            var state = MakeState(5);
            var validator = MakeValidator(5);

            state.SetCellColor(0, 0, "orange");
            state.PlaceAnimal(0, 0, "cat");

            state.SetCellColor(2, 2, "orange");
            bool valid = validator.IsValidPlacement(state, 2, 2, "orange");
            Assert.IsFalse(valid);
        }

        // RULE 4: No touching, including diagonal
        [Test]
        public void AdjacencyRule_DiagonalTouch_IsInvalid()
        {
            var state = MakeState(5);
            var validator = MakeValidator(5);

            state.PlaceAnimal(2, 2, "cat");
            bool valid = validator.IsValidPlacement(state, 3, 3, "blue");
            Assert.IsFalse(valid);
        }

        [Test]
        public void AdjacencyRule_TwoCellsAway_IsValid()
        {
            var state = MakeState(5);
            var validator = MakeValidator(5);

            state.PlaceAnimal(2, 2, "cat");
            bool valid = validator.IsValidPlacement(state, 4, 4, "blue");
            Assert.IsTrue(valid);
        }

        // BONUS: X marks must NOT count as placed animals
        [Test]
        public void XMark_DoesNotCountAsAnimal()
        {
            var state = MakeState(5);
            var validator = MakeValidator(5);

            state.MarkEliminated(0, 1);
            state.MarkEliminated(0, 2);
            bool valid = validator.IsValidPlacement(state, 0, 0, "orange");
            Assert.IsTrue(valid);
        }
    }
}