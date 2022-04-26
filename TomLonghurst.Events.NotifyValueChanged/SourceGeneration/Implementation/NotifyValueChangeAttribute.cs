namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

[AttributeUsage(AttributeTargets.Field)]
public class NotifyValueChangeAttribute : Attribute
{
    public bool GenerateGenericTypeValueChangeEvent { get; set; }
    public bool GenerateAnyValueChangeInClassEvent { get; set; }
    
    // TODO Private / Internal / Protected getters and setters
}