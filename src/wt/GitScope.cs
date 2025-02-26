namespace io.atlassianlabs.git.wt
{
    internal class GitScope
    {
        public static readonly GitScope GLOBAL = new GitScope("--global");
        public static readonly GitScope LOCAL = new GitScope("--local");
        public static readonly GitScope SYSTEM = new GitScope("--system");

        private GitScope(string parameter)
        {
            this.Parameter = parameter;
        }

        public string Parameter { get; }
    }
}