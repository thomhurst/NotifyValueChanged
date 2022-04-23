using TomLonghurst.Events.NotifyContextChanged.SourceGeneration;

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