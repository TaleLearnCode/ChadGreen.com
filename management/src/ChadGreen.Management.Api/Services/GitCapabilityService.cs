using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ChadGreen.Management.Api.Services;

public interface IGitCapabilityService
{
    Task<GitCapabilityResponse> GetCapabilityAsync(CancellationToken cancellationToken = default);
}

public sealed class GitCapabilityService(IOptions<ManagementOptions> options) : IGitCapabilityService
{
    private readonly ManagementOptions _options = options.Value;

    public async Task<GitCapabilityResponse> GetCapabilityAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Features.GitIntegration)
        {
            return new GitCapabilityResponse(
                Enabled: false,
                IsRepository: false,
                GitCliAvailable: false,
                SupportsLocalCommitsOnly: true,
                ConventionalCommitsRequired: true,
                Message: "Git integration is disabled by feature flag.");
        }

        var siteRoot = ResolveSiteRoot();
        var isRepository = Directory.Exists(Path.Combine(siteRoot, ".git"));
        var gitCliAvailable = await IsGitCliAvailableAsync(cancellationToken);

        var message = isRepository && gitCliAvailable
            ? "Git capabilities available (manual local Save + Commit only)."
            : "Git capabilities unavailable due to missing .git folder or git CLI.";

        return new GitCapabilityResponse(
            Enabled: true,
            IsRepository: isRepository,
            GitCliAvailable: gitCliAvailable,
            SupportsLocalCommitsOnly: true,
            ConventionalCommitsRequired: true,
            Message: message);
    }

    private string ResolveSiteRoot()
    {
        return SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);
    }

    private static async Task<bool> IsGitCliAvailableAsync(CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            if (!process.Start())
            {
                return false;
            }

            var waitTask = process.WaitForExitAsync(cancellationToken);
            var completed = await Task.WhenAny(waitTask, Task.Delay(TimeSpan.FromSeconds(3), cancellationToken)) == waitTask;
            if (!completed)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
