using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentRide.Models;

public class RentRideContext(DbContextOptions<RentRideContext> options) : IdentityDbContext<RentRide.Data.ApplicationUser>(options)
{
    public DbSet<Car> Cars => Set<Car>();
}
