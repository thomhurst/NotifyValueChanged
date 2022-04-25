using System.IO.Pipelines;
using TomLonghurst.Events.NotifyContextChanged.SourceGeneration.Implementation;

namespace TomLonghurst.Events.NotifyContextChanged.Example;

public partial class MyClass : IMyClass
{
    [NotifyContextChange]
    private string _myName;

    [NotifyContextChange]
    private int _myAge;
    
    [NotifyContextChange]
    private bool _isMale;
    
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

    [NotifyContextChange(GenerateGenericTypeContextChangeEvent = true)]
    private Pipe _fieldWithGenericTypeEventEnabled;
}