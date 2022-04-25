// See https://aka.ms/new-console-template for more information

using TomLonghurst.Events.NotifyValueChanged.Example;

var myClass = new MyClass();
myClass.OnMyNameValueChange += (_, eventArgs) =>
{
    Console.WriteLine($"Name was: {eventArgs.PreviousValue} and is now {eventArgs.NewValue}\n");
};

myClass.OnMyAgeValueChange += (_, eventArgs) =>
{
    Console.WriteLine($"Age was: {eventArgs.PreviousValue} and is now {eventArgs.NewValue}\n");
};

myClass.OnIsMaleValueChange += (_, eventArgs) =>
{
    Console.WriteLine($"IsMale was: {eventArgs.PreviousValue} and is now {eventArgs.NewValue}\n");
};

myClass.MyName = "Tom";
myClass.MyName = "Tom Longhurst";
myClass.MyName = null;

myClass.MyAge = 29;
myClass.MyAge = 0;

myClass.IsMale = false;
myClass.IsMale = true;

var myClassAsInterface = myClass as IMyClass;
myClassAsInterface.OnMyAgeValueChange += (_, _) => Console.WriteLine("My second event!");

myClass.MyAge = 29;
myClass.MyAge = 0;