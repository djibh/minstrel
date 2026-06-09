using Microsoft.AspNetCore.Mvc;
using Minstrel.Api.Contracts.Sources;
using Minstrel.Api.Mapping;
using Minstrel.Application.Abstractions.Providers;
using Minstrel.Infrastructure.Providers.PCloud;
using Minstrel.Infrastructure.Providers.WebDav;

namespace Minstrel.Api.Controllers;

[ApiController]
[Route("sources")]
public class SourcesController : ControllerBase
{
    private readonly ISourceRegistry _sourceRegistry;
    private readonly PCloudConfigStore _pCloudConfigStore;
    private readonly PCloudApiClient _apiClient;
    private readonly WebDavConfigStore _webDavConfigStore;

    public SourcesController(
        ISourceRegistry sourceRegistry,
        PCloudConfigStore pCloudConfigStore,
        PCloudApiClient apiClient,
        WebDavConfigStore webDavConfigStore)
    {
        _sourceRegistry = sourceRegistry;
        _pCloudConfigStore = pCloudConfigStore;
        _apiClient = apiClient;
        _webDavConfigStore = webDavConfigStore;
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
        => Ok(new { connected = _pCloudConfigStore.Current.IsConfigured });

    [HttpGet("pcloud/config")]
    public IActionResult GetPCloudConfig()
    {
        var config = _pCloudConfigStore.Current;
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

        await _pCloudConfigStore.UpdateAsync(apiBaseUrl, email, password, folderPath);

        if (!_pCloudConfigStore.Current.IsConfigured)
            return Ok(new { connected = false });

        var result = await _apiClient.AuthenticateAsync(request.VerificationCode, cancellationToken);

        return Ok(new
        {
            connected = result.Connected,
            requiresVerification = result.RequiresVerification,
            error = result.Error,
        });
    }

    [HttpGet("webdav/status")]
    public IActionResult GetWebDavStatus()
        => Ok(new { connected = _webDavConfigStore.Current.IsConfigured });

    [HttpGet("webdav/config")]
    public IActionResult GetWebDavConfig()
    {
        var config = _webDavConfigStore.Current;
        return Ok(new
        {
            isConfigured = config.IsConfigured,
            serverUrl = config.ServerUrl,
            username = config.Username,
            musicFolderPath = config.MusicFolderPath,
        });
    }

    [HttpPut("webdav/config")]
    public async Task<IActionResult> UpdateWebDavConfig(
        [FromBody] WebDavConfigRequest request,
        CancellationToken cancellationToken)
    {
        var serverUrl = request.ServerUrl?.Trim() ?? string.Empty;
        var username = request.Username?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var folderPath = string.IsNullOrWhiteSpace(request.MusicFolderPath) ? "/" : request.MusicFolderPath.Trim();

        await _webDavConfigStore.UpdateAsync(serverUrl, username, password, folderPath);

        return Ok(new { connected = _webDavConfigStore.Current.IsConfigured });
    }
}
