using System.Net.Http.Json;
using System.Text.Json;
using CustomerManagement.Contracts.Customers;

namespace CustomerManagement.Client.Services;

public class CustomerApiService
{
    private readonly HttpClient _httpClient;

    public CustomerApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CustomerResponse>> GetAllAsync(
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            parameters.Add(
                $"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (isActive.HasValue)
        {
            parameters.Add(
                $"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        var url = "api/customers";

        if (parameters.Count > 0)
        {
            url += "?" + string.Join("&", parameters);
        }

        using var response = await _httpClient.GetAsync(
            url, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<List<CustomerResponse>>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "API trả về dữ liệu không hợp lệ.");
    }

    public async Task<CustomerResponse> CreateAsync(
        CustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/customers", request, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await ReadCustomerAsync(response, cancellationToken);
    }

    public async Task<CustomerResponse> UpdateAsync(
        int id,
        CustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            $"api/customers/{id}", request, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await ReadCustomerAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/customers/{id}", cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<CustomerResponse> ReadCustomerAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        return await response.Content
            .ReadFromJsonAsync<CustomerResponse>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "API trả về dữ liệu khách hàng không hợp lệ.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = $"Yêu cầu thất bại ({(int)response.StatusCode}).";

        // Không hiển thị chi tiết lỗi nội bộ của server lên giao diện.
        if ((int)response.StatusCode >= 500)
        {
            message = "Máy chủ gặp lỗi. Vui lòng thử lại sau.";
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync(
                cancellationToken);

            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("errors", out var errors) &&
                        errors.ValueKind == JsonValueKind.Object)
                    {
                        var messages = new List<string>();

                        foreach (var field in errors.EnumerateObject())
                        {
                            if (field.Value.ValueKind != JsonValueKind.Array)
                            {
                                continue;
                            }

                            foreach (var error in field.Value.EnumerateArray())
                            {
                                if (error.ValueKind == JsonValueKind.String &&
                                    !string.IsNullOrWhiteSpace(error.GetString()))
                                {
                                    messages.Add(error.GetString()!);
                                }
                            }
                        }

                        if (messages.Count > 0)
                        {
                            message = string.Join(
                                " ", messages.Distinct());
                        }
                    }
                    else if (root.TryGetProperty("title", out var title) &&
                             title.ValueKind == JsonValueKind.String &&
                             !string.IsNullOrWhiteSpace(title.GetString()))
                    {
                        message = title.GetString()!;
                    }
                }
            }
            catch (JsonException)
            {
                // Giữ thông báo mặc định nếu phản hồi không phải JSON.
            }
        }

        throw new HttpRequestException(
            message, null, response.StatusCode);
    }
}