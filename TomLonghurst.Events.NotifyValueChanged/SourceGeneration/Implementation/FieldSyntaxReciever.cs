using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

internal class FieldSyntaxReciever : ISyntaxContextReceiver
{
    public List<IFieldSymbol> IdentifiedFields { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclaration || !fieldDeclaration.AttributeLists.Any())
        {
            return;
        }

        var variableDeclaration = fieldDeclaration.Declaration.Variables;
        foreach (var field in variableDeclaration.Select(variable => context.SemanticModel.GetDeclaredSymbol(variable)))
        {
            if (field is IFieldSymbol fieldInfo && fieldInfo.GetAttributes().Any(x=> x.AttributeClass.ToDisplayString() == typeof(NotifyValueChangeAttribute).FullName))
            {
                IdentifiedFields.Add(fieldInfo);
            }
        }
    }
}