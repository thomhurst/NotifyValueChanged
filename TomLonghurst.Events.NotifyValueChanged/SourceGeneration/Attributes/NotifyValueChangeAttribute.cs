namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class NotifyValueChangeAttribute : Attribute
{
    // TODO Private / Internal / Protected getters and setters
    public string PropertyName { get; set; }
}