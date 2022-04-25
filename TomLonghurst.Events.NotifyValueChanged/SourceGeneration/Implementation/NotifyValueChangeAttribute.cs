namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

[AttributeUsage(AttributeTargets.Field)]
public class NotifyValueChangeAttribute : Attribute
{
    public bool GenerateGenericTypeValueChangeEvent { get; set; }
}