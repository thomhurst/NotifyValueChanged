using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

namespace TomLonghurst.Events.NotifyValueChanged.Helpers;

internal static class SymbolHelper
{
    public static TValue? GetAttributePropertyValue<TAttribute, TValue>(this ISymbol symbol, Expression<Func<TAttribute, TValue>> propertyNameExpression)
    {
        var attribute = symbol?.GetAttributes().FirstOrDefault(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(TAttribute).FullName);
        if (attribute == null)
        {
            return default;
        }

        var propertyExists = attribute.NamedArguments.Any(x => string.Equals(x.Key, GetPropertyNameFromLambdaExpression(propertyNameExpression.Body)));

        if (!propertyExists)
        {
            return default;
        }
        
        var property = attribute.NamedArguments.First(x => string.Equals(x.Key, GetPropertyNameFromLambdaExpression(propertyNameExpression.Body)));

        if (property.Value.Value is TValue value)
        {
            return value;
        }

        return default;
    }
    
    private static string GetPropertyNameFromLambdaExpression(Expression expression)
    {
        return expression.NodeType switch
        {
            ExpressionType.MemberAccess => ((MemberExpression)expression).Member.Name,
            ExpressionType.Convert => GetPropertyNameFromLambdaExpression(((UnaryExpression)expression).Operand),
            _ => throw new NotSupportedException(expression.NodeType.ToString())
        };
    }
}