using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentRide.Models.Repositories;
using RentRide.Models.ViewModels;

namespace RentRide.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly IUserRepository _userRepository;

        public MemberController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
            {
                return Forbid();
            }

            var user = _userRepository.GetById(userId);
            if (user is null)
            {
                return NotFound();
            }

            return View(new MemberDashboardViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                MemberSince = user.CreatedAt
            });
        }
    }
}
