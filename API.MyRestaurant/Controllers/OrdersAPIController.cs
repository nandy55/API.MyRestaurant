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
    public class OrdersAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrdersAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = new List<OrdersAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllOrders", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    orders.Add(new OrdersAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        TableID = reader.GetInt32(reader.GetOrdinal("TableID")),
                        StaffID = reader.GetInt32(reader.GetOrdinal("StaffID")),
                        OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                        Status = reader["Status"]?.ToString()
                    });
                }

                return orders.Any() ? Ok(orders) : NotFound("No order records found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllOrders: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> postOrders([FromBody] OrdersAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertOrders", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@CustomerID", model.CustomerID);
                command.Parameters.AddWithValue("@TableID", model.TableID);
                command.Parameters.AddWithValue("@StaffID", model.StaffID);
                command.Parameters.AddWithValue("@Status", model.Status ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return Ok("Order created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] InsertOrder: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            OrdersAPIViewModel order = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetOrderByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    order = new OrdersAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        TableID = reader.GetInt32(reader.GetOrdinal("TableID")),
                        StaffID = reader.GetInt32(reader.GetOrdinal("StaffID")),
                        OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                        Status = reader["Status"]?.ToString()
                    };
                }

                return order != null ? Ok(order) : NotFound($"No order found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetOrderById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("latest/{customerId}")]
        public async Task<IActionResult> GetLatestOrderByCustomerId(int customerId)
        {
            OrdersAPIViewModel order = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestOrderByCustomerID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@CustomerID", SqlDbType.Int) { Value = customerId });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    order = new OrdersAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        TableID = reader.GetInt32(reader.GetOrdinal("TableID")),
                        StaffID = reader.GetInt32(reader.GetOrdinal("StaffID")),
                        OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                        Status = reader["Status"]?.ToString()
                    };
                }

                return order != null ? Ok(order) : NotFound($"No order found for CustomerID: {customerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestOrderByCustomerId: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrdersAPIViewModel model)
        {
            if (id != model.ID) return BadRequest("ID mismatch.");
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateOrder", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@CustomerID", model.CustomerID);
                command.Parameters.AddWithValue("@TableID", model.TableID);
                command.Parameters.AddWithValue("@StaffID", model.StaffID);
              
                command.Parameters.AddWithValue("@Status", model.Status ?? (object)DBNull.Value);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                    ? Ok("Orders  updated successfully.")
                : NotFound("Order record not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateOrder: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteOrder", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                 ? Ok("Order deleted successfully.")
                : NotFound($"No order record found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteOrder: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


    }
}
