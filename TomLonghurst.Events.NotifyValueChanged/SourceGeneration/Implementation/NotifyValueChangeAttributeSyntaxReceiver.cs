using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomLonghurst.Events.NotifyValueChanged.Extensions;
using TomLonghurst.Events.NotifyValueChanged.Helpers;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

internal class NotifyValueChangeAttributeSyntaxReceiver : ISyntaxContextReceiver
{
    public List<IFieldSymbol> IdentifiedFields { get; } = new();
    public Dictionary<IPropertySymbol, List<IFieldSymbol>> IdentifiedPropertiesAndAssociatedFields { get; } = new();

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

        if (IsBasedOnNotifyFields(property, context, out var associatedField))
        {
            IdentifiedPropertiesAndAssociatedFields.Add(propertySymbol, associatedField);
        }
    }

    private void ProcessField(GeneratorSyntaxContext context, FieldDeclarationSyntax fieldDeclaration)
    {
        var variableDeclaration = fieldDeclaration.Declaration.Variables;
        foreach (var field in variableDeclaration.Select(variable => context.SemanticModel.GetDeclaredSymbol(variable)))
        {
            if (field is IFieldSymbol fieldInfo && fieldInfo.HasAttribute<NotifyValueChangeAttribute>())
            {
                IdentifiedFields.Add(fieldInfo);
            }
        }
    }
    
    private bool IsBasedOnNotifyFields(ISymbol? symbol, GeneratorSyntaxContext context, out List<IFieldSymbol> fieldSymbols)
    {
        fieldSymbols = new List<IFieldSymbol>();
        switch (symbol)
        {
            case null:
                break;
            case IFieldSymbol castFieldSymbol when castFieldSymbol.HasAttribute<NotifyValueChangeAttribute>():
                fieldSymbols.Add(castFieldSymbol);
                break;
            case IPropertySymbol propertySymbol:
            {
                var location = propertySymbol.GetMethod.Locations.FirstOrDefault();

                var locationSourceTree = location?.SourceTree;

                if (locationSourceTree is null)
                {
                    break;
                }
                
                var getterNode = locationSourceTree.GetRoot().FindNode(location.SourceSpan);

                foreach (var node in getterNode.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
                {
                    var nodeSymbol = context.SemanticModel.GetSymbolInfo(node).Symbol;

                    if (nodeSymbol is IMethodSymbol)
                    {
                        continue;
                    }
                    
                    if (nodeSymbol != null && IsBasedOnNotifyFields(nodeSymbol, context, out var innerFieldSymbols))
                    {
                        fieldSymbols.AddRange(innerFieldSymbols);
                        continue;
                    }

                    var fieldDeclarationSyntaxes = node.GetLocation().SourceTree.GetRoot().DescendantNodes()
                        .OfType<FieldDeclarationSyntax>();

                    var symbols = fieldDeclarationSyntaxes.SelectMany(x => x.Declaration.Variables)
                        .Select(x => context.SemanticModel.GetDeclaredSymbol(x));
                    
                    var fields = symbols.OfType<IFieldSymbol>().ToList();

                    var fieldsWithCustomPropertyNameAttribute = fields.Where(x =>
                        x.GetAttributePropertyValue<NotifyValueChangeAttribute, string>(a => a.PropertyName) == node.Identifier.Text)
                        .ToList();

                    if (fieldsWithCustomPropertyNameAttribute.Any())
                    {
                        fieldSymbols.AddRange(fieldsWithCustomPropertyNameAttribute);
                        continue;
                    }

                    var fieldsWithMatchingNameAndNotifyAttribute = fields
                        .Where(x => string.Equals(x.Name.TrimStart('_'), node.Identifier.Text, StringComparison.OrdinalIgnoreCase))
                        .Where(x => x.HasAttribute<NotifyValueChangeAttribute>())
                        .ToList();
                    
                    if (fieldsWithMatchingNameAndNotifyAttribute.Any())
                    {
                        fieldSymbols.AddRange(fieldsWithMatchingNameAndNotifyAttribute);
                        continue;
                    }
                }

                break;
            }
            default: break;
        }

        return fieldSymbols.Any();
    }
}