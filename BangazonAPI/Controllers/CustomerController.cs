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
    public class customersController : ControllerBase

    {
        private readonly IConfiguration _config;


        public customersController(IConfiguration config)
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
                    var DeliveryTruck = "SELECT * FROM Customer WHERE 1 = 1 AND Active = 1 ";


                    if (q != null)
                    {
                        DeliveryTruck += " AND FirstName Like @q OR LastName Like @q  ";
                        var AddNewParameter = cmd.Parameters;
                        AddNewParameter.Add(new SqlParameter("@q", "%" + q + "%"));

                    }
                    cmd.CommandText = DeliveryTruck;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        int currentCustomerId = reader.GetInt32(reader.GetOrdinal("Id"));


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
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                Active = reader.GetBoolean(reader.GetOrdinal("Active")),

                            };

                            customers.Add(customer);

                            var hasCustomer = reader.IsDBNull(reader.GetOrdinal("Id"));

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
                                    Phone = reader.GetString(reader.GetOrdinal("phone")),
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
        private async Task<Customer> GetCustomerWithProducts(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                {

                    using (SqlCommand cmd = conn.CreateCommand())
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
	                                            WHERE c.Id = @Id ";
                        cmd.Parameters.Add(new SqlParameter("@Id", id));

                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        //I dont understand this bit of code. Ask ADAM about this line^^^^ SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        List<Product> products = new List<Product>();

                        Customer customer = null;

                        while (reader.Read())
                        {
                            if (customer == null)
                            {
                                customer = new Customer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    City = reader.GetString(reader.GetOrdinal("City")),
                                    State = reader.GetString(reader.GetOrdinal("State")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                                    Products = products

                                };
                            }
                            var hasProduct = !reader.IsDBNull(reader.GetOrdinal("ProductId"));
                            if (hasProduct)
                            {
                                Product product = new Product
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                                };
                                products.Add(product);
                            };
                        }
                        reader.Close();
                        return customer;
                    }
                }
            }
        }


        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> GetOneCustomer([FromRoute] int id, [FromQuery]string includes)
        {
            if (includes == "products")
            {
                var oneCustomerWithProducts = await GetCustomerWithProducts(id);

                if (oneCustomerWithProducts == null)
                {
                    return NotFound();
                }
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
                                                    Customer.[State],
                                                    Customer.City, 
                                                    Customer.Email,
                                                    Customer.Phone,
                                                    Customer.CreatedDate,
                                                    Customer.Active FROM Customer WHERE Id = @id
                                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    Customer customer = null;

                    if (reader.Read())
                    {


                        {
                            customer = new Customer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                State = reader.GetString(reader.GetOrdinal("State")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetString(reader.GetOrdinal("phone")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("createdDate")),
                                Active = reader.GetBoolean(reader.GetOrdinal("active")),

                            };

                        }
                        reader.Close();
                        return Ok(customer);
                    }
                    reader.Close();
                    return NotFound();



                }

            }
        }
        [HttpPost]
        public async Task<IActionResult> post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT Into Customer(FirstName,LastName,Address,City,State,Email,Phone,CreatedDate,Active) OUTPUT Inserted.id
                   values (@FirstName,@LastName,@Address,@City,@State,@Email,@Phone,@CreatedDate,@Active)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));
                    cmd.Parameters.Add(new SqlParameter("@Address", customer.Address));
                    cmd.Parameters.Add(new SqlParameter("@City", customer.City));
                    cmd.Parameters.Add(new SqlParameter("@State", customer.State));
                    cmd.Parameters.Add(new SqlParameter("@Email", customer.Email));
                    cmd.Parameters.Add(new SqlParameter("@Phone", customer.Phone));
                    cmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                    cmd.Parameters.Add(new SqlParameter("@Active", customer.Active));
                    int newcustomerId = (int)cmd.ExecuteScalar();
                    customer.Id = newcustomerId;

                    return CreatedAtRoute("GetCustomer", new { id = newcustomerId }, customer);

                }

            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditCustomer([FromRoute] int id, [FromBody] Customer customer)
        {
            try
            {

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                         UPDATE 
                                         Customer 
                                         Set  FirstName = @FirstName,
                                              LastName = @LastName,
                                              Address = @Address,
                                              City = @City, 
                                              State = @State,
                                              Email = @Email,
                                              Phone = @Phone,
                                              CreatedDate = @CreatedDate, 
                                              Active = @Active
                                              WHERE Id = @Id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));
                        cmd.Parameters.Add(new SqlParameter("@Address", customer.Address));
                        cmd.Parameters.Add(new SqlParameter("@City", customer.City));
                        cmd.Parameters.Add(new SqlParameter("@State", customer.State));
                        cmd.Parameters.Add(new SqlParameter("@Email", customer.Email));
                        cmd.Parameters.Add(new SqlParameter("@Phone", customer.Phone));
                        cmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                        cmd.Parameters.Add(new SqlParameter("@Active", customer.Active));
                        cmd.Parameters.Add(new SqlParameter("@id", id));


                        int numRowsAffected = cmd.ExecuteNonQuery();
                        if (numRowsAffected > 0)
                        {

                            return new StatusCodeResult(StatusCodes.Status204NoContent);

                        }
                        return BadRequest($"Customer with id:{id} not found");
                    }
                }
            }
            catch (Exception)
            {
                if (!CustomerFound(id))
                {
                    return NotFound();
                }
                else
                {

                    throw;

                }
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
                        cmd.CommandText = @"UPDATE Customer
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
                if (!CustomerFound(id))
                {
                    return NotFound("No ID exists of that type");
                }
                else
                {
                    throw;
                }
            }
        }
        private bool CustomerFound(int id)

        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" Select * FROM Customer WHERE Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }




        }

    }

}






