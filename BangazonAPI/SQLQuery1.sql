SELECT id, Name FROM ProductType;

SELECT * FROM Product;

SELECT p.Id as ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId, p.Price, p.Title, p.Description, pt.Name, pt.Id
FROM ProductType AS pt
LEFT JOIN Product p ON p.ProductTypeId = pt.Id
WHERE pt.Id = 1
;

