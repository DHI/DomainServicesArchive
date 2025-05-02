namespace DHI.Services.Models.WebApi
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Data transfer object (DTO) for model data reader resource representation when adding or updating model data readers
    /// </summary>
    public class ModelDataReaderDtoRequest
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the name of the model data reader type.
        ///     E.g. "Namespace.MyClass, MyAssembly"
        /// </summary>
        [Required]
        public string ModelDataReaderTypeName { get; set; }

        /// <summary>
        ///     Converts the DTO to a model data reader object.
        /// </summary>
        public IModelDataReader ToModelDataReader()
        {
            var modelDataReaderType = Type.GetType(ModelDataReaderTypeName);
            if (modelDataReaderType is null)
            {
                throw new ArgumentException($"The model data reader type '{ModelDataReaderTypeName}' is not found.", nameof(ModelDataReaderTypeName));
            }

            var modelType = typeof(ModelDataReader<>).MakeGenericType(modelDataReaderType);
            Guard.Against.NullOrEmpty(Id, nameof(Id));
            return (IModelDataReader)Activator.CreateInstance(modelType, Id, Name);
        }
    }
}