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
    public class OrderItemsAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrderItemsAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllOrderItems()
        {
            var orderItems = new List<OrderItemsAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllOrderItems", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orderItems.Add(new OrderItemsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        MenuItemID = reader.GetInt32(reader.GetOrdinal("MenuItemID")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        ItemPrice = reader.GetDecimal(reader.GetOrdinal("ItemPrice"))
                    });
                }

                return orderItems.Any() ? Ok(orderItems) : NotFound("No order items found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllOrderItems: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostOrderItem([FromBody] OrderItemsAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertOrderItem", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@OrderID", model.OrderID);
                command.Parameters.AddWithValue("@MenuItemID", model.MenuItemID);
                command.Parameters.AddWithValue("@Quantity", model.Quantity);
                command.Parameters.AddWithValue("@ItemPrice", model.ItemPrice);

                await command.ExecuteNonQueryAsync();

                return Ok("Orderitem created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] PostOrderItem: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderItemById(int id)
        {
            OrderItemsAPIViewModel orderItem = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetOrderItemByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    orderItem = new OrderItemsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        MenuItemID = reader.GetInt32(reader.GetOrdinal("MenuItemID")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        ItemPrice = reader.GetDecimal(reader.GetOrdinal("ItemPrice"))
                    };
                }

                return orderItem != null ? Ok(orderItem) : NotFound($"No order item found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetOrderItemById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("latest/{orderId}")]
        public async Task<IActionResult> GetLatestOrderItemByOrderId(int orderId)
        {
            OrderItemsAPIViewModel orderItem = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestOrderItemID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@OrderID", SqlDbType.Int) { Value = orderId });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    orderItem = new OrderItemsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        MenuItemID = reader.GetInt32(reader.GetOrdinal("MenuItemID")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        ItemPrice = reader.GetDecimal(reader.GetOrdinal("ItemPrice"))
                    };
                }

                return orderItem != null ? Ok(orderItem) : NotFound($"No latest order item found for Order ID: {orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestOrderItemByOrderId: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderItem(int id, [FromBody] OrderItemsAPIViewModel model)
        {
            if (id != model.ID)
                return BadRequest("ID mismatch.");

            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateOrderItem", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@OrderID", model.OrderID);
                command.Parameters.AddWithValue("@MenuItemID", model.MenuItemID);
                command.Parameters.AddWithValue("@Quantity", model.Quantity);
                command.Parameters.AddWithValue("@ItemPrice", model.ItemPrice);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                       ? Ok("OrderItems updated successfully.")
                    : NotFound("Order item not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateOrderItem: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteOrderItem", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                   ? Ok("OrderItem deleted successfully.")
                    : NotFound($"No order item found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteOrderItem: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


    }
}
