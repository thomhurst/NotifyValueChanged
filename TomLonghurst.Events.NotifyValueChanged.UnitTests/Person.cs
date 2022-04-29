using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;

namespace TomLonghurst.Events.NotifyValueChanged.UnitTests;

[NotifyTypeValueChange(typeof(string))]
[NotifyTypeValueChange(typeof(int))]
[NotifyAnyValueChange]
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

    public string UppercaseDescription => Description.ToUpper();
}