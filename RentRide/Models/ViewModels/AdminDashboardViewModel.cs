namespace RentRide.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int TotalMembers { get; set; }

        public IReadOnlyList<User> RecentUsers { get; set; } = Array.Empty<User>();
    }
}
