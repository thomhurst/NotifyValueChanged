using System;
using Moq;
using NUnit.Framework;

namespace TomLonghurst.Events.NotifyContextChanged.UnitTests;

public class Tests
{
    private MyClass _myClass;
    private Mock<IDummyInterface> _dummyInterface;

    [SetUp]
    public void Setup()
    {
        _dummyInterface = new Mock<IDummyInterface>();
        _myClass = new MyClass();
    }

    [Test]
    public void Test1()
    {
        // _myClass += (sender, args) =>
        // {
        //     Console.WriteLine(args.PropertyName);
        //     Console.WriteLine(args.PreviousContext);
        //     Console.WriteLine(args.NewContext);
        // };
        //
        // _myClass.MyNullableInt1 = 1;
    }
}