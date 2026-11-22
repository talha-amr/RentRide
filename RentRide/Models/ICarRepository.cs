namespace RentRide.Models;

public interface ICarRepository
{
    Task<List<Car>> GetAllAsync();

    Task<List<Car>> GetAvailableAsync();

    Task<Car?> GetByIdAsync(int id);

    Task AddAsync(Car car);

    Task UpdateAsync(Car car);

    Task DeleteAsync(Car car);
}
