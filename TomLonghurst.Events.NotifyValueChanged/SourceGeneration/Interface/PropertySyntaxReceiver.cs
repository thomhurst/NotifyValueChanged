using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Interface;

internal class PropertySyntaxReceiver : ISyntaxContextReceiver
{
    public List<IPropertySymbol> IdentifiedProperties { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration || !propertyDeclaration.AttributeLists.Any())
        {
            return;
        }

        var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
 
        if(property is IPropertySymbol propertySymbol 
           && property.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == typeof(GenerateInterfaceValueChangeEventAttribute).FullName))
        {
            IdentifiedProperties.Add(propertySymbol);
        }
    }
}