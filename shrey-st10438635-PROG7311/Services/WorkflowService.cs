//Code attribution
//Title: Strategy Design Pattern
//Author: Refactoring Guru
//Date: 16 April 2026
//Version: 1
//Availability: https://refactoring.guru/design-patterns/strategy

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


using shrey_st10438635_PROG7311.Models;

namespace shrey_st10438635_PROG7311.Services
{
    // The interface that defines the workflow rules around service requests and contracts
    public interface IWorkflowService
    {
        (bool isValid, string errorMessage) CanCreateServiceRequest(Contract contract);
        void AutoExpireContracts(IEnumerable<Contract> contracts);
    }

    // Handles the business rules that decide whether certain actions are allowed
    // Keeps this logic out of controllers so it can be unit tested on its own
    public class WorkflowService : IWorkflowService
    {
        // Checks whether a service request can be raised against the given contract
        // Returns a tuple with a yes/no answer and an error message to show the user if not
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

        // Goes through a list of contracts and flips any Active ones past their end date into Expired
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