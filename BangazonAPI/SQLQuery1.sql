SELECT * FROM [Order];

SELECT * FROM OrderProduct;
/*
SELECT * FROM PaymentType;

select * from Customer;

SELECT * FROM Product;
*/



SELECT p.Id AS ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId as SellerId, p.Price, p.Title, p.Description, +
o.CustomerId, o.Id as OrderId, o.UserPaymentTypeId, +
op.Id AS OrderProductId, op.OrderId, op.ProductId
FROM [Order] AS o
LEFT JOIN OrderProduct AS op ON o.Id = op.OrderId
LEFT JOIN Product AS p ON op.ProductId = p.Id
WHERE o.Id = 2;

SELECT o.Id AS OrderID, o.CustomerId AS OrderCustomerID, o.UserPaymentTypeId +
       p.Id AS ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId AS SellerId, p.Price, p.Title, p.Description +
       op. OrderId, op.ProductId
FROM [Order] AS o
LEFT JOIN OrderProduct AS op ON o.Id = op.OrderId
LEFT JOIN Product as p ON op.ProductId = P.Id
WHERE o.Id = 2;


