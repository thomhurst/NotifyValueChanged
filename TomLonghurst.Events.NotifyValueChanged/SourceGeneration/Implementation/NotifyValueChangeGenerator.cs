using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomLonghurst.Events.NotifyValueChanged.Extensions;
using TomLonghurst.Events.NotifyValueChanged.Helpers;

namespace TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Implementation;

[Generator]
public class NotifyValueChangeGenerator : ISourceGenerator
{
    private NotifyValueChangeAttributeSyntaxReceiver _notifyValueChangeAttributeSyntaxReceiver;

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new NotifyValueChangeAttributeSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
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
            
            var typesWithTypeValueChangeEvents = containingClass.GetAttributes()
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
            context.AddSource($"{containingClass.Name}_NotifyValueChanged.generated", SourceText.From(source, Encoding.UTF8));
        }

        var interfaceSource = WriteTypeEventInterfaces(context, typeEventsThatNeedInterfacesGenerating, allFields, allProperties);
        context.AddSource($"INotifyValueChangedInterfaces_NotifyValueChanged.generated", SourceText.From(interfaceSource, Encoding.UTF8));
    }
    
    private string GenerateClass(GeneratorExecutionContext context, INamedTypeSymbol @class, INamespaceSymbol @namespace, List<IFieldSymbol> fields, List<IPropertySymbol> properties) {
        var stringWriter = new StringWriter();
        var classBuilder = new IndentedTextWriter(stringWriter);
        classBuilder.WriteLine("using System;");
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventArgs<>)));
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventHandler<>)));
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(CallerMemberNameAttribute)));
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(Dictionary<,>)));
        classBuilder.WriteLine($"namespace {@namespace.ToDisplayString()}");
        classBuilder.WriteLine("{");
        
        classBuilder.WriteLine($"\tpublic partial class {@class.Name}");
        
        var listOfInterfaces = fields.Concat<ISymbol>(properties).Where(x => ShouldGenerateTypeValueChangeImplementation(x, @class)).Select(x => x.GetSymbolType().GetSimpleTypeName()).Select(simpleFieldType => $"INotifyType{simpleFieldType}ValueChanged").Distinct().ToList();
        var shouldGenerateAnyValueChangeImplementation = ShouldGenerateAnyValueChangeImplementation(@class);
        if (shouldGenerateAnyValueChangeImplementation)
        {
            listOfInterfaces.Add("INotifyValueChanged");
        }

        if (listOfInterfaces.Any())
        {
            classBuilder.WriteLine("\t:");
            var commaSeparatedListOfInterfaces = listOfInterfaces.Aggregate((a, x) => $"{a}, {x}");
            classBuilder.WriteLine($"\t{commaSeparatedListOfInterfaces}"); 
        }

        classBuilder.WriteLine("\t{");

        foreach(var field in fields) {
            var propertiesDependentOnField = _notifyValueChangeAttributeSyntaxReceiver.IdentifiedPropertiesAndAssociatedFields
                .Where(x => SymbolEqualityComparer.Default.Equals(x.Value, field)).ToList();
            
            var fullyQualifiedFieldType = field.Type.GetFullyQualifiedType();
            var simpleFieldType = field.Type.GetSimpleTypeName();
            var fieldName = field.Name;
            var propertyName = NormalizePropertyName(fieldName);
            classBuilder.WriteLine($"\t\tprivate DateTimeOffset _dateTime{propertyName}Set;");
            classBuilder.WriteLine($"\t\tpublic {fullyQualifiedFieldType} {propertyName}");
            classBuilder.WriteLine("\t\t{");
            classBuilder.WriteLine($"\t\t\tget => {fieldName};");
            classBuilder.WriteLine("\t\t\tset");
            classBuilder.WriteLine("\t\t\t{");
            classBuilder.WriteLine($"\t\t\t\tif({fieldName} == value)");
            classBuilder.WriteLine("\t\t\t\t{");
            classBuilder.WriteLine("\t\t\t\t\treturn;");
            classBuilder.WriteLine("\t\t\t\t}");
            
            classBuilder.WriteLine($"\t\t\t\tvar previousComputedPropertyValues = new Dictionary<string, object>();");
            
            foreach (var propertyDependentOnField in propertiesDependentOnField)
            {
                classBuilder.WriteLine($"\t\t\t\tpreviousComputedPropertyValues.Add(\"{propertyDependentOnField.Key.Name}\", {propertyDependentOnField.Key.Name});");
            }
            
            classBuilder.WriteLine($"\t\t\t\tvar previousValueDateTimeSet = _dateTime{propertyName}Set;");
            classBuilder.WriteLine($"\t\t\t\tvar previousValue = {fieldName};");
            classBuilder.WriteLine($"\t\t\t\t{fieldName} = value;"); 
            classBuilder.WriteLine($"\t\t\t\t_dateTime{propertyName}Set = DateTimeOffset.UtcNow;");
            classBuilder.WriteLine($"\t\t\t\tNotify{propertyName}ValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");

            foreach (var propertyDependentOnField in propertiesDependentOnField)
            {
                classBuilder.WriteLine($"\t\t\t\tNotify{propertyDependentOnField.Key.Name}ValueChanged(({propertyDependentOnField.Key.Type.GetFullyQualifiedType()}) previousComputedPropertyValues[\"{propertyDependentOnField.Key.Name}\"], ({propertyDependentOnField.Key.Type.GetFullyQualifiedType()}) {propertyDependentOnField.Key.Name}, previousValueDateTimeSet, _dateTime{propertyName}Set);");
            }

            if (ShouldGenerateTypeValueChangeImplementation(field, @class))
            {
                classBuilder.WriteLine($"\t\t\t\tOnType{simpleFieldType}ValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");
            }
            
            if (shouldGenerateAnyValueChangeImplementation)
            {
                classBuilder.WriteLine($"\t\t\t\tOnAnyValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");
            }

            classBuilder.WriteLine("\t\t\t}");
            classBuilder.WriteLine("\t\t}");
            classBuilder.WriteLine();
            
            classBuilder.WriteLine(GenerateClassValueChangedImplementation(propertyName, fullyQualifiedFieldType));
        }

        foreach (var propertyDependentOnField in _notifyValueChangeAttributeSyntaxReceiver.IdentifiedPropertiesAndAssociatedFields
                     .Where(x => fields.Any(field => SymbolEqualityComparer.Default.Equals(field, x.Value)))
                     .Select(x => x.Key))
        {
            classBuilder.WriteLine(GenerateClassValueChangedImplementation(propertyDependentOnField.Name, propertyDependentOnField.Type.GetFullyQualifiedType()));
        }
        
        WriteTypeChangeImplementations(classBuilder, fields, @class);
        WriteAnyChangeImplementations(classBuilder, @class);
        
        classBuilder.WriteLine("\t}");
        classBuilder.WriteLine("}");
        
        classBuilder.Flush();
        return stringWriter.ToString();
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
            
            classBuilder.WriteLine($"\t\tpublic event {nameof(ValueChangedEventHandler<object>)}<{fullyQualifiedFieldType}> OnType{simpleFieldType}ValueChange;");
            classBuilder.WriteLine($"\t\tprivate void OnType{simpleFieldType}ValueChanged({fullyQualifiedFieldType} previousValue, {fullyQualifiedFieldType} newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = null)");
            classBuilder.WriteLine("\t\t{");
            classBuilder.WriteLine($"\t\t\tOnType{simpleFieldType}ValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<{fullyQualifiedFieldType}>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));");
            classBuilder.WriteLine("\t\t}");
            classBuilder.WriteLine();
        }
    }

    private void WriteAnyChangeImplementations(TextWriter classBuilder, INamedTypeSymbol @class)
    {
        if (!ShouldGenerateAnyValueChangeImplementation(@class))
        {
            return;
        }
        
        classBuilder.WriteLine($"\t\tpublic event {nameof(ValueChangedEventHandler<object>)}<object> OnAnyValueChange;");
        classBuilder.WriteLine($"\t\tprivate void OnAnyValueChanged(object previousValue, object newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = null)");
        classBuilder.WriteLine("\t\t{");
        classBuilder.WriteLine($"\t\t\tOnAnyValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<object>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));");
        classBuilder.WriteLine("\t\t}");
        classBuilder.WriteLine();
    }

    private static string WriteTypeEventInterfaces(GeneratorExecutionContext context, List<ITypeSymbol?> types, List<IFieldSymbol> fields, List<IPropertySymbol> properties)
    { 
        var interfacesCreated = new List<string>();

        var stringWriter = new StringWriter();
        var classBuilder = new IndentedTextWriter(stringWriter);
        var notifyPropertyChangedSymbol = context.Compilation.GetTypeByMetadataName(typeof(ValueChangedEventHandler<>).FullName);

        classBuilder.WriteLine("using System;");
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventHandler<>)));
        classBuilder.WriteLine(context.GetUsingStatementForNamespace(typeof(CallerMemberNameAttribute)));
        classBuilder.WriteLine($"namespace {notifyPropertyChangedSymbol.ContainingNamespace.ToDisplayString()}");
        classBuilder.WriteLine("{");

        foreach (var type in types)
        {
            GenerateGenericTypeEvent(type, fields.Concat<ISymbol>(properties).ToList(), interfacesCreated, classBuilder);
        }
        
        classBuilder.WriteLine("}");
        
        classBuilder.Flush();
        return stringWriter.ToString();
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

        classBuilder.WriteLine($"\tpublic interface {interfaceName}");
        classBuilder.WriteLine("\t{");
        classBuilder.WriteLine(
            $"\t\tevent {nameof(ValueChangedEventHandler<object>)}<{fullyQualifiedFieldType}> OnType{simpleTypeName}ValueChange;");
        classBuilder.WriteLine("\t}");
        classBuilder.WriteLine();
    }

    private string NormalizePropertyName(string fieldName) {
        return Regex.Replace(fieldName, "_[a-z]", m => m.ToString().TrimStart('_').ToUpper());
    }

    private string GenerateClassValueChangedImplementation(string propertyName, string type)
    {
        return $@"
        public event ValueChangedEventHandler<{type}> On{propertyName}ValueChange;
        
        private void Notify{propertyName}ValueChanged({type} previousValue, {type} newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = """") 
        {{
            On{propertyName}ValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<{type}>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));
        }}
        ";
    }

    private static bool ShouldGenerateTypeValueChangeImplementation(ISymbol symbol, INamedTypeSymbol @class)
    {
        if (symbol is IFieldSymbol field)
        {
            return @class.GetAttributes().Any(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(NotifyTypeValueChangeAttribute).FullName && (x.ConstructorArguments.First().Value as ITypeSymbol).ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == field.Type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType));
        }

        if (symbol is IPropertySymbol property)
        {
            return @class.GetAttributes().Any(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(NotifyTypeValueChangeAttribute).FullName && (x.ConstructorArguments.First().Value as ITypeSymbol).ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == property.Type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType));
        }

        return false;
    }
    
    private static bool ShouldGenerateAnyValueChangeImplementation(INamedTypeSymbol @class)
    {
        return @class.GetAttributes().Any(x => x.AttributeClass.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(NotifyAnyValueChangeAttribute).FullName);
    }
}