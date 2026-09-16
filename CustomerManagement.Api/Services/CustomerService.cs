using CustomerManagement.Api.Data;
using CustomerManagement.Api.Entities;
using CustomerManagement.Contracts.Customers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _dbContext;

    public CustomerService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CustomerResponse>> GetAllAsync(
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();

            query = query.Where(customer =>
                customer.FullName.Contains(keyword) ||
                customer.PhoneNumber.Contains(keyword));
        }

        if (isActive.HasValue)
        {
            query = query.Where(customer =>
                customer.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(customer => customer.FullName)
            .ThenBy(customer => customer.Id)
            .Select(customer => new CustomerResponse
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                DateOfBirth = customer.DateOfBirth,
                IsActive = customer.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                customer => customer.Id == id,
                cancellationToken);

        return customer is null ? null : ToResponse(customer);
    }

    public async Task<CustomerResponse> CreateAsync(
        CustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerCode = request.CustomerCode.Trim();

        await EnsureUniqueCodeAsync(
            customerCode, null, cancellationToken);

        var customer = new Customer();
        ApplyRequest(customer, request);

        _dbContext.Customers.Add(customer);
        await SaveChangesAsync(cancellationToken);

        return ToResponse(customer);
    }

    public async Task<CustomerResponse?> UpdateAsync(
        int id,
        CustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .SingleOrDefaultAsync(
                customer => customer.Id == id,
                cancellationToken);

        if (customer is null)
        {
            return null;
        }

        var customerCode = request.CustomerCode.Trim();

        await EnsureUniqueCodeAsync(
            customerCode, id, cancellationToken);

        ApplyRequest(customer, request);
        await SaveChangesAsync(cancellationToken);

        return ToResponse(customer);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .SingleOrDefaultAsync(
                customer => customer.Id == id,
                cancellationToken);

        if (customer is null)
        {
            return false;
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureUniqueCodeAsync(
        string customerCode,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Customers
            .Where(customer => customer.CustomerCode == customerCode);

        if (excludedId.HasValue)
        {
            query = query.Where(customer =>
                customer.Id != excludedId.Value);
        }

        if (await query.AnyAsync(cancellationToken))
        {
            throw new DuplicateCustomerCodeException();
        }
    }

    private async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException
            { Number: 2601 or 2627 })
        {
            throw new DuplicateCustomerCodeException();
        }
    }

    private static void ApplyRequest(
        Customer customer,
        CustomerRequest request)
    {
        customer.CustomerCode = request.CustomerCode.Trim();
        customer.FullName = request.FullName.Trim();

        customer.Email = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim();

        customer.PhoneNumber = request.PhoneNumber.Trim();
        customer.DateOfBirth = request.DateOfBirth;
        customer.IsActive = request.IsActive;
    }

    private static CustomerResponse ToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            CustomerCode = customer.CustomerCode,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DateOfBirth = customer.DateOfBirth,
            IsActive = customer.IsActive
        };
    }
}