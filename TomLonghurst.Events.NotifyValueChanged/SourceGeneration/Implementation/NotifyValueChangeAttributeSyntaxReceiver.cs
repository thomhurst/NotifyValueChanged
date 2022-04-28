using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

internal class NotifyValueChangeAttributeSyntaxReceiver : ISyntaxContextReceiver
{
    public List<IFieldSymbol> IdentifiedFields { get; } = new();
    public List<IPropertySymbol> IdentifiedProperties { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is FieldDeclarationSyntax fieldDeclaration && fieldDeclaration.AttributeLists.Any())
        {
            ProcessField(context, fieldDeclaration);
        }
        
        if (context.Node is PropertyDeclarationSyntax propertyDeclaration)
        {
            ProcessProperty(context, propertyDeclaration);
        }
    }

    private static void ProcessProperty(GeneratorSyntaxContext context, PropertyDeclarationSyntax propertyDeclaration)
    {
        var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

        if (property is not IPropertySymbol propertySymbol)
        {
            return;
        }

        if (IsBasedOnNotifyField(property, context))
        {
            IdentifiedProperties.Add(propertySymbol);
        }
    }

    private void ProcessField(GeneratorSyntaxContext context, FieldDeclarationSyntax fieldDeclaration)
    {
        var variableDeclaration = fieldDeclaration.Declaration.Variables;
        foreach (var field in variableDeclaration.Select(variable => context.SemanticModel.GetDeclaredSymbol(variable)))
        {
            if (field is IFieldSymbol fieldInfo && fieldInfo.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == typeof(NotifyValueChangeAttribute).FullName))
            {
                IdentifiedFields.Add(fieldInfo);
            }
        }
    }
    
    private bool IsBasedOnNotifyField(ISymbol? symbol, GeneratorSyntaxContext context)
    {
        switch (symbol)
        {
            case null:
                return false;
            case IFieldSymbol when symbol.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == typeof(NotifyValueChangeAttribute).FullName):
                return true;
            case IPropertySymbol propertySymbol:
            {
                var location = propertySymbol.GetMethod.Locations.FirstOrDefault();

                if (location is null)
                {
                    return false;
                }

                var getterNode = location.SourceTree.GetRoot().FindNode(location.SourceSpan);
            
                if (getterNode.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
                    .Select(node => context.SemanticModel.GetSymbolInfo(node).Symbol)
                    .Any(nodeSymbol => IsBasedOnNotifyField(nodeSymbol, context)))
                {
                    return true;
                }

                break;
            }
        }

        return false;
    }
}