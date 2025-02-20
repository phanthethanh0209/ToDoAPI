using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TodoListAPI.Models;

namespace TodoListAPI.Repositories
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetTodoListWithPaginationAsync(Expression<Func<T, bool>> expression = null, int pageNumber = 1, int limit = 10);
        //Task<T?> GetById(Expression<Func<T, bool>> expression = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression = null);
        //Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> expression = null);
        T SingleOrDefault(Expression<Func<T, bool>> expression = null);
        Task CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }

    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly MyDBContext _context;
        private readonly DbSet<T> _dbSet;
        public RepositoryBase(MyDBContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            IQueryable<T> query = _dbSet;

            return await query.ToListAsync();
        }

        public T SingleOrDefault(Expression<Func<T, bool>> expression = null)
        {
            IQueryable<T> query = _dbSet;

            if (expression != null)
            {
                query = query.Where(expression);
            }

            return query.SingleOrDefault();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression = null)
        {
            DbSet<T> query = _dbSet;

            if (expression != null)
            {
                return await query.AnyAsync(expression);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<T>> GetTodoListWithPaginationAsync(Expression<Func<T, bool>> expression = null, int pageNumber = 1, int limit = 10)
        {
            IQueryable<T> query = _dbSet;
            if (expression != null)
            {
                query = query.Where(expression);
            }

            query = query.Skip((pageNumber - 1) * limit).Take(limit);
            return await query.ToListAsync();
        }
    }
}
