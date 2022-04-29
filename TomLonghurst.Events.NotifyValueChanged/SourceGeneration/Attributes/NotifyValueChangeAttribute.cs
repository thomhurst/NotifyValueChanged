using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Options;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class NotifyValueChangeAttribute : Attribute
{
    public string PropertyName { get; set; }
    public PropertyAccessLevel GetterAccessLevel { get; set; } = PropertyAccessLevel.Public;
    public PropertyAccessLevel SetterAccessLevel { get; set; } = PropertyAccessLevel.Public;
}