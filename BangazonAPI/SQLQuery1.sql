SELECT id, Name FROM ProductType;

SELECT * FROM Product;

SELECT p.Id as ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId, p.Price, p.Title, p.Description, pt.Name, pt.Id
FROM Product AS p
LEFT JOIN ProductType pt ON p.ProductTypeId = pt.Id;

