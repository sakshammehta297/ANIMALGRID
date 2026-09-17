namespace AnimalGrid.Core
{
    /// <summary>
    /// A single cell on the board. Pure C# - no Unity dependencies!
    /// </summary>
    public class PuzzleCell
    {
        public int row;
        public int column;
        public string colorId;        // The color region this cell belongs to
        public CellState state;       // Empty, Eliminated, or Selected
        public string placedAnimalId; // Animal ID if Selected, otherwise null

        public PuzzleCell(int row, int column)
        {
            this.row = row;
            this.column = column;
            this.colorId = null;
            this.state = CellState.Empty;
            this.placedAnimalId = null;
        }
    }
}