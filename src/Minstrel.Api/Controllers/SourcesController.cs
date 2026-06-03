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
        try
        {
            var result = await _pcloudAuth.ConnectAsync(request.Email, request.Password, request.Code, cancellationToken);

            if (result.RequiresEmailCode)
                return Ok(new { connected = false, requiresEmailCode = true });

            return Ok(new { connected = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("pcloud/token")]
    public IActionResult SetPCloudToken([FromBody] PCloudSetTokenRequest request)
    {
        _pcloudAuth.SetToken(request.Token, request.ApiBaseUrl);
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
