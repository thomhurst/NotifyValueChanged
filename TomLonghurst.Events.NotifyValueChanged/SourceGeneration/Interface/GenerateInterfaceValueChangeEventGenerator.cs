using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomLonghurst.Events.NotifyValueChanged.Extensions;
using TomLonghurst.Events.NotifyValueChanged.Helpers;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Interface;

[Generator]
public class GenerateInterfaceValueChangeEventGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new PropertyGenerateInterfaceValueChangeEventAttributeSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not PropertyGenerateInterfaceValueChangeEventAttributeSyntaxReceiver syntaxReciever)
        {
            return;
        }

        var fieldsThatNeedInterfacesGenerating = new List<IPropertySymbol>();
        foreach (var containingInterfaceGroup in syntaxReciever.IdentifiedProperties.GroupBy(x => x.ContainingType)) 
        {
            var containingInterface = containingInterfaceGroup.Key;
            var namespaceSymbol = containingInterface.ContainingNamespace;
            var properties = containingInterfaceGroup.ToList();
            
            var source = GenerateInterface(context, containingInterface, namespaceSymbol, properties);
            fieldsThatNeedInterfacesGenerating.AddRange(properties);
            context.AddSource($"{containingInterface.Name}_GenerateValueChangedEventForInterface.generated", SourceText.From(source, Encoding.UTF8));
        }
    }
    
    private string GenerateInterface(GeneratorExecutionContext context, INamedTypeSymbol @interface, INamespaceSymbol @namespace, List<IPropertySymbol> properties) {
        var stringWriter = new StringWriter();
        var classBuilder = new CodeGenerationTextWriter(stringWriter);

        classBuilder.WriteLine("using System;");
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventArgs<>)));
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventHandler<>)));
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(CallerMemberNameAttribute)));
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(GenerateInterfaceValueChangeEventAttribute)));
        classBuilder.WriteLine($"namespace {@namespace.ToDisplayString()}");
        classBuilder.WriteLine("{");
        
        classBuilder.WriteLine($"public partial interface {@interface.Name}");
        classBuilder.WriteLine("{");

        foreach(var property in properties) {
            var fullyQualifiedFieldType = property.Type.GetFullyQualifiedType();
            classBuilder.WriteLine($"public event {nameof(ValueChangedEventHandler<object>)}<{fullyQualifiedFieldType}> On{property.Name}ValueChange;");
            classBuilder.WriteLine();
        }

        classBuilder.WriteLine("}");
        classBuilder.WriteLine("}");

        classBuilder.Flush();
        return stringWriter.ToString();
    }
}