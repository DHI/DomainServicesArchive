namespace DHI.Services.Tables
{
    using System;

    public struct Column : IEquatable<Column>
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public bool IsKey { get; set; }
        public string Quantity { get; set; }

        public Column(string name, Type dataType, bool isKey = false, string quantity = null) : this()
        {
            Guard.Against.NullOrEmpty(name, nameof(name));
            Name = name;
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            IsKey = isKey;
            Quantity = quantity;
        }

        public bool Equals(Column other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals((Column)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}