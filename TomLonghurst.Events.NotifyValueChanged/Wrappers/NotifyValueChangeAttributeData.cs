using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Options;

namespace TomLonghurst.Events.NotifyValueChanged.Wrappers;

internal class NotifyValueChangeAttributeData
{
    private readonly AttributeData _attributeData;

    public NotifyValueChangeAttributeData(AttributeData attributeData)
    {
        _attributeData = attributeData;
    }

    public string? CustomPropertyName => GetValue(x => x.PropertyName);
    public PropertyAccessLevel GetterAccessLevel => GetEnumOrDefault(x => x.GetterAccessLevel, PropertyAccessLevel.Public);
    public PropertyAccessLevel SetterAccessLevel => GetEnumOrDefault(x => x.SetterAccessLevel, PropertyAccessLevel.Public);

    private TValue? GetEnumOrDefault<TValue>(Expression<Func<NotifyValueChangeAttribute, TValue>> propertyNameExpression, TValue defaultValue) where TValue : Enum
    {
        var value = GetValue(propertyNameExpression);
        
        if (value == null || !Enum.IsDefined(typeof(TValue), value))
        {
            return defaultValue;
        }

        return value;
    }
    
    private TValue? GetValue<TValue>(Expression<Func<NotifyValueChangeAttribute, TValue>> propertyNameExpression)
    {
        var propertyName = GetPropertyNameFromLambdaExpression(propertyNameExpression.Body);
        
        var propertyExists = _attributeData.NamedArguments.Any(x => string.Equals(x.Key, propertyName));

        if (!propertyExists)
        {
            return default;
        }
        
        var property = _attributeData.NamedArguments.First(x => string.Equals(x.Key, propertyName));

        if (property.Value.Value is TValue value)
        {
            return value;
        }

        if (property.Value.Kind == TypedConstantKind.Enum)
        {
            return (TValue)property.Value.Value;
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