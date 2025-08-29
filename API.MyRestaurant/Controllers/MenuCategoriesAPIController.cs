using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using API.MyRestaurant.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace API.MyRestaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuCategoriesAPIController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;

        private string GetConnectionString()
        {
            var connectionString = _configuration.GetConnectionString("Restaurants");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string 'Restaurants' is not configured.");
            return connectionString;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMenuCategories()
        {
            var categories = new List<MenuCategoriesAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllMenuCategories", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    categories.Add(new MenuCategoriesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        CategoryName = reader["CategoryName"] as string
                    });
                }

                return categories.Count > 0 ? Ok(categories) : NotFound("No menu categories found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllMenuCategories: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        
        public async Task<IActionResult> PostMenuCategory([FromBody] MenuCategoriesAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertMenuCategory", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@CategoryName", model.CategoryName);

                await command.ExecuteNonQueryAsync();
                return Ok("Menu Category Created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] PostMenuCategory: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuCategoryById(int id)
        {
            MenuCategoriesAPIViewModel category = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetMenuCategoryByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    category = new MenuCategoriesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName"))
                    ? null : reader.GetString(reader.GetOrdinal("CategoryName"))
                    };
                }

                return category != null ? Ok(category) : NotFound($"No menu category found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetMenuCategoryById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("latestid")]
        public async Task<IActionResult> GetLatestMenuCategory()
        {
            MenuCategoriesAPIViewModel category = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestMenuCategoryID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    category = new MenuCategoriesAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        CategoryName = reader["CategoryName"] as string
                    };

                }

                return category != null ? Ok(category) : NotFound("No menu category found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestMenuCategory: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuCategory(int id, [FromBody] MenuCategoriesAPIViewModel model)
        {
            if (id != model.ID)
                return BadRequest("ID mismatch.");

            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateMenuCategory", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@CategoryName", model.CategoryName ?? (object)DBNull.Value);

                var rows = await command.ExecuteNonQueryAsync();
                return(rows > 0)
                  ? Ok("MenuCategories updated successfully.")
                
                  : NotFound("Customer record not found.");
            
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateMenuCategory: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuCategory(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteMenuCategory", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0 
                     ? Ok("Menucategories deleted successfully.")
                    : NotFound($"No menu category found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteMenuCategory: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
