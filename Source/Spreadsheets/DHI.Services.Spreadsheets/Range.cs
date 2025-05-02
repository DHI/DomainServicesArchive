namespace DHI.Services.Spreadsheets
{
    public class Range : ISerializable
    {
        public Range(Cell upperLeft, Cell lowerRight)
        {
            UpperLeft = upperLeft;
            LowerRight = lowerRight;
        }

        public Cell UpperLeft { get; }

        public Cell LowerRight { get; }

        public virtual string Serialize()
        {
            return $"{UpperLeft}:{LowerRight}";
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}