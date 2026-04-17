using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence;

/// <summary>
/// Unit of Work con operaciones de ESCRITURA.
/// Usa DbContext internamente.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TwitterDbContext _context;

    public IUserRepository Users { get; }
    public IPostRepository Posts { get; }
    public IAuthRepository Auth { get; }
    public IRoleRepository Roles { get; }

    public UnitOfWork(
        TwitterDbContext context,
        IUserRepository userRepository,
        IPostRepository postRepository,
        IAuthRepository authRepository,
        IRoleRepository roleRepository)
    {
        _context = context;
        Users = userRepository;
        Posts = postRepository;
        Auth = authRepository;
        Roles = roleRepository;
    }

    public void Create<T>(T entity) where T : class
    {
        _context.Add(entity);
    }

    public void Update<T>(T entity) where T : class
    {
        _context.Update(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
        _context.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
