using TodoListAPI.Models;

namespace TodoListAPI.Repositories
{
    public interface IRepositoryWrapper
    {
        IRepositoryBase<User> User { get; }
        IRepositoryBase<Todo> Todo { get; }
        IRepositoryBase<RefreshToken> RefreshToken { get; }

        Task SavechangeAsync();
    }

    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly MyDBContext _db;

        public RepositoryWrapper(MyDBContext db)
        {
            _db = db;
        }


        public IRepositoryBase<User> UserRepositoryBase;
        public IRepositoryBase<User> User => UserRepositoryBase ??= new RepositoryBase<User>(_db);


        public IRepositoryBase<Todo> TodoRepositoryBase;
        public IRepositoryBase<Todo> Todo => TodoRepositoryBase ??= new RepositoryBase<Todo>(_db);


        public IRepositoryBase<RefreshToken> RefreshTokenRepositoryBase;
        public IRepositoryBase<RefreshToken> RefreshToken => RefreshTokenRepositoryBase ??= new RepositoryBase<RefreshToken>(_db);


        public async Task SavechangeAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
