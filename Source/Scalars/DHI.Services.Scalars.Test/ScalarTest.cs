namespace DHI.Services.Scalars.Test
{
    using System;
    using Xunit;

    public class ScalarTest
    {
        [Fact]
        public void IllegalValueTypeThrows()
        {
            Assert.Throws<Exception>(() => new Scalar<int>("myScalar", "My Scalar", "System.Int32", new ScalarData<int>(99.9, DateTime.Now)));
        }

        [Fact]
        public void CreateBoolScalarIsOk()
        {
            var scalar = new Scalar<int>("myScalar", "My Scalar", "System.Boolean", new ScalarData<int>(true, DateTime.Now));
            Assert.Equal(true, scalar.GetData().Value.Value);
        }

        [Fact]
        public void CreateDoubleScalarIsOk()
        {
            var scalar = new Scalar<int>("myScalar", "My Scalar", "System.Double", new ScalarData<int>(99.9, DateTime.Now));
            Assert.Equal(99.9, scalar.GetData().Value.Value);
        }

        [Fact]
        public void CreateIntScalarIsOk()
        {
            var scalar = new Scalar<int>("myScalar", "My Scalar", "System.Int32", new ScalarData<int>(99, DateTime.Now));
            Assert.Equal(99, scalar.GetData().Value.Value);
        }

        [Fact]
        public void CreateStringScalarIsOk()
        {
            var scalar = new Scalar<int>("myScalar", "My Scalar", "System.String", new ScalarData<int>("99", DateTime.Now));
            Assert.Equal("99", scalar.GetData().Value.Value);
        }

        [Fact]
        public void LockedDefaultIsFalse()
        {
            var scalar = new Scalar<float>("myScalar", "My Scalar", "System.String");
            Assert.False(scalar.Locked);
        }
    }
}