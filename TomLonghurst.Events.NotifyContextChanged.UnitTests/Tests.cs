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
    public void When_Value_Never_Changes_Then_Dont_Invoke_Event()
    {
        _myClass.OnMyString1ContextChange += (sender, args) =>
        {
            _dummyInterface.Object.TwoStrings(args.PreviousContext, args.NewContext, args.PropertyName);
        };
        
        _dummyInterface.Verify(x => x.TwoStrings(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>()), Times.Never);
    }
    
    [Test]
    public void When_Value_Changes_Then_Invoke_Event()
    {
        _myClass.OnMyString1ContextChange += (sender, args) =>
        {
            _dummyInterface.Object.TwoStrings(args.PreviousContext, args.NewContext, args.PropertyName);
        };

        _myClass.MyString1 = "Hi";
        
        _dummyInterface.Verify(x => x.TwoStrings(null, "Hi", nameof(MyClass.MyString1)), Times.Once);
    }
    
    [Test]
    public void When_Different_Value_Changes_Then_Dont_Invoke_Event()
    {
        _myClass.OnMyString1ContextChange += (sender, args) =>
        {
            _dummyInterface.Object.TwoStrings(args.PreviousContext, args.NewContext, args.PropertyName);
        };

        _myClass.MyString2 = "Hi";
        
        _dummyInterface.Verify(x => x.TwoStrings(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>()), Times.Never);
    }
    
    [Test]
    public void When_Value_Changes_X_Times_Then_Invoke_Event()
    {
        _myClass.OnMyString1ContextChange += (sender, args) =>
        {
            _dummyInterface.Object.TwoStrings(args.PreviousContext, args.NewContext, args.PropertyName);
        };

        for (int i = 0; i < 100; i++)
        {
            _myClass.MyString1 = i.ToString();
        }

        _dummyInterface.Verify(x => x.TwoStrings(It.IsAny<string?>(), It.IsAny<string?>(), nameof(MyClass.MyString1)), Times.Exactly(100));
    }
    
    [Test]
    public void When_Value_Is_Same_Then_Dont_Invoke_Event()
    {
        _myClass.OnMyString1ContextChange += (sender, args) =>
        {
            _dummyInterface.Object.TwoStrings(args.PreviousContext, args.NewContext, args.PropertyName);
        };

        for (int i = 0; i < 100; i++)
        {
            _myClass.MyString1 = "Hi";
        }

        _dummyInterface.Verify(x => x.TwoStrings(null, "Hi", nameof(MyClass.MyString1)), Times.Once);
        _dummyInterface.Verify(x => x.TwoStrings("Hi", "Hi", nameof(MyClass.MyString1)), Times.Never);
    }
    
    [Test]
    public void When_Value_Changes_Then_Invoke_Generic_Type_Event()
    {
        _myClass.OnTypeStringContextChange += (sender, args) =>
        {
            _dummyInterface.Object.TwoStrings(args.PreviousContext, args.NewContext, args.PropertyName);
        };

        _myClass.MyString1 = "Hi";
        _myClass.MyString2 = "Hello";
        
        _dummyInterface.Verify(x => x.TwoStrings(null, "Hi", nameof(MyClass.MyString1)), Times.Once);
        _dummyInterface.Verify(x => x.TwoStrings(null, "Hello", nameof(MyClass.MyString2)), Times.Once);
    }
}