using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;

namespace ChadGreen.Management.Api.Services;

public interface IGitCommitService
{
    Task<CommitResponse> CommitAsync(CommitRequest request, CancellationToken cancellationToken = default);
}

public sealed class GitCommitService(IOptions<ManagementOptions> options) : IGitCommitService
{
    private readonly ManagementOptions _options = options.Value;

    public async Task<CommitResponse> CommitAsync(CommitRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.Features.GitIntegration)
        {
            return new CommitResponse(false, "Git integration is disabled by feature flag.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return new CommitResponse(false, "Commit description is required.");
        }

        var siteRoot = SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);
        if (!Directory.Exists(Path.Combine(siteRoot, ".git")))
        {
            return new CommitResponse(false, "The configured site root is not a git repository.");
        }

        var staged = await ExecuteGitAsync(siteRoot, request.IncludeAllChanges ? "add -A" : "add .", cancellationToken);
        if (!staged.success)
        {
            return new CommitResponse(false, $"Failed to stage changes: {staged.output}");
        }

        var status = await ExecuteGitAsync(siteRoot, "status --porcelain", cancellationToken);
        if (!status.success)
        {
            return new CommitResponse(false, $"Unable to check git status: {status.output}");
        }

        if (string.IsNullOrWhiteSpace(status.output))
        {
            return new CommitResponse(false, "No staged changes were found to commit.");
        }

        var commitMessage = BuildCommitMessage(request);
        var commit = await ExecuteGitAsync(siteRoot, $"commit -m {Quote(commitMessage.Title)}{BuildBodyArgument(commitMessage.Body)}", cancellationToken);
        if (!commit.success)
        {
            return new CommitResponse(false, $"git commit failed: {commit.output}");
        }

        var shaResult = await ExecuteGitAsync(siteRoot, "rev-parse HEAD", cancellationToken);
        var sha = shaResult.success ? shaResult.output.Trim() : null;
        return new CommitResponse(true, "Commit created successfully.", sha, commitMessage.FullMessage);
    }

    private static (string Title, string? Body, string FullMessage) BuildCommitMessage(CommitRequest request)
    {
        var type = request.Type.ToString().ToLowerInvariant();
        var scope = string.IsNullOrWhiteSpace(request.Scope) ? string.Empty : $"({request.Scope.Trim()})";
        var title = $"{type}{scope}: {request.Description.Trim()}";
        var body = string.IsNullOrWhiteSpace(request.Body) ? null : request.Body.Trim();
        var full = body is null ? title : $"{title}{Environment.NewLine}{Environment.NewLine}{body}";
        return (title, body, full);
    }

    private static string BuildBodyArgument(string? body)
        => string.IsNullOrWhiteSpace(body) ? string.Empty : $" -m {Quote(body.Trim())}";

    private static string Quote(string value)
        => $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static async Task<(bool success, string output)> ExecuteGitAsync(string workingDirectory, string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        if (!process.Start())
        {
            return (false, "Failed to start git process.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var outputBuilder = new StringBuilder();
        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        if (!string.IsNullOrWhiteSpace(stdout))
        {
            outputBuilder.AppendLine(stdout.Trim());
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            outputBuilder.AppendLine(stderr.Trim());
        }

        return (process.ExitCode == 0, outputBuilder.ToString().Trim());
    }
}
