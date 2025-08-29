using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using API.MyRestaurant.ViewModels; // Make sure this matches your ViewModel namespace
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace API.MyRestaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTypesAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserTypesAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllUserTypes()
        {
            var userTypeList = new List<UserTypesAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllUserTypes", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    userTypeList.Add(new UserTypesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        Types = reader["Types"] as string
                    });
                }

                return userTypeList.Any() ? Ok(userTypeList) : NotFound("No user types found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllUserTypes: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateUserType([FromBody] UserTypesAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertUserType", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Types", model.Types ?? (object)DBNull.Value);

                var rows = await command.ExecuteNonQueryAsync();

                return rows > 0
                    ? Ok("UserType created successfully.")
                    : BadRequest("Failed to insert user type.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] CreateUserType: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserTypeById(int id)
        {
            UserTypesAPIViewModel userType = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetUserTypeById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userType = new UserTypesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        Types = reader["Types"] as string
                    };
                }

                return userType != null ? Ok(userType) : NotFound($"No user type found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetUserTypeById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestUserType()
        {
            UserTypesAPIViewModel userType = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestUserTypes", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userType = new UserTypesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        Types = reader["Types"] as string
                    };
                }

                return userType != null ? Ok(userType) : NotFound("No user types found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestUserType: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserType(int id, [FromBody] UserTypesAPIViewModel model)
        {
            if (id != model.ID)
                return BadRequest("ID mismatch.");

            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateUserType", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@Types", model.Types ?? (object)DBNull.Value);

                var rows = await command.ExecuteNonQueryAsync();
                if (rows > 0)
                    return Ok("Usertype updated successfully.");
                else
                    return NotFound("Usertype record not found.");

                return rows > 0 ? NoContent() : NotFound("UserType not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateUserType: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserType(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteUserType", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                     ? Ok("UserTypes deleted successfully.")
                    : NotFound($"No UserType found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteUserType: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
