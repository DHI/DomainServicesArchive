namespace DHI.Services.Rasters.Radar
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public class BiasCorrectionService : BaseUpdatableDiscreteService<Matrix, DateTime>
    {
        public BiasCorrectionService(IUpdatableRepository<Matrix, DateTime> correctionMatrixRepository) : base(correctionMatrixRepository)
        {
        }

        public Maybe<Matrix> GetLastBefore(DateTime dateTime)
        {
            var date = GetIds().LastOrDefault(d => d < dateTime);
            if (date == default || !TryGet(date, out var matrix))
            {
                return Maybe.Empty<Matrix>();
            }

            return matrix.ToMaybe();
        }

        /// <summary>
        ///  Create a spatial variant bias correction matrix with method developed by Jensen, N.E., 2015. Radar Data Processing at AROS radar.
        /// </summary>
        /// <param name="matrixSize">Size of the correction matrix</param>
        /// <param name="dateTime">Date time of the correction</param>
        /// <param name="gauges">Gauges used for correction</param>
        /// <returns></returns>
        public static Matrix CreateCorrectionMatrix(Size matrixSize, DateTime dateTime, IEnumerable<Gauge> gauges)
        {
            gauges = gauges?.ToList() ?? throw new ArgumentNullException(nameof(gauges));
            if (!gauges.Any())
            {
                throw new ArgumentException("Gauges collection is empty.", nameof(gauges));
            }

            // prime matrix with value 1 in all pixels
            var correctionMatrix = new Matrix(dateTime) { Size = matrixSize };
            for (var i = 0; i < matrixSize.Height * matrixSize.Width; i++)
            {
                correctionMatrix.Values.Add(1f);
            }

            // calculate weight in each pixel as average of influence from each gauge
            for (var row = 1; row <= correctionMatrix.Size.Height; row++)
            {
                for (var col = 1; col <= correctionMatrix.Size.Width; col++)
                {
                    var pixel = new Pixel(col, row);
                    var w = gauges.Average(gauge => gauge.GetWeight(pixel));
                    correctionMatrix.UpdateValue(pixel, (float)w);
                }
            }

            return correctionMatrix;
        }

        /// <summary>
        /// Create a spatial uniform bias correction matrix with mean field bias.
        /// See Thorndahl et al., 2014. Bias adjustment and advection interpolation of long-term high resolution radar rainfall series.
        /// </summary>
        /// <param name="matrixSize">Size of the correction matrix</param>
        /// <param name="dateTime">Date time of the correction</param>
        /// <param name="gaugeRadarDepths">Rainfall depth of gauges and radars used for correction</param>
        /// <returns></returns>
        public static Matrix CreateCorrectionMatrix(Size matrixSize, DateTime dateTime, IEnumerable<GaugeRadarDepth> gaugeRadarDepths)
        {
            gaugeRadarDepths = gaugeRadarDepths?.ToList() ?? throw new ArgumentNullException(nameof(gaugeRadarDepths));
            if (!gaugeRadarDepths.Any())
            {
                throw new ArgumentException("Collection of radar-rain gauge pair depths is empty.", nameof(gaugeRadarDepths));
            }

            // calculate mean field bias and assign to all pixels
            var gaugeSum = gaugeRadarDepths.Sum(d => d.GaugeDepth);
            var radarSum = gaugeRadarDepths.Sum(d => d.RadarDepth);
            var meanFieldBias = radarSum > 0 ? gaugeSum / radarSum : 1.0;

            // prime matrix with value 1 in all pixels
            var correctionMatrix = new Matrix(dateTime) { Size = matrixSize };
            for (var i = 0; i < matrixSize.Height * matrixSize.Width; i++)
            {
                correctionMatrix.Values.Add(Convert.ToSingle(meanFieldBias));
            }

            return correctionMatrix;
        }
    }
}