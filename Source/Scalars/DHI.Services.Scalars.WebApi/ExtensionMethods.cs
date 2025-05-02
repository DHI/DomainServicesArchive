namespace DHI.Services.Scalars.WebApi
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ExtensionMethods
    {
        public static IEnumerable<ScalarDTO> ToDTOs(this IEnumerable<Scalar<string, int>> scalars)
        {
            return scalars.Select(scalar => new ScalarDTO(scalar));
        }

        public static ScalarDTO ToDTO(this Scalar<string, int> scalar)
        {
            return new ScalarDTO(scalar);
        }
    }
}