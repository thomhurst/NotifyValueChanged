using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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
        var callerMemberSymbol = context.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.CallerMemberNameAttribute");
        
        classBuilder.AppendLine("using System;");
        classBuilder.AppendLine($"using {callerMemberSymbol.ContainingNamespace};");
        classBuilder.AppendLine($"namespace {@namespace.ToDisplayString()}");
        classBuilder.AppendLine("{");
        
        classBuilder.AppendLine($"public partial interface {@interface.Name}");
        classBuilder.AppendLine("{");

        foreach(var property in properties) {
            var fullyQualifiedFieldType = GetFullyQualifiedFieldType(property);
            classBuilder.AppendLine($"public event ContextChangedEventHandler<{fullyQualifiedFieldType}> On{property.Name}ContextChange;");
        }

        classBuilder.AppendLine("}");
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }

    private static string GetFullyQualifiedFieldType(IPropertySymbol property)
    {
        return property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}