using Microsoft.AspNetCore.Mvc;
using Minstrel.Api.Contracts.Sources;
using Minstrel.Api.Mapping;
using Minstrel.Application.Abstractions.Providers;

namespace Minstrel.Api.Controllers;

[ApiController]
[Route("sources")]
public class SourcesController : ControllerBase
{
    private readonly ISourceRegistry _sourceRegistry;

    public SourcesController(ISourceRegistry sourceRegistry)
    {
        _sourceRegistry = sourceRegistry;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SourceResponse>>> GetSources(CancellationToken cancellationToken)
    {
        var providers = _sourceRegistry.GetEnabledProviders();
        var result = new List<SourceResponse>();

        foreach (var provider in providers)
        {
            var source = await provider.GetSourceAsync(cancellationToken);
            result.Add(source.ToResponse());
        }

        return Ok(result);
    }
}
