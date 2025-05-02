namespace DHI.Services.Places.Test
{
    using System;
    using AutoFixture.Xunit2;
    using Xunit;

    public class FeatureIdTest
    {
        [Fact]
        public void CreateWithNullOrEmptyFeatureCollectionIdThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new FeatureId(null, "key", "value"));
            Assert.Throws<ArgumentException>(() => new FeatureId("", "key", "value"));
        }

        [Fact]
        public void CreateWithNullOrEmptyAttributeKeyThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new FeatureId("featureCollectionId", null, "value"));
            Assert.Throws<ArgumentException>(() => new FeatureId("featureCollectionId", "", "value"));
        }

        [Fact]
        public void CreateWithNullAttributeValueThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new FeatureId("featureCollectionId", "key", null));
        }

        [Theory, AutoData]
        public void EqualityIsOk(string featureCollectionId, string attributeKey, object attributeValue)
        {
            var featureId1 = new FeatureId(featureCollectionId, attributeKey, attributeValue);
            var featureId2 = new FeatureId(featureId1.FeatureCollectionId, featureId1.AttributeKey, featureId1.AttributeValue);
            var featureId3 = new FeatureId(featureId1.FeatureCollectionId, featureId1.AttributeKey, Guid.NewGuid());

            Assert.Equal(featureId1, featureId2);
            Assert.True(featureId1 == featureId2);
            Assert.NotEqual(featureId1, featureId3);
            Assert.True(featureId1 != featureId3);
        }
    }
}