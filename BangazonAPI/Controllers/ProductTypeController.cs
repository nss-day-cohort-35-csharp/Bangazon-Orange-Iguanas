using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
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
        public async Task<IActionResult> Get( int id)
        {
           
            
                var productTypes = await GetProductTypes();
                return Ok(productTypes);
            

        }


        private async Task<List<ProductType>> GetProductTypes()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name FROM ProductType";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<ProductType> productTypes = new List<ProductType>();

                    while (reader.Read())
                    {

                        int currentProductTypeID = reader.GetInt32(reader.GetOrdinal("Id"));
                        ProductType newProductType = productTypes.FirstOrDefault(p => p.Id == currentProductTypeID);

                        string nameValue = reader.GetString(reader.GetOrdinal("Name"));


                        if (newProductType == null)
                        {
                            newProductType = new ProductType
                            {
                                Id = currentProductTypeID,
                                Name = nameValue

                            };

                        }


                        productTypes.Add(newProductType);
                    }
                    reader.Close();

                    return productTypes;
                }
            }
        }


        private async Task<ProductType> GetProductTypeWithProducts(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {


                    cmd.CommandText += @"SELECT p.Id as ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId, p.Price, p.Title, p.Description, pt.Name, pt.Id
                                       FROM ProductType AS pt
                                       LEFT JOIN Product p ON p.ProductTypeId = pt.Id
                                       WHERE pt.Id = @id";


                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    ProductType productType = null;

                    while (reader.Read())
                    {
                        
                        var hasProduct = !reader.IsDBNull(reader.GetOrdinal("ProductId"));
                        if (productType == null)
                        {
                            productType = new ProductType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))


                            };

                        }
                        if (hasProduct)
                        {
                            var dateIsNull = reader.IsDBNull(reader.GetOrdinal("DateAdded"));
                            Product product = new Product()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description"))
                            };
                            if (!dateIsNull)
                            {
                                product.DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded"));
                            }

                            productType.Products.Add(product);
                        }
                        


                    }

                    reader.Close();
                    return productType;
                }
            }
        }

        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] string include)
        {
            if (include == "products")
            {
                var productTypeWithProducts = await GetProductTypeWithProducts(id);
                return Ok(productTypeWithProducts);
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    ProductType productType = null;

                    if (reader.Read())
                    {
                        int idValue = reader.GetInt32(reader.GetOrdinal("Id"));
                        string name = reader.GetString(reader.GetOrdinal("Name"));


                        productType = new ProductType
                        {
                            Id = idValue,
                            Name = name,

                        };

                    };
                    reader.Close();

                    if (productType == null)
                    {
                        return NotFound($"No Product Type found with the id of {id}");
                    }
                    return Ok(productType);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType productType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO ProductType (Name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", productType.Name));
                    int newId = (int)await cmd.ExecuteScalarAsync();
                    productType.Id = newId;
                    return CreatedAtRoute("GetProductType", new { id = newId }, productType);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ProductType productType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE ProductType
                                            SET Name = @name
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", productType.Name));
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
                if (!ProductTypeExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductTypeExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name 
                        FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }



    }
}
