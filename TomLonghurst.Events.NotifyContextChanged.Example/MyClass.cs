using TomLonghurst.Events.NotifyContextChanged.SourceGeneration;
using TomLonghurst.Events.NotifyContextChanged.SourceGeneration.Implementation;

namespace TomLonghurst.Events.NotifyContextChanged.Example;

public partial class MyClass
{
    [NotifyContextChange]
    private string _myName;

    [NotifyContextChange]
    private int _myAge;
    
    [NotifyContextChange]
    private bool _isMale;
}