using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using API.MyRestaurant.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
namespace MyRestaurant.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDetailsAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserDetailsAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllUserDetails()
        {
            var userDetailsList = new List<UserDetailsAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllUserDetails", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    userDetailsList.Add(new UserDetailsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserID = reader["UserID"] as string,
                        FirstName = reader["FirstName"] as string,
                        LastName = reader["LastName"] as string,
                        Address1 = reader["Address1"] as string,
                        Phone = reader["Phone"] as string,
                        Email = reader["Email"] as string,
                        UserTypesID = reader.GetInt32(reader.GetOrdinal("UserTypesID")),
                        RestaurantID = reader.GetInt32(reader.GetOrdinal("RestaurantID")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }

                return userDetailsList.Any() ? Ok(userDetailsList) : NotFound("No user detail records found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllUserDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostUserDetails([FromBody] UserDetailsAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertUserDetails", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", model.UserID);
                command.Parameters.AddWithValue("@FirstName", model.FirstName);
                command.Parameters.AddWithValue("@LastName", model.LastName);
                command.Parameters.AddWithValue("@Address1", model.Address1);
                command.Parameters.AddWithValue("@Phone", model.Phone);
                command.Parameters.AddWithValue("@Email", model.Email);
                command.Parameters.AddWithValue("@UserTypesID", model.UserTypesID);
                command.Parameters.AddWithValue("@RestaurantID", model.RestaurantID);
                command.Parameters.AddWithValue("@IsActive", model.IsActive);

                await command.ExecuteNonQueryAsync();

                return Ok("User details inserted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] PostUserDetails: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetailById(int id)
        {
            UserDetailsAPIViewModel userDetail = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetUserDetailByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userDetail = new UserDetailsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserID = reader["UserID"] as string,
                        FirstName = reader["FirstName"] as string,
                        LastName = reader["LastName"] as string,
                        Address1 = reader["Address1"] as string,
                        Phone = reader["Phone"] as string,
                        Email = reader["Email"] as string,
                        UserTypesID = reader.GetInt32(reader.GetOrdinal("UserTypesID")),
                        RestaurantID = reader.GetInt32(reader.GetOrdinal("RestaurantID")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }

                return userDetail != null ? Ok(userDetail) : NotFound($"No user detail found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetUserDetailById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserDetail(int id, [FromBody] UserDetailsAPIViewModel model)
        {
            if (id != model.ID) return BadRequest("ID mismatch.");
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateUserDetails", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@UserID", model.UserID ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FirstName", model.FirstName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastName", model.LastName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Address1", model.Address1 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UserTypesID", model.UserTypesID);
                command.Parameters.AddWithValue("@RestaurantID", model.RestaurantID);
                command.Parameters.AddWithValue("@IsActive", model.IsActive);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0 ? NoContent() : NotFound("User detail not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateUserDetail: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserDetail(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteUserDetails", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0 
                    ? Ok("UserDetail deleted successfully.") //
                    : NotFound($"No UserDetail found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteUserDetail: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

    }
}
