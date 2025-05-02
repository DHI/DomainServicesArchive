namespace DHI.Services.Scalars
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Class ScalarService.
    /// </summary>
    /// <typeparam name="TId">The type of the scalar identifier.</typeparam>
    /// <typeparam name="TFlag">The type of the quality flag.</typeparam>
    public class ScalarService<TId, TFlag> : BaseUpdatableDiscreteService<Scalar<TId, TFlag>, TId>, IScalarService<TId, TFlag> where TFlag : struct
    {
        private readonly IScalarRepository<TId, TFlag> _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScalarService{TId, TFlag}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="logger">The logger.</param>
        public ScalarService(IScalarRepository<TId, TFlag> repository, ILogger logger = null)
            : base(repository)
        {
            if (!(logger is null))
            {
                Updated += (sender, args) =>
                {
                    var (scalarBefore, scalarAfter, log) = args.Item;
                    var scalarDataBefore = scalarBefore.GetData().Value;
                    var scalarDataAfter = scalarAfter.GetData().Value;
                    if (log && scalarDataBefore != scalarDataAfter)
                    {
                        logger.LogInformation("The value of scalar '{ScalarBefore}' has changed. Old data: '{Old}'. New data: '{New}'", scalarBefore, (scalarDataBefore is null ? "Empty" : scalarDataBefore.ToString()), (scalarDataAfter is null ? "Empty" : scalarDataAfter.ToString()));
                    }
                };
            }

            _repository = repository;
        }

        /// <summary>
        ///     Updates the specified scalar.
        /// </summary>
        /// <param name="scalar">The scalar.</param>
        /// <param name="user">The user.</param>
        /// <exception cref="KeyNotFoundException" />
        public override void Update(Scalar<TId, TFlag> scalar, ClaimsPrincipal user = null)
        {
            var maybe = _repository.Get(scalar.Id);
            if (maybe.HasValue)
            {
                var scalarBefore = maybe.Value;
                if (scalarBefore.Locked)
                {
                    throw new Exception($"Cannot update scalar with id '{scalar.Id}' because it is locked.");
                }

                var cancelEventArgs = new CancelEventArgs<Scalar<TId, TFlag>>(scalarBefore);
                OnUpdating(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Update(scalar, user);
                    OnUpdated(scalarBefore, scalar);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Scalar with id '{scalar.Id}' was not found.");
            }
        }

        /// <summary>
        ///     Adds or updates the specified scalar.
        /// </summary>
        /// <param name="scalar">The scalar.</param>
        /// <param name="user">The user.</param>
        public override void AddOrUpdate(Scalar<TId, TFlag> scalar, ClaimsPrincipal user = null)
        {
            var maybe = _repository.Get(scalar.Id);
            if (maybe.HasValue)
            {
                var scalarBefore = maybe.Value;
                if (scalarBefore.Locked)
                {
                    throw new Exception($"Cannot update scalar with id '{scalar.Id}' because it is locked.");
                }

                var cancelEventArgs = new CancelEventArgs<Scalar<TId, TFlag>>(scalarBefore);
                OnUpdating(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Update(scalar, user);
                    OnUpdated(scalarBefore, scalar);
                }
            }
            else
            {
                var cancelEventArgs = new CancelEventArgs<Scalar<TId, TFlag>>(scalar);
                OnAdding(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Add(scalar, user);
                    OnAdded(scalar);
                }
            }
        }

        /// <summary>
        ///     Try updating the specified scalar without existence check.
        /// </summary>
        /// <param name="scalar">The scalar.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if scalar was successfully updated, <c>false</c> otherwise.</returns>
        public override bool TryUpdate(Scalar<TId, TFlag> scalar, ClaimsPrincipal user = null)
        {
            try
            {
                var maybe = _repository.Get(scalar.Id);
                if (!maybe.HasValue)
                {
                    return false;
                }

                var scalarBefore = maybe.Value;
                if (scalarBefore.Locked)
                {
                    return false;
                }

                var cancelEventArgs = new CancelEventArgs<Scalar<TId, TFlag>>(scalarBefore);
                OnUpdating(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return false;
                }

                _repository.Update(scalar, user);
                OnUpdated(scalarBefore, scalar);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Sets the data of a scalar.
        /// </summary>
        /// <param name="id">The scalar identifier.</param>
        /// <param name="data">The scalar data.</param>
        /// <param name="log">If true, logging is performed if data is modified</param>
        /// <param name="user">The user.</param>
        public void SetData(TId id, ScalarData<TFlag> data, bool log = true, ClaimsPrincipal user = null)
        {
            var maybe = _repository.Get(id);
            if (maybe.HasValue)
            {
                var scalarBefore = maybe.Value;
                if (scalarBefore.Locked)
                {
                    throw new Exception($"Cannot update scalar with id '{scalarBefore.Id}' because it is locked.");
                }

                var cancelEventArgs = new CancelEventArgs<Scalar<TId, TFlag>>(scalarBefore);
                OnUpdating(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.SetData(id, data, user);
                    OnUpdated(scalarBefore, _repository.Get(id).Value, log);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Scalar with id '{id}' was not found.");
            }
        }

        /// <summary>
        ///     Try setting the data of the scalar or adding the scalar if it does not exist.
        /// </summary>
        /// <param name="scalar">The scalar.</param>
        /// <param name="log">If true, logging is performed if data is modified</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if scalar data was successfully set or the scalar was added, <c>false</c> otherwise.</returns>
        public bool TrySetDataOrAdd(Scalar<TId, TFlag> scalar, bool log = true, ClaimsPrincipal user = null)
        {
            try
            {
                var maybe = _repository.Get(scalar.Id);
                if (maybe.HasValue)
                {
                    var scalarBefore = maybe.Value;
                    if (scalarBefore.Locked)
                    {
                        return false;
                    }

                    var cancelEventArgs = new CancelEventArgs<Scalar<TId, TFlag>>(scalarBefore);
                    OnUpdating(cancelEventArgs);
                    if (cancelEventArgs.Cancel)
                    {
                        return false;
                    }

                    _repository.SetData(scalar.Id, scalar.GetData().Value, user);
                    OnUpdated(scalarBefore, scalar, log);
                    return true;
                }
                else
                {
                    var cancelEventArgs = new CancelEventArgs<Scalar<TId, TFlag>>(scalar);
                    OnAdding(cancelEventArgs);
                    if (cancelEventArgs.Cancel)
                    {
                        return false;
                    }

                    _repository.Add(scalar, user);
                    OnAdded(scalar);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Sets the Locked property of the scalar with the given identifier
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="locked">if set to <c>true</c> [locked].</param>
        /// <param name="user">The user.</param>
        public void SetLocked(TId id, bool locked, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(id, user))
            {
                throw new KeyNotFoundException($"Scalar with id '{id}' was not found.");
            }

            _repository.SetLocked(id, locked, user);
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IScalarRepository<TId, TFlag>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IScalarRepository<TId, TFlag>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly
        ///     is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wild card
        ///     (* and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IScalarRepository<TId, TFlag>>(path, searchPattern);
        }

        /// <summary>
        ///     Occurs when [updated].
        /// </summary>
        public new event EventHandler<EventArgs<(Scalar<TId, TFlag>, Scalar<TId, TFlag>, bool)>> Updated;

        /// <summary>
        ///     Called when [updated].
        /// </summary>
        /// <param name="scalarBefore">The scalar before.</param>
        /// <param name="scalarAfter">The scalar after.</param>
        /// <param name="log">If true, logging is performed if data is modified</param>
        protected void OnUpdated(Scalar<TId, TFlag> scalarBefore, Scalar<TId, TFlag> scalarAfter, bool log = true)
        {
            Updated?.Invoke(this, new EventArgs<(Scalar<TId, TFlag>, Scalar<TId, TFlag>, bool)>((scalarBefore, scalarAfter, log)));
        }
    }

    /// <inheritdoc />
    public class ScalarService<TFlag> : ScalarService<string, TFlag> where TFlag : struct
    {
        /// <inheritdoc />
        public ScalarService(IScalarRepository<string, TFlag> repository, ILogger logger = null)
            : base(repository, logger)
        {
        }
    }

    /// <inheritdoc />
    public class ScalarService : ScalarService<string, int>
    {
        /// <inheritdoc />
        public ScalarService(IScalarRepository<string, int> repository, ILogger logger = null)
            : base(repository, logger)
        {
        }
    }
}