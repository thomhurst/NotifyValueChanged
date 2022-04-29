using Microsoft.CodeAnalysis;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;
using TomLonghurst.Events.NotifyValueChanged.Wrappers;

namespace TomLonghurst.Events.NotifyValueChanged.Extensions;

internal static class SymbolExtensions
{
    private static readonly Dictionary<string, string> FullyQualifiedToSimpleNameDictionary = new();
    public static string GetFullyQualifiedType(this ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType);
    }
    
    public static string GetSimpleTypeName(this ITypeSymbol type)
    {
        var fullyQualifiedType = GetFullyQualifiedType(type);

        if (FullyQualifiedToSimpleNameDictionary.TryGetValue(fullyQualifiedType, out var simpleTypeName))
        {
            return simpleTypeName;
        }
        
        simpleTypeName = fullyQualifiedType.Split('.').Last();

        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            simpleTypeName = $"Nullable{simpleTypeName}".Replace("?", string.Empty);
        }

        var typeArguments = GetGenericTypeArguments(type).ToList();

        if (typeArguments.Any() && simpleTypeName.Contains('<') && simpleTypeName.Contains('>'))
        {
            var firstDiamondBracketIndex = simpleTypeName.IndexOf('<');
            var lastDiamondBracketIndex = simpleTypeName.LastIndexOf('>');

            simpleTypeName = simpleTypeName.Replace(simpleTypeName.Substring(firstDiamondBracketIndex, lastDiamondBracketIndex - firstDiamondBracketIndex+1), string.Join("", typeArguments));
        }

        if (FullyQualifiedToSimpleNameDictionary.Any(x => x.Value == simpleTypeName))
        {
            simpleTypeName = type.ContainingNamespace.ToDisplayString().Replace(".", "") + simpleTypeName;
        }

        FullyQualifiedToSimpleNameDictionary[fullyQualifiedType] = simpleTypeName;

        return simpleTypeName;
    }

    public static ITypeSymbol GetSymbolType(this ISymbol symbol)
    {
        return symbol switch
        {
            IPropertySymbol propertySymbol => propertySymbol.Type,
            IFieldSymbol fieldSymbol => fieldSymbol.Type,
            _ => throw new ArgumentException($"{symbol} is neither a Field or a Property", nameof(symbol))
        };
    }

    public static NotifyValueChangeAttributeData? GetNotifyValueChangeAttribute(this ISymbol symbol)
    {
        var notifyValueChangeAttribute = symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(NotifyValueChangeAttribute).FullName);
        
        return notifyValueChangeAttribute is null ? null : new NotifyValueChangeAttributeData(notifyValueChangeAttribute);
    }

    public static bool HasAttribute<TAttribute>(this ISymbol symbol) where TAttribute : Attribute
    {
        return symbol.GetAttributes().Any(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(TAttribute).FullName);
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