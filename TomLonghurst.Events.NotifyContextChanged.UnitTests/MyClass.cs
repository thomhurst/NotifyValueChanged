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
    
    [NotifyContextChange]
    private IDictionary<string, int> _genericWithTwoTypeParameters;
    
    [NotifyContextChange]
    private IDictionary<string?, int> _genericWithTwoTypeParametersNullableFirstType;
    
    [NotifyContextChange]
    private IDictionary<string, int?> _genericWithTwoTypeParametersNullableSecondType;
    
    [NotifyContextChange]
    private IDictionary<string?, int?> _genericWithTwoTypeParametersNullableBothType;
    
    [NotifyContextChange]
    private IDictionary<string, int>? _nullableGenericWithTwoTypeParameters;
    
    [NotifyContextChange]
    private IDictionary<string?, int>? _nullableGenericWithTwoTypeParametersNullableFirstType;
    
    [NotifyContextChange]
    private IDictionary<string, int?>? _nullableGenericWithTwoTypeParametersNullableSecondType;
    
    [NotifyContextChange]
    private IDictionary<string?, int?>? _nullableGenericWithTwoTypeParametersNullableBothType;
}