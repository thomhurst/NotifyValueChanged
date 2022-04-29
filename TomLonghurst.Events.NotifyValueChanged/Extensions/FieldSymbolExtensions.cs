using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

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

        var customPropertyName = attribute.CustomPropertyName;

        return !string.IsNullOrWhiteSpace(customPropertyName) ? customPropertyName : NormalizePropertyName(fieldSymbol);
    }

    private static string NormalizePropertyName(IFieldSymbol fieldSymbol)
    {
        return Regex.Replace(fieldSymbol.Name, "_[a-z]", m => m.ToString().TrimStart('_').ToUpper());
    }
}