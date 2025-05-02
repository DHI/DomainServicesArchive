namespace DHI.Services.GIS.Maps
{
    using System;
    using Spatial;

    [Serializable]
    public class Tile
    {
        public Tile(BoundingBox boundingBox, uint row, uint col)
        {
            BoundingBox = boundingBox;
            Row = row;
            Col = col;
        }

        public BoundingBox BoundingBox { get; }

        public uint Row { get; }

        public uint Col { get; }

        public override string ToString()
        {
            return $"({Row}, {Col})";
        }
    }
}