using System.CommandLine;
using Clipboard;

namespace io.atlassianlabs.git.wt;

class Commands
{

    internal static RootCommand BuildWtCommand()
    {
        var wtCommand = new RootCommand("Git Worktree Extension");

        // porcelain - general usage
        wtCommand.Add(Commands.BuildOpenCommand());
        wtCommand.Add(Commands.BuildCloseCommand());
        wtCommand.Add(Commands.BuildHomeCommand());
        wtCommand.Add(Commands.BuildListCommand());
        wtCommand.Add(Commands.BuildStatusCommand());
        
        // configuration
        wtCommand.Add(Commands.BuildSetCommand());
        wtCommand.Add(Commands.BuildUnsetCommand());
        wtCommand.Add(Commands.BuildInstallCommand());
        wtCommand.Add(Commands.BuildUninstallCommand());

        // plumbing
        wtCommand.Add(Commands.BuildNewCommand());
        wtCommand.Add(Commands.BuildRemoveCommand());


        return wtCommand;
    }

    internal static Command BuildSetCommand()
    {
        var setCommand = new Command("set", "[Config] Set the parent worktree path");
        var rootOption = new Option<FileInfo?>(
            name: "--root",
            description: "The root path to use as worktree parent.",
            getDefaultValue: () => Git.GetDefaultPath());
        setCommand.AddOption(rootOption);
        setCommand.SetHandler((path) =>
        {
            if (path == null)
            {
                Console.WriteLine("ERROR: Root not defined.");
                return;
            }

            var fullPath = path?.FullName;
            if (fullPath == null)
            {
                Console.WriteLine("ERROR: Root not defined.");
                return;
            }

            Console.Write("Set the parent worktree path to: " + fullPath + "? [y/N]:");
            var response = Console.ReadKey();
            Console.WriteLine();
            if (response.Key == ConsoleKey.Y)
            {
                Git.SetGitConfig(Git.GIT_CONFIG_WT_ROOT, fullPath, GitScope.LOCAL);
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
        var unsetCommand = new Command("unset", "[Config] Unset the parent worktree path");
        unsetCommand.SetHandler(() =>
        {
            Console.Write("Unset the root parent worktree path? [y/N]:");
            var response = Console.ReadKey();
            Console.WriteLine();
            if (response.Key == ConsoleKey.Y)
            {
                Git.UnsetGitConfig(Git.GIT_CONFIG_WT_ROOT, GitScope.LOCAL);
                Console.WriteLine("WT Root unset.");
            }
            else
            {
                Console.WriteLine("SKIPPING: WT Root not unset.");
            }
        });
        return unsetCommand;
    }

    internal static Command BuildStatusCommand()
    {
        var viewCommand = new Command("status", "View the worktree status, configuration and active worktrees");
        viewCommand.SetHandler(() =>
        {
            Console.WriteLine("View the worktree status");
            // config
            Console.WriteLine(Git.GetGitWtConfig());
            // list
            Git.Worktree.List().ForEach(w => Console.WriteLine(w));
            
        });
        return viewCommand;
    }

    internal static Command BuildNewCommand()
    {
        var newCommand = new Command("new", "[Plumbing] Create a new worktree branch");
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
            NewWorktreeExistingBranchFlow(branch);

        }, branchOption);
        return newCommand;
    }

    internal static Command BuildRemoveCommand()
    {
        var command = new Command("remove", "[Plumbing] Remove a worktree branch");
        var branchOption = new Option<string?>(
            name: "--branch",
            description: "The name of the worktree branch to remove");
        command.AddOption(branchOption);
        command.SetHandler((branch) =>
        {
            if (branch == null)
            {
                Console.WriteLine("ERROR: Branch not defined.");
                return;
            }
            DeleteWorktreeFlow(branch);

        }, branchOption);
        return command;
    }

    internal static Command BuildCloseCommand()
    {
        var command = new Command("close", "Close a worktree branch");
        var branchOption = new Option<string?>(
            name: "--branch",
            description: "The name of the worktree branch to remove");
        command.AddOption(branchOption);
        command.SetHandler((branch) =>
        {
            if (branch == null)
            {
                Console.WriteLine("ERROR: Branch not defined.");
                return;
            }
            DeleteWorktreeFlow(branch);

        }, branchOption);
        return command;
    }

    internal static Command BuildInstallCommand()
    {
        var command = new Command("install", "[Config] Install Git-Wt");

        command.SetHandler(() =>
        {
            var alias = "wta";
            var command = $"{Environment.ProcessPath} $*";
            // TODO validate destination
            Console.Write($"Add alias {alias} to {Environment.ProcessPath}? [y/N]:");
            var response = Console.ReadKey();
            Console.WriteLine();
            if (response.Key == ConsoleKey.Y)
            {
                Git.AddAlias(alias, $"{Environment.ProcessPath}");
                Console.WriteLine($"Alias {alias}:{Environment.ProcessPath} added.");
            }
            else
            {
                Console.WriteLine("SKIPPING: Alias not added.");
            }

        });
        return command;
    }

    internal static Command BuildUninstallCommand()
    {
        var command = new Command("uninstall", "[Config] Uninstall Git-Wt");

        command.SetHandler(() =>
        {

            var alias = "wta";
            var command = $"{Environment.ProcessPath} $*";
            // TODO validate destination
            Console.Write($"Remove alias {alias} to {command}? [y/N]:");
            var response = Console.ReadKey();
            Console.WriteLine();
            if (response.Key == ConsoleKey.Y)
            {
                Git.RemoveAlias(alias);
                Console.WriteLine($"Alias {alias}:{command} removed.");
            }
            else
            {
                Console.WriteLine("SKIPPING: Alias not removed.");
            }

        });
        return command;
    }

    internal static Command BuildOpenCommand()
    {
        var command = new Command("open", "Open a new of existing worktree");
        var branchOption = new Option<string?>(
            name: "--branch",
            description: "The name of the worktree branch to open");
        command.AddOption(branchOption);
        command.SetHandler((branch) =>
        {
            if (branch == null)
            {
                Console.WriteLine("ERROR: Branch not defined.");
                return;
            }

            // does the worktree already exist?
            var worktrees = Git.Worktree.List();
            var existingWorktree = worktrees.Where(wt => wt.Item1 == branch).FirstOrDefault();
            if (existingWorktree != null)
            {
                Console.WriteLine($"Found worktree {existingWorktree.Item1} at {existingWorktree.Item2}");
                SendToClipboard(existingWorktree.Item2, $"Worktree path {existingWorktree.Item2} copied to clipboard.");
                return;
            }

            // Does the branch exist?
            // git branch --list <pattern> 
            var branches = Git.Branch.List($"*{branch}*");
            var existingBranch = branches.Where(b => b == branch).FirstOrDefault();
            if (existingBranch != null)
            {
                var worktree = NewWorktreeExistingBranchFlow(branch);
                SendToClipboard(worktree, $"Worktree path {worktree} copied to clipboard.");
                return;
            }

            // create new brance/worktree
            var newWorktree = NewWorktreeNewBranchFlow(branch);
            SendToClipboard(newWorktree, $"Worktree path {newWorktree} copied to clipboard.");

        }, branchOption);
        return command;
    }

    private static void SendToClipboard(string value, string message)
    {
        Clipboard.Clipboard.Default.SetText(value);
        Console.WriteLine(message);
    }

    private static string NewWorktreeNewBranchFlow(string branch)
    {
        // TODO validate branch is really new
        var destination = Path.Combine(Git.GetWtRoot(), branch.Trim());
        // TODO validate destination
        if (destination == null)
        {
            Console.WriteLine("ERROR: Destination not defined.");
            return string.Empty;
        }

        Console.Write($"Create a new worktree with new branch {branch} at {destination}? [y/N]:");
        var response = Console.ReadKey();
        Console.WriteLine();
        if (response.Key == ConsoleKey.Y)
        {
            Git.Worktree.Create(branch, destination);
            Console.WriteLine("New worktree created at: " + destination);
            return destination;
        }
        else
        {
            Console.WriteLine("SKIPPING: New worktree not created.");
            return string.Empty;
        }
    }

    private static string NewWorktreeExistingBranchFlow(string branch)
    {
        // TODO validate branch is really new
        var destination = Path.Combine(Git.GetWtRoot(), branch.Trim());
        // TODO validate destination
        Console.Write($"Create a new worktree with existing branch {branch} at {destination}? [y/N]:");
        var response = Console.ReadKey();
        Console.WriteLine();
        if (response.Key == ConsoleKey.Y)
        {
            Git.Worktree.Add(branch, destination);
            Console.WriteLine("New worktree created at: " + destination);
            return destination;
        }
        else
        {
            Console.WriteLine("SKIPPING: New worktree not created.");
            return string.Empty;
        }
    }

        private static void DeleteWorktreeFlow(string branch)
    {
        // TODO validate branch is really new
        var destination = Path.Combine(Git.GetWtRoot(), branch.Trim());
        // TODO validate destination
        Console.Write($"Remove worktree with name {branch} from {destination}? [y/N]:");
        var response = Console.ReadKey();
        Console.WriteLine();
        if (response.Key == ConsoleKey.Y)
        {
            Git.RemoveWorktree(branch);
            Git.PruneWorktree();
            Console.WriteLine($"Worktree removed {branch} from {destination}");
        }
        else
        {
            Console.WriteLine("SKIPPING: Worktree not removed.");
        }
    }

    private static Command BuildHomeCommand() {
        var command = new Command("home", "Get the home worktree folder");
        command.SetHandler(() => {
            if (Git.Home != null)
            {
                SendToClipboard(Git.Home.FullName, $"Worktree home {Git.Home} copied to clipboard.");
            }
            else
            {
                Console.WriteLine("ERROR: Home not defined.");
                return;
            }
        });
        return command;
    }

    private static Command BuildListCommand() {
        var command = new Command("list", "List open worktrees");
        command.SetHandler(() => {
            var worktrees = Git.Worktree.List();
            worktrees.ForEach(w => Console.WriteLine(w));
        });
        return command;
    }
}
