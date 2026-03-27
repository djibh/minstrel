using Microsoft.AspNetCore.Mvc;
using Minstrel.Api.Contracts.Search;
using Minstrel.Api.Mapping;
using Minstrel.Application.Search.Services;

namespace Minstrel.Api.Controllers;

[ApiController]
[Route("search")]
public class SearchController : ControllerBase
{
    private readonly SearchAggregationService _searchService;

    public SearchController(SearchAggregationService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResultsResponse>> Search(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Query is required.");
        }

        var results = await _searchService.SearchAsync(q, cancellationToken);
        return Ok(results.ToResponse());
    }
}
