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
    public class StaffsAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StaffsAPIController(IConfiguration configuration)
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
        public async Task<IActionResult> GetAllStaff()
        {
            var staffList = new List<StaffsAPIViewModel>();
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("sp_GetAllStaff", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    staffList.Add(new StaffsAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserDetailsID = reader.GetInt32(reader.GetOrdinal("UserDetailsID")),
                        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                        HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate"))
                    });
                }

                return staffList.Any() ? Ok(staffList) : NotFound("No staff records found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetAllStaff: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostStaff([FromBody] StaffsAPIViewModel model)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertStaffs", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserDetailsID", model.UserDetailsID);
                command.Parameters.AddWithValue("@Salary", model.Salary);
                command.Parameters.AddWithValue("@HireDate", (object?)model.HireDate ?? DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return Ok("Staff created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] InsertStaff: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            StaffsAPIViewModel staff = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetStaffByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    staff = new StaffsAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserDetailsID = reader.GetInt32(reader.GetOrdinal("UserDetailsID")),
                        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                        HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate"))
                    };
                }

                return staff != null ? Ok(staff) : NotFound($"No staff record found for ID: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetStaffById: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestStaff()
        {
            StaffsAPIViewModel? staff = null;
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                int usersid;
                using (var getIdCommand = new SqlCommand("sp_GetLatestStaffID", connection))
                {
                    getIdCommand.CommandType = CommandType.StoredProcedure;
                    var result = await getIdCommand.ExecuteScalarAsync();
                    if (result == null)
                        return NotFound("No staff records found.");

                    usersid = Convert.ToInt32(result);
                }

                using var getStaffCommand = new SqlCommand("sp_GetStaffByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                getStaffCommand.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = usersid });

                using var reader = await getStaffCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    staff = new StaffsAPIViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        UserDetailsID = reader.GetInt32(reader.GetOrdinal("UserDetailsID")),
                        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                        HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                  
                    };
                }

                return staff != null ? Ok(staff) : NotFound("Latest staff record not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] GetLatestStaff: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] StaffsAPIViewModel model)
        {
            if (id != model.Id)
                return BadRequest("ID mismatch.");

            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateStaff", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Id", model.Id);
                command.Parameters.AddWithValue("@UserDetailsID", model.UserDetailsID);
                command.Parameters.AddWithValue("@Salary", model.Salary);
                command.Parameters.AddWithValue("@HireDate", model.HireDate);


                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                      ? Ok("Staff updated successfully.")
                    : NotFound("Staff record not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] UpdateStaff: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var connectionString = GetConnectionString();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteStaff", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0
                      ? Ok("staff deleted successfully.")
                    : NotFound($"No staff record found for ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[❌ ERROR] DeleteStaff: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


    }
}
