-- Clients
SELECT 
    Id,
    Name,
    ContactEmail,
    ContactPhone,
    Region
FROM dbo.Clients
ORDER BY Id;

-- Contracts
SELECT 
    Id,
    ClientId,
    Title,
    StartDate,
    EndDate,
    CASE Status
        WHEN 0 THEN 'Draft'
        WHEN 1 THEN 'Active'
        WHEN 2 THEN 'Expired'
        WHEN 3 THEN 'OnHold'
        ELSE 'Unknown'
    END AS Status,
    CASE ServiceLevel
        WHEN 0 THEN 'Standard'
        WHEN 1 THEN 'Express'
        WHEN 2 THEN 'Premium'
        ELSE 'Unknown'
    END AS ServiceLevel,
    SignedAgreementPath,
    SignedAgreementFileName,
    CreatedAt
FROM dbo.Contracts
ORDER BY Id;

-- Service Requests
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