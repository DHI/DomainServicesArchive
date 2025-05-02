namespace DHI.Services.Places.WebApi
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class PlaceDTO<TCollectionId> where TCollectionId : notnull
    {
        public PlaceDTO()
        {
        }

        public PlaceDTO(Place<TCollectionId> place)
        {
            Indicators = new Dictionary<string, IndicatorDTO>();
            FullName = place.FullName;
            FeatureId = place.FeatureId;
            if (place.Metadata is not null)
            {
                Metadata = (Dictionary<string, object>)place.Metadata;
            }

            if (place.Indicators is not null)
                foreach (var kvp in place.Indicators)
                {
                    Indicators.Add(kvp.Key, new IndicatorDTO(kvp.Value));
                }
        }

        /// <summary>
        ///     Gets or sets the fullname.
        /// </summary>
        [Required]
        public string FullName { get; set; }

        [Required]
        public FeatureId<TCollectionId> FeatureId { get; set; }

        public Dictionary<string, IndicatorDTO> Indicators { get; set; }

        public Dictionary<string, object> Metadata { get; set; }

        public Place<TCollectionId> ToPlace()
        {
            var fullName = DHI.Services.FullName.Parse(FullName);
            var place = new Place<TCollectionId>(fullName.ToString(), fullName.Name, FeatureId, fullName.Group);

            if (Metadata is not null)
            {
                foreach (var item in Metadata)
                {
                    place.Metadata.Add(item.Key, item.Value);
                }
            }

            if (Indicators is not null)
            {
                foreach (var indicatorDTO in Indicators)
                {
                    place.Indicators.Add(indicatorDTO.Key, indicatorDTO.Value.ToIndicator());
                }
            }

            return place;
        }
    }

    public class PlaceDTO : PlaceDTO<string>
    {
        public PlaceDTO()
        {
        }
        public PlaceDTO(Place place) : base(place)
        {
            //Indicators = new Dictionary<string, IndicatorDTO>();
            //FullName = place.FullName;
            //FeatureId = place.FeatureId;
            //if (place.Metadata != null)
            //{
            //    Metadata = (Dictionary<string, object>)place.Metadata;
            //}

            //foreach (var kvp in place.Indicators)
            //{
            //    Indicators.Add(kvp.Key, new IndicatorDTO(kvp.Value));
            //}
        }

        ///// <summary>
        /////     Gets or sets the fullname.
        ///// </summary>
        //[Required]
        //public string FullName { get; set; }

        //[Required]
        //public FeatureId FeatureId { get; set; }

        //public Dictionary<string, IndicatorDTO> Indicators { get; set; }

        //public Dictionary<string, object> Metadata { get; set; }

        //public Place ToPlace()
        //{
        //    var fullName = DHI.Services.FullName.Parse(FullName);
        //    var place = new Place(fullName.ToString(), fullName.Name, FeatureId, fullName.Group);

        //    if (Metadata != null)
        //    {
        //        foreach (var item in Metadata)
        //        {
        //            place.Metadata.Add(item.Key, item.Value);
        //        }
        //    }

        //    if (Indicators is null)
        //    {
        //        return place;
        //    }

        //    foreach (var indicatorDTO in Indicators)
        //    {
        //        place.Indicators.Add(indicatorDTO.Key, indicatorDTO.Value.ToIndicator());
        //    }

        //    return place;
        //}
    }
}