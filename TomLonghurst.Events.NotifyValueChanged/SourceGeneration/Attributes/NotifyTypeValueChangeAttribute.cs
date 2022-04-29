namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NotifyTypeValueChangeAttribute : Attribute
{
    public Type Type { get; }

    public NotifyTypeValueChangeAttribute(Type type)
    {
        Type = type;
    }
}