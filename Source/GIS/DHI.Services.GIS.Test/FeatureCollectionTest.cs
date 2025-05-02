using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHI.Services.GIS.Test
{
    using Spatial;
    using Xunit;

    public class FeatureCollectionTest
    {
        [Fact]
        public void CreateFromSpatialFeatureCollectionIsOk()
        {
            var copenhagen = new Feature(new Point(new Position(102, 0.5)));
            copenhagen.AttributeValues.Add("name", "Copenhagen");
            var oslo = new Feature(new Point(new Position(102, 0.5)));
            oslo.AttributeValues.Add("name", "Oslo");
            var features = new List<IFeature>( new [] { copenhagen, oslo } );
            var spatialFeatureCollection = new DHI.Spatial.FeatureCollection(features);
            spatialFeatureCollection.Attributes.Add(new Attribute("name", typeof(string), 200));
            Assert.IsType<DHI.Spatial.FeatureCollection>(spatialFeatureCollection);

            var featureCollection = new DHI.Services.GIS.FeatureCollection(Guid.NewGuid().ToString(), "Scandinavian Cities", "Cities", spatialFeatureCollection );
            Assert.IsType<DHI.Services.GIS.FeatureCollection>(featureCollection);
            Assert.Contains(featureCollection.Attributes, attribute => attribute.Name == "name");
            Assert.Equal(2, featureCollection.Features.Count);
            Assert.Contains(featureCollection.Features, feature => (string)feature.AttributeValues["name"] == "Oslo");
        }
    }
}
