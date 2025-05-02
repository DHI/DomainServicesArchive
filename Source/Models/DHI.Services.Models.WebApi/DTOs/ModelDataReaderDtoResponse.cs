namespace DHI.Services.Models.WebApi
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Data transfer object (DTO) for a model data reader resource representation in a response
    /// </summary>
    public class ModelDataReaderDtoResponse
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelDataReaderDtoResponse"/> class.
        /// </summary>
        public ModelDataReaderDtoResponse()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelDataReaderDtoResponse" /> class.
        /// </summary>
        public ModelDataReaderDtoResponse(IModelDataReader modelDataReader)
        {
            Id = modelDataReader.Id;
            Name = modelDataReader.Name;
            TypeName = modelDataReader.TypeName;
            Parameters = modelDataReader.GetParameterList();
            InputTimeSeriesList = modelDataReader.GetInputTimeSeriesList();
            OutputTimeSeriesList = modelDataReader.GetOutputTimeSeriesList();
        }

        /// <summary>
        ///     Gets the identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name { get; set; } 

        /// <summary>
        ///     Gets the type of the model data reader.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        ///     Gets the parameters.
        /// </summary>
        public IDictionary<string, Type> Parameters { get; set; }

        /// <summary>
        ///     Gets the input time series list.
        /// </summary>
        public IDictionary<string, string> InputTimeSeriesList { get; set; }

        /// <summary>
        ///     Gets the output time series list.
        /// </summary>
        public IDictionary<string, string> OutputTimeSeriesList { get; set; }
    }
}