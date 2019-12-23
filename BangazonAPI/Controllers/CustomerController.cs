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
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase

    {
        private readonly IConfiguration _config;
        private IConfiguration config;

        public CustomerController(IConfiguration _config)
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
        public async Task<IActionResult> GetAllCustomers()
        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"SELECT Customer.Id, 
                                               Customer.FirstName, 
                                               Customer.LastName,
                                               Customer.[Address],
                                               Customer.City, 
                                               Customer.Email,
                                               Customer.Phone,
                                               Customer.CreatedDate,
                                               Customer.Active FROM Customer ";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        int currentCustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                        var customerAlreadyAdded = customers.FirstOrDefault(i => i.Id == currentCustomerId);
                        if (customerAlreadyAdded == null)
                        {
                            var customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                State = reader.GetString(reader.GetOrdinal("State")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetInt32(reader.GetOrdinal("Phone")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                Active = reader.GetBoolean(reader.GetOrdinal("Active")),

                            };

                            customers.Add(customer);


                            var hasCustomer = !reader.IsDBNull(reader.GetOrdinal("CustomerId");

                            if (hasCustomer)
                            {
                                customers.Add(new Customer()

                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    City = reader.GetString(reader.GetOrdinal("City")),
                                    State = reader.GetString(reader.GetOrdinal("State")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Phone = reader.GetInt32(reader.GetOrdinal("phone")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("createdDate")),
                                    Active = reader.GetBoolean(reader.GetOrdinal("active")),

                                });

                            }

                            else
                            {
                                hasCustomer = !reader.IsDBNull(reader.GetOrdinal("CustomerId"));

                                if (hasCustomer)
                                {
                                    customers.Add(new Customer()

                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                        Address = reader.GetString(reader.GetOrdinal("Address")),
                                        City = reader.GetString(reader.GetOrdinal("City")),
                                        State = reader.GetString(reader.GetOrdinal("State")),
                                        Email = reader.GetString(reader.GetOrdinal("Email")),
                                        Phone = reader.GetInt32(reader.GetOrdinal("phone")),
                                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("createdDate")),
                                        Active = reader.GetBoolean(reader.GetOrdinal("active")),

                                    });


                                }


                            }
                        }

                        reader.Close();

                        return Ok(customers);
                    }

                }
            }
        }


        private async Task<Product> GetCustomerWithProducts()
        {




        }



        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> GetOneCustomer([FromRoute] int id [FromQuery]string includes)
        {
           if(includes == "products")
            {
                var oneCustomerWithProducts = await GetCustomerWithProducts(id);
                return Ok(oneCustomerWithProducts);


           }
             
            using (SqlConnection conn = Connection)

            {
               




                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"SELECT Customer.Id, 
                                                    Customer.FirstName, 
                                                    Customer.LastName,
                                                    Customer.[Address],
                                                    Customer.City, 
                                                    Customer.Email,
                                                    Customer.Phone,
                                                    Customer.CreatedDate,
                                                    Customer.Active FROM Customer WHERE Id = @Id
                                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        int currentCustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                        var customerAlreadyAdded = customers.FirstOrDefault(i => i.Id == currentCustomerId);
                        if (customerAlreadyAdded == null)
                        {
                            var customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                State = reader.GetString(reader.GetOrdinal("State")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetInt32(reader.GetOrdinal("phone")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("createdDate")),
                                Active = reader.GetBoolean(reader.GetOrdinal("active")),

                            };

                            customers.Add(customer);


                            var hasCustomer = !reader.IsDBNull(reader.GetOrdinal("CustomerId");

                            if (hasCustomer)
                            {
                                customers.Add(new Customer()

                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    City = reader.GetString(reader.GetOrdinal("City")),
                                    State = reader.GetString(reader.GetOrdinal("State")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Phone = reader.GetInt32(reader.GetOrdinal("phone")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("createdDate")),
                                    Active = reader.GetBoolean(reader.GetOrdinal("active")),

                                });

                            }

                            else
                            {
                                hasCustomer = !reader.IsDBNull(reader.GetOrdinal("CustomerId"));

                                if (hasCustomer)
                                {
                                    customers.Add(new Customer()

                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                        Address = reader.GetString(reader.GetOrdinal("Address")),
                                        City = reader.GetString(reader.GetOrdinal("City")),
                                        State = reader.GetString(reader.GetOrdinal("State")),
                                        Email = reader.GetString(reader.GetOrdinal("Email")),
                                        Phone = reader.GetInt32(reader.GetOrdinal("phone")),
                                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("createdDate")),
                                        Active = reader.GetBoolean(reader.GetOrdinal("active")),

                                    });


                                }


                            }
                        }

                        reader.Close();

                        return Ok(customers);
                    }
                }
            }
        }








       