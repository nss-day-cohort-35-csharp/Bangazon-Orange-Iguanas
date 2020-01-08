using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BangazonAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPaymentTypesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UserPaymentTypesController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery] int? customerId)
        {
            if (!String.IsNullOrWhiteSpace(customerId.ToString()))
            {



                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @" SELECT Id,  AcctNumber, Active, CustomerId, PaymentTypeId From UserPaymentType Where CustomerId = @customerId And Active = 1 ";
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                        SqlDataReader reader = cmd.ExecuteReader();

                        UserPaymentType userPaymentType = null;
                        List<UserPaymentType> userpayments = new List<UserPaymentType>();

                        while (reader.Read())
                        {
                            int currentCustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                            //UserPaymentType newUserPaymentType = userpayments.FirstOrDefau(i => i.Id == currentCustomerId);

                            if (isUserPaymentType(reader.GetInt32(reader.GetOrdinal("Id"))))
                            {
                                userPaymentType = new UserPaymentType
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    CustomerId = currentCustomerId,
                                    PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                                    AcctNumber = reader.GetString(reader.GetOrdinal("AcctNumber")),
                                    Active = reader.GetBoolean(reader.GetOrdinal("Active")),


                                };
                                userpayments.Add(userPaymentType);
                            }




                            if (userPaymentType == null)
                            {
                                return NotFound($"No user payment type found with id of {customerId} ");
                            };

                        }
                        reader.Close();
                        return Ok(userpayments);
                    }
                }


            }
            return Ok();
        }




        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserPaymentType userPaymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" INSERT INTO UserPaymentType (AcctNumber, Active, CustomerId, PaymentTypeId)
                                      OUTPUT INSERTED.Id VALUES (@acctNumber, @active, @customerId, @paymentTypeId)";
                    cmd.Parameters.Add(new SqlParameter("@acctNumber", userPaymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@active", userPaymentType.Active));
                    cmd.Parameters.Add(new SqlParameter("@customerId", userPaymentType.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@paymentTypeId", userPaymentType.PaymentTypeId));

                    int newId = (int)cmd.ExecuteScalar();
                    userPaymentType.Id = newId;

                    var Value = CreatedAtAction("GetUserPayment", new { id = newId }, userPaymentType);
                    return Value;
                }

            }
        }

        [HttpPut("{id}")]

        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] UserPaymentType userPaymenType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE UserPaymentType 
                                             SET AcctNumber = @acctNumber,
                                                 Active = @active WHERE id = @id";
                        //Im unsure whether I should include payment typeId in the edit.... SO frustrating 
                        cmd.Parameters.Add(new SqlParameter("@acctNumber", userPaymenType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@active", userPaymenType.Active));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int numberOfRowsEffected = cmd.ExecuteNonQuery();
                        if (numberOfRowsEffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!isUserPaymentType(id))
                {
                    return NotFound();
                }
                else throw;
            }




        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE UserPaymentType
                                            SET Active = @Active
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Active", false));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

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
                if (!isUserPaymentType(id))
                {
                    return NotFound("No ID exists of that type");
                }
                else
                {
                    throw;
                }
            }
        }

        private bool isUserPaymentType(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, AcctNumber, Active FROM UserPaymentType WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}


