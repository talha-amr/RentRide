using Microsoft.Data.SqlClient;
using System.Data;

namespace RentRide.Models.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString =
            @"Data Source=(localdb)\ProjectModels;Initial Catalog=RentalCarDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public User? GetByEmail(string email)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
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

        public User? GetById(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
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

        public void Create(User user)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Users
                                (FullName, Email, PasswordHash, Role)
                                VALUES
                                (@FullName, @Email, @PasswordHash, @Role)";

                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = user.FullName;
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = user.Email;
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = user.PasswordHash;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = "Member";

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool UpdateRole(int userId, string role)
        {
            if (role is not ("Admin" or "Member"))
            {
                throw new ArgumentException("Role must be Admin or Member.", nameof(role));
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
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
            using (SqlConnection connection = new SqlConnection(_connectionString))
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
            using (SqlConnection connection = new SqlConnection(_connectionString))
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
    }
}
