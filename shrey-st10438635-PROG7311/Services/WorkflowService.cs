using shrey_st10438635_PROG7311.Models;

// Shrey Singh
// ST10438635
// References:
// <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
// <Code Maze, 2026. Repository Pattern with ASP.NET Core and Entity Framework. [online] Available at: https://code-maze.com/the-repository-pattern-aspnet-core [Accessed 15 April 2026].>
// <Refactoring Guru, 2026. Strategy Design Pattern. [online] Available at: https://refactoring.guru/design-patterns/strategy [Accessed 16 April 2026].>
// <Tutorials Teacher, 2026. Consuming a Web API using HttpClient. [online] Available at: https://www.tutorialsteacher.com/core/consume-web-api-httpclient [Accessed 17 April 2026].>
// <GeeksforGeeks, 2026. async and await in C#. [online] Available at: https://www.geeksforgeeks.org/async-and-await-in-c-sharp [Accessed 18 April 2026].>

namespace shrey_st10438635_PROG7311.Services
{
    // Strategy Pattern: Workflow validation separated from controllers
    public interface IWorkflowService
    {
        (bool isValid, string errorMessage) CanCreateServiceRequest(Contract contract);
        void AutoExpireContracts(IEnumerable<Contract> contracts);
    }

    public class WorkflowService : IWorkflowService
    {
        public (bool isValid, string errorMessage) CanCreateServiceRequest(Contract contract)
        {
            if (contract == null)
                return (false, "Contract not found.");

            if (contract.Status == ContractStatus.Expired)
                return (false, $"Cannot create a Service Request against an Expired contract ('{contract.Title}').");

            if (contract.Status == ContractStatus.OnHold)
                return (false, $"Cannot create a Service Request against an On Hold contract ('{contract.Title}'). Please activate it first.");

            if (contract.Status == ContractStatus.Draft)
                return (false, $"Contract '{contract.Title}' is still in Draft status. It must be Active before raising requests.");

            if (contract.EndDate < DateTime.Today)
                return (false, $"Contract '{contract.Title}' has passed its end date and is effectively expired.");

            return (true, string.Empty);
        }

        public void AutoExpireContracts(IEnumerable<Contract> contracts)
        {
            foreach (var contract in contracts)
            {
                if (contract.Status == ContractStatus.Active && contract.EndDate < DateTime.Today)
                    contract.Status = ContractStatus.Expired;
            }
        }
    }
}
