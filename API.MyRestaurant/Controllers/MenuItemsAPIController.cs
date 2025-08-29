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
    public class MenuItemsAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MenuItemsAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllMenuItems()
        {
            var menuItems = new List<MenuItemsAPIViewModel>();
            var connectionString = GetConnectionString(); // your method

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllMenuItems", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    menuItems.Add(new MenuItemsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        Name = reader["Name"]?.ToString(),
                        Description = reader["Description"]?.ToString(),
                        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                        CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                        IsAvailable = reader.GetBoolean(reader.GetOrdinal("IsAvailable"))
                    });
                }

                return menuItems.Any() ? Ok(menuItems) : NotFound("No menu items found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllMenuItems: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostMenuItem([FromBody] MenuItemsAPIViewModel model)
        {
            var connectionString = GetConnectionString(); // Replace with your actual method

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertMenuItem", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Price", model.Price);
                command.Parameters.AddWithValue("@CategoryID", model.CategoryID);
                command.Parameters.AddWithValue("@IsAvailable", model.IsAvailable);

                await command.ExecuteNonQueryAsync();

                return Ok("Menu item inserted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in PostMenuItem: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuItemById(int id)
        {
            MenuItemsAPIViewModel menuItem = null;
            var connectionString = GetConnectionString(); // Your DB connection method

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetMenuItemById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    menuItem = new MenuItemsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        Name = reader["Name"]?.ToString(),
                        Description = reader["Description"]?.ToString(),
                        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                        CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                        IsAvailable = reader.GetBoolean(reader.GetOrdinal("IsAvailable"))
                    };
                }

                return menuItem != null ? Ok(menuItem) : NotFound($"No menu item found with ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetMenuItemById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestMenuItem()
        {
            MenuItemsAPIViewModel menuItem = null;
            var connectionString = GetConnectionString(); // Your connection fetch method

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestMenuItems", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    menuItem = new MenuItemsAPIViewModel
                    {

                        ID = Convert.ToInt32(reader["ID"]),
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString(),
                        Price = Convert.ToDecimal(reader["Price"]),
                        CategoryID = Convert.ToInt32(reader["CategoryID"]),
                        IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                    };
                }

                return menuItem != null ? Ok(menuItem) : NotFound("No menu items found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestMenuItem: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItemsAPIViewModel model)
        {
            if (id != model.ID) return BadRequest("ID mismatch.");
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateMenuItem", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Price", model.Price);
                command.Parameters.AddWithValue("@CategoryID", model.CategoryID);
                command.Parameters.AddWithValue("@IsAvailable", model.IsAvailable);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                      ? Ok("MenuItems updated successfully.")
                : NotFound("Menu item not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateMenuItem: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteMenuItem", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                     ? Ok("MenuItem deleted successfully.")
                    : NotFound($"No menu item found with ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteMenuItem: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


    }
}
