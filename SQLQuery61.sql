SELECT 
    Id,
    ContractId,
    Description,
    SourceCurrency,
    Cost,
    CostZAR,
    ExchangeRateUsed,
    Status,
    RequestedOn
FROM dbo.ServiceRequests
ORDER BY Id;