namespace DHI.Services.Rasters.Test.DELIMITEDASCII
{
    using Radar.DELIMITEDASCII;

    public class DelimitedAsciiRepositoryFixture : BaseFileRepositoryFixture
    {
        public DelimitedAsciiRepositoryFixture() : base(new[] {
            "PM_2018031712_001.txt",
            "PM_2018031712_002.txt",
            "PM_2018031712_003.txt" })
        {
            Repository = new DelimitedAsciiRepository($"{FolderPath};PM_{{datetimeFormat}}.txt;yyyyMMddHH_$$$");
        }

        public DelimitedAsciiRepository Repository { get; }
    }
}