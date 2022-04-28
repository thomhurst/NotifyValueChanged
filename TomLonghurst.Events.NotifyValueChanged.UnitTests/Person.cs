using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

namespace TomLonghurst.Events.NotifyValueChange.UnitTests;

public partial class Person
{
    [NotifyValueChange]
    private string _firstName;
    
    [NotifyValueChange]
    private string _lastName;

    public string FullName => $"{_firstName} {_lastName}";
}