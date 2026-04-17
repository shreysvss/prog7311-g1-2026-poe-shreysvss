SELECT 
    sr.Id,
    sr.Description,
    sr.CostUSD,
    sr.CostZAR,
    sr.ExchangeRateUsed,
    sr.Status,
    sr.RequestedOn,
    c.Title AS ContractTitle,
    cl.Name AS ClientName
FROM [dbo].[ServiceRequests] sr
JOIN [dbo].[Contracts] c ON sr.ContractId = c.Id
JOIN [dbo].[Clients] cl ON c.ClientId = cl.Id