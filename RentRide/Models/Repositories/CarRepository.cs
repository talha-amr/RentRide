using Microsoft.EntityFrameworkCore;
using RentRide.Models;

namespace RentRide.Models.Repositories;

public class CarRepository : ICarRepository
{
    private readonly RentRideContext _context;

    public CarRepository(RentRideContext context)
    {
        _context = context;
    }

    public Task<List<Car>> GetAllAsync()
    {
        return _context.Cars
            .AsNoTracking()
            .OrderBy(car => car.Make)
            .ThenBy(car => car.Model)
            .ToListAsync();
    }

    public Task<List<Car>> GetAvailableAsync()
    {
        return _context.Cars
            .AsNoTracking()
            .Where(car => car.IsAvailable)
            .OrderBy(car => car.Make)
            .ThenBy(car => car.Model)
            .ToListAsync();
    }

    public Task<Car?> GetByIdAsync(int id)
    {
        return _context.Cars.FirstOrDefaultAsync(car => car.Id == id);
    }

    public async Task AddAsync(Car car)
    {
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Car car)
    {
        _context.Cars.Update(car);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Car car)
    {
        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
    }
}
