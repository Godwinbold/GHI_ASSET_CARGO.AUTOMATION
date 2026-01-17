using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Data;
using GHI_ASSET_CARGO.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Infrastructure.Repositories
{
    public class Repository : IRepository
    {
        private readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Add<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : BaseEntity
        {
            return _context.Set<TEntity>();
        }

        public async Task<TEntity?> FindById<TEntity>(string id) where TEntity : BaseEntity
        {
            return await _context.Set<TEntity>().FindAsync(id) ?? null;
        }

        public void Update<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            _context.Set<TEntity>().Remove(entity);
        }
    }
}