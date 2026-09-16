namespace CustomerManagement.Contracts.Customers;

public class CustomerResponse
{
    public int Id { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public bool IsActive { get; set; }
}