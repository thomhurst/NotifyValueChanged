using TomLonghurst.Events.NotifyContextChanged.SourceGeneration;

namespace TomLonghurst.Events.NotifyContextChanged.UnitTests;

public partial class MyClass
{
    [NotifyContextChange]
    private string _myString1;
    
    [NotifyContextChange]
    private string _myString2;
    
    [NotifyContextChange]
    private int _myInt1;
    
    [NotifyContextChange]
    private int _myInt2;
    
    [NotifyContextChange]
    private int? _myNullableInt1;
    
    [NotifyContextChange]
    private int? _myNullableInt2;
}