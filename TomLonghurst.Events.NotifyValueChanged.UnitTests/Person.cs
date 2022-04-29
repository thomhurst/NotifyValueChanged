using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

namespace TomLonghurst.Events.NotifyValueChanged.UnitTests;

public partial class Person
{
    [NotifyValueChange]
    private string _firstName;
    
    [NotifyValueChange]
    private string _lastName;

    [NotifyValueChange]
    private int _age;

    public string FullName => $"{_firstName} {_lastName}";

    public string AgeInYears => $"{Age} years old";

    public string Description => $"{FullName} is {AgeInYears}";
}