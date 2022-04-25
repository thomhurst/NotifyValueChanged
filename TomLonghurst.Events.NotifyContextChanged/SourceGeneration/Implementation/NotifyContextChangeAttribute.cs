namespace TomLonghurst.Events.NotifyContextChanged.SourceGeneration.Implementation;

[AttributeUsage(AttributeTargets.Field)]
public class NotifyContextChangeAttribute : Attribute
{
    public bool GenerateGenericTypeContextChangeEvent { get; set; }
}