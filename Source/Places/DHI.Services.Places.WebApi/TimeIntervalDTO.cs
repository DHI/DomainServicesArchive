namespace DHI.Services.Places.WebApi
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TimeIntervalDTO
    {
        public TimeIntervalDTO()
        {
        }

        public TimeIntervalDTO(TimeInterval timeInterval)
        {
            Type = timeInterval.Type;
            Start = timeInterval.Start;
            End = timeInterval.End;
        }

        [Required]
        public TimeIntervalType Type { get; set; }

        public double? Start { get; set; }

        public double? End { get; set; }

        public TimeInterval ToTimeInterval()
        {
            switch (Type)
            {
                case TimeIntervalType.Fixed:
                    Guard.Against.Null(Start, nameof(Start));
                    Guard.Against.Null(End, nameof(End));
                    return TimeInterval.CreateFixed((double)Start, (double)End);
                case TimeIntervalType.RelativeToNow:
                    Guard.Against.Null(Start, nameof(Start));
                    Guard.Against.Null(End, nameof(End));
                    return TimeInterval.CreateRelativeToNow((double)Start, (double)End);
                case TimeIntervalType.RelativeToDateTime:
                    Guard.Against.Null(Start, nameof(Start));
                    Guard.Against.Null(End, nameof(End));
                    return TimeInterval.CreateRelativeToDateTime((double)Start, (double)End);
                case TimeIntervalType.All:
                    return TimeInterval.CreateAll();
                default:
                    throw new NotSupportedException($"Time interval type '{Type}' is not supported.");
            }
        }
    }
}