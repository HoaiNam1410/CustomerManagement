using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace CustomerManagement.Client.Services;

public class ApiAuthorizationHandler : DelegatingHandler
{
    private readonly CustomAuthStateProvider _authState;
    private readonly NavigationManager _navigation;
    private readonly Uri _apiBaseUri;

    public ApiAuthorizationHandler(
        CustomAuthStateProvider authState,
        NavigationManager navigation,
        Uri apiBaseUri)
    {
        _authState = authState;
        _navigation = navigation;
        _apiBaseUri = apiBaseUri;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;

        // Chỉ gửi token tới đúng API đã cấu hình.
        var isApiRequest = uri is not null
            && _apiBaseUri.IsBaseOf(uri);

        var loginUri = new Uri(_apiBaseUri, "api/auth/login");

        var isLoginRequest = uri is not null
            && string.Equals(
                uri.AbsolutePath,
                loginUri.AbsolutePath,
                StringComparison.OrdinalIgnoreCase);

        var requiresToken = isApiRequest && !isLoginRequest;
        string? sentToken = null;

        if (requiresToken)
        {
            sentToken = await _authState.GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(sentToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", sentToken);
            }
        }

        var response = await base.SendAsync(
            request,
            cancellationToken);

        if (requiresToken
            && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var currentToken = await _authState.GetAccessTokenAsync();

            // Không xóa phiên mới vì một request cũ trả về muộn.
            if (string.Equals(
                sentToken,
                currentToken,
                StringComparison.Ordinal))
            {
                await _authState.LogoutAsync();

                _navigation.NavigateTo(
                    "/login",
                    replace: true);
            }
        }

        return response;
    }
}