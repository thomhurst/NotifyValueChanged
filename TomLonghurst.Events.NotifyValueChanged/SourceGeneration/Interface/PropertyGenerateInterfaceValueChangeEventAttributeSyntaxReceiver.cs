using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomLonghurst.Events.NotifyValueChanged.Extensions;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Interface;

internal class PropertyGenerateInterfaceValueChangeEventAttributeSyntaxReceiver : ISyntaxContextReceiver
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
           && property.HasAttribute<GenerateInterfaceValueChangeEventAttribute>())
        {
            IdentifiedProperties.Add(propertySymbol);
        }
    }
}