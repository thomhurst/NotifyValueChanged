using System.IO.Pipelines;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

namespace TomLonghurst.Events.NotifyValueChanged.Example;

[NotifyAnyValueChange]
[NotifyTypeValueChange(typeof(Pipe))]
public partial class MyClass : IMyClass
{
    [NotifyValueChange]
    private string _myName;

    [NotifyValueChange]
    private int _myAge;
    
    [NotifyValueChange]
    private bool _isMale;
    
    [NotifyValueChange]
    private IEnumerable<string> _genericEnumerable;
    
    [NotifyValueChange]
    private IEnumerable<string>? _genericNullableEnumerable;
    
    [NotifyValueChange]
    private IEnumerable<string?> _genericEnumerableWithNullableTypeParameter;
    
    [NotifyValueChange]
    private IEnumerable<string?>? _genericNullableEnumerableWithNullableTypeParameter;
    
    [NotifyValueChange]
    private IDictionary<string, int> _genericWithTwoTypeParameters;
    
    [NotifyValueChange]
    private IDictionary<string?, int> _genericWithTwoTypeParametersNullableFirstType;
    
    [NotifyValueChange]
    private IDictionary<string, int?> _genericWithTwoTypeParametersNullableSecondType;
    
    [NotifyValueChange]
    private IDictionary<string?, int?> _genericWithTwoTypeParametersNullableBothType;
    
    [NotifyValueChange]
    private IDictionary<string, int>? _nullableGenericWithTwoTypeParameters;
    
    [NotifyValueChange]
    private IDictionary<string?, int>? _nullableGenericWithTwoTypeParametersNullableFirstType;
    
    [NotifyValueChange]
    private IDictionary<string, int?>? _nullableGenericWithTwoTypeParametersNullableSecondType;
    
    [NotifyValueChange]
    private IDictionary<string?, int?>? _nullableGenericWithTwoTypeParametersNullableBothType;
    
    [NotifyValueChange]
    private IDictionary<string?, int?>? _nullableGenericWithTwoTypeParametersNullableBothType2;

    [NotifyValueChange]
    private Pipe _fieldWithGenericTypeEventEnabled;

    public string HelloString => $"I depend on notify Fields. {_myName} is {_myAge}";

    public string PropertyDependentOnAnotherComputedProperty => $"I depend on {HelloString} which depends on Notify fields";

    [NotifyValueChange(PropertyName = "FOO")]
    private string _stringWithCustomPropertyName;
}