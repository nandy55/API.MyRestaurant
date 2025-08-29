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
    public class CustomersAPIController : ControllerBase 
    {
        private readonly IConfiguration _configuration;

        public CustomersAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllCustomers()
        {
            var customerList = new List<CustomersAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllCustomers", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    customerList.Add(new CustomersAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        FirstName = reader["FirstName"] as string,
                        LastName = reader["LastName"] as string,
                        Phone = reader["Phone"] as string,
                        Email = reader["Email"] as string,
                        Address = reader["Address"] as string,
                        LoyaltyPoints = reader.IsDBNull(reader.GetOrdinal("LoyaltyPoints")) ? 0 : reader.GetInt32(reader.GetOrdinal("LoyaltyPoints")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }

                return customerList.Any() ? Ok(customerList) : NotFound("No customer records found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllCustomers: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> postCustomer([FromBody] CustomersAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertCustomer", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@FirstName", model.FirstName);
                command.Parameters.AddWithValue("@LastName", model.LastName);
                command.Parameters.AddWithValue("@Phone", model.Phone);
                command.Parameters.AddWithValue("@Email", model.Email);
                command.Parameters.AddWithValue("@Address", model.Address);
                command.Parameters.AddWithValue("@LoyaltyPoints", model.LoyaltyPoints);
                command.Parameters.AddWithValue("@IsActive", model.IsActive);

                await command.ExecuteNonQueryAsync();
                return Ok("Customer Created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] InsertCustomer: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
            [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            CustomersAPIViewModel customer = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetCustomerByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    customer = new CustomersAPIViewModel
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        FirstName = reader["FirstName"] as string,
                        LastName = reader["LastName"] as string,
                        Phone = reader["Phone"] as string,
                        Email = reader["Email"] as string,
                        Address = reader["Address"] as string,
                        LoyaltyPoints = reader.IsDBNull(reader.GetOrdinal("LoyaltyPoints")) ? 0 : reader.GetInt32(reader.GetOrdinal("LoyaltyPoints")),
                        IsActive = reader.IsDBNull(reader.GetOrdinal("IsActive")) ? false : reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }

                return customer != null ? Ok(customer) : NotFound($"No customer found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetCustomerById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("latestid")]
        public async Task<IActionResult> GetLatestCustomerID()
        {
            int? latestCustomerId = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetCustomerLatestID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    latestCustomerId = reader.GetInt32(reader.GetOrdinal("ID"));
                }

                return latestCustomerId.HasValue ? Ok(latestCustomerId) : NotFound("No customers found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestCustomerID: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomersAPIViewModel model)
        {
          
            if (id != model.ID)
                return BadRequest("ID mismatch.");


            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateCustomer", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ID", model.ID);
                command.Parameters.AddWithValue("@FirstName", model.FirstName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastName", model.LastName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Address", model.Address ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LoyaltyPoints", model.LoyaltyPoints);
                command.Parameters.AddWithValue("@IsActive", model.IsActive);

                var rows = await command.ExecuteNonQueryAsync();

                if (rows > 0)
                    return Ok("Customer updated successfully.");
                else
                    return NotFound("Customer record not found.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateCustomer: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteCustomer", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                    
                     ? Ok("Customer deleted successfully.")
                    : NotFound($"No customer found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteCustomer: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


    }
}



