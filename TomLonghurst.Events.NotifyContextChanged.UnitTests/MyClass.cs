using System.Collections.Generic;
using TomLonghurst.Events.NotifyContextChanged.SourceGeneration;
using TomLonghurst.Events.NotifyContextChanged.SourceGeneration.Implementation;

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

    [NotifyContextChange]
    private IEnumerable<string> _genericEnumerable;
    
    [NotifyContextChange]
    private IEnumerable<string>? _genericNullableEnumerable;
    
    [NotifyContextChange]
    private IEnumerable<string?> _genericEnumerableWithNullableTypeParameter;
    
    [NotifyContextChange]
    private IEnumerable<string?>? _genericNullableEnumerableWithNullableTypeParameter;
}