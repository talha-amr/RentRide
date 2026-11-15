namespace RentRide.Models.Repositories
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);

        User? GetByNormalizedEmail(string normalizedEmail);

        User? GetById(int userId);

        int Create(User user);

        bool Update(User user);

        bool Delete(int userId);

        bool UpdateRole(int userId, string role);

        int GetUserCount();

        int GetMemberCount();

        IReadOnlyList<User> GetRecentUsers(int count);
    }
}
