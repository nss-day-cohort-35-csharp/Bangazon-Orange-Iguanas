using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : Controller
    {
        private readonly IConfiguration _config;
        public PaymentTypeController(IConfiguration config)
        {
            _config = config;
        }
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaymentTypes()
        {
            var paymentTypes = await getPaymentTypes();
            return Ok(paymentTypes);

        }

        private async Task<List<PaymentType>> getPaymentTypes()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], Active
                        FROM PaymentType
                        WHERE Active = 1";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    var paymentTypes = new List<PaymentType>();

                    while (reader.Read())
                    {
                        paymentTypes.Add(new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("Active"))
                        });
                    }

                    reader.Close();
                    return paymentTypes;
                }
            }
        }

        [HttpGet("{id}", Name="GetPaymentType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Active
                                        FROM PaymentType 
                                        WHERE Active = 1 AND Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    PaymentType paymentType = null;
                    while (reader.Read())
                    {
                        if (paymentType == null)
                        {
                            paymentType = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("Active"))

                            };
                        }

                    }
                    reader.Close();
                    return Ok(paymentType);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentType paymentType)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO PaymentType ([Name])
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", paymentType.Name));

                    var newId = (int)await cmd.ExecuteScalarAsync();
                    //paymentType.IsActive = true;
                    paymentType.Id = newId;
                    return CreatedAtRoute("GetPaymentType", new { id = newId }, paymentType);
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentType([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType 
                                        SET Active = @false
                                        WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@false", false));




                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool PaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Active, Name FROM PaymentType WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
