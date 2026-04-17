using shrey_st10438635_PROG7311.Models;

namespace shrey_st10438635_PROG7311.Services
{
    // ─── Strategy Pattern: Workflow validation separated from controllers ─────────
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
