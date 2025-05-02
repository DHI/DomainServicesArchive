namespace DHI.Services.Models.WebApi
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ExtensionMethods
    {
        //public static ScenarioDTO ToDTO(this Scenario place)
        //{
        //    return new ScenarioDTO(place);
        //}

        public static ModelDataReaderDtoResponse ToDTO(this IModelDataReader reader)
        {
            return new ModelDataReaderDtoResponse(reader);
        }

        public static IEnumerable<ModelDataReaderDtoResponse> ToDTOs(this IEnumerable<IModelDataReader> readers)
        {
            return readers.Select(model => new ModelDataReaderDtoResponse(model));
        }
    }
}