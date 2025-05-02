namespace DHI.Services.GIS.Test
{
    using System.Collections.Generic;
    using Maps;

    public class GroupedMapSourceFixture
    {
        public GroupedMapSourceFixture()
        {
            var layers = new List<Layer>();
            var inundation = FullName.Parse("Riverbank gardens/CDZ_inundation_2016-2018");
            layers.Add(new Layer(inundation.ToString(), inundation.Name, inundation.Group));
            var rivergarden = FullName.Parse("Riverbank gardens/CDZ_rivergarden_2016-2018");
            layers.Add(new Layer(rivergarden.ToString(), rivergarden.Name, rivergarden.Group));
            var westgarden = FullName.Parse("Westbank gardens/CDZ_westgarden");
            layers.Add(new Layer(westgarden.ToString(), westgarden.Name, westgarden.Group));

            GroupedMapSource = new FakeGroupedMapSource(layers);
        }

        public IGroupedMapSource GroupedMapSource { get; }
    }
}