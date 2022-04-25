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
        context.RegisterForSyntaxNotifications(() => new PropertySyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not PropertySyntaxReceiver syntaxReciever)
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
        var classBuilder = new StringBuilder();

        classBuilder.AppendLine("using System;");
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(INotifyValueChanged<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventArgs<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventHandler<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(CallerMemberNameAttribute)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(GenerateInterfaceValueChangeEventAttribute)));
        classBuilder.AppendLine($"namespace {@namespace.ToDisplayString()}");
        classBuilder.AppendLine("{");
        
        classBuilder.AppendLine($"\tpublic partial interface {@interface.Name}");
        classBuilder.AppendLine("\t{");

        foreach(var property in properties) {
            var fullyQualifiedFieldType = property.Type.GetFullyQualifiedType();
            classBuilder.AppendLine($"\t\tpublic event {nameof(ValueChangedEventHandler<object>)}<{fullyQualifiedFieldType}> On{property.Name}ValueChange;");
            classBuilder.AppendLine();
        }

        classBuilder.AppendLine("\t}");
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }
}