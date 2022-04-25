using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomLonghurst.Events.NotifyContextChanged.Extensions;
using TomLonghurst.Events.NotifyContextChanged.Helpers;

namespace TomLonghurst.Events.NotifyContextChanged.SourceGeneration.Interface;

[Generator]
public class GenerateInterfaceContextChangeEventGenerator : ISourceGenerator
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
            context.AddSource($"{containingInterface.Name}_GenerateContextChangeEventForInterface.generated", SourceText.From(source, Encoding.UTF8));
        }
    }
    
    private string GenerateInterface(GeneratorExecutionContext context, INamedTypeSymbol @interface, INamespaceSymbol @namespace, List<IPropertySymbol> properties) {
        var classBuilder = new StringBuilder();

        classBuilder.AppendLine("using System;");
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(INotifyContextChanged<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(ContextChangedEventArgs<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(ContextChangedEventHandler<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(CallerMemberNameAttribute)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(GenerateInterfaceContextChangeEventAttribute)));
        classBuilder.AppendLine($"namespace {@namespace.ToDisplayString()}");
        classBuilder.AppendLine("{");
        
        classBuilder.AppendLine($"\tpublic partial interface {@interface.Name}");
        classBuilder.AppendLine("\t{");

        foreach(var property in properties) {
            var fullyQualifiedFieldType = property.Type.GetFullyQualifiedType();
            classBuilder.AppendLine($"\t\tpublic event ContextChangedEventHandler<{fullyQualifiedFieldType}> On{property.Name}ContextChange;");
            classBuilder.AppendLine();
        }

        classBuilder.AppendLine("\t}");
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }
}