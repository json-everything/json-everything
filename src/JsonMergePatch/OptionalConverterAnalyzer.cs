using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Json.MergePatch;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionalConverterAnalyzer : DiagnosticAnalyzer
{
    private const string _attributeFullName = "Json.MergePatch.GenerateMergePatchUpdateAttribute";
    private const string _diagnosticId = "JMP001";
    private static readonly DiagnosticDescriptor _rule = new(
        _diagnosticId,
        "DotNext.OptionalConverterFactory not registered",
        "System.Text.Json serializer options must include DotNext.OptionalConverterFactory for Optional<T> support",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

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

        // Check if any argument is a generated patch model type
        var patchModelArg = invocation.Arguments.FirstOrDefault(arg => IsGeneratedPatchModelType(arg.Value.Type));
        if (patchModelArg == null) return;

        // Check if options argument is present and contains OptionalConverterFactory
        var optionsArg = invocation.Arguments.FirstOrDefault(arg => arg.Parameter?.Name == "options");
        if (optionsArg == null || !OptionsHasOptionalConverterFactory(optionsArg.Value)) 
	        context.ReportDiagnostic(Diagnostic.Create(_rule, invocation.Syntax.GetLocation()));
    }

    private static bool OptionsHasOptionalConverterFactory(IOperation optionsOp)
    {
        // Best-effort static check:
        // 1) Look for OptionalConverterFactory in the provided operation tree.
        // 2) If options is a local variable reference, scan earlier statements in the same block
        //    for options.Converters.Add(new OptionalConverterFactory()).
        if (ContainsOptionalConverterFactoryCreation(optionsOp)) return true;

        if (optionsOp is ILocalReferenceOperation localReference)
            return LocalHasOptionalConverterFactoryAdded(localReference);

        return false;
    }

    private static bool ContainsOptionalConverterFactoryCreation(IOperation operation)
    {
        if (operation is IObjectCreationOperation objectCreation &&
            objectCreation.Type?.ToDisplayString() == "DotNext.Text.Json.OptionalConverterFactory")
            return true;

        foreach (var child in operation.ChildOperations)
        {
            if (ContainsOptionalConverterFactoryCreation(child))
                return true;
        }

        return false;
    }

    private static bool LocalHasOptionalConverterFactoryAdded(ILocalReferenceOperation localReference)
    {
        var localName = localReference.Local.Name;

        if (localReference.Local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is VariableDeclaratorSyntax declarator &&
            declarator.Initializer?.Value.ToString().Contains("OptionalConverterFactory") == true)
            return true;

        var invocationSyntax = localReference.Syntax.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        var block = invocationSyntax?.FirstAncestorOrSelf<BlockSyntax>();
        if (block is null) return false;

        foreach (var statement in block.Statements)
        {
            if (statement.SpanStart >= invocationSyntax!.SpanStart)
                break;

            var text = statement.ToString();
            if (text.Contains($"{localName}.Converters.Add(") && text.Contains("OptionalConverterFactory"))
                return true;
        }

        return false;
    }

    private static bool IsGeneratedPatchModelType(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol namedType) return false;

        var containingType = namedType.ContainingType;
        if (containingType is null) return false;

        foreach (var attr in containingType.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == _attributeFullName)
                return true;
        }

        return false;
    }
}
