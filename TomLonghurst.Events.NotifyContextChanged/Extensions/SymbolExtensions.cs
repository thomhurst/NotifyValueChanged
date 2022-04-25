using Microsoft.CodeAnalysis;

namespace TomLonghurst.Events.NotifyContextChanged.Extensions;

internal static class SymbolExtensions
{
    private const string GlobalPrefix = "global::";

    public static string GetFullyQualifiedType(this ITypeSymbol type)
    {
        var fullyQualifiedFormat = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (fullyQualifiedFormat.StartsWith(GlobalPrefix))
        {
            fullyQualifiedFormat = fullyQualifiedFormat.Substring(GlobalPrefix.Length, fullyQualifiedFormat.Length - GlobalPrefix.Length);
        }

        return fullyQualifiedFormat;
    }
    
    public static string GetSimpleTypeName(this ITypeSymbol type)
    {
        var simpleFieldName = GetFullyQualifiedType(type).Split('.').Last();

        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            simpleFieldName = $"Nullable{simpleFieldName.CapitalizeFirstLetter()}".Replace("?", string.Empty);
        }

        var typeArguments = GetGenericTypeArguments(type).ToList();

        if (!typeArguments.Any())
        {
            return simpleFieldName.CapitalizeFirstLetter();   
        }

        if (simpleFieldName.Contains('<') && simpleFieldName.Contains('>'))
        {
            var firstDiamondBracketIndex = simpleFieldName.IndexOf('<');
            var lastDiamondBracketIndex = simpleFieldName.LastIndexOf('>');

            return simpleFieldName.CapitalizeFirstLetter().Replace(simpleFieldName.Substring(firstDiamondBracketIndex, lastDiamondBracketIndex - firstDiamondBracketIndex+1), string.Join("", typeArguments));
        }

        return simpleFieldName.CapitalizeFirstLetter();
    }

    private static IEnumerable<string> GetGenericTypeArguments(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedTypeSymbol || !namedTypeSymbol.TypeArguments.Any())
        {
            yield break;
        }
        
        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
        {
            yield return GetSimpleTypeName(typeArgument);
        }
    }
}