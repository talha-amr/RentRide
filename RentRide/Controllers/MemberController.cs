using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentRide.Data;
using RentRide.Models.ViewModels;

namespace RentRide.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound();
            }

            return View(new MemberDashboardViewModel
            {
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? string.Empty : user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                MemberSince = user.CreatedAt
            });
        }
    }
}
