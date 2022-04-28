using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using TomLonghurst.Events.NotifyValueChanged.Extensions;
using TomLonghurst.Events.NotifyValueChanged.Helpers;

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

                foreach (var node in getterNode.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
                {
                    var nodeSymbol = context.SemanticModel.GetSymbolInfo(node).Symbol;
                    if (nodeSymbol != null && IsBasedOnNotifyField(nodeSymbol, context, out fieldSymbol))
                    {
                        return true;
                    }

                    var fieldDeclarationSyntaxes = node.GetLocation().SourceTree.GetRoot().DescendantNodes()
                        .OfType<FieldDeclarationSyntax>();

                    var symbols = fieldDeclarationSyntaxes.SelectMany(x => x.Declaration.Variables)
                        .Select(x => context.SemanticModel.GetDeclaredSymbol(x));
                    
                    var fields = symbols.OfType<IFieldSymbol>();

                    var fieldWithCustomPropertyNameAttribute = fields.FirstOrDefault(x =>
                        x.GetAttributePropertyValue<NotifyValueChangeAttribute, string>(a => a.PropertyName) == node.Identifier.Text);

                    if (fieldWithCustomPropertyNameAttribute is not null)
                    {
                        fieldSymbol = fieldWithCustomPropertyNameAttribute;
                        return true;
                    }
                }

                break;
            }
        }

        return false;
    }
}