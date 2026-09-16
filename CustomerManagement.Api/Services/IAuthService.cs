using CustomerManagement.Contracts.Auth;

namespace CustomerManagement.Api.Services;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
}