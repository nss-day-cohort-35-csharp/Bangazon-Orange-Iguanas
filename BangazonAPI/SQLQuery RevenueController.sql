/*SELECT pt.Id as ProductTypeId, pt.[Name] AS ProductType, ISNULL((SELECT COUNT(OrderProduct.Id)*Product.Price
FROM OrderProduct LEFT JOIN Product ON OrderProduct.ProductId = Product.Id
WHERE Product.ProductTypeId = pt.Id GROUP BY Product.Price),0) AS TotalRevenue FROM ProductType pt;/*
/*SELECTING The ProductTYPE Id and the name of the product type allows you to identify each product name with the corresponding Id. then we check to see if the expression is not NULL, this function "ISNULL" returns the expression.
 Then By SELECT The orderproduct.ID and using the COUNT Function  COUNT() returns the number of rows that matches a specified criteria. Which We are trying to find the all the ordered products and total price we then GROUP By the product price starting at 0 and then setting the total ordered product as Total revenue From each Product type */
