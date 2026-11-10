using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentRide.Models.Repositories;
using RentRide.Models.ViewModels;

namespace RentRide.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AdminController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = _userRepository.GetUserCount(),
                TotalMembers = _userRepository.GetMemberCount(),
                RecentUsers = _userRepository.GetRecentUsers(6)
            };

            return View(model);
        }
    }
}
