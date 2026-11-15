using Microsoft.AspNetCore.Identity;
using RentRide.Models;
using RentRide.Models.Repositories;

namespace RentRide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<IUserRepository>(services =>
                services.GetRequiredService<UserRepository>());
            builder.Services.AddScoped<IUserStore<ApplicationUser>>(services =>
                services.GetRequiredService<UserRepository>());

            builder.Services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddSignInManager();

            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, RentRideClaimsPrincipalFactory>();

            builder.Services
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ApplicationScheme, options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.Name = "RentRide.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

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

            app.Run();
        }
    }
}