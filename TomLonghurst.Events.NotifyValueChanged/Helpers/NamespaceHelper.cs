using Microsoft.CodeAnalysis;

namespace TomLonghurst.Events.NotifyValueChanged.Helpers;

public static class NamespaceHelper
{
    public static string GetUsingStatementsForTypes(this GeneratorExecutionContext context, params Type[] types)
    {
        var namespaces = types
            .Select(type => context.Compilation.GetTypeByMetadataName(type.FullName).ContainingNamespace).Distinct();

        return string.Join(Environment.NewLine, namespaces.Select(@namespace => $"using {@namespace};"));
    } 
}