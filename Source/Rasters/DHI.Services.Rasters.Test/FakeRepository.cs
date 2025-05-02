namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;

    public class FakeRepository<TEntity, TEntityId> : IRepository<TEntity, TEntityId>, IDiscreteRepository<TEntity, TEntityId>, IUpdatableRepository<TEntity, TEntityId> where TEntity : IEntity<TEntityId>
    {
        protected readonly Dictionary<TEntityId, TEntity> _entities;

        public FakeRepository()
        {
            _entities = new Dictionary<TEntityId, TEntity>();
        }

        public FakeRepository(IEnumerable<TEntity> entities)
            : this()
        {
            foreach (var entity in entities)
            {
                _entities.Add(entity.Id, entity);
            }
        }

        protected Dictionary<TEntityId, TEntity> Entities => _entities;

        public int Count(ClaimsPrincipal user = null)
        {
            return GetAll().Count();
        }

        public bool Contains(TEntityId id, ClaimsPrincipal user = null)
        {
            return _entities.ContainsKey(id);
        }

        public IEnumerable<TEntity> GetAll(ClaimsPrincipal user = null)
        {
            return _entities.Values.ToArray();
        }

        public IEnumerable<TEntityId> GetIds(ClaimsPrincipal user = null)
        {
            return _entities.Values.Select(e => e.Id).ToArray();
        }

        public Maybe<TEntity> Get(TEntityId id, ClaimsPrincipal user = null)
        {
            _entities.TryGetValue(id, out var entity);
            return entity == null || entity.Equals(default(TEntity)) ? Maybe.Empty<TEntity>() : entity.ToMaybe();
        }

        public void Add(TEntity entity, ClaimsPrincipal user = null)
        {
            _entities[entity.Id] = entity;
        }

        public void Remove(TEntityId id, ClaimsPrincipal user = null)
        {
            _entities.Remove(id);
        }

        public void Update(TEntity updatedEntity, ClaimsPrincipal user = null)
        {
            _entities[updatedEntity.Id] = updatedEntity;
        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
        {
            return _entities.Values.AsQueryable().Where(predicate).ToArray();
        }

        public IEnumerable<TEntity> Get(IEnumerable<QueryCondition> query)
        {
            var predicate = ExpressionBuilder.Build<TEntity>(query);
            return Get(predicate);
        }

        public void Remove(Expression<Func<TEntity, bool>> predicate)
        {
            var toRemove = Get(predicate);
            foreach (var entity in toRemove)
            {
                _entities.Remove(entity.Id);
            }
        }
    }
}