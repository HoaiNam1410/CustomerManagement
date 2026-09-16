using CustomerManagement.Contracts.Customers;

namespace CustomerManagement.Api.Services;

public interface ICustomerService
{
    Task<List<CustomerResponse>> GetAllAsync(
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<CustomerResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CustomerResponse> CreateAsync(
        CustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerResponse?> UpdateAsync(
        int id,
        CustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}