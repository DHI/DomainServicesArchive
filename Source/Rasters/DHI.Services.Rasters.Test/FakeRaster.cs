namespace DHI.Services.Rasters.Test
{
    using DHI.Services.Authorization;
    using Rasters;
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    public class FakeRaster : BaseRaster
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeRaster" /> class
        /// </summary>
        /// <param name="dateTime">The dateTime.</param>
        /// <param name="name">The name.</param>
        /// <param name="values">The values.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="permissions">The permissions.</param>
        public FakeRaster(DateTime dateTime, string name, IList<float> values = null, IDictionary<string, object> metadata = null, IList<Permission> permissions = null)
            : base(dateTime, name, values, metadata, permissions)
        {
        }

        public override Bitmap ToBitmap()
        {
            throw new NotImplementedException();
        }
    }
}
