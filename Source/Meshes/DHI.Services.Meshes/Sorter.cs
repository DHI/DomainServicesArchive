namespace DHI.Services.Meshes
{
    public class Sorter
    {
        public Sorter(int value, int index)
        {
            Value = value;
            Index = index;
        }

        public int Value { get; set; }
        public int Index { get; set; }
    }
}
