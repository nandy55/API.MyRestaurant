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
    public class PaymentsAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PaymentsAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllPayments()
        {
            var paymentList = new List<PaymentsAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllPayments", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    paymentList.Add(new PaymentsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        PaymentType = reader["PaymentType"] as string,
                        PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate"))
                    });
                }

                return paymentList.Any() ? Ok(paymentList) : NotFound("No payment records found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllPayments: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostPayment([FromBody] PaymentsAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertPayment", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@OrderID", model.OrderID);
                command.Parameters.AddWithValue("@CustomerID", model.CustomerID);
                command.Parameters.AddWithValue("@Amount", model.Amount);
                command.Parameters.AddWithValue("@PaymentType", model.PaymentType);

                await command.ExecuteNonQueryAsync();

                return Ok("Payment recorded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] PostPayment: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            PaymentsAPIViewModel payment = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetPaymentByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    payment = new PaymentsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        PaymentType = reader["PaymentType"] as string,
                        PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate"))
                    };
                }

                return payment != null ? Ok(payment) : NotFound($"No payment found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetPaymentById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestPayment()
        {
            PaymentsAPIViewModel payment = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetLatestPaymentID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    payment = new PaymentsAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        PaymentType = reader["PaymentType"]?.ToString(),
                        PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate"))
                    };
                }

                return payment != null ? Ok(payment) : NotFound("No payments found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestPayment: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentsAPIViewModel model)
        {
            if (id != model.ID) return BadRequest("ID mismatch.");

            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdatePayment", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@OrderID", model.OrderID);
                command.Parameters.AddWithValue("@CustomerID", model.CustomerID);
                command.Parameters.AddWithValue("@Amount", model.Amount);
                command.Parameters.AddWithValue("@PaymentType", model.PaymentType ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PaymentDate", model.PaymentDate);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                   ? Ok("payments updated successfully.")
                    : NotFound("Payment record not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdatePayment: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeletePayment", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                   ? Ok("payments Deleted successfully.")
                    : NotFound($"No payment record found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeletePayment: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

    }
}
