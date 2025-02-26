using System.Diagnostics;
using System.Text;

namespace io.atlassianlabs.git.wt;

class Git {

    internal const string GIT_EXE = "git";
    internal const string GIT_CONFIG_WT_ROOT = "wt.root";
    internal const string GIT_ALIAS_COMMAND = "wta";
    internal const string GIT_CONFIG_ALIAS_WT = $"alias.{GIT_ALIAS_COMMAND}";

    public static readonly Worktree Worktree = new Worktree();
    public static readonly Branch Branch = new Branch();

    internal static string GetGitWtConfig()
    {
        var configResults = new StringBuilder();
        configResults.AppendLine($"{GIT_CONFIG_WT_ROOT} = {GetGitConfig(GIT_CONFIG_WT_ROOT)!.Trim()}");
        configResults.AppendLine($"{GIT_CONFIG_ALIAS_WT} = {GetGitConfig(GIT_CONFIG_ALIAS_WT)!.Trim()}");
        return configResults.ToString();
    }

    internal static string GetGitConfig(string key)
    {
        return CommandLine.Run(GIT_EXE, "config --get " + key);
    }

    internal static string SetGitConfig(string key, string value, GitScope scope)
    {
        return CommandLine.Run(GIT_EXE, $"config {scope.Parameter} {key} {value}");
    }

    internal static string UnsetGitConfig(string key, GitScope scope)
    {
        return CommandLine.Run(GIT_EXE, $"config {scope.Parameter} --unset-all " + key);
    }

    internal static FileInfo? TopLevelPath
    {
        get
        {
            var response = CommandLine.Run(GIT_EXE, "rev-parse --show-toplevel");
            return new FileInfo(response.Trim());
        }
    }

    internal static FileInfo? Home
    {
        get
        {
            var response = new FileInfo(CommandLine.Run(GIT_EXE, "rev-parse --path-format=absolute --git-common-dir").Trim());
            Directory.GetDirectoryRoot(response.FullName);
            return new FileInfo(response.DirectoryName!);
        }
    }

    internal static FileInfo GetDefaultPath()
    {
        return new FileInfo(TopLevelPath + "-wt");
    }

    internal static string GetWtRoot()
    {
        return GetGitConfig(GIT_CONFIG_WT_ROOT).Trim();
    }

    internal static void ListWorktrees()
    {
        var response = CommandLine.Run(GIT_EXE, $"worktree list");
    }

    internal static void RemoveWorktree(string branch)
    {
        var response = CommandLine.Run(GIT_EXE, $"worktree remove {branch}");
    }

    internal static void PruneWorktree()
    {
        var response = CommandLine.Run(GIT_EXE, $"worktree prune");
    }

    internal static void AddAlias(string alias, string command, GitScope scope)
    {
        SetGitConfig($"alias.{alias}", $"!{command} #", scope);
    }

    internal static void AddAlias(string alias, string command)
    {
        AddAlias(alias, command, GitScope.GLOBAL);
    }

    internal static void RemoveAlias(string alias, GitScope scope)
    {
       UnsetGitConfig($"alias.{alias}", scope);
    }

    internal static void RemoveAlias(string alias)
    {
       RemoveAlias(alias, GitScope.GLOBAL);
    }



}

class Worktree
{
    internal List<Tuple<String, String>> List()
    {
        var response = CommandLine.Run(Git.GIT_EXE, "worktree list");
        var worktrees = response.Split(Environment.NewLine)
            .Where(l => !string.IsNullOrEmpty(l))
            .Select(l => {
                var parts = l.Split(" ");
                var path = parts[0];
                var branch = parts[parts.Length - 1].Replace("[", "").Replace("]", "");
                return new Tuple<string, string>(branch, path);
            })
            .ToList();
        return worktrees;
    }

    internal void Create(string branch, string destination)
    {
        var response = CommandLine.Run(Git.GIT_EXE, $"worktree add -b {branch} {destination}");
    }

    internal void Add(string branch, string destination)
    {
        var response = CommandLine.Run(Git.GIT_EXE, $"worktree add {destination} {branch}");
    }
}

class Branch
{
    internal List<string> List(string branch)
    {
        var response = CommandLine.Run(Git.GIT_EXE, $"branch --list {branch}");
        var branches = response.Split(Environment.NewLine)
            .Where(l => !string.IsNullOrEmpty(l))
            .Select(l => {
                return l.Trim();
            })
            .ToList();
        return branches;
    }
}

class CommandLine
{
    internal static String Run(string exe, string arguments)
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
            // Console.WriteLine($"Running: {exe} {arguments}");
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

        return string.Empty;
    }
}