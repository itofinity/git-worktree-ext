using System.CommandLine;

namespace io.atlassianlabs.git.wt;

class Commands
{
    internal static RootCommand BuildWtCommand()
    {
        var wtCommand = new RootCommand("Sample command-line app");

        wtCommand.Add(Commands.BuildSetCommand());
        wtCommand.Add(Commands.BuildUnsetCommand());
        wtCommand.Add(Commands.BuildNewCommand());
        wtCommand.Add(Commands.BuildViewCommand());
        return wtCommand;
    }

    internal static Command BuildSetCommand()
    {
        var setCommand = new Command("set", "Set the parent worktree path");
        var rootOption = new Option<FileInfo?>(
            name: "--root",
            description: "The root path to use as worktree parent.",
            getDefaultValue: () => GitHelper.GetDefaultPath());
        setCommand.AddOption(rootOption);
        setCommand.SetHandler((path) =>
        {
            if (path == null)
            {
                Console.WriteLine("ERROR: Root not defined.");
                return;
            }

            var fullPath = path?.FullName;

            Console.Write("Set the parent worktree path to: " + fullPath + "? [y/N]:");
            var response = Console.ReadKey();
            Console.WriteLine();
            if (response.Key == ConsoleKey.Y)
            {
                GitHelper.SetGitConfig(GitHelper.GIT_CONFIG_WT_ROOT, fullPath);
                Console.WriteLine("WT Root set to: " + fullPath);
            }
            else
            {
                Console.WriteLine("SKIPPING: WT Root not set.");
            }
        }, rootOption);
        return setCommand;
    }

    internal static Command BuildUnsetCommand()
    {
        var unsetCommand = new Command("unset", "Unset the parent worktree path");
        unsetCommand.SetHandler(() =>
        {
            Console.Write("Unset the root parent worktree path? [y/N]:");
            var response = Console.ReadKey();
            Console.WriteLine();
            if (response.Key == ConsoleKey.Y)
            {
                GitHelper.UnsetGitConfig(GitHelper.GIT_CONFIG_WT_ROOT);
                Console.WriteLine("WT Root unset.");
            }
            else
            {
                Console.WriteLine("SKIPPING: WT Root not unset.");
            }
        });
        return unsetCommand;
    }

    internal static Command BuildViewCommand()
    {
        var viewCommand = new Command("view", "View the worktree setup");
        viewCommand.SetHandler(() =>
        {
            Console.WriteLine("View the worktree setup");
            // config
            // list
            Console.WriteLine(GitHelper.GetGitWtConfig());
        });
        return viewCommand;
    }

    internal static Command BuildNewCommand()
    {
        var newCommand = new Command("new", "Create a new worktree branch");
        var branchOption = new Option<string?>(
            name: "--branch",
            description: "The name of the new worktree branch.");
        newCommand.AddOption(branchOption);
        newCommand.SetHandler((branch) =>
        {
            if (branch == null)
            {
                Console.WriteLine("ERROR: Branch not defined.");
                return;
            }
            // TODO validate branch is really new
            var destination = Path.Combine(GitHelper.GetWtRoot(), branch.Trim());
            // TODO validate destination
            Console.Write($"Create a new worktree with name {branch} at {destination}? [y/N]:");
            var response = Console.ReadKey();
            Console.WriteLine();
            if (response.Key == ConsoleKey.Y)
            {
                GitHelper.CreateWorktree(branch, destination);
                Console.WriteLine("New worktree created at: " + destination);
            }
            else
            {
                Console.WriteLine("SKIPPING: New worktree not created.");
            }

        }, branchOption);
        return newCommand;
    }
}
