namespace UdpToolkit.Cli
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis.MSBuild;
    using UdpToolkit.CodeGenerator;

    public static class Program
    {
        public static async Task<int> Main(
            string[] args)
        {
            return await Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(options => SafeExecutor(() => GenerateCodeAsync(options)), PrintErrors)
                .ConfigureAwait(false);
        }

        private static Task<int> PrintErrors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }

            return Task.FromResult(-1);
        }

        private static async Task<int> SafeExecutor(Func<Task<int>> func)
        {
            try
            {
                return await func().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }

        private static async Task<int> GenerateCodeAsync(
            CommandLineOptions options)
        {
            var cts = new CancellationTokenSource();
            MSBuildLocator.RegisterDefaults();
            var workspace = MSBuildWorkspace.Create();
            var project = await workspace
                .OpenProjectAsync(projectFilePath: options.ProjectPath, cancellationToken: cts.Token)
                .ConfigureAwait(false);

            var compilation = await project
                .GetCompilationAsync(cts.Token)
                .ConfigureAwait(false);

            if (compilation == null)
            {
                return -1;
            }

            var generatedCode = SyntaxTreesProcessor.Process(compilation.SyntaxTrees, !options.Unsafe);
            SaveGeneratedCode(options.OutputPath, generatedCode);

            return 0;
        }

        private static void SaveGeneratedCode(
            string path,
            string text)
        {
            string file = $"{path}/{SyntaxTreesProcessor.GeneratedFileName}";
            Directory.CreateDirectory(path);
            using (var fileStream = File.Create(file))
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                fileStream.Write(bytes);
            }
        }
    }
}
