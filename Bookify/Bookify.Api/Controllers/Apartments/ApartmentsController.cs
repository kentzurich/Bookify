using Asp.Versioning;
using Bookify.Application.Apartments.SearchApartments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Apartments;

[Authorize]
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/apartments")]
public class ApartmentsController : ControllerBase
{
    private readonly ISender _sender;

    public ApartmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> SearchApartment(
        DateTime StartDate,
        DateTime EndDate,
        CancellationToken cancellationToken)
    {
        var query = new SearchApartmentsQuery(StartDate, EndDate);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }
}
