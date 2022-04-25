using TomLonghurst.Events.NotifyContextChanged.SourceGeneration.Interface;

namespace TomLonghurst.Events.NotifyContextChanged.Example;

public partial interface IMyClass
{
    [GenerateInterfaceContextChangeEvent]
    int MyAge { get; }
}