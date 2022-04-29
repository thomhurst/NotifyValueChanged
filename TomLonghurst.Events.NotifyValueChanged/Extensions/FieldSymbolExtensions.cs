using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

namespace TomLonghurst.Events.NotifyValueChanged.Extensions;

internal static class FieldSymbolExtensions
{
    public static string GetPropertyName(this IFieldSymbol fieldSymbol)
    {
        var attribute = fieldSymbol.GetNotifyValueChangeAttribute();

        if (attribute == null)
        {
            return NormalizePropertyName(fieldSymbol);
        }

        var anyPropertyNameArgument = attribute.NamedArguments.Any(x => x.Key == nameof(NotifyValueChangeAttribute.PropertyName));

        if (!anyPropertyNameArgument)
        {
            return NormalizePropertyName(fieldSymbol);
        }
        
        var customPropertyName = attribute.NamedArguments.First(x => x.Key == nameof(NotifyValueChangeAttribute.PropertyName)).Value.Value?.ToString();

        if (!string.IsNullOrWhiteSpace(customPropertyName))
        {
            return customPropertyName;
        }

        return NormalizePropertyName(fieldSymbol);

    }

    private static string NormalizePropertyName(IFieldSymbol fieldSymbol)
    {
        return Regex.Replace(fieldSymbol.Name, "_[a-z]", m => m.ToString().TrimStart('_').ToUpper());
    }
}