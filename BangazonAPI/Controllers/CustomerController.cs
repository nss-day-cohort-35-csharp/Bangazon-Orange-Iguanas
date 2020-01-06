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
        public async Task<IActionResult> GetAllCustomers([FromQuery]string q)
        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    var DeliveryTruck = "SELECT * FROM Customer WHERE 1 = 1 ";
                   
                    
                    if(q != null)
                    {
                        DeliveryTruck += " AND LastName Like @q";
                       var AddNewParameter = cmd.Parameters;
                        AddNewParameter.Add(new SqlParameter("@q", "%" + q + "%"));

                    }
                    cmd.CommandText = DeliveryTruck;
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

                    }
                    reader.Close();

                    return Ok(customers);

                }
            }
        }


        private async Task<Product> GetCustomerWithProducts(int id)
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                {

                    using(SqlCommand cmd = conn.CreateCommand())
                    {

                        cmd.CommandText += @"SELECT c.id as 
	                                            CustomerId, 
	                                            c.FirstName, 
	                                            c.LastName, 
	                                            c.[Address], 
	                                            c.city, 
	                                            c.[State], 
	                                            c.Email, 
	                                            c.Phone, 
	                                            c.CreatedDate, 
	                                            c.Active,
	                                            p.Id AS ProductId, 
	                                            p.Title, 
	                                            p.[Description],
	                                            p.CustomerId,
	                                            p.DateAdded,
	                                            p.ProductTypeId,
	                                            p.Price
	                                            FROM Customer as c
	                                            LEFT JOIN Product p ON c.Id = p.CustomerId 
	                                            WHERE 1=1 ";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        //I dont understand this bit of code. Ask ADAM about this line^^^^ SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        List<Product> products = new List<Product>();
                        Customer customer = null;

                      if(reader.Read())
                        {
                            Product product = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductPrimaryId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Address")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                            };

                            products.Add(product);
                           
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


                        }
                        reader.Close();
                        return Ok(customer);



                        }
                    }


                }
            }






        }



        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> GetOneCustomer([FromRoute] int id [FromQuery]string includes)
        {
            if (includes == "products")
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

                    }
                    reader.Close();

                    return Ok(customers);
                }
            }
        }



    } 
}




       