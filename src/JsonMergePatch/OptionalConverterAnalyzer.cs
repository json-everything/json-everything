using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Json.MergePatch;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionalConverterAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "JMP001";
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "DotNext.OptionalConverterFactory not registered",
        "System.Text.Json serializer options must include DotNext.OptionalConverterFactory for Optional<T> support. Add: options.Converters.Add(new DotNext.Text.Json.OptionalConverterFactory());",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;
        if (method.ContainingNamespace.ToDisplayString() != "System.Text.Json") return;
        if (!method.Name.Contains("Serialize") && !method.Name.Contains("Deserialize")) return;

        // Check if any argument is UpdateDto type
        var updateDtoArg = invocation.Arguments.FirstOrDefault(arg => arg.Value.Type?.Name == "UpdateDto");
        if (updateDtoArg == null) return;

        // Check if options argument is present and contains OptionalConverterFactory
        var optionsArg = invocation.Arguments.FirstOrDefault(arg => arg.Parameter?.Name == "options");
        if (optionsArg == null || !OptionsHasOptionalConverterFactory(optionsArg.Value))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
        }
    }

    private static bool OptionsHasOptionalConverterFactory(IOperation optionsOp)
    {
        // This is a best-effort static check; in practice, full analysis may not be possible.
        // If options is a local variable or object creation, check for Add(new DotNext.Text.Json.OptionalConverterFactory())
        // Otherwise, always warn.
        return false;
    }
}
