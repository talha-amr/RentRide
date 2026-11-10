namespace RentRide.Models.Repositories
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);

        User? GetById(int userId);

        void Create(User user);

        bool UpdateRole(int userId, string role);

        int GetUserCount();

        int GetMemberCount();

        IReadOnlyList<User> GetRecentUsers(int count);
    }
}
