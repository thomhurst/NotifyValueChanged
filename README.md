# NotifyValueChanged - Automatic Event Firing on Property Value Changes

A source generated approach, to turn your backing fields into properties that can fire events when their value changes - Automagically!

[![nuget](https://img.shields.io/nuget/v/TomLonghurst.Events.NotifyValueChanged.svg)](https://www.nuget.org/packages/TomLonghurst.Events.NotifyValueChanged/)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/16305948c33040f0982da5322df8d8e1)](https://www.codacy.com/gh/thomhurst/NotifyValueChanged/dashboard?utm_source=github.com&utm_medium=referral&utm_content=thomhurst/NotifyValueChanged&utm_campaign=Badge_Grade)
[![CodeFactor](https://www.codefactor.io/repository/github/thomhurst/notifyvaluechanged/badge)](https://www.codefactor.io/repository/github/thomhurst/notifyvaluechanged)

## Support

If this library helped you, consider buying me a coffee

<a href="https://www.buymeacoffee.com/tomhurst" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

## Installation

Install via Nuget
`Install-Package TomLonghurst.Events.NotifyValueChanged`

## Usage

### Fields
-   Make your class `partial` 
-   Declare a `private` _field_ - This is the backing field for which the property will be generated for.
-   Add the `[NotifyValueChange]` attribute to the field
-   That's it!

```csharp
public partial class Person
{
    [NotifyValueChange]
    private string _name;
}
```

Your class now has a property called `Name` - And you can subscribe to an event called `OnNameValueChange` that will fire whenever the value of `Name` changes.

You can do this for multiple fields, and each one should generate you a separate event that you can subscribe to called `On{PropertyName}ValueChange`

```csharp
var person = new Person();

person.OnNameValueChange += (sender, eventArgs) =>
{
    Console.WriteLine($"Name was: '{eventArgs.PreviousValue}' and is now '{eventArgs.NewValue}'\n");
};

person.Name = "Tom"; // Event will fire and log to the console "Name was: '' and is now 'Tom'"
person.Name = "Tom"; // No event will fire because the value hasn't changed
person.Name = ""; // Event will fire and log to the console "Name was: 'Tom' and is now ''"
```

### Computed Properties
If you have a property that has its value computed based on the value of a backing field with a `[NotifyValueChange]` attribute, then this should automatically produce an event to subscribe to also.
This event will fire when any of the backing field's value changes.

```csharp
public partial class Person
{
    [NotifyValueChange]
    private string _firstName;
    
    [NotifyValueChange]
    private string _lastName;
    
    public string FullName => $"{_firstName} {_lastName}";
}
```

```csharp
var person = new Person 
{
    FirstName = "Tom",
    LastName = "Jones"
};

person.OnFullNameValueChange += (sender, eventArgs) =>
{
    Console.WriteLine($"The Person's Full Name was: '{eventArgs.PreviousValue}' and is now '{eventArgs.NewValue}'\n");
};

person.LastName = "Longhurst"; // Will output The Person's Full Name was: 'Tom Jones' and is now 'Tom Longhurst'
```

### Interfaces
If your class implements an interface and you want this event to be exposed on the interface, then:

-   Make your interface partial
-   Declare the property on the interface as normal
-   Add the attribute `[GenerateInterfaceValueChangeEvent`]

```csharp
public partial interface IPerson
{
    [GenerateInterfaceValueChangeEvent]
    public string Name { get; }
}
```

### Class Attributes

**NotifyAnyValueChange**
Any field with the `[NotifyValueChange]` attribute will also fire an any value changed event

```csharp
[NotifyAnyValueChange]
public partial class Person
{
    [NotifyValueChange]
    private string _name; // Will fire the AnyValueChange event because it has the NotifyValueChange attribute
    
    [NotifyValueChange]
    private int _age; // Will fire the AnyValueChange event because it has the NotifyValueChange attribute
}
```

```csharp
var person = new Person();

person.OnAnyValueChange += (sender, eventArgs) =>
{
    Console.WriteLine($"Property Name: {eventArgs.PropertyName} | Previous Value: {eventArgs.PreviousValue} | New Value: {eventArgs.NewValue}\n");
};
```

**NotifyTypeValueChange**
Any field with the `[NotifyValueChange]` attribute will also fire an type specific value changed event if that type was passed into the `[NotifyTypeValueChange(type)] attribute

```csharp
[NotifyTypeValueChange(typeof(string))]
public partial class Person
{
    [NotifyValueChange]
    private string _name; // Will fire the OnTypeStringValueChange event because it has the NotifyValueChange attribute combined with the NotifyTypeValueChange(string) attribute
    
    [NotifyValueChange]
    private int _age; // Will not fire the OnTypeStringValueChange event because it is not a string
}
```

```csharp
var person = new Person();

person.OnTypeStringValueChange += (sender, eventArgs) =>
{
    Console.WriteLine($"Property Name: {eventArgs.PropertyName} | Previous Value: {eventArgs.PreviousValue} | New Value: {eventArgs.NewValue}\n");
};
```
