namespace DHI.Services.Rasters.Test.ESRIASCII
{
    using Radar.ESRIASCII;

    public class EsriAsciiRepositoryFixture : BaseFileRepositoryFixture
    {
        public EsriAsciiRepositoryFixture() : base(new[] {
            "NAWABS_Rainfall_20180315_1245.asc",
            "NAWABS_Rainfall_20180315_1545.asc",
            "NAWABS_Rainfall_20180316_1245.asc"})
        {
            Repository = new EsriAsciiRepository($"{FolderPath};NAWABS_Rainfall_{{datetimeFormat}}.asc;yyyyMMdd_HHmm");
        }

        public EsriAsciiRepository Repository { get; }
    }
}