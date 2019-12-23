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
    public class TrainingProgramController : Controller
    {
        private readonly IConfiguration _config;
        public TrainingProgramController(IConfiguration config)
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
        public async Task<IActionResult> GetAllTrainingPrograms()
        {
            var trainingPrograms = await getTrainingPrograms();
            return Ok(trainingPrograms);
        }

        private async Task<List<TrainingProgram>> getTrainingPrograms()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, [Name], StartDate, EndDate, MaxAttendees  
                        FROM TrainingProgram
                        WHERE GETDATE() < StartDate";


                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    var trainingProgram = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        trainingProgram.Add(new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        });
                    }

                    reader.Close();
                    return trainingProgram;
                }
            }
        }
    }
}
