using System.ComponentModel.DataAnnotations;

namespace CustomerManagement.Contracts.Customers;

public class CustomerRequest : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng nhập mã khách hàng.")]
    [StringLength(20, ErrorMessage = "Mã khách hàng tối đa 20 ký tự.")]
    public string CustomerCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(150, ErrorMessage = "Họ và tên tối đa 150 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(254, ErrorMessage = "Email tối đa 254 ký tự.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (DateOfBirth is DateOnly birthDate &&
            birthDate > DateOnly.FromDateTime(DateTime.Today))
        {
            yield return new ValidationResult(
                "Ngày sinh không được ở tương lai.",
                new[] { nameof(DateOfBirth) });
        }
    }
}