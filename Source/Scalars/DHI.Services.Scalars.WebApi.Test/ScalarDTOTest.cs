namespace DHI.Services.Scalars.WebApi.Test
{
    using Logging;
    using System;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class ScalarDTOTest
    {
        private readonly ScalarDTO _scalarDTO;

        public ScalarDTOTest()
        {
            _scalarDTO = new ScalarDTO()
            {
                FullName = new FullName("foo/bar", "WaterLevel").ToString(),
                Description = "Water Level",
                DateTime = DateTime.Now,
                Value = "99.99",
                Flag = 0,
                ValueTypeName = "System.Double"
            };
        }

        [Fact]
        public void DtoToScalarIsOk()
        {
            var scalar = _scalarDTO.ToScalar();
            var data = scalar.GetData().Value;

            Assert.Equal(_scalarDTO.FullName, scalar.Id);
            Assert.Equal(_scalarDTO.Description, scalar.Description);
            Assert.Equal(_scalarDTO.ValueTypeName, scalar.ValueTypeName);
            Assert.Equal("foo/bar", scalar.Group);
            Assert.Equal("WaterLevel", scalar.Name);
            Assert.Equal(_scalarDTO.DateTime, data.DateTime);
            Assert.Equal(99.99, data.Value);
            Assert.Equal(0, data.Flag);
        }

        [Fact]
        public void DtoToIntScalarIsOk()
        {
            _scalarDTO.ValueTypeName = "System.Int32";
            _scalarDTO.Value = "99";
            var data = _scalarDTO.ToScalar().GetData().Value;
            Assert.Equal(99, data.Value);
        }

        [Fact]
        public void DtoToBooleanScalarIsOk()
        {
            _scalarDTO.ValueTypeName = "System.Boolean";
            _scalarDTO.Value = "false";
            var data = _scalarDTO.ToScalar().GetData().Value;
            Assert.Equal(false, data.Value);
        }

        [Fact]
        public void DtoToStringScalarIsOk()
        {
            _scalarDTO.ValueTypeName = "System.String";
            _scalarDTO.Value = "VeryHigh";
            var data = _scalarDTO.ToScalar().GetData().Value;
            Assert.Equal("VeryHigh", data.Value);
        }

        [Fact]
        public void DtoToDateTimeScalarIsOk()
        {
            _scalarDTO.ValueTypeName = "System.DateTime";
            _scalarDTO.Value = "2019-08-28T13:30:00";
            var data = _scalarDTO.ToScalar().GetData().Value;
            Assert.Equal(DateTime.Parse(_scalarDTO.Value), data.Value);
        }

        [Fact]
        public void DtoToLogLevelScalarIsOk()
        {
            _scalarDTO.ValueTypeName = "Microsoft.Extensions.Logging.LogLevel, Microsoft.Extensions.Logging.Abstractions, Version=8.0.0.0";
            _scalarDTO.Value = "LogLevel.Critical";
            var data = _scalarDTO.ToScalar().GetData().Value;
            Assert.Equal(LogLevel.Critical, data.Value);
        }

        [Fact]
        public void ScalarToDtoIsOk()
        {
            var scalar = _scalarDTO.ToScalar();
            var dto = scalar.ToDTO();

            Assert.Equal(_scalarDTO.FullName, dto.FullName);
            Assert.Equal(_scalarDTO.Description, dto.Description);
            Assert.Equal(_scalarDTO.ValueTypeName, dto.ValueTypeName);
            Assert.Equal(_scalarDTO.DateTime, dto.DateTime);
            Assert.Equal(_scalarDTO.Value, dto.Value);
            Assert.Equal(_scalarDTO.Flag, dto.Flag);
        }
    }
}