namespace DHI.Services.GIS.NetCDF
{
    public class NcDimension
    {
        public int Id { get; set;}
        public string Name { get; set; }
        public int Length { get; set; }
        public bool IsRecord => Length == 0;
    }
}
