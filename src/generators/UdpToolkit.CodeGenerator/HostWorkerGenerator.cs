namespace UdpToolkit.CodeGenerator
{
    using Microsoft.CodeAnalysis;

    [Generator]
    public class HostWorkerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // nothing to do
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var generatedCode = SyntaxTreesProcessor.Process(context.Compilation.SyntaxTrees, true);

            context.AddSource(SyntaxTreesProcessor.GeneratedFileName, generatedCode);
        }
    }
}
