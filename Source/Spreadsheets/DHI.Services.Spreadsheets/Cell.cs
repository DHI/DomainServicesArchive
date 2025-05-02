namespace DHI.Services.Spreadsheets
{
    using System;

    public class Cell : ISerializable
    {
        public Cell(int row, int col)
        {
            if (col < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if (row < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(col));
            }

            Row = row;
            Col = col;
        }

        public int Col { get; }

        public int Row { get; }

        public virtual string Serialize()
        {
            return $"R{Row}C{Col}";
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}