using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentRide.Data;
using RentRide.Models.ViewModels;

namespace RentRide.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalMembers = (await _userManager.GetUsersInRoleAsync("Member")).Count
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RecentUsers()
        {
            var users = await _userManager.Users
                .OrderByDescending(user => user.CreatedAt)
                .ThenByDescending(user => user.Id)
                .Take(6)
                .ToListAsync();
            var recentUsers = new List<AdminRecentUserViewModel>(users.Count);

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                recentUsers.Add(new AdminRecentUserViewModel
                {
                    FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? string.Empty : user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Role = string.Join(", ", roles),
                    CreatedAt = user.CreatedAt
                });
            }

            return PartialView("_RecentUsers", recentUsers);
        }
    }
}
