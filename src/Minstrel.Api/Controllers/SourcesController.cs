using Microsoft.AspNetCore.Mvc;
using Minstrel.Api.Contracts.Sources;
using Minstrel.Api.Mapping;
using Minstrel.Application.Abstractions.Providers;
using Minstrel.Application.Sources.Interfaces;

namespace Minstrel.Api.Controllers;

[ApiController]
[Route("sources")]
public class SourcesController : ControllerBase
{
    private readonly ISourceRegistry _sourceRegistry;
    private readonly IPCloudAuthService _pcloudAuth;

    public SourcesController(ISourceRegistry sourceRegistry, IPCloudAuthService pcloudAuth)
    {
        _sourceRegistry = sourceRegistry;
        _pcloudAuth = pcloudAuth;
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

    [HttpPost("pcloud/connect")]
    public async Task<IActionResult> ConnectPCloud([FromBody] PCloudConnectRequest request, CancellationToken cancellationToken)
    {
        var success = await _pcloudAuth.ConnectAsync(request.Email, request.Password, cancellationToken);

        if (!success)
            return Unauthorized(new { error = "pCloud authentication failed. Check your credentials." });

        return Ok(new { connected = true });
    }

    [HttpGet("pcloud/status")]
    public IActionResult GetPCloudStatus()
        => Ok(new { connected = _pcloudAuth.IsConnected });

    [HttpDelete("pcloud")]
    public IActionResult DisconnectPCloud()
    {
        _pcloudAuth.Disconnect();
        return NoContent();
    }
}
