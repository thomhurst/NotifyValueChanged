// See https://aka.ms/new-console-template for more information

using TomLonghurst.Events.NotifyContextChanged.Example;


var myClass = new MyClass();
myClass.OnMyNameContextChange += (sender, eventArgs) =>
{
    Console.WriteLine($"Name was: {eventArgs.PreviousContext} and is now {eventArgs.NewContext}\n");
};

myClass.OnMyAgeContextChange += (sender, eventArgs) =>
{
    Console.WriteLine($"Age was: {eventArgs.PreviousContext} and is now {eventArgs.NewContext}\n");
};

myClass.OnIsMaleContextChange += (sender, eventArgs) =>
{
    Console.WriteLine($"IsMale was: {eventArgs.PreviousContext} and is now {eventArgs.NewContext}\n");
};

myClass.MyName = "Tom";
myClass.MyName = "Tom Longhurst";
myClass.MyName = null;

myClass.MyAge = 29;
myClass.MyAge = 0;

myClass.IsMale = false;
myClass.IsMale = true;

var myClassAsInterface = myClass as IMyClass;
myClassAsInterface.OnMyAgeContextChange += (sender, eventArgs) => Console.WriteLine("My second event!");

myClass.MyAge = 29;
myClass.MyAge = 0;