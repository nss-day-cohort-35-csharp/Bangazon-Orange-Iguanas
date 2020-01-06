﻿using System;
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

        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT t.Id, Name, StartDate, EndDate, e.Id as EmployeeId, e.FirstName, e.LastName, e.DepartmentId, e.IsSupervisor, e.ComputerId, e.Email 
                                        FROM TrainingProgram t
                                        LEFT JOIN EmployeeTraining et ON t.Id = et.TrainingProgramId
                                        LEFT JOIN Employee e ON e.Id = et.EmployeeId
                                        WHERE t.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    TrainingProgram trainingProgram = null;
                    while (reader.Read())
                    {
                        if (trainingProgram == null)
                        {
                            trainingProgram = new TrainingProgram
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")), 
                            };

                            var employee = new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Email = reader.GetString(reader.GetOrdinal("Email"))
                            };

                            trainingProgram.Employees.Add(employee);
                        }
                    }
                    reader.Close();
                    return Ok(trainingProgram);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @startDate, @endDate, @maxAttendees)";
                    cmd.Parameters.Add(new SqlParameter("@name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@startDate", trainingProgram.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@maxAttendees", trainingProgram.MaxAttendees));

                    var newId = (int)await cmd.ExecuteScalarAsync();
                    trainingProgram.Id = newId;
                    return CreatedAtRoute("GetTrainingProgram", new { id = newId }, trainingProgram);
                }
            }
        }

        [HttpPost("{id}")]
        [Route ("employee")]
        public async Task<IActionResult> Post([FromRoute] int id, [FromBody] EmployeeTraining employeeTraining)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TraingingProgramId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@employeeId, @trainingProgramId)";
                    cmd.Parameters.Add(new SqlParameter("@employeeId", employeeTraining.EmployeeId));
                    cmd.Parameters.Add(new SqlParameter("@trainingProgramId", id));

                    var newId = (int)await cmd.ExecuteScalarAsync();
                    employeeTraining.Id = newId;
                    return CreatedAtRoute("GetTrainingProgram", new { id = newId }, employeeTraining);
                }
            }
        }


    }
}
