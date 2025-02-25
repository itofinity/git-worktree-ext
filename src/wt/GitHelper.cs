using System.Diagnostics;
using System.Text;

namespace io.atlassianlabs.git.wt;

class GitHelper {

    const string GIT_EXE = "git";
    internal const string GIT_CONFIG_WT_ROOT = "wt.root";

    internal static string GetGitWtConfig()
    {
        var configResults = new StringBuilder();
        var response = GetGitConfig(GIT_CONFIG_WT_ROOT);
        configResults.AppendLine($"{GIT_CONFIG_WT_ROOT} = {response?.Trim()}");
        return configResults.ToString();
    }

    internal static string GetGitConfig(string key)
    {
        return RunProcess(GIT_EXE, "config --get " + key);
    }

    internal static string SetGitConfig(string key, string value)
    {
        return RunProcess(GIT_EXE, "config " + key + " " + value);
    }

    internal static string UnsetGitConfig(string key)
    {
        return RunProcess(GIT_EXE, "config --unset-all " + key);
    }

    internal static FileInfo? GetGitTopLevelPath()
    {
        var response = RunProcess(GIT_EXE, "rev-parse --show-toplevel");
        return new FileInfo(response.Trim());
    }

    internal static FileInfo GetDefaultPath()
    {
        return new FileInfo(GetGitTopLevelPath() + "-wt");
    }

    internal static String RunProcess(string exe, string arguments)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start process.");
            }
            Console.WriteLine($"Running: {exe} {arguments}");
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return output;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return null;
    }

    internal static string GetWtRoot()
    {
        return GetGitConfig(GIT_CONFIG_WT_ROOT).Trim();
    }

    internal static void CreateWorktree(string branch, string destination)
    {
        var response = RunProcess(GIT_EXE, $"worktree add -b {branch} {destination}");
    }

    internal static void ListWorktrees()
    {
        var response = RunProcess(GIT_EXE, $"worktree ls");
    }
}