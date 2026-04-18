SELECT 
    Id,
    ContractId,
    Description,
    SourceCurrency,
    Cost,
    CostZAR,
    ExchangeRateUsed,
    CASE Status
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'InProgress'
        WHEN 2 THEN 'Completed'
        WHEN 3 THEN 'Cancelled'
        ELSE 'Unknown'
    END AS Status,
    RequestedOn
FROM dbo.ServiceRequests
ORDER BY Id;