using Microsoft.AspNetCore.Mvc;
using Minstrel.Api.Contracts.Sources;
using Minstrel.Api.Mapping;
using Minstrel.Application.Abstractions.Providers;
using Minstrel.Infrastructure.Providers.PCloud;

namespace Minstrel.Api.Controllers;

[ApiController]
[Route("sources")]
public class SourcesController : ControllerBase
{
    private readonly ISourceRegistry _sourceRegistry;
    private readonly PCloudConfigStore _configStore;
    private readonly PCloudApiClient _apiClient;

    public SourcesController(ISourceRegistry sourceRegistry, PCloudConfigStore configStore, PCloudApiClient apiClient)
    {
        _sourceRegistry = sourceRegistry;
        _configStore = configStore;
        _apiClient = apiClient;
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

    [HttpGet("pcloud/status")]
    public IActionResult GetPCloudStatus()
        => Ok(new { connected = _configStore.Current.IsConfigured });

    [HttpGet("pcloud/config")]
    public IActionResult GetPCloudConfig()
    {
        var config = _configStore.Current;
        return Ok(new
        {
            isConfigured = config.IsConfigured,
            apiBaseUrl = config.ApiBaseUrl,
            email = config.Email,
            musicFolderPath = config.MusicFolderPath,
        });
    }

    [HttpPut("pcloud/config")]
    public async Task<IActionResult> UpdatePCloudConfig(
        [FromBody] PCloudConfigRequest request,
        CancellationToken cancellationToken)
    {
        var apiBaseUrl = request.ApiBaseUrl ?? "https://api.pcloud.com";
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var folderPath = string.IsNullOrWhiteSpace(request.MusicFolderPath) ? "/" : request.MusicFolderPath.Trim();

        await _configStore.UpdateAsync(apiBaseUrl, email, password, folderPath);

        if (!_configStore.Current.IsConfigured)
            return Ok(new { connected = false });

        var result = await _apiClient.AuthenticateAsync(request.VerificationCode, cancellationToken);

        return Ok(new
        {
            connected = result.Connected,
            requiresVerification = result.RequiresVerification,
            error = result.Error,
        });
    }
}
