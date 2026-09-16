namespace CustomerManagement.Api.Services;

public class DuplicateCustomerCodeException : Exception
{
    public DuplicateCustomerCodeException()
        : base("Mã khách hàng đã tồn tại.")
    {
    }
}