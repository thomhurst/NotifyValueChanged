using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;

namespace TomLonghurst.Events.NotifyValueChanged.Example;

public partial interface IMyClass
{
    [GenerateInterfaceValueChangeEvent]
    int MyAge { get; }
}