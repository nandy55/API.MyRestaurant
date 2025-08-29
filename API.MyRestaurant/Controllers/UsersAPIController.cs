using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using API.MyRestaurant.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace API.MyRestaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersAPIController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string GetConnectionString()
        {
            var connectionString = _configuration.GetConnectionString("Restaurants");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string Restaurants is not configured.");
            return connectionString;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var usersList = new List<UsersAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllUsers", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    usersList.Add(new UsersAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        PasswordHash = reader["PasswordHash"]?.ToString()

                    });
                }

                return usersList.Any() ? Ok(usersList) : NotFound("No users records found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllUsers: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UsersAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", model.UserId);
                command.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PasswordHash", model.PasswordHash ?? (object)DBNull.Value);

                var rows = await command.ExecuteNonQueryAsync();

                return rows > 0
                    ? Ok("User created successfully.")
                    : BadRequest("Failed to insert user.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] CreateUser: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            UsersAPIViewModel user = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetUserById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    user = new UsersAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        PasswordHash = reader["PasswordHash"]?.ToString()
                    };
                }

                return user != null ? Ok(user) : NotFound($"No user found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetUserById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("latestuser/{userId}")]
        public async Task<IActionResult> GetLatestUsersId(int userId)
        {
            UsersAPIViewModel user = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestUsersId", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    user = new UsersAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        PasswordHash = reader["PasswordHash"]?.ToString()
                    };
                }

                return user != null ? Ok(user) : NotFound($"No user found for UserID: {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestUserByUserId: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UsersAPIViewModel model)
        {
            if (id != model.Id) return BadRequest("ID mismatch.");
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateUsers", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.Id);
                command.Parameters.AddWithValue("@UserId", model.UserId);
                command.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PasswordHash", model.PasswordHash ?? (object)DBNull.Value);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                     ? Ok("Useers Updated successfully.")
                    : NotFound("User record not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateUser: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                     ? Ok("Users deleted successfully.")
                    : NotFound($"No user found with ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteUser: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


    }
}
