using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentRide.Models;
using RentRide.Models.Repositories;

namespace RentRide.Controllers;

[Authorize(Roles = "Admin,Member")]
public class FleetController : Controller
{
    private readonly ICarRepository _carRepository;

    public FleetController(ICarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    [HttpGet]
    public async Task<IActionResult> AvailableCars()
    {
        var cars = await _carRepository.GetAvailableAsync();
        return PartialView("_AvailableCars", cars);
    }
}
