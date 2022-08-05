using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Helpers;

public static class Commands
{
    public static readonly Tool GitHub = GetTool("gh");
    public static readonly Tool Git = GetTool("git");

    static readonly AbsolutePath Dir = NukeBuild.RootDirectory;

    public static Tool GetTool(string name) =>
        ToolResolver.TryGetEnvironmentTool(name) ??
        ToolResolver.GetPathTool(name);

    public static IProcess RunCommand(string command, params string[] args) =>
        ProcessTasks.StartProcess(command,
            string.Join(" ", args.Select(a => a.Contains(' ') ? $"\"{a.Trim()}\"" : a.Trim())),
            Dir);

    public static Tool BrowseHtml => GetTool(
        Platform switch
        {
            PlatformFamily.Windows => "explorer",
            PlatformFamily.OSX => "open",
            _ => new[] { "google-chrome", "firefox" }
                    .FirstOrDefault(b => RunCommand("which", b).ExitCode == 0)

        });

    public static void DotnetToolEnsureInstalled(string packageName)
    {
        var hasPackageInstalled = DotNet("tool list --global", logOutput: false)
            .Any(output => output.Text.Contains(packageName));

        if (!hasPackageInstalled)
            return;

        DotNetToolInstall(s => s
            .EnableGlobal()
            .SetPackageName(packageName));
    }

    static string ClearOutput(string output) => Regex.Replace(output, @"\s+", " ");

    public static string GitHubLatestRunId(string workflowName, string branch, string filter = "") =>
        GitHub($"workflow view {workflowName}", Dir, logOutput: false)
            .Select(o => ClearOutput(o.Text))
            .Where(o => o.Contains(branch) && o.Contains(filter))
            .Select(o => o.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Select(o => o.LastOrDefault())
            .FirstOrDefault();

    public static string CurrentBranch() =>
        Git("branch --show-current", Dir).Single().Text.Trim();

    public static IReadOnlyCollection<Output> GitHubBuildStatus(string workflowName, string branchName, string filter = "")
    {
        var latestRunId = GitHubLatestRunId(workflowName, branchName, filter);
        if (latestRunId is null)
        {
            Log.Warning("No build triggered for this branch");
            return Array.Empty<Output>();
        }

        var outputs = GitHub($"run watch {latestRunId} -i 2");
        if (outputs.Any(x => x.Text.Contains("has already completed with") && x.Text.Contains("failure")))
            return GitHub($"run view {latestRunId}  --exit-status");

        return outputs;
    }
}
