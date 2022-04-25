using Microsoft.CodeAnalysis;

namespace TomLonghurst.Events.NotifyContextChanged.Helpers;

public static class NamespaceHelper
{
    public static string GetUsingStatementForNamespace(this GeneratorExecutionContext context, Type type)
    {
        var typeSymbol = context.Compilation.GetTypeByMetadataName(type.FullName);

        return $"using {typeSymbol.ContainingNamespace};";
    } 
}