using Microsoft.AspNetCore.Authorization;
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
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("API is alive ✅");
        }

        [Authorize]
        [HttpGet("secure")]
        public IActionResult SecureEndpoint()
        {
            return Ok("You hit a secure endpoint 🔐");
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantTablesAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RestaurantTablesAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllRestaurantTables()
        {
            var tableList = new List<RestaurantTablesAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllRestaurantTables", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tableList.Add(new RestaurantTablesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        TableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber")),
                        Capacity = reader.GetInt32(reader.GetOrdinal("Capacity")),
                        Status = reader["Status"]?.ToString()
                    });
                }

                return tableList.Any() ? Ok(tableList) : NotFound("No restaurant tables found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllRestaurantTables: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostRestaurantTable([FromBody] RestaurantTablesAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertRestaurantTables", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@TableNumber", model.TableNumber);
                command.Parameters.AddWithValue("@Capacity", model.Capacity);
                command.Parameters.AddWithValue("@Status", model.Status ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();

                return Ok("Restaurant table added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] PostRestaurantTable: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurantTableById(int id)
        {
            RestaurantTablesAPIViewModel table = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetRestaurantTableByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    table = new RestaurantTablesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        TableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber")),
                        Capacity = reader.GetInt32(reader.GetOrdinal("Capacity")),
                        Status = reader["Status"]?.ToString()
                    };
                }

                return table != null ? Ok(table) : NotFound($"No restaurant table found with ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetRestaurantTableById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRestaurantTable()
        {
            RestaurantTablesAPIViewModel table = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestRestaurantTableID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    table = new RestaurantTablesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        TableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber")),
                        Capacity = reader.GetInt32(reader.GetOrdinal("Capacity")),
                        Status = reader["Status"]?.ToString()
                    };
                }

                return table != null ? Ok(table) : NotFound("No table found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestRestaurantTable: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurantTable(int id, [FromBody] RestaurantTablesAPIViewModel model)
        {
            if (id != model.ID)
                return BadRequest("ID mismatch.");

            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_UpdateRestaurantTable", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@TableNumber", model.TableNumber);
                command.Parameters.AddWithValue("@Capacity", model.Capacity);
                command.Parameters.AddWithValue("@Status", model.Status ?? (object)DBNull.Value);

                await connection.OpenAsync();
                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                    ? Ok("Restaurant table updated successfully.")
                    : NotFound("Table record not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateRestaurantTable: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurantTable(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteRestaurantTable", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                    ? Ok("Restaurant table deleted successfully.")
                    : NotFound($"No restaurant table found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteRestaurantTable: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
