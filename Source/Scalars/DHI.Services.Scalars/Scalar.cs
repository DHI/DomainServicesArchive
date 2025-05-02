namespace DHI.Services.Scalars
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Class Scalar.
    /// </summary>
    /// <typeparam name="TId">The type of the scalar identifier.</typeparam>
    /// <typeparam name="TFlag">The type of the data quality flag.</typeparam>
    [Serializable]
    public class Scalar<TId, TFlag> : BaseGroupedEntity<TId> where TFlag : struct
    {
        private ScalarData<TFlag> _data;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Scalar{TId, TFlag}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="valueTypeName">Name of the value type (e.g. 'System.Double').</param>
        /// <param name="group">The group.</param>
        /// <param name="data">The data.</param>
        public Scalar(TId id, string name, string valueTypeName, string group, ScalarData<TFlag> data) : base(id, name, group)
        {
            ValueTypeName = valueTypeName;
            if (!(data is null))
            {
                SetData(data);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Scalar{TId, TFlag}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="valueTypeName">Name of the value type (e.g. 'System.Double').</param>
        /// <param name="data">The data.</param>
        public Scalar(TId id, string name, string valueTypeName, ScalarData<TFlag> data) : this(id, name, valueTypeName, null, data)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Scalar{TId, TFlag}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="valueTypeName">Name of the value type (e.g. 'System.Double').</param>
        /// <param name="group">The group.</param>
        public Scalar(TId id, string name, string valueTypeName, string group) : this(id, name, valueTypeName, group, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Scalar{TId, TFlag}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="valueTypeName">Name of the value type (e.g. 'System.Double').</param>
        public Scalar(TId id, string name, string valueTypeName) : this(id, name, valueTypeName, null, null)
        {
        }

        /// <summary>
        ///     Gets the name of the value type.
        /// </summary>
        public string ValueTypeName { get; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Scalar{TId, TFlag}" /> is locked.
        /// </summary>
        /// <value><c>true</c> if locked; otherwise, <c>false</c>.</value>
        public bool Locked { get; set; } = false;

        /// <summary>
        ///     Sets the scalar data.
        /// </summary>
        /// <param name="data">The scalar data.</param>
        public void SetData(ScalarData<TFlag> data)
        {
            var valueType = data.Value.GetType();
            if (valueType != Type.GetType(ValueTypeName))
            {
                throw new Exception($"Illegal value type '{valueType}'. Value must be of type '{Type.GetType(ValueTypeName)}'.");
            }

            _data = data;
        }

        /// <summary>
        ///     Gets the scalar data.
        /// </summary>
        /// <returns>Maybe&lt;ScalarData&lt;TFlag&gt;&gt;.</returns>
        public Maybe<ScalarData<TFlag>> GetData()
        {
            return _data?.ToMaybe() ?? Maybe.Empty<ScalarData<TFlag>>();
        }
    }

    /// <inheritdoc />
    [Serializable]
    public class Scalar<TFlag> : Scalar<string, TFlag> where TFlag : struct
    {
        /// <inheritdoc />
        [JsonConstructor]
        public Scalar(string id, string name, string valueTypeName, string group, ScalarData<TFlag> data)
            : base(id, name, valueTypeName, group, data)
        {
        }

        /// <inheritdoc />
        public Scalar(string id, string name, string valueTypeName, ScalarData<TFlag> data)
            : base(id, name, valueTypeName, data)
        {
        }

        /// <inheritdoc />
        public Scalar(string id, string name, string valueTypeName, string group)
            : base(id, name, valueTypeName, group)
        {
        }

        /// <inheritdoc />
        public Scalar(string id, string name, string valueTypeName)
            : base(id, name, valueTypeName)
        {
        }
    }

    /// <inheritdoc />
    [Serializable]
    public class Scalar : Scalar<int>
    {
        /// <inheritdoc />
        [JsonConstructor]
        public Scalar(string id, string name, string valueTypeName, string group, ScalarData<int> data)
            : base(id, name, valueTypeName, group, data)
        {
        }

        /// <inheritdoc />
        public Scalar(string id, string name, string valueTypeName, ScalarData<int> data) : base(id, name, valueTypeName, data)
        {
        }

        /// <inheritdoc />
        public Scalar(string id, string name, string valueTypeName, string group) : base(id, name, valueTypeName, group)
        {
        }

        /// <inheritdoc />
        public Scalar(string id, string name, string valueTypeName) : base(id, name, valueTypeName)
        {
        }
    }
}