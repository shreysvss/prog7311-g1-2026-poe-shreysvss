UPDATE dbo.ServiceRequests
SET SourceCurrency = 'USD'
WHERE SourceCurrency IS NULL OR SourceCurrency = '';