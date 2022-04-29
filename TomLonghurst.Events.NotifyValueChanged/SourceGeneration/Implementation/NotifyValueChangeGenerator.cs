#if DEBUG
using System.Diagnostics;
#endif

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomLonghurst.Events.NotifyValueChanged.Extensions;
using TomLonghurst.Events.NotifyValueChanged.Helpers;
using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Attributes;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

[Generator]
public class NotifyValueChangeGenerator : ISourceGenerator
{
    private NotifyValueChangeAttributeSyntaxReceiver _notifyValueChangeAttributeSyntaxReceiver;

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
        
        context.RegisterForSyntaxNotifications(() => new NotifyValueChangeAttributeSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not NotifyValueChangeAttributeSyntaxReceiver syntaxReciever)
        {
            return;
        }

        _notifyValueChangeAttributeSyntaxReceiver = syntaxReciever;

        var typeEventsThatNeedInterfacesGenerating = new List<ITypeSymbol?>();
        var allFields = new List<IFieldSymbol>();
        var allProperties = new List<IPropertySymbol>();

        foreach (var containingClassGroup in syntaxReciever.IdentifiedFields.GroupBy(x => x.ContainingType)) 
        {
            var containingClass = containingClassGroup.Key;
            var namespaceSymbol = containingClass.ContainingNamespace;
            
            var typesWithTypeValueChangeEvents = containingClass
                .GetAttributes()
                .Where(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(NotifyTypeValueChangeAttribute).FullName)
                .Select(x => x.ConstructorArguments.First().Value as ITypeSymbol);

            var properties = syntaxReciever.IdentifiedPropertiesAndAssociatedFields.Keys
                .Where(x => SymbolEqualityComparer.Default.Equals(x.ContainingType, containingClass))
                .ToList();
            allProperties.AddRange(properties);
            
            var fields = containingClassGroup.ToList();
            allFields.AddRange(fields);
            
            var source = GenerateClass(context, containingClass, namespaceSymbol, fields, properties);
            typeEventsThatNeedInterfacesGenerating.AddRange(typesWithTypeValueChangeEvents);
            context.AddSource($"{containingClass.ContainingNamespace.ToDisplayString().Replace('.', '_')}_{containingClass.Name}_NotifyValueChanged.generated", SourceText.From(source, Encoding.UTF8));
        }

