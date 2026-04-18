# GLMS - Global Logistics Management System

Shrey Singh | ST10438635 | PROG7311 POE Part 2

## About

ASP.NET Core MVC app for TechMove Logistics to manage clients, contracts and service requests. Built for Part 2 of the POE.

The brief asks for an enterprise-grade prototype that handles the relationship between clients, contracts and service requests, validates business workflows, consumes an external currency API, and has proper unit test coverage. That's what this app does.

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core (Code-First)
- SQL Server
- xUnit + Moq for unit testing
- Bootstrap 5 + Bootstrap Icons for the UI
- open.er-api.com for live currency rates

## Running it

1. Open the solution in Visual Studio 2022.
2. Check the connection string in `appsettings.json` matches your SQL Server. Mine is `SHREYSVS\SQLEXPRESS`, database `GLMS_DB1`.
3. First-time setup — open Package Manager Console and run:
   Update-Database
   This applies all migrations and creates the database with seed data (2 clients, 2 contracts).
4. Press F5 to run. The app opens in your browser on https://localhost:xxxx.

## Running the tests

Easiest way: Test menu → Test Explorer → Run All in Visual Studio.

Or from the command line: dotnet test

All tests should pass (green). They also run automatically on every push to GitHub via GitHub Actions — you can see the workflow under the Actions tab of the repo.

## What it does

### Client Management
Standard CRUD for clients with contact details and region.

### Contract Management
- Full CRUD with Client as a foreign key
- Status tracking — Draft, Active, Expired, OnHold
- Service levels — Standard, Express, Premium
- PDF upload for signed agreements (PDF only, max 5 MB, saved with a UUID prefix to prevent filename collisions)
- Auto-expire — contracts past their end date are automatically moved to Expired status on the Index page
- LINQ filtering — admins can filter contracts by date range and status

### Service Requests
- Linked to a specific contract
- Workflow validation — requests can only be raised against Active contracts. Expired, OnHold, and Draft contracts are blocked with a clear error message
- Multi-currency support — users can enter costs in USD, EUR, GBP, AUD, CAD, or JPY. The system fetches the live rate to ZAR from open.er-api.com, auto-calculates the ZAR amount in real time via JavaScript, and stores both the original amount and the converted amount
- Fallback rates are used if the API is unavailable (so the feature never breaks)
- Rate is cached for 30 minutes per currency to avoid hammering the free API

### File Handling
- Only PDFs allowed — both the file extension AND the MIME type are checked (prevents someone renaming a .exe to .pdf to bypass validation)
- Maximum file size enforced server-side AND with a client-side check that shows the exact file size in a friendly error message
- Every saved file is prefixed with a Guid.NewGuid() UUID so no two uploads ever overwrite each other
- Original filename preserved in the database for display and download

## Architecture & Patterns

- Repository Pattern — IContractRepository / ContractRepository. Controllers don't touch DbContext directly; they go through the repository. This separates data access from the controller layer.
- Strategy Pattern — IWorkflowService / WorkflowService encapsulates the business rules for whether a service request can be raised. The logic is separate from controllers and is fully unit-tested.
- Dependency Injection throughout — every service is registered in Program.cs and injected via constructors.
- Async/Await used everywhere for database and HTTP calls to avoid blocking threads.

## Tests

xUnit + Moq, with EF Core InMemory for repository tests so they run fast and don't need a real database.

49 tests split across four test files:

### CurrencyServiceTests (11 tests)
Covers USD → ZAR conversion math. Includes happy path, rounding to 2 decimal places, zero amounts, large amounts (50,000 contracts scenario), multiple rates via [Theory], and the throw paths for zero/negative rates and negative amounts.

### FileServiceTests (19 tests)
Covers PDF-only validation AND the new max-file-size feature:
- Valid PDFs accepted (happy path)
- Rejected types: .exe, .docx, .jpg, plus 5 more via [Theory] — .bat, .js, .zip, .csv, .html
- Edge cases: null file, empty/zero-byte file, PDF extension with spoofed MIME type
- File size: under limit accepted, exact 5 MB boundary accepted, 6 MB rejected, null rejected
- Throw path: SaveContractFileAsync throws InvalidOperationException for invalid types and oversized files

### WorkflowServiceTests (9 tests)
Covers the business rules in CanCreateServiceRequest:
- Active contract allows requests
- Expired, OnHold, and Draft contracts block requests with specific error messages
- Null contract handled
- Active contract with a past end date also blocked
- AutoExpireContracts flips Active+past contracts to Expired, leaves future-dated Active contracts alone, and never touches Draft contracts

### ContractRepositoryTests (10 tests)
Covers the LINQ filtering logic used by the admin search feature:
- GetAllAsync returns everything
- GetByIdAsync returns the correct contract and handles non-existent IDs
- FilterAsync by status, by date range, no filters
- GetActiveContractsAsync returns only Active
- AddAsync and DeleteAsync work correctly

## Database

EF Core Code-First with three entities:

- Client — Name, ContactEmail, ContactPhone, Region
- Contract — Title, Client (FK), StartDate, EndDate, Status, ServiceLevel, SignedAgreementPath, SignedAgreementFileName, CreatedAt
- ServiceRequest — Contract (FK), Description, SourceCurrency, Cost, CostZAR, ExchangeRateUsed, Status, RequestedOn

Relationships configured via Fluent API in ApplicationDbContext.OnModelCreating:
- Client → Contracts (one-to-many, restrict delete)
- Contract → ServiceRequests (one-to-many, cascade delete)

Decimal precision explicitly set via HasColumnType("decimal(18,2)") to avoid rounding issues.

Migrations folder is included in the repo so anyone cloning can run Update-Database and get the full schema with seed data.
