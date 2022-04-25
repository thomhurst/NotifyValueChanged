using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

namespace TomLonghurst.Events.NotifyValueChanged.UnitTests;

public partial class MyClass2
{
    [NotifyValueChange]
    private string _myString1;
    
    [NotifyValueChange]
    private string _myString2;
    
    [NotifyValueChange]
    private int _myInt1;
    
    [NotifyValueChange]
    private int _myInt2;
    
    [NotifyValueChange]
    private int? _myNullableInt1;
    
    [NotifyValueChange]
    private int? _myNullableInt2;
}