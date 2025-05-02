namespace DHI.Services.TimeSeries
{
    using System;

    internal class Window<TValue> where TValue : struct, IComparable<TValue>
    {
        private bool _active;

        public Window(Interval<TValue> interval)
        {
            Interval = interval;
        }

        public event EventHandler ActiveChanged;

        public bool Active
        {
            get => _active;

            set
            {
                if (value != _active)
                {
                    _active = value;
                    _OnActiveChanged();
                }
            }
        }

        public Interval<TValue> Interval { get; }

        public DateTime StartTime { get; set; }

        public override string ToString()
        {
            return $"{Interval}: Active={Active} StartTime={StartTime}";
        }

        private void _OnActiveChanged()
        {
            ActiveChanged?.Invoke(this, new EventArgs());
        }
    }
}