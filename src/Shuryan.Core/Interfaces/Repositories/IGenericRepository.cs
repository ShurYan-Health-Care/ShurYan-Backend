using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IGenericRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(Guid id);

		Task<IEnumerable<T>> GetAllAsync();

		Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

		// Fetch a single record based on a specific condition
		Task<T?> GetAsync(Expression<Func<T, bool>> filter, string includeProperties = "");

		// Checking for a record
		Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);

		// Get queryable for complex LINQ queries
		IQueryable<T> GetQueryable();

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
		// ========================

		Task<T> AddAsync(T entity);
		Task DeleteByIdAsync(Guid id);
		void Update(T entity);
		void Delete(T entity);
    }
}
