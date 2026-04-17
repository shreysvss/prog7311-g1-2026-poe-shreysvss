# GLMS — Global Logistics Management System
**Student:** Shrey | **Student Number:** ST10438635  
**Module:** PROG7311 / EAPD7111 — Programming 3A  
**Part:** 2 — Core Prototype & Unit Testing

---

## Prerequisites

| Tool | Version |
|------|---------|
| Visual Studio 2022 | 17.x+ |
| .NET SDK | 8.0 |
| SQL Server | LocalDB (included with VS) or SQL Server Express |

---

## How to Run

### 1. Clone / Open the Solution
Open `shrey-st10438635-PROG7311.sln` in Visual Studio 2022.

### 2. Update Connection String (if needed)
In `shrey-st10438635-PROG7311/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GLMS_DB;Trusted_Connection=True;"
}
```
Change `(localdb)\\mssqllocaldb` to your SQL Server instance if needed.

### 3. Apply Migrations
The app auto-migrates on startup. Alternatively, run in Package Manager Console:
```
Update-Database
```
Or via CLI:
```bash
dotnet ef database update --project shrey-st10438635-PROG7311
```

### 4. Run the Application
Press **F5** or **Ctrl+F5** in Visual Studio.  
The app launches at `https://localhost:xxxx` with seed data preloaded.

---

## Running Unit Tests

### Via Visual Studio Test Explorer
1. Open **Test > Test Explorer**
2. Click **Run All Tests**
3. All tests should show green ✅

### Via CLI
```bash
cd shrey-st10438635-PROG7311.Tests
dotnet test --verbosity normal
```

---

## Feature Overview

### Part 2 Checklist

| Feature | Status |
|---------|--------|
| SQL Server + EF Core (Code-First) | ✅ |
| Client, Contract, ServiceRequest models | ✅ |
| One-to-Many relationships (Fluent API) | ✅ |
| PDF upload (wwwroot/uploads, UUID naming) | ✅ |
| PDF-only file validation | ✅ |
| Contract status workflow validation | ✅ |
| Block ServiceRequests on Expired/OnHold contracts | ✅ |
| LINQ filter by Date Range + Status | ✅ |
| External Currency API (open.er-api.com) | ✅ |
| Async/Await throughout | ✅ |
| Live USD→ZAR conversion on Service Request page | ✅ |
| Repository Pattern (IContractRepository) | ✅ |
| Strategy Pattern (IWorkflowService) | ✅ |
| xUnit Test Project | ✅ |
| Currency math tests | ✅ |
| File validation tests | ✅ |
| Workflow/business logic tests | ✅ |
| Repository LINQ filter tests | ✅ |

---

## Project Structure

```
shrey-st10438635-PROG7311.sln
│
├── shrey-st10438635-PROG7311/          ← MVC Web App
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── ClientsController.cs
│   │   ├── ContractsController.cs
│   │   └── ServiceRequestsController.cs
│   ├── Models/
│   │   ├── Client.cs
│   │   ├── Contract.cs
│   │   ├── ServiceRequest.cs
│   │   └── ViewModels.cs
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Services/
│   │   ├── CurrencyService.cs       ← ICurrencyService + implementation
│   │   ├── FileService.cs           ← IFileService + implementation
│   │   ├── ContractRepository.cs    ← Repository Pattern
│   │   └── WorkflowService.cs       ← Strategy Pattern (business rules)
│   ├── Views/
│   │   ├── Home/Index.cshtml        ← Dashboard
│   │   ├── Clients/                 ← Full CRUD
│   │   ├── Contracts/               ← Full CRUD + PDF download + filter
│   │   └── ServiceRequests/         ← Full CRUD + live currency conversion
│   ├── Migrations/
│   │   └── 20260101000000_InitialCreate.cs
│   ├── wwwroot/
│   │   ├── css/site.css
│   │   ├── js/site.js
│   │   └── uploads/contracts/       ← PDF files stored here
│   ├── appsettings.json
│   └── Program.cs
│
└── shrey-st10438635-PROG7311.Tests/  ← xUnit Test Project
    ├── CurrencyServiceTests.cs       ← 9 tests (math + edge cases)
    ├── FileServiceTests.cs           ← 10 tests (PDF validation + types)
    ├── WorkflowServiceTests.cs       ← 9 tests (contract status rules)
    └── ContractRepositoryTests.cs    ← 9 tests (LINQ filter + CRUD)
```

---

## Design Patterns Implemented

1. **Repository Pattern** — `IContractRepository / ContractRepository`  
   Separates data access from controllers, enabling testability.

2. **Strategy Pattern** — `IWorkflowService / WorkflowService`  
   Encapsulates business rules for service request validation outside controllers.

3. **Dependency Injection** — All services registered in `Program.cs` via DI container.  
   Controllers receive interfaces, not concrete implementations (LSP/DIP).

---

## External API
- **open.er-api.com** — Free, no API key required.
- Fallback rate of **18.50** ZAR if API is unavailable.
- 30-minute in-memory cache to avoid rate limiting.
