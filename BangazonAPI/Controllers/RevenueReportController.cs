using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonAPI.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
namespace BangazonAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RevenueReportController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RevenueReportController(IConfiguration config)
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
        public async Task<IActionResult> GetRevenueReport()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT pt.Id as ProductTypeId, pt.[Name] AS ProductType, ISNULL((SELECT COUNT(OrderProduct.Id)*Product.Price
                                        FROM OrderProduct LEFT JOIN Product ON OrderProduct.ProductId = Product.Id
                                        WHERE Product.ProductTypeId = pt.Id GROUP BY Product.Price),0) AS TotalRevenue FROM ProductType pt;";
                    /*SELECTING The ProductTYPE Id and the name of the product type allows you to identify each product name with the corresponding Id. then we check to see if the expression is not NULL, this function "ISNULL" returns the expression.
                     Then By SELECT The orderproduct.ID and using the COUNT Function  COUNT() returns the number of rows that matches a specified criteria. Which We are trying to find the all the ordered products and total price we then GROUP 
                      By the product price starting at 0 and then setting the total ordered product as Total revenue From each Product type */

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                List<ProductTypeRevenue> productTypeRevenues = new List<ProductTypeRevenue>();

                while (reader.Read())
                {
                    ProductTypeRevenue productTypeRevenue = new ProductTypeRevenue()
                    {
                        ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                        ProductType = reader.GetString(reader.GetOrdinal("ProductType")),
                        TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue")),

                    };

                    productTypeRevenues.Add(productTypeRevenue);
                }

                reader.Close();
                return Ok(productTypeRevenues);
                }
            }
        }
        }
    }
