using System.Net;
using System.Net.Http.Json;
using CustomerManagement.Contracts.Auth;

namespace CustomerManagement.Client.Services;

public class AuthApiService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authState;

    public AuthApiService(
        HttpClient http,
        CustomAuthStateProvider authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync(
            "api/auth/login",
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new HttpRequestException(
                "Tên đăng nhập hoặc mật khẩu không đúng.",
                null,
                response.StatusCode);
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = response.StatusCode == HttpStatusCode.BadRequest
                ? "Vui lòng kiểm tra thông tin đăng nhập."
                : "Không thể đăng nhập lúc này. Vui lòng thử lại sau.";

            throw new HttpRequestException(
                message,
                null,
                response.StatusCode);
        }

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponse>(
                cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(
                "Máy chủ không trả về thông tin đăng nhập.");
        }

        await _authState.SignInAsync(result);
    }

    public Task LogoutAsync()
    {
        return _authState.LogoutAsync();
    }
}