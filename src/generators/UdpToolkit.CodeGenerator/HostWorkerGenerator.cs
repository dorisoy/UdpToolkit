namespace UdpToolkit.CodeGenerator
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Source generator for HostWorker.
    /// </summary>
    [Generator]
    public class HostWorkerGenerator : ISourceGenerator
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="context">Initialization context.</param>
        public void Initialize(GeneratorInitializationContext context)
        {
            // nothing to do
        }

        /// <summary>
        /// Execute.
        /// </summary>
        /// <param name="context">Execution context.</param>
        public void Execute(GeneratorExecutionContext context)
        {
            var generatedCode = SyntaxTreesProcessor.Process(context.Compilation.SyntaxTrees, true);

            context.AddSource(SyntaxTreesProcessor.GeneratedFileName, generatedCode);
        }
    }
}
