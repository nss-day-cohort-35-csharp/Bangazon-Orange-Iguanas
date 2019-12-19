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


    }
}