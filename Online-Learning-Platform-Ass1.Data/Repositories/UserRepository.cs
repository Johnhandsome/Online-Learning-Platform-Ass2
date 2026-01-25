using Online_Learning_Platform_Ass1.Data.Database;
using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Repositories;

public class UserRepository(OnlineLearningContext context) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
        => await Query().FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByEmailAsync(string email)
        => await Query().FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(Guid id)
        => await Query().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByIdForUpdateAsync(Guid id)
        => await context.Users.FirstOrDefaultAsync(u => u.Id == id); // With tracking for updates

    public async Task AddAsync(User user) => await context.Users.AddAsync(user);

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();

    public async Task<IEnumerable<User>> GetAllAsync() => await Query().ToListAsync();

    private IQueryable<User> Query() => context.Users
        .AsNoTracking()
        .Include(u => u.Role);
}
