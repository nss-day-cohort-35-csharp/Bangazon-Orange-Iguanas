using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private readonly IConfiguration _config;

        public ProductController(IConfiguration config)
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
        public async Task<IActionResult> Get(
            [FromQuery] string orderBy,
            [FromQuery] string Title,
            [FromQuery] string description)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    List<Product> products = new List<Product>();
                    {
                        cmd.CommandText = @"SELECT Id, DateAdded, ProductTypeId, 
                                        CustomerId, Price, Title, [Description] 
                                        FROM Product
                                        WHERE 1 = 1";
                         

                    }

                    if (!string.IsNullOrWhiteSpace(Title))
                    {
                        cmd.CommandText += @" AND Title LIKE @Title";
                    
                        
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                       
                        cmd.CommandText += @" AND Description LIKE @description";

                    }
                    if (orderBy == "recent")
                    {
                        cmd.CommandText += " Order By DateAdded";
                    }

                    cmd.Parameters.Add(new SqlParameter("@Title", "%" + Title + "%"));
                    cmd.Parameters.Add(new SqlParameter("@description", "%" + description + "%"));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    //sort by popularity

                    List<Product> productsByPopularity = new List<Product>();
                    {
                        cmd.CommandText = @"SELECT Title,Id, COUNT(ProductTypeId) as ProductCount 
                                            FROM Product
                                            GROUP BY Title,Id;";
                    }

                    if (orderBy == "popularity")
                    {
                        cmd.CommandText += " Order By ProductTypeId";
                    }

                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description"))

                        };

                        products.Add(product);
                    }
                    reader.Close();

                    return Ok(products);
                }
            }
        }

        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> GetProductName([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"SELECT
                        Id,DateAdded, ProductTypeId, 
                        CustomerId, Price, Title, [Description] 
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Product product = null;

                    if (reader.Read())
                    {
                        product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description"))

                        };
                    }
                    reader.Close();

                    return Ok(product);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Product (ProductTypeId,
                                        CustomerId, Price, Title, [Description], DateAdded) 
                                        OUTPUT INSERTED.Id
                                        VALUES  (@ProductTypeId, 
                                        @CustomerId, @Price, @Title, @Description,@DateAdded)";

                  
                    cmd.Parameters.Add(new SqlParameter("@ProductTypeId", product.ProductTypeId));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", product.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@Price", product.Price));
                    cmd.Parameters.Add(new SqlParameter("@Title", product.Title));
                    cmd.Parameters.Add(new SqlParameter("@Description", product.Description));
                    cmd.Parameters.Add(new SqlParameter("@DateAdded", DateTime.Now));



                    int newId = (int)await cmd.ExecuteScalarAsync();
                   // DateTime now = DateTime.Now;
                    //string asString = now.ToString("dd MMMM yyyy hh:mm:ss tt");
                    //product.DateAdded = asString;
                    product.Id = newId;
                    return CreatedAtRoute("GetProduct", new { id = newId }, product);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Product product)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Product
                                            SET DateAdded = @DateAdded, ProductTypeId = @ProductTypeId, Title = @Title,CustomerId = @CustomerId, Price = @Price,Description = @Description 
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@DateAdded", product.DateAdded));
                        cmd.Parameters.Add(new SqlParameter("@ProductTypeId", product.ProductTypeId));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", product.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@Price", product.Price));
                        cmd.Parameters.Add(new SqlParameter("@Title", product.Title));
                        cmd.Parameters.Add(new SqlParameter("@Description", product.Description));
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
                if (!ProductExists(id))
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
                        cmd.CommandText = @"DELETE FROM Product 
                                            WHERE Id = @id";
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
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, DateAdded, ProductTypeId, 
                        CustomerId, Price, Title, Description
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}







