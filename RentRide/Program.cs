using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentRide.Data;
using RentRide.Models;
using RentRide.Models.Repositories;

namespace RentRide
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("RentRideContext") ?? throw new InvalidOperationException("Connection string 'RentRideContext' not found.");

            builder.Services.AddDbContext<RentRideContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<RentRideContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<ICarRepository, CarRepository>();

            builder.Services.AddAuthorization();

            var app = builder.Build();

            await using (var scope = app.Services.CreateAsyncScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                foreach (var roleName in new[] { "Admin", "Member" })
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (!result.Succeeded)
                        {
                            throw new InvalidOperationException($"Unable to create the '{roleName}' role: {string.Join(", ", result.Errors.Select(error => error.Description))}");
                        }
                    }
                }

                if (app.Environment.IsDevelopment())
                {
                    await SeedDevelopmentUsersAsync(
                        scope.ServiceProvider,
                        app.Configuration,
                        app.Logger);
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }

        private static async Task SeedDevelopmentUsersAsync(
            IServiceProvider services,
            IConfiguration configuration,
            ILogger logger)
        {
            var password = configuration["SeedUsers:Password"];
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Development account seeding was skipped because SeedUsers:Password is not configured in user secrets.");
                return;
            }

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var seedUsers = new[]
            {
                (Email: "admin.rentride@gmail.com", PreviousEmail: "rentride.admin.demo@gmail.com", FullName: "RentRide Admin", Role: "Admin"),
                (Email: "member1.rentride@gmail.com", PreviousEmail: "rentride.member1.demo@gmail.com", FullName: "RentRide Member One", Role: "Member"),
                (Email: "member2.rentride@gmail.com", PreviousEmail: "rentride.member2.demo@gmail.com", FullName: "RentRide Member Two", Role: "Member"),
                (Email: "member3.rentride@gmail.com", PreviousEmail: "rentride.member3.demo@gmail.com", FullName: "RentRide Member Three", Role: "Member")
            };

            foreach (var seed in seedUsers)
            {
                var user = await userManager.FindByEmailAsync(seed.Email)
                    ?? await userManager.FindByEmailAsync(seed.PreviousEmail);
                var isNewUser = user is null;

                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        UserName = seed.Email,
                        Email = seed.Email,
                        EmailConfirmed = true,
                        FullName = seed.FullName,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await userManager.CreateAsync(user, password);
                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Unable to seed development account '{seed.Email}': {string.Join(", ", createResult.Errors.Select(error => error.Description))}");
                    }
                }
                else
                {
                    var previousEmail = user.Email;
                    user.Email = seed.Email;
                    user.UserName = seed.Email;
                    user.EmailConfirmed = true;
                    user.FullName = seed.FullName;
                    var updateResult = await userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Unable to update development account '{previousEmail}' to '{seed.Email}': {string.Join(", ", updateResult.Errors.Select(error => error.Description))}");
                    }

                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    var resetResult = await userManager.ResetPasswordAsync(user, resetToken, password);
                    if (!resetResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Unable to reset the development password for '{seed.Email}': {string.Join(", ", resetResult.Errors.Select(error => error.Description))}");
                    }
                }

                if (!await userManager.IsInRoleAsync(user, seed.Role))
                {
                    var roleResult = await userManager.AddToRoleAsync(user, seed.Role);
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Unable to assign role '{seed.Role}' to development account '{seed.Email}': {string.Join(", ", roleResult.Errors.Select(error => error.Description))}");
                    }
                }

                logger.LogInformation("{Action} development account {Email} with role {Role}.",
                    isNewUser ? "Created" : "Updated", seed.Email, seed.Role);
            }
        }
    }
}
