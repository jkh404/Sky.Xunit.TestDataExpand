using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;

namespace Sky.Xunit.TestDataExpand
{
    [Generator(LanguageNames.CSharp)]
    internal sealed class TestGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();
            context.RegisterSourceOutput(context.ParseOptionsProvider, (sourceProductionContext, compilation) =>
            {

                Debug.WriteLine(compilation.Language);
            });

            Console.WriteLine();
        }
    }
}
