using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using RentRide.Models;

namespace RentRide.Models.Repositories
{
    public class UserRepository :
        IUserRepository,
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>
    {
        public const string ConnectionString =
            @"Data Source=(localdb)\ProjectModels;Initial Catalog=RentalCarDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public User? GetByEmail(string email)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using SqlCommand command = new SqlCommand(
                    @"SELECT UserId, FullName, Email, PasswordHash, Role, CreatedAt
                      FROM Users
                      WHERE Email = @Email",
                    connection);
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = email;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? ReadUser(reader) : null;
                }
            }
        }

        public User? GetByNormalizedEmail(string normalizedEmail)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using SqlCommand command = new SqlCommand(
                    @"SELECT UserId, FullName, Email, PasswordHash, Role, CreatedAt
                      FROM Users
                      WHERE UPPER(Email) = @Email",
                    connection);
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = normalizedEmail;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? ReadUser(reader) : null;
                }
            }
        }

        public User? GetById(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using SqlCommand command = new SqlCommand(
                    @"SELECT UserId, FullName, Email, PasswordHash, Role, CreatedAt
                      FROM Users
                      WHERE UserId = @UserId",
                    connection);
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? ReadUser(reader) : null;
                }
            }
        }

        public int Create(User user)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Users
                                (FullName, Email, PasswordHash, Role)
                                OUTPUT INSERTED.UserId
                                VALUES
                                (@FullName, @Email, @PasswordHash, @Role)";

                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = user.FullName;
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = user.Email;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = user.PasswordHash;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = user.Role;

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool Update(User user)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using SqlCommand command = new SqlCommand(
                    @"UPDATE Users
                      SET FullName = @FullName, Email = @Email, PasswordHash = @PasswordHash, Role = @Role
                      WHERE UserId = @UserId",
                    connection);
                command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = user.FullName;
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = user.Email;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = user.PasswordHash;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = user.Role;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = user.UserId;
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        public bool Delete(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using SqlCommand command = new SqlCommand(
                    "DELETE FROM Users WHERE UserId = @UserId",
                    connection);
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        public bool UpdateRole(int userId, string role)
        {
            if (role is not ("Admin" or "Member"))
            {
                throw new ArgumentException("Role must be Admin or Member.", nameof(role));
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using SqlCommand command = new SqlCommand(
                    "UPDATE Users SET Role = @Role WHERE UserId = @UserId",
                    connection);
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        public int GetUserCount()
        {
            return ExecuteCount("SELECT COUNT(*) FROM Users");
        }

        public int GetMemberCount()
        {
            return ExecuteCount("SELECT COUNT(*) FROM Users WHERE Role = N'Member'");
        }

        public IReadOnlyList<User> GetRecentUsers(int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
            }

            var users = new List<User>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using SqlCommand command = new SqlCommand(
                    @"SELECT TOP (@Count) UserId, FullName, Email, Role, CreatedAt
                      FROM Users
                      ORDER BY CreatedAt DESC, UserId DESC",
                    connection);
                command.Parameters.Add("@Count", SqlDbType.Int).Value = count;
                connection.Open();

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Role = reader.GetString(reader.GetOrdinal("Role")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
            }

            return users;
        }

        private int ExecuteCount(string query)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static User ReadUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        public void Dispose()
        {
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.CreateAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(user.FullName) ||
                user.FullName.Length > 100 ||
                string.IsNullOrWhiteSpace(user.Email) ||
                user.Email.Length > 150 ||
                string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidUser",
                    Description = "A name, email, and password are required."
                }));
            }

            try
            {
                user.Role = "Member";
                user.Id = Create(ToUser(user)).ToString();
                return Task.FromResult(IdentityResult.Success);
            }
            catch (SqlException exception) when (exception.Number is 2601 or 2627)
            {
                return Task.FromResult(DuplicateEmail());
            }
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.UpdateAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!int.TryParse(user.Id, out int userId))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidUserId",
                    Description = "The user ID is invalid."
                }));
            }

            try
            {
                return Task.FromResult(Update(ToUser(user, userId))
                    ? IdentityResult.Success
                    : IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = "The user could not be found."
                    }));
            }
            catch (SqlException exception) when (exception.Number is 2601 or 2627)
            {
                return Task.FromResult(DuplicateEmail());
            }
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.DeleteAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                int.TryParse(user.Id, out int userId) && Delete(userId)
                    ? IdentityResult.Success
                    : IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = "The user could not be found."
                    }));
        }

        Task<string> IUserStore<ApplicationUser>.GetUserIdAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.Id);
        }

        Task<string?> IUserStore<ApplicationUser>.GetUserNameAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.UserName);
        }

        Task IUserStore<ApplicationUser>.SetUserNameAsync(
            ApplicationUser user,
            string? userName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.UserName = userName;
            user.Email = userName;
            return Task.CompletedTask;
        }

        Task<string?> IUserStore<ApplicationUser>.GetNormalizedUserNameAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.NormalizedUserName);
        }

        Task IUserStore<ApplicationUser>.SetNormalizedUserNameAsync(
            ApplicationUser user,
            string? normalizedName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        Task<ApplicationUser?> IUserStore<ApplicationUser>.FindByIdAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                int.TryParse(userId, out int id)
                    ? ToApplicationUser(GetById(id))
                    : null);
        }

        Task<ApplicationUser?> IUserStore<ApplicationUser>.FindByNameAsync(
            string normalizedUserName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(ToApplicationUser(GetByNormalizedEmail(normalizedUserName)));
        }

        Task IUserPasswordStore<ApplicationUser>.SetPasswordHashAsync(
            ApplicationUser user,
            string? passwordHash,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        Task<string?> IUserPasswordStore<ApplicationUser>.GetPasswordHashAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.PasswordHash);
        }

        Task<bool> IUserPasswordStore<ApplicationUser>.HasPasswordAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        Task IUserEmailStore<ApplicationUser>.SetEmailAsync(
            ApplicationUser user,
            string? email,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.Email = email;
            user.UserName = email;
            return Task.CompletedTask;
        }

        Task<string?> IUserEmailStore<ApplicationUser>.GetEmailAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.Email);
        }

        Task<bool> IUserEmailStore<ApplicationUser>.GetEmailConfirmedAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.EmailConfirmed);
        }

        Task IUserEmailStore<ApplicationUser>.SetEmailConfirmedAsync(
            ApplicationUser user,
            bool confirmed,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        Task<ApplicationUser?> IUserEmailStore<ApplicationUser>.FindByEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(ToApplicationUser(GetByNormalizedEmail(normalizedEmail)));
        }

        Task<string?> IUserEmailStore<ApplicationUser>.GetNormalizedEmailAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.NormalizedEmail);
        }

        Task IUserEmailStore<ApplicationUser>.SetNormalizedEmailAsync(
            ApplicationUser user,
            string? normalizedEmail,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        private static IdentityResult DuplicateEmail()
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateEmail",
                Description = "An account with this email already exists."
            });
        }

        private static User ToUser(ApplicationUser user, int userId = 0)
        {
            return new User
            {
                UserId = userId,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PasswordHash = user.PasswordHash ?? string.Empty,
                Role = user.Role
            };
        }

        private static ApplicationUser? ToApplicationUser(User? user)
        {
            if (user is null)
            {
                return null;
            }

            return new ApplicationUser
            {
                Id = user.UserId.ToString(),
                FullName = user.FullName,
                UserName = user.Email,
                NormalizedUserName = user.Email.ToUpperInvariant(),
                Email = user.Email,
                NormalizedEmail = user.Email.ToUpperInvariant(),
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }

    public sealed class RentRideClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public RentRideClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var nameClaimType = Options.ClaimsIdentity.UserNameClaimType;
            var userNameClaim = identity.FindFirst(nameClaimType);
            if (userNameClaim is not null)
            {
                identity.RemoveClaim(userNameClaim);
            }

            identity.AddClaim(new Claim(nameClaimType, user.FullName));
            identity.AddClaim(new Claim(Options.ClaimsIdentity.RoleClaimType, user.Role));
            return identity;
        }
    }
}
