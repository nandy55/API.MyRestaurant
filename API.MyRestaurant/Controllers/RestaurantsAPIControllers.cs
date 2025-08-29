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
    public class RestaurantsAPIControllers : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RestaurantsAPIControllers(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllRestaurants()
        {
            var restaurantList = new List<RestaurantsAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllRestaurants", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    restaurantList.Add(new RestaurantsAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        RestaurantName = reader["RestaurantName"] as string,
                        Location = reader["Location"] as string,
                        ContactNumber = reader["ContactNumber"] as string,
                        Email = reader["Email"] as string,
                        OpeningTime = reader.IsDBNull(reader.GetOrdinal("OpeningTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("OpeningTime")),
                        ClosingTime = reader.IsDBNull(reader.GetOrdinal("ClosingTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("ClosingTime")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }

                return restaurantList.Any() ? Ok(restaurantList) : NotFound("No restaurant records found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllRestaurants: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> postRestaurant([FromBody] RestaurantsAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertRestaurant", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@RestaurantName", model.RestaurantName);
                command.Parameters.AddWithValue("@Location", model.Location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OpeningTime", model.OpeningTime);
                command.Parameters.AddWithValue("@ClosingTime", model.ClosingTime);
                command.Parameters.AddWithValue("@IsActive", model.IsActive);

                await command.ExecuteNonQueryAsync();
                return Ok("Restaurant created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] InsertRestaurant: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            RestaurantsAPIViewModel restaurant = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetRestaurantByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    restaurant = new RestaurantsAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        RestaurantName = reader["RestaurantName"] as string,
                        Location = reader["Location"] as string,
                        ContactNumber = reader["ContactNumber"] as string,
                        Email = reader["Email"] as string,
                        OpeningTime = reader.IsDBNull(reader.GetOrdinal("OpeningTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("OpeningTime")),
                        ClosingTime = reader.IsDBNull(reader.GetOrdinal("ClosingTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("ClosingTime")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }

                return restaurant != null ? Ok(restaurant) : NotFound($"No restaurant found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetRestaurantById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRestaurant()
        {
            RestaurantsAPIViewModel restaurant = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestRestaurant", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    restaurant = new RestaurantsAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        RestaurantName = reader["RestaurantName"] as string,
                        Location = reader["Location"] as string,
                        ContactNumber = reader["ContactNumber"] as string,
                        Email = reader["Email"] as string,
                        OpeningTime = reader.IsDBNull(reader.GetOrdinal("OpeningTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("OpeningTime")),
                        ClosingTime = reader.IsDBNull(reader.GetOrdinal("ClosingTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("ClosingTime")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }

                return restaurant != null ? Ok(restaurant) : NotFound("No restaurant record found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestRestaurant: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurant(int id, [FromBody] RestaurantsAPIViewModel model)
        {
            if (id != model.Id)
                return BadRequest("ID mismatch.");

            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateRestaurant", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.Id);
                command.Parameters.AddWithValue("@RestaurantName", model.RestaurantName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Location", model.Location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OpeningTime", model.OpeningTime ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ClosingTime", model.ClosingTime ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", model.IsActive);

                var rows = await command.ExecuteNonQueryAsync();
 
                    if (rows > 0)
                    return Ok("Restaurant updated successfully.");
                else
                    return NotFound("Restaurant record not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateRestaurant: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteRestaurant", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0

                     ? Ok("Restaurant deleted successfully.")
                    : NotFound($"No restaurant found with ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteRestaurant: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


    }
}
