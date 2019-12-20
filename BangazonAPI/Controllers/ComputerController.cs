using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonAPI.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;

namespace BangazonAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComputersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
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

        //Get all computers

        [HttpGet]
        public async Task<IActionResult> GetAllComputers()
        {
            string sqlStatement = "SELECT Id, PurchaseDate, DecomissionDate, Make, Model FROM Computer";


            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sqlStatement;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Computer> computers = new List<Computer>();

                    while (reader.Read())
                    {
                        var dateIsNull = reader.IsDBNull(reader.GetOrdinal("DecomissionDate"));
                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model"))
                        };

                        if (!dateIsNull)
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }

                        computers.Add(computer);
                    }
                    reader.Close();

                    return Ok(computers);
                }
            }
        }
        ///Get computers by Id

        [HttpGet("{id}", Name = "GetComputers")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, PurchaseDate, DecomissionDate, Make, Model
                        FROM Computer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computer = null;

                    if (reader.Read())
                    {
                        var dateIsNull = reader.IsDBNull(reader.GetOrdinal("DecomissionDate"));
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model"))
                        };

                        if (!dateIsNull)
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                    }
                    reader.Close();

                    return Ok(computer);
                }
            }


        }

        //Add computer

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Model)
                                        OUTPUT INSERTED.Id
                                        VALUES (@purchaseDate, @decomissionDate, @make, @model)";
                    cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));
                    cmd.Parameters.Add(new SqlParameter("@decomissionDate", computer.DecomissionDate));
                    cmd.Parameters.Add(new SqlParameter("@model", computer.Model));
                    cmd.Parameters.Add(new SqlParameter("make", computer.Make));

                    int newId = (int)cmd.ExecuteScalar();
                    computer.Id = newId;
                    return CreatedAtRoute("GetComputers", new { id = newId }, computer);
                }
            }
        }

    }
}