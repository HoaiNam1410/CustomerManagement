using System.Security.Claims;
using System.Text.Json;
using CustomerManagement.Contracts.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace CustomerManagement.Client.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "customerManagement.auth";

    private readonly IJSRuntime _js;
    private LoginResponse? _session;
    private bool _loaded;

    public CustomAuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        await LoadSessionAsync();

        if (_session is null)
        {
            return CreateAnonymousState();
        }

        if (_session.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            await LogoutAsync();
            return CreateAnonymousState();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, _session.Username),
            new Claim(ClaimTypes.Role, _session.Role)
        };

        var identity = new ClaimsIdentity(claims, "jwt");

        return new AuthenticationState(
            new ClaimsPrincipal(identity));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var state = await GetAuthenticationStateAsync();

        return state.User.Identity?.IsAuthenticated == true
            ? _session?.AccessToken
            : null;
    }

    public async Task SignInAsync(LoginResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.AccessToken)
            || response.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException(
                "Thông tin đăng nhập trả về không hợp lệ.");
        }

        var json = JsonSerializer.Serialize(response);

        await _js.InvokeVoidAsync(
            "sessionStorage.setItem", StorageKey, json);

        _session = response;
        _loaded = true;

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        _session = null;
        _loaded = true;

        NotifyAuthenticationStateChanged(
            Task.FromResult(CreateAnonymousState()));

        await _js.InvokeVoidAsync(
            "sessionStorage.removeItem", StorageKey);
    }

    private async Task LoadSessionAsync()
    {
        if (_loaded)
        {
            return;
        }

        var json = await _js.InvokeAsync<string?>(
            "sessionStorage.getItem", StorageKey);

        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                _session = JsonSerializer.Deserialize<LoginResponse>(json);
            }
            catch (JsonException)
            {
                await _js.InvokeVoidAsync(
                    "sessionStorage.removeItem", StorageKey);
            }
        }

        if (_session is not null
            && string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            _session = null;

            await _js.InvokeVoidAsync(
                "sessionStorage.removeItem", StorageKey);
        }

        _loaded = true;
    }

    private static AuthenticationState CreateAnonymousState()
    {
        return new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity()));
    }
}