using shrey_st10438635_PROG7311.Models;
using shrey_st10438635_PROG7311.Services;
using Xunit;

namespace shrey_st10438635_PROG7311_Tests
{
    /// <summary>
    /// Unit tests for WorkflowService — verifies business rules for ServiceRequest creation.
    /// Rubric: "Workflow logic: Correctly prevents requests on Expired contracts."
    /// </summary>
    public class WorkflowServiceTests
    {
        private readonly WorkflowService _service = new WorkflowService();

        private static Contract MakeContract(ContractStatus status, DateTime? endDate = null)
            => new Contract
            {
                Id = 1,
                Title = "Test Contract",
                ClientId = 1,
                StartDate = DateTime.Today.AddMonths(-6),
                EndDate = endDate ?? DateTime.Today.AddMonths(6),
                Status = status,
                ServiceLevel = ServiceLevel.Standard
            };

        // Active Contract — should ALLOW 

        [Fact]
        public void CanCreateServiceRequest_ActiveContract_ReturnsTrue()
        {
            // Arrange
            var contract = MakeContract(ContractStatus.Active);

            // Act
            var (isValid, errorMsg) = _service.CanCreateServiceRequest(contract);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errorMsg);
        }

        // Expired Contract — should BLOCK 

        [Fact]
        public void CanCreateServiceRequest_ExpiredContract_ReturnsFalse()
        {
            // Arrange
            var contract = MakeContract(ContractStatus.Expired);

            // Act
            var (isValid, errorMsg) = _service.CanCreateServiceRequest(contract);

            // Assert
            Assert.False(isValid);
            Assert.Contains("Expired", errorMsg);
        }

        //  OnHold Contract — should BLOCK 

        [Fact]
        public void CanCreateServiceRequest_OnHoldContract_ReturnsFalse()
        {
            // Arrange
            var contract = MakeContract(ContractStatus.OnHold);

            // Act
            var (isValid, errorMsg) = _service.CanCreateServiceRequest(contract);

            // Assert
            Assert.False(isValid);
            Assert.Contains("On Hold", errorMsg);
        }

        // Draft Contract — should BLOCK 

        [Fact]
        public void CanCreateServiceRequest_DraftContract_ReturnsFalse()
        {
            // Arrange
            var contract = MakeContract(ContractStatus.Draft);

            // Act
            var (isValid, errorMsg) = _service.CanCreateServiceRequest(contract);

            // Assert
            Assert.False(isValid);
            Assert.Contains("Draft", errorMsg);
        }

        //  Null contract — should BLOCK 

        [Fact]
        public void CanCreateServiceRequest_NullContract_ReturnsFalse()
        {
            // Act
            var (isValid, errorMsg) = _service.CanCreateServiceRequest(null!);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Contract not found.", errorMsg);
        }

        // ── Active contract but end date in the past — should BLOCK 

        [Fact]
        public void CanCreateServiceRequest_ActiveButPastEndDate_ReturnsFalse()
        {
            // Arrange — end date is yesterday
            var contract = MakeContract(ContractStatus.Active, endDate: DateTime.Today.AddDays(-1));

            // Act
            var (isValid, errorMsg) = _service.CanCreateServiceRequest(contract);

            // Assert
            Assert.False(isValid);
            Assert.Contains("expired", errorMsg, StringComparison.OrdinalIgnoreCase);
        }

        // AutoExpireContracts 

        [Fact]
        public void AutoExpireContracts_ActiveWithPastEndDate_SetsExpired()
        {
            // Arrange
            var contracts = new List<Contract>
            {
                new Contract { Id = 1, Status = ContractStatus.Active, EndDate = DateTime.Today.AddDays(-10), Title = "Old", ClientId = 1, StartDate = DateTime.Today.AddYears(-1), ServiceLevel = ServiceLevel.Standard }
            };

            // Act
            _service.AutoExpireContracts(contracts);

            // Assert
            Assert.Equal(ContractStatus.Expired, contracts[0].Status);
        }

        [Fact]
        public void AutoExpireContracts_ActiveWithFutureEndDate_KeepsActive()
        {
            // Arrange
            var contracts = new List<Contract>
            {
                new Contract { Id = 2, Status = ContractStatus.Active, EndDate = DateTime.Today.AddDays(30), Title = "Valid", ClientId = 1, StartDate = DateTime.Today.AddMonths(-1), ServiceLevel = ServiceLevel.Standard }
            };

            // Act
            _service.AutoExpireContracts(contracts);

            // Assert
            Assert.Equal(ContractStatus.Active, contracts[0].Status);
        }

        [Fact]
        public void AutoExpireContracts_MixedContracts_OnlyExpiresEligible()
        {
            // Arrange
            var contracts = new List<Contract>
            {
                new Contract { Id = 1, Status = ContractStatus.Active, EndDate = DateTime.Today.AddDays(-5), Title = "Old", ClientId = 1, StartDate = DateTime.Today.AddYears(-1), ServiceLevel = ServiceLevel.Standard },
                new Contract { Id = 2, Status = ContractStatus.Active, EndDate = DateTime.Today.AddDays(30), Title = "Current", ClientId = 1, StartDate = DateTime.Today.AddMonths(-1), ServiceLevel = ServiceLevel.Standard },
                new Contract { Id = 3, Status = ContractStatus.Draft, EndDate = DateTime.Today.AddDays(-5), Title = "Draft", ClientId = 1, StartDate = DateTime.Today.AddYears(-1), ServiceLevel = ServiceLevel.Standard }
            };

            // Act
            _service.AutoExpireContracts(contracts);

            // Assert
            Assert.Equal(ContractStatus.Expired, contracts[0].Status); 
            Assert.Equal(ContractStatus.Active, contracts[1].Status);  
            Assert.Equal(ContractStatus.Draft, contracts[2].Status);   
        }
    }
}