        var interfaceSource = WriteTypeEventInterfaces(context, typeEventsThatNeedInterfacesGenerating, allFields, allProperties);
        context.AddSource("INotifyValueChangedInterfaces_NotifyValueChanged.generated", SourceText.From(interfaceSource, Encoding.UTF8));
    }
    
    private string GenerateClass(GeneratorExecutionContext context, INamedTypeSymbol @class, INamespaceSymbol @namespace, List<IFieldSymbol> fields, List<IPropertySymbol> properties) {
        var classBuilder = new CodeGenerationTextWriter();
        classBuilder.WriteLine(context.GetUsingStatementsForTypes(typeof(string), typeof(EqualityComparer<>), typeof(ValueChangedEventArgs<>), typeof(ValueChangedEventHandler<>), typeof(CallerMemberNameAttribute)));
        classBuilder.WriteLine($"namespace {@namespace.ToDisplayString()}");
        classBuilder.WriteLine("{");
        
        classBuilder.WriteLine($"public partial class {@class.Name}");
        
        var listOfInterfaces = fields.Concat<ISymbol>(properties).Where(x => ShouldGenerateTypeValueChangeImplementation(x, @class)).Select(x => x.GetSymbolType().GetSimpleTypeName()).Select(simpleFieldType => $"INotifyType{simpleFieldType}ValueChanged").Distinct().ToList();
        var shouldGenerateAnyValueChangeImplementation = ShouldGenerateAnyValueChangeImplementation(@class);
        if (shouldGenerateAnyValueChangeImplementation)
        {
            listOfInterfaces.Add("INotifyValueChanged");
        }

        if (listOfInterfaces.Any())
        {
            classBuilder.WriteLine(":");
            var commaSeparatedListOfInterfaces = listOfInterfaces.Aggregate((a, x) => $"{a}, {x}");
            classBuilder.WriteLine($"{commaSeparatedListOfInterfaces}"); 
        }

        classBuilder.WriteLine("{");

        foreach(var field in fields) {
            var propertiesDependentOnField = _notifyValueChangeAttributeSyntaxReceiver.IdentifiedPropertiesAndAssociatedFields
                .Where(x => x.Value.Contains(field, SymbolEqualityComparer.Default)).ToList();

            var attributeOnField = field.GetNotifyValueChangeAttribute();
            var propertyAccessLevelMetadata = PropertyAccessLevelHelper.GetPropertyAccessLevelMetadata(attributeOnField);
            
            var fullyQualifiedFieldType = field.Type.GetFullyQualifiedType();
            var simpleFieldType = field.Type.GetSimpleTypeName();
            var fieldName = field.Name;
            var propertyName = field.GetPropertyName();
            classBuilder.WriteLine($"private DateTimeOffset _dateTime{propertyName}Set;");
            classBuilder.WriteLine($"{propertyAccessLevelMetadata.MainProperty}{fullyQualifiedFieldType} {propertyName}");
            classBuilder.WriteLine("{");
            classBuilder.WriteLine($"{propertyAccessLevelMetadata.Getter}get => {fieldName};");
            classBuilder.WriteLine($"{propertyAccessLevelMetadata.Setter}set");
            classBuilder.WriteLine("{");
            classBuilder.WriteLine($"if (EqualityComparer<{fullyQualifiedFieldType}>.Default.Equals({fieldName}, value))");
            classBuilder.WriteLine("{");
            classBuilder.WriteLine("return;");
            classBuilder.WriteLine("}");
            classBuilder.WriteLine();

            foreach (var propertyDependentOnField in propertiesDependentOnField)
            {
                classBuilder.WriteLine($"var previousValue{propertyDependentOnField.Key.Name} = {propertyDependentOnField.Key.Name};");
            }
            
            classBuilder.WriteLine($"var previousValueDateTimeSet = _dateTime{propertyName}Set;");
            classBuilder.WriteLine($"var previousValue = {fieldName};");
            classBuilder.WriteLine();
            classBuilder.WriteLine($"{fieldName} = value;"); 
            classBuilder.WriteLine($"_dateTime{propertyName}Set = DateTimeOffset.UtcNow;");
            classBuilder.WriteLine();
            classBuilder.WriteLine($"Notify{propertyName}ValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");

            foreach (var propertyDependentOnField in propertiesDependentOnField)
            {
                classBuilder.WriteLine($"Notify{propertyDependentOnField.Key.Name}ValueChanged(previousValue{propertyDependentOnField.Key.Name}, {propertyDependentOnField.Key.Name}, previousValueDateTimeSet, _dateTime{propertyName}Set, \"{propertyDependentOnField.Key.Name}\");");
            }

            if (ShouldGenerateTypeValueChangeImplementation(field, @class))
            {
                classBuilder.WriteLine($"NotifyType{simpleFieldType}ValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");
            }
            
            if (shouldGenerateAnyValueChangeImplementation)
            {
                classBuilder.WriteLine($"NotifyAnyValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");
            }

            classBuilder.WriteLine("}");
            classBuilder.WriteLine("}");
            classBuilder.WriteLine();
            
            classBuilder.WriteLine(GenerateClassValueChangedImplementation(propertyName, fullyQualifiedFieldType));
        }

        foreach (var propertyDependentOnField in properties)
        {
            classBuilder.WriteLine(GenerateClassValueChangedImplementation(propertyDependentOnField.Name, propertyDependentOnField.Type.GetFullyQualifiedType()));
        }
        
        WriteTypeChangeImplementations(classBuilder, fields, @class);
        WriteAnyChangeImplementations(classBuilder, @class);
        
        classBuilder.WriteLine("}");
        classBuilder.WriteLine("}");
        
        return classBuilder.ToString();
    }

    private void WriteTypeChangeImplementations(TextWriter classBuilder, List<IFieldSymbol> fields, INamedTypeSymbol @class)
    {
        var interfacesCreated = new List<string>();

        foreach (var field in fields)
        {
            var shouldGenerateInterfaceImplementation = ShouldGenerateTypeValueChangeImplementation(field, @class);
            
            if (!shouldGenerateInterfaceImplementation)
            {
                continue;
            }
            
            var fullyQualifiedFieldType = field.Type.GetFullyQualifiedType();
            var simpleFieldType = field.Type.GetSimpleTypeName();

            var interfaceName = $"INotifyType{simpleFieldType}ValueChanged";
            
            if (interfacesCreated.Contains(interfaceName))
            {
                continue;
            }
            
            interfacesCreated.Add(interfaceName);
            
            classBuilder.WriteLine($"public event {nameof(ValueChangedEventHandler<object>)}<{fullyQualifiedFieldType}> OnType{simpleFieldType}ValueChange;");
            classBuilder.WriteLine($"private void NotifyType{simpleFieldType}ValueChanged({fullyQualifiedFieldType} previousValue, {fullyQualifiedFieldType} newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = null)");
            classBuilder.WriteLine("{");
            classBuilder.WriteLine($"OnType{simpleFieldType}ValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<{fullyQualifiedFieldType}>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));");
            classBuilder.WriteLine("}");
            classBuilder.WriteLine();
        }
    }

    private void WriteAnyChangeImplementations(TextWriter classBuilder, INamedTypeSymbol @class)
    {
        if (!ShouldGenerateAnyValueChangeImplementation(@class))
        {
            return;
        }
        
        classBuilder.WriteLine($"public event {nameof(ValueChangedEventHandler<object>)}<object> OnAnyValueChange;");
        classBuilder.WriteLine("private void NotifyAnyValueChanged(object previousValue, object newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = null)");
        classBuilder.WriteLine("{");
        classBuilder.WriteLine($"OnAnyValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<object>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));");
        classBuilder.WriteLine("}");
        classBuilder.WriteLine();
    }

    private static string WriteTypeEventInterfaces(GeneratorExecutionContext context, List<ITypeSymbol?> types, List<IFieldSymbol> fields, List<IPropertySymbol> properties)
    { 
        var interfacesCreated = new List<string>();

        var classBuilder = new CodeGenerationTextWriter();
        var notifyPropertyChangedSymbol = context.Compilation.GetTypeByMetadataName(typeof(ValueChangedEventHandler<>).FullName);

        classBuilder.WriteLine("using System;");
        classBuilder.WriteLine(context.GetUsingStatementsForTypes(typeof(string), typeof(ValueChangedEventArgs<>), typeof(ValueChangedEventHandler<>), typeof(CallerMemberNameAttribute)));
        classBuilder.WriteLine($"namespace {notifyPropertyChangedSymbol.ContainingNamespace.ToDisplayString()}");
        classBuilder.WriteLine("{");

        foreach (var type in types)
        {
            GenerateGenericTypeEvent(type, fields.Concat<ISymbol>(properties).ToList(), interfacesCreated, classBuilder);
        }
        
        classBuilder.WriteLine("}");
        
        return classBuilder.ToString();
    }

    private static void GenerateGenericTypeEvent(ITypeSymbol? type, List<ISymbol> fieldsAndProperties, List<string> interfacesCreated, TextWriter classBuilder)
    {
        var fullyQualifiedFieldType = type.GetFullyQualifiedType();
        var field = fieldsAndProperties.FirstOrDefault(x => x.GetSymbolType().ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == fullyQualifiedFieldType);

        if (field == null)
        {
            return;
        }

        var simpleTypeName = field.GetSymbolType().GetSimpleTypeName();

        var interfaceName = $"INotifyType{simpleTypeName}ValueChanged";

        if (interfacesCreated.Contains(interfaceName))
        {
            return;
        }

        interfacesCreated.Add(interfaceName);

        classBuilder.WriteLine($"public interface {interfaceName}");
        classBuilder.WriteLine("{");
        classBuilder.WriteLine(
            $"event {nameof(ValueChangedEventHandler<object>)}<{fullyQualifiedFieldType}> OnType{simpleTypeName}ValueChange;");
        classBuilder.WriteLine("}");
        classBuilder.WriteLine();
    }

    private string GenerateClassValueChangedImplementation(string propertyName, string type)
    {
        return $@"public event ValueChangedEventHandler<{type}> On{propertyName}ValueChange;
        
        private void Notify{propertyName}ValueChanged({type} previousValue, {type} newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = """") 
        {{
            On{propertyName}ValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<{type}>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));
        }}
        ";
    }

    private static bool ShouldGenerateTypeValueChangeImplementation(ISymbol symbol, INamedTypeSymbol @class)
    {
        ITypeSymbol type;

        switch (symbol)
        {
            case IFieldSymbol field:
                type = field.Type;
                break;
            case IPropertySymbol property:
                type = property.Type;
                break;
            default:
                return false;
        }

        return @class.GetAttributes().Any(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(NotifyTypeValueChangeAttribute).FullName && (x.ConstructorArguments.First().Value as ITypeSymbol).ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType));
    }
    
    private static bool ShouldGenerateAnyValueChangeImplementation(INamedTypeSymbol @class)
    {
        return @class.GetAttributes().Any(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(NotifyAnyValueChangeAttribute).FullName);
    }
}