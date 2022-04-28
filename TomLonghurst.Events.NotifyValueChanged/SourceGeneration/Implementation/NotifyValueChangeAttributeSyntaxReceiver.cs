using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

internal class NotifyValueChangeAttributeSyntaxReceiver : ISyntaxContextReceiver
{
    public List<IFieldSymbol> IdentifiedFields { get; } = new();
    public Dictionary<IPropertySymbol, IFieldSymbol> IdentifiedPropertiesAndAssociatedFields { get; } = new();

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

    private void ProcessProperty(GeneratorSyntaxContext context, PropertyDeclarationSyntax propertyDeclaration)
    {
        var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

        if (property is not IPropertySymbol propertySymbol)
        {
            return;
        }

        if (IsBasedOnNotifyField(property, context, out var associatedField))
        {
            IdentifiedPropertiesAndAssociatedFields.Add(propertySymbol, associatedField);
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
    
    private bool IsBasedOnNotifyField(ISymbol? symbol, GeneratorSyntaxContext context, out IFieldSymbol? fieldSymbol)
    {
        fieldSymbol = null;
        switch (symbol)
        {
            case null:
                return false;
            case IFieldSymbol castFieldSymbol when castFieldSymbol.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == typeof(NotifyValueChangeAttribute).FullName):
                fieldSymbol = castFieldSymbol;
                return true;
            case IPropertySymbol propertySymbol:
            {
                var location = propertySymbol.GetMethod.Locations.FirstOrDefault();

                if (location is null)
                {
                    return false;
                }

                var getterNode = location.SourceTree.GetRoot().FindNode(location.SourceSpan);

                foreach (var nodeSymbol in getterNode.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
                             .Select(node => context.SemanticModel.GetSymbolInfo(node).Symbol))
                {
                    if (IsBasedOnNotifyField(nodeSymbol, context, out fieldSymbol))
                    {
                        return true;
                    }
                }

                break;
            }
        }

        return false;
    }
}