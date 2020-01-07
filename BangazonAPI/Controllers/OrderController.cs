using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace BangazonAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrderController(IConfiguration config)
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
        public async Task<IActionResult> Orders([FromQuery]int? customerId, bool cart)
        {
            if (cart && !String.IsNullOrWhiteSpace(customerId.ToString()))
            {
                Order newOrder = await GetOrderWithCart(customerId);

                if (newOrder == null)
                {
                    return NotFound("No Orders present in cart.");
                }
                else
                {
                    return Ok(newOrder);
                }
            }
            else
            {
                List<Order> orders = await GetOrdersWithoutCart(customerId);

                if (orders.Count == 0)
                {
                    return NotFound("No Orders with this customer Id");
                }
                else
                {
                    return Ok(orders);
                }
            }
        }

        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Orders([FromRoute]int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT [Order].Id as OrderId, [Order].CustomerId, [Order].UserPaymentTypeId, op.OrderId as OrderProductId, op.ProductId, p.Id as ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId as SellerId, p.Price, p.Title, p.Description  
                                        FROM [Order] 
                                        LEFT JOIN OrderProduct AS op ON [Order].Id = op.OrderId  
                                        LEFT JOIN Product as p ON op.ProductId = p.Id 
                                        WHERE [Order].Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    Order order = null;
                    while (reader.Read())
                    {
                        if (order == null)
                        {
                            order = new Order();
                            order.Id = reader.GetInt32(reader.GetOrdinal("OrderId"));
                            order.CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))
                            {
                                order.UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }
                            order.Products = new List<Product>();
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {

                            int currentProductId = reader.GetInt32(reader.GetOrdinal("ProductId"));

                            if (order.Id == reader.GetInt32(reader.GetOrdinal("OrderProductId")))
                            {
                                var dateIsNull = reader.IsDBNull(reader.GetOrdinal("DateAdded"));
                                Product newProduct = new Product
                                {
                                    Id = currentProductId,
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("SellerId")),
                                    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                   
                                };
                                if (!dateIsNull)
                                {
                                    newProduct.DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded"));
                                }

                                order.Products.Add(newProduct);
                            }
                        }
                    }

                    if (order == null)
                    {
                        return NotFound();
                    }

                    reader.Close();

                    return Ok(order);
                }
            }
        }

        private async Task<List<Order>> GetOrdersWithoutCart(int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT [Order].Id as OrderId, [Order].CustomerId, [Order].UserPaymentTypeId, op.OrderId as OrderProductId, op.ProductId, p.Id as ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId as SellerId, p.Price, p.Title, p.Description
                                        FROM [Order] 
                                        LEFT JOIN OrderProduct AS op ON [Order].Id = op.OrderId
                                        LEFT JOIN Product as p ON op.ProductId = p.Id";
                    if (!String.IsNullOrWhiteSpace(customerId.ToString()))
                    {
                        cmd.CommandText += " WHERE [Order].CustomerId = @customerId";
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                    }
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Order> orders = new List<Order>();
                    while (reader.Read())
                    {
                        int currentOrderId = reader.GetInt32(reader.GetOrdinal("OrderId"));
                        Order order = orders.FirstOrDefault(o => o.Id == currentOrderId);
                        if (order == null)
                        {
                            Order newOrder = new Order();

                            newOrder.Id = currentOrderId;
                            newOrder.CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))
                            {
                                newOrder.UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }
                            newOrder.Products = new List<Product>();

                            orders.Add(newOrder);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            int currentProductId = reader.GetInt32(reader.GetOrdinal("ProductId"));
                            foreach (Order orderList in orders)
                            {
                                if (orderList.Id == reader.GetInt32(reader.GetOrdinal("OrderProductId")))
                                {
                                    var dateIsNull = reader.IsDBNull(reader.GetOrdinal("DateAdded"));
                                    Product newProduct = new Product
                                    {
                                        Id = currentProductId,
                                        CustomerId = reader.GetInt32(reader.GetOrdinal("SellerId")),
                                        ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                        Description = reader.GetString(reader.GetOrdinal("Description")),
                                        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                        Title = reader.GetString(reader.GetOrdinal("Title")),
                                        
                                    };
                                    if (!dateIsNull)
                                    {
                                        newProduct.DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded"));
                                    }

                                    orderList.Products.Add(newProduct);
                                }
                            }
                        }
                    }
                    reader.Close();

                    return orders;
                }
            }
        }

        private async Task<Order> GetOrderWithCart(int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT [Order].Id as OrderId, [Order].CustomerId, [Order].UserPaymentTypeId, p.Id as ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId as SellerId, p.Price, p.Title, p.Description
                                        FROM [Order] 
                                        LEFT JOIN OrderProduct AS op ON [Order].Id = op.OrderId
                                        LEFT JOIN Product as p ON op.ProductId = p.Id";
                    if (!String.IsNullOrWhiteSpace(customerId.ToString()))
                    {
                        cmd.CommandText += " WHERE [Order].CustomerId = @customerId ";
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                    }
                    cmd.CommandText += " AND [Order].UserPaymentTypeId IS NULL";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    Order order = null;
                    while (reader.Read())
                    {
                        if (order == null)
                        {
                            order = new Order();
                            order.Id = reader.GetInt32(reader.GetOrdinal("OrderId"));
                            order.CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))
                            {
                                order.UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }
                            order.Products = new List<Product>();
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            var dateIsNull = reader.IsDBNull(reader.GetOrdinal("DateAdded"));
                            Product newProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("SellerId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                
                            };
                            if (!dateIsNull)
                            {
                                newProduct.DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded"));
                            }
                            order.Products.Add(newProduct);
                        }
                    }

                    reader.Close();

                    return order;
                }
            }
        }

        
        [HttpPost("post")]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO [Order] (CustomerId, UserPaymentTypeId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Customer";
                    if (order.UserPaymentTypeId == 0)
                    {
                        cmd.CommandText += ", NULL)";
                    }
                    else
                    {
                        cmd.CommandText += ", @UserPaymentType)";
                        cmd.Parameters.Add(new SqlParameter("@UserPaymentType", order.UserPaymentTypeId));
                        
                    }
                    cmd.Parameters.Add(new SqlParameter("@Customer", order.CustomerId));
                    int newId = (int)await cmd.ExecuteScalarAsync();
                    order.Id = newId;
                    return CreatedAtRoute("GetOrder", new { id = newId }, order);
                }
            }
        }

    

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order updateOrder)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE [Order]
                                            SET UserPaymentTypeId = @userPaymentType
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@userPaymentType", updateOrder.UserPaymentTypeId));
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
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}/product{productid}")]
        public async Task<IActionResult> Delete([FromRoute] int id, int productId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE OrderProduct 
                                            FROM OrderProduct  
                                            LEFT JOIN [Order] on [Order].Id = OrderProduct.OrderId 
                                            WHERE OrderProduct.ProductId = @productId AND OrderId = @orderId AND [Order].UserPaymentTypeId IS NULL";
                        cmd.Parameters.Add(new SqlParameter("@productId", productId));
                        cmd.Parameters.Add(new SqlParameter("@orderId", id));

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
                if (!OrderExists(id))
                {
                    return NotFound("No ID exists of that type.");
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpPost("addProductToOrder")]
        public async Task<IActionResult> AddProductToOrder([FromBody] CustomerProduct customerProduct)
        {
            int orderId = GetCustomerId(customerProduct.CustomerId);
            if (orderId > 0)
            {
                await PostOrderProduct(orderId, customerProduct.ProductId);
                return Ok();
            }
            else
            {
                await PostOrder(customerProduct.CustomerId);
                int newOrderId = GetCustomerId(customerProduct.CustomerId);
                await PostOrderProduct(newOrderId, customerProduct.ProductId);
                return Ok();
            }
        }

        private int GetCustomerId(int customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id FROM [Order] WHERE CustomerId = @id AND UserPaymentTypeId = 0";
                    cmd.Parameters.Add(new SqlParameter("@id", customerId));

                    SqlDataReader reader = cmd.ExecuteReader();

                    int orderId = -1;

                    if (reader.Read())
                    {
                        orderId = reader.GetInt32(reader.GetOrdinal("Id"));
                        return orderId;
                    }
                    reader.Close();
                    return orderId;
                }
            }
        }

        private async Task<IActionResult> PostOrder(int customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    Order newOrder = new Order()
                    {
                        CustomerId = customerId,
                        UserPaymentTypeId = 0,
                    
                    };

                    cmd.CommandText = @"INSERT INTO [Order] (CustomerId, UserPaymentTypeId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@customerId, @UserPaymentTypeId)";
                    cmd.Parameters.Add(new SqlParameter("@customerId", newOrder.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@UserPaymentTypeId", newOrder.UserPaymentTypeId));

                    newOrder.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetOrderProduct", new { id = newOrder.Id }, newOrder);
                }
            }
        }
        private async Task<IActionResult> PostOrderProduct(int orderId, int productId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    OrderProduct newOrderProduct = new OrderProduct()
                    {
                        OrderId = orderId,
                        ProductId = productId
                    };

                    cmd.CommandText = @"INSERT INTO OrderProduct (OrderId, ProductId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@orderId, @productId)";
                    cmd.Parameters.Add(new SqlParameter("@orderId", newOrderProduct.OrderId));
                    cmd.Parameters.Add(new SqlParameter("@productId", newOrderProduct.ProductId));

                    newOrderProduct.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetOrderProduct", new { id = newOrderProduct.Id }, newOrderProduct);
                }
            }
        }

        private bool OrderExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, CustomerId, UserPaymentTypeId
                        FROM [Order]
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
        
    }
}
