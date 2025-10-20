using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		protected readonly ShuryanDbContext _context;
		protected readonly DbSet<T> _dbSet;

		public GenericRepository(ShuryanDbContext context)
		{
			_context = context;
			_dbSet = _context.Set<T>();
		}

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        public virtual async Task<T?> GetByIdAsync(Guid id)
		{
			var entity = await _dbSet.FindAsync(id);

			if (entity is SoftDeletableEntity softDeletable && softDeletable.IsDeleted)
				return null;

			return entity;
		}
		public virtual async Task<IEnumerable<T>> GetAllAsync()
		{
			IQueryable<T> query = _dbSet;

			if (typeof(SoftDeletableEntity).IsAssignableFrom(typeof(T)))
			{
				query = query.Where(e => !((SoftDeletableEntity)(object)e).IsDeleted);
			}

			return await query.ToListAsync();
		}

		public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
		{
			IQueryable<T> query = _dbSet;

			if (typeof(SoftDeletableEntity).IsAssignableFrom(typeof(T)))
			{
				query = query.Where(e => !((SoftDeletableEntity)(object)e).IsDeleted);
			}

			if (filter != null)
			{
				query = query.Where(filter);
			}

			return await query.CountAsync();
		}

		public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string includeProperties = "")
		{
			IQueryable<T> query = _dbSet;

			if (typeof(SoftDeletableEntity).IsAssignableFrom(typeof(T)))
			{
				query = query.Where(e => !((SoftDeletableEntity)(object)e).IsDeleted);
			}

			query = query.Where(filter);

			foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				query = query.Include(includeProperty.Trim());
			}

			return await query.FirstOrDefaultAsync();
		}

		public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
		{
			IQueryable<T> query = _dbSet;

			if (typeof(SoftDeletableEntity).IsAssignableFrom(typeof(T)))
			{
				query = query.Where(e => !((SoftDeletableEntity)(object)e).IsDeleted);
			}

			return await query.AnyAsync(filter);
		}

		public virtual IQueryable<T> GetQueryable()
		{
			IQueryable<T> query = _dbSet;

			// Note: Soft delete filtering is not applied here to allow controllers
			// to have full control over querying and filtering
			return query;
		}

		public virtual async Task<T> AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
			return entity;
		}

		public virtual async Task DeleteByIdAsync(Guid id)
		{
			var entity = await _dbSet.FindAsync(id);
			if (entity != null)
			{
				Delete(entity);
			}
		}

		public virtual void Update(T entity)
		{
			_dbSet.Update(entity);
		}

		public virtual void Delete(T entity)
		{
			if (entity is SoftDeletableEntity softDeletable)
			{
				softDeletable.IsDeleted = true;
				softDeletable.DeletedAt = DateTime.UtcNow;
				_dbSet.Update(entity);
			}
			else
			{
				_dbSet.Remove(entity);
			}
		}
	}
}