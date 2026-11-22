using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentRide.Models;
using RentRide.Models.Repositories;

namespace RentRide.Controllers;

[Authorize(Roles = "Admin")]
public class CarsController : Controller
{
    private readonly ICarRepository _carRepository;

    public CarsController(ICarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _carRepository.GetAllAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var car = await _carRepository.GetByIdAsync(id);
        return car is null ? NotFound() : View(car);
    }

    public IActionResult Create()
    {
        return View(new Car());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Car car)
    {
        if (!ModelState.IsValid)
        {
            return View(car);
        }

        await _carRepository.AddAsync(car);
        TempData["StatusMessage"] = "Car added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var car = await _carRepository.GetByIdAsync(id);
        return car is null ? NotFound() : View(car);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Car car)
    {
        if (id != car.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(car);
        }

        var existingCar = await _carRepository.GetByIdAsync(id);
        if (existingCar is null)
        {
            return NotFound();
        }

        existingCar.Make = car.Make;
        existingCar.Model = car.Model;
        existingCar.Year = car.Year;
        existingCar.DailyRate = car.DailyRate;
        existingCar.IsAvailable = car.IsAvailable;
        await _carRepository.UpdateAsync(existingCar);
        TempData["StatusMessage"] = "Car updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var car = await _carRepository.GetByIdAsync(id);
        return car is null ? NotFound() : View(car);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var car = await _carRepository.GetByIdAsync(id);
        if (car is null)
        {
            return NotFound();
        }

        await _carRepository.DeleteAsync(car);
        TempData["StatusMessage"] = "Car deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
