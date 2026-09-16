using CustomerManagement.Api.Services;
using CustomerManagement.Contracts.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CustomerManagement.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.GetAllAsync(
            search, isActive, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.GetByIdAsync(
            id, cancellationToken);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Không tìm thấy khách hàng.");
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(
        [FromBody] CustomerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _customerService.CreateAsync(
                request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (DuplicateCustomerCodeException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: exception.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> Update(
        int id,
        [FromBody] CustomerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _customerService.UpdateAsync(
                id, request, cancellationToken);

            if (result is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Không tìm thấy khách hàng.");
            }

            return Ok(result);
        }
        catch (DuplicateCustomerCodeException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.DeleteAsync(
            id, cancellationToken);

        if (!result)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Không tìm thấy khách hàng.");
        }

        return NoContent();
    }
}