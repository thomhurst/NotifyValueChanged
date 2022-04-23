using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TomLonghurst.Events.NotifyContextChanged.SourceGeneration;

[Generator]
public class NotifyContextChangeGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new FieldSyntaxReciever());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not FieldSyntaxReciever syntaxReciever)
        {
            return;
        }
        
        var notifySymbol = context.Compilation.GetTypeByMetadataName(typeof(NotifyContextChangeAttribute).FullName);
        
        foreach (var containingClassGroup in syntaxReciever.IdentifiedFields.GroupBy(x => x.ContainingType)) 
        {
            var containingClass = containingClassGroup.Key;
            var namespaceSymbol = containingClass.ContainingNamespace;
            var hasNotifyImplementtion = containingClass.Interfaces.Contains(notifySymbol);
            var source = GenerateClass(context, containingClass, namespaceSymbol, containingClassGroup.ToList(), !hasNotifyImplementtion);
            context.AddSource($"{containingClass.Name}_NotifyContextChanged.generated", SourceText.From(source, Encoding.UTF8));
        }
    }
    
    private string GenerateClass(GeneratorExecutionContext context, INamedTypeSymbol @class, INamespaceSymbol @namespace, List <IFieldSymbol> fields, bool implementNotifyPropertyChange) {
        var classBuilder = new StringBuilder();
        classBuilder.AppendLine("using System;");
        if (implementNotifyPropertyChange) {
            var notifyPropertyChangedSymbol = context.Compilation.GetTypeByMetadataName(typeof(NotifyContextChangeAttribute).FullName);
            var callerMemberSymbol = context.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.CallerMemberNameAttribute");
            classBuilder.AppendLine($"using {notifyPropertyChangedSymbol.ContainingNamespace};");
            classBuilder.AppendLine($"using {callerMemberSymbol.ContainingNamespace};");
            classBuilder.AppendLine($"namespace {@namespace.ToDisplayString()}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"public partial class {@class.Name}:{notifyPropertyChangedSymbol.Name}");
            classBuilder.AppendLine("{");
        } else {
            classBuilder.AppendLine($"namespace {@namespace.ToDisplayString()}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"public partial class {@class.Name}");
            classBuilder.AppendLine("{");
        }
        
        foreach(var field in fields) {
            var fieldName = field.Name;
            var fieldType = field.Type.Name;
            var propertyName = NormalizePropertyName(fieldName);
            classBuilder.AppendLine($"public {fieldType} {propertyName}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"get => {fieldName};");
            classBuilder.AppendLine("set");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"if({fieldName} == value)");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine("\treturn;");
            classBuilder.AppendLine("}");
            classBuilder.AppendLine($"var previousValue = {fieldName};");
            classBuilder.AppendLine($"{fieldName} = value;");
            classBuilder.AppendLine($"Notify{propertyName}ContextChanged(previousValue, value);");
            classBuilder.AppendLine("}");
            classBuilder.AppendLine("}");
            
            classBuilder.AppendLine(GenerateContextChangeImplementation(propertyName, fieldType));
        }
        
        classBuilder.AppendLine("}");
        classBuilder.AppendLine("}");
        
        return classBuilder.ToString();
    }
    private string NormalizePropertyName(string fieldName) {
        return Regex.Replace(fieldName, "_[a-z]", delegate(Match m) {
            return m.ToString().TrimStart('_').ToUpper();
        });
    }
    private string GenerateContextChangeImplementation(string propertyName, string fieldType)
    {
        return $@"
        public event ContextChangedEventHandler<{fieldType}> On{propertyName}ContextChange;
        
        public void Notify{propertyName}ContextChanged({fieldType} previousValue, {fieldType} newValue, [CallerMemberName] string propertyName = """") 
        {{
            On{propertyName}ContextChange?.Invoke(this, new ContextChangedEventArgs<{fieldType}>(propertyName, previousValue, newValue));
        }}
        ";
    }
}