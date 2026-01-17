using GHI_ASSET_CARGO.Domain.Entities;


namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IRepository
    {
        public Task Add<TEntity>(TEntity entity) where TEntity : BaseEntity;

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : BaseEntity;

        public Task<TEntity?> FindById<TEntity>(string id) where TEntity : BaseEntity;

        public void Update<TEntity>(TEntity entity) where TEntity : BaseEntity;

        public void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity;
    }
}
