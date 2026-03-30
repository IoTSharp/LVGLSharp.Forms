using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LVGLSharp.Analyzers;

/// <summary>
/// Enforces project-level global-using conventions for remote demo projects.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemoteDemoConventionAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Style";
    private const string RemoteRuntimeAssemblyName = "LVGLSharp.Runtime.Remote";

    private static readonly LocalizableString NonGlobalUsingTitle = new LocalizableResourceString("RemoteDemoNonGlobalUsingRuleTitle", AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString NonGlobalUsingMessage = new LocalizableResourceString("RemoteDemoNonGlobalUsingRuleMessage", AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString NonGlobalUsingDescription = new LocalizableResourceString("RemoteDemoNonGlobalUsingRuleDescription", AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString QualifiedNameTitle = new LocalizableResourceString("RemoteDemoQualifiedNameRuleTitle", AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString QualifiedNameMessage = new LocalizableResourceString("RemoteDemoQualifiedNameRuleMessage", AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString QualifiedNameDescription = new LocalizableResourceString("RemoteDemoQualifiedNameRuleDescription", AnalyzerResources.ResourceManager, typeof(AnalyzerResources));

    internal static readonly DiagnosticDescriptor NonGlobalUsingRule = new(
        id: "LVGL004",
        title: NonGlobalUsingTitle,
        messageFormat: NonGlobalUsingMessage,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: NonGlobalUsingDescription);

    internal static readonly DiagnosticDescriptor QualifiedNameRule = new(
        id: "LVGL005",
        title: QualifiedNameTitle,
        messageFormat: QualifiedNameMessage,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: QualifiedNameDescription);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [NonGlobalUsingRule, QualifiedNameRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterRemoteDemoAnalysis);
    }

    private static void RegisterRemoteDemoAnalysis(CompilationStartAnalysisContext context)
    {
        if (!IsRemoteDemoCompilation(context.Compilation))
        {
            return;
        }

        context.RegisterSyntaxNodeAction(AnalyzeUsingDirective, SyntaxKind.UsingDirective);
        context.RegisterSyntaxNodeAction(AnalyzeQualifiedName, SyntaxKind.QualifiedName);
    }

    private static void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
    {
        if (!IsDemoSourceFile(context.Node.SyntaxTree) || context.Node is not UsingDirectiveSyntax usingDirective)
        {
            return;
        }

        if (usingDirective.GlobalKeyword.RawKind != 0 || usingDirective.Alias is not null || usingDirective.StaticKeyword.RawKind != 0)
        {
            return;
        }

        var namespaceName = NormalizeName(usingDirective.Name?.ToString());
        if (!IsLvglSharpNamespace(namespaceName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(NonGlobalUsingRule, usingDirective.GetLocation(), namespaceName));
    }

    private static void AnalyzeQualifiedName(SyntaxNodeAnalysisContext context)
    {
        if (!IsDemoSourceFile(context.Node.SyntaxTree) || context.Node is not QualifiedNameSyntax qualifiedName)
        {
            return;
        }

        if (qualifiedName.Parent is QualifiedNameSyntax or UsingDirectiveSyntax)
        {
            return;
        }

        var qualifiedText = NormalizeName(qualifiedName.ToString());
        if (!IsLvglSharpNamespace(qualifiedText))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(QualifiedNameRule, qualifiedName.GetLocation(), qualifiedText));
    }

    private static bool IsRemoteDemoCompilation(Compilation compilation)
    {
        if (!ContainsAssembly(compilation.ReferencedAssemblyNames, RemoteRuntimeAssemblyName))
        {
            return false;
        }

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            if (IsDemoSourceFile(syntaxTree))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDemoSourceFile(SyntaxTree syntaxTree)
    {
        var filePath = syntaxTree.FilePath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        var normalizedPath = filePath.Replace('\\', '/');
        return normalizedPath.Contains("/src/Demos/", StringComparison.OrdinalIgnoreCase)
            && !normalizedPath.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsAssembly(IEnumerable<AssemblyIdentity> referencedAssemblies, string assemblyName)
    {
        foreach (var referencedAssembly in referencedAssemblies)
        {
            if (string.Equals(referencedAssembly.Name, assemblyName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsLvglSharpNamespace(string? namespaceName)
    {
        if (namespaceName is null || namespaceName.Length == 0)
        {
            return false;
        }

        return namespaceName.StartsWith("LVGLSharp.", StringComparison.Ordinal);
    }

    private static string NormalizeName(string? name)
        => name?.Replace("global::", string.Empty) ?? string.Empty;
}
