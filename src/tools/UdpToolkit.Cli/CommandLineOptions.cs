namespace UdpToolkit.Cli
{
    using CommandLine;

    public class CommandLineOptions
    {
        [Option('p', "project", Required = true, HelpText = "Path to C# project file (*.csproj)")]
        public string ProjectPath { get; set; }

        [Option('o', "output", Required = true, HelpText = "Path to output generated file (*.cs)")]
        public string OutputPath { get; set; }

        [Option('u', "unsafe", Required = false, HelpText = "Unsafe mode, code will be generated for all classes in project")]
        public bool Unsafe { get; set; }
    }
}