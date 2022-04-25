using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Interface;

namespace TomLonghurst.Events.NotifyValueChanged.Example;

public partial interface IMyClass
{
    [GenerateInterfaceValueChangeEvent]
    int MyAge { get; }
}