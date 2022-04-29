using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;

namespace TomLonghurst.Events.NotifyValueChanged.UnitTests.TestNamespace1;

public partial class SameTypeNameInDifferentNamespace
{
    [NotifyValueChange]
    private string _someValue;
}