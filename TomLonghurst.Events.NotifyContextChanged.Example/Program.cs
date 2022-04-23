// See https://aka.ms/new-console-template for more information

using TomLonghurst.Events.NotifyContextChanged.Example;


var myClass = new MyClass();
myClass.OnMyNameContextChange += (sender, eventArgs) =>
{
    Console.WriteLine($"Property Name: {eventArgs.PropertyName}");
    Console.WriteLine($"Previous Value: {eventArgs.PreviousContext}");
    Console.WriteLine($"Previous Value: {eventArgs.NewContext}");
    Console.WriteLine();
};

myClass.MyName = "Tom";
myClass.MyName = "Tom Longhurst";
myClass.MyName = null;

myClass.MyAge