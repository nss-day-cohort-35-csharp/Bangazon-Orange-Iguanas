using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly IConfiguration _config;
        public EmployeeController(IConfiguration config)
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
        public async Task<IActionResult> GetAllEmployees()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, FirstName, LastName, DepartmentId, IsSupervisor, ComputerId, Email FROM Employee";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Employee> allEmployees = new List<Employee>();

                    while (reader.Read())
                    {
                        var employeeId = reader.GetInt32(reader.GetOrdinal("Id"));
                        var employeeAlreadyAdded = allEmployees.FirstOrDefault(e => e.Id == employeeId);

                        if (employeeAlreadyAdded == null)
                        {
                            Employee employee = new Employee()
                            {
                                Id = employeeId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Email = reader.GetString(reader.GetOrdinal("Email"))
                            };

                            allEmployees.Add(employee);
                        }
                    }
                    reader.Close();
                    return Ok(allEmployees);
                }
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT FirstName, LastName, e.Id as EmployeeId, DepartmentId, IsSupervisor, ComputerId, Email, c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model
                                        FROM Employee e
                                        LEFT JOIN Computer c ON ComputerId = c.Id
                                        WHERE e.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    Employee employee = null;
                    while (reader.Read())
                    {
                        if (employee == null)
                        {
                            employee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Email = reader.GetString(reader.GetOrdinal("Email"))

                            };


                            var computer = new Computer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Model = reader.GetString(reader.GetOrdinal("Model"))
                            };

                            var dateIsNull = reader.IsDBNull(reader.GetOrdinal("DecomissionDate"));
                            if (!dateIsNull)
                            {
                                computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                            }

                            employee.Computer = computer;
                        }

                    }
                    reader.Close();
                    if (employee == null)
                    {
                        return NotFound("No department found with this id.");
                    }
                    return Ok(employee);
                }
            }
        }

        [HttpGet]
        [HttpGet()]
        public async Task<IActionResult> GetEmployeeByName(
            [FromQuery] string firstName,
            [FromQuery] string lastName)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId 
                        FROM STUDENT ";

                    if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
                    {
                        cmd.CommandText += @"WHERE FirstName LIKE @firstName OR LastName LIKE @lastName";
                    }

                    cmd.Parameters.Add(new SqlParameter("@firstName", "%" + firstName + "%"));
                    cmd.Parameters.Add(new SqlParameter("@lastName", "%" + lastName + "%"));


                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    var employees = new List<Employee>();

                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                            Email = reader.GetString(reader.GetOrdinal("Email"))
                        });
                    }

                    reader.Close();
                    return Ok(employees);
                }
            }
        }
    }
}
