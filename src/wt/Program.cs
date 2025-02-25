// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using io.atlassianlabs.git.wt;

await Commands.BuildWtCommand().InvokeAsync(args);

