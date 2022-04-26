﻿using System.Diagnostics;
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
    private static bool _anyTypeValueChangeEventWritten = false;
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new FieldSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            //Debugger.Launch();
        }
#endif
        if (context.SyntaxContextReceiver is not FieldSyntaxReceiver syntaxReciever)
        {
            return;
        }

        var fieldsThatNeedInterfacesGenerating = new List<IFieldSymbol>();
        foreach (var containingClassGroup in syntaxReciever.IdentifiedFields.GroupBy(x => x.ContainingType)) 
        {
            var containingClass = containingClassGroup.Key;
            var namespaceSymbol = containingClass.ContainingNamespace;
            var fields = containingClassGroup.ToList();
            
            var source = GenerateClass(context, containingClass, namespaceSymbol, fields);
            fieldsThatNeedInterfacesGenerating.AddRange(fields);
            context.AddSource($"{containingClass.Name}_NotifyValueChanged.generated", SourceText.From(source, Encoding.UTF8));
        }

        var interfaceSource = WriteInterfaces(context, fieldsThatNeedInterfacesGenerating);
        context.AddSource($"INotifyValueChangedInterfaces_NotifyValueChanged.generated", SourceText.From(interfaceSource, Encoding.UTF8));
    }
    
    private string GenerateClass(GeneratorExecutionContext context, INamedTypeSymbol @class, INamespaceSymbol @namespace, List<IFieldSymbol> fields) {
        var classBuilder = new StringBuilder();
        classBuilder.AppendLine("using System;");
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventArgs<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventHandler<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(CallerMemberNameAttribute)));
        classBuilder.AppendLine($"namespace {@namespace.ToDisplayString()}");
        classBuilder.AppendLine("{");
        
        classBuilder.AppendLine($"\tpublic partial class {@class.Name}");
        
        var listOfInterfaces = fields.Where(ShouldGenerateTypeValueChangeImplementation).Select(x => x.Type.GetSimpleTypeName()).Select(simpleFieldType => $"INotifyType{simpleFieldType}ValueChanged").Distinct().ToList();

        if (listOfInterfaces.Any())
        {
            classBuilder.AppendLine("\t:");
            var commaSeparatedListOfInterfaces = listOfInterfaces.Aggregate((a, x) => $"{a}, {x}");
            classBuilder.AppendLine($"\t{commaSeparatedListOfInterfaces}"); 
        }

        if (fields.Any(ShouldGenerateAnyValueChangeImplementation))
        {
            classBuilder.AppendLine($"\t, INotifyValueChanged");
        }
        
        classBuilder.AppendLine("\t{");

        foreach(var field in fields) {
            var fullyQualifiedFieldType = field.Type.GetFullyQualifiedType();
            var simpleFieldType = field.Type.GetSimpleTypeName();
            var fieldName = field.Name;
            var propertyName = NormalizePropertyName(fieldName);
            classBuilder.AppendLine($"\t\tprivate DateTimeOffset _dateTime{propertyName}Set;");
            classBuilder.AppendLine($"\t\tpublic {fullyQualifiedFieldType} {propertyName}");
            classBuilder.AppendLine("\t\t{");
            classBuilder.AppendLine($"\t\t\tget => {fieldName};");
            classBuilder.AppendLine("\t\t\tset");
            classBuilder.AppendLine("\t\t\t{");
            classBuilder.AppendLine($"\t\t\t\tif({fieldName} == value)");
            classBuilder.AppendLine("\t\t\t\t{");
            classBuilder.AppendLine("\t\t\t\t\treturn;");
            classBuilder.AppendLine("\t\t\t\t}");
            classBuilder.AppendLine($"\t\t\t\tvar previousValueDateTimeSet = _dateTime{propertyName}Set;");
            classBuilder.AppendLine($"\t\t\t\tvar previousValue = {fieldName};");
            classBuilder.AppendLine($"\t\t\t\t{fieldName} = value;"); 
            classBuilder.AppendLine($"\t\t\t\t_dateTime{propertyName}Set = DateTimeOffset.UtcNow;");
            classBuilder.AppendLine($"\t\t\t\tNotify{propertyName}ValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");
            
            if (ShouldGenerateTypeValueChangeImplementation(field))
            {
                classBuilder.AppendLine($"\t\t\t\tOnType{simpleFieldType}ValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");
            }
            
            if (ShouldGenerateAnyValueChangeImplementation(field))
            {
                classBuilder.AppendLine($"\t\t\t\tOnAnyValueChanged(previousValue, value, previousValueDateTimeSet, _dateTime{propertyName}Set);");
            }

            classBuilder.AppendLine("\t\t\t}");
            classBuilder.AppendLine("\t\t}");
            classBuilder.AppendLine();
            
            classBuilder.AppendLine(GenerateClassValueChangedImplementation(propertyName, fullyQualifiedFieldType));
        }
        
        WriteTypeChangeImplementations(classBuilder, fields);
        WriteAnyChangeImplementations(classBuilder, fields);
        
        classBuilder.AppendLine("\t}");
        classBuilder.AppendLine("}");
        
        return classBuilder.ToString();
    }

    private void WriteTypeChangeImplementations(StringBuilder classBuilder, List<IFieldSymbol> fields)
    {
        var interfacesCreated = new List<string>();

        foreach (var field in fields)
        {
            var shouldGenerateInterfaceImplementation = ShouldGenerateTypeValueChangeImplementation(field);
            
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
            
            classBuilder.AppendLine($"\t\tpublic event {nameof(ValueChangedEventHandler<object>)}<{fullyQualifiedFieldType}> OnType{simpleFieldType}ValueChange;");
            classBuilder.AppendLine($"\t\tprivate void OnType{simpleFieldType}ValueChanged({fullyQualifiedFieldType} previousValue, {fullyQualifiedFieldType} newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = null)");
            classBuilder.AppendLine("\t\t{");
            classBuilder.AppendLine($"\t\t\tOnType{simpleFieldType}ValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<{fullyQualifiedFieldType}>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));");
            classBuilder.AppendLine("\t\t}");
            classBuilder.AppendLine();
        }
    }

    private void WriteAnyChangeImplementations(StringBuilder classBuilder, List<IFieldSymbol> fields)
    {
        if (_anyTypeValueChangeEventWritten || !fields.Any(x => ShouldGenerateAnyValueChangeImplementation(x)))
        {
            return;
        }
        
        classBuilder.AppendLine($"\t\tpublic event {nameof(ValueChangedEventHandler<object>)}<object> OnAnyValueChange;");
        classBuilder.AppendLine($"\t\tprivate void OnAnyValueChanged(object previousValue, object newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = null)");
        classBuilder.AppendLine("\t\t{");
        classBuilder.AppendLine($"\t\t\tOnAnyValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<object>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));");
        classBuilder.AppendLine("\t\t}");
        classBuilder.AppendLine();
    }

    private static string WriteInterfaces(GeneratorExecutionContext context,
        List<IFieldSymbol> fields)
    { 
        var interfacesCreated = new List<string>();
    
        var classBuilder = new StringBuilder();
        var notifyPropertyChangedSymbol = context.Compilation.GetTypeByMetadataName(typeof(ValueChangedEventHandler<>).FullName);

        classBuilder.AppendLine("using System;");
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(ValueChangedEventHandler<>)));
        classBuilder.AppendLine(context.GetUsingStatementForNamespace(typeof(CallerMemberNameAttribute)));
        classBuilder.AppendLine($"namespace {notifyPropertyChangedSymbol.ContainingNamespace.ToDisplayString()}");
        classBuilder.AppendLine("{");

        foreach (var field in fields)
        {
            GenerateGenericTypeEvent(field, interfacesCreated, classBuilder);
        }
        
        classBuilder.AppendLine("}");
        
        return classBuilder.ToString();
    }

    private static void GenerateGenericTypeEvent(IFieldSymbol field, List<string> interfacesCreated, StringBuilder classBuilder)
    {
        var shouldGenerateInterfaceImplementation = ShouldGenerateTypeValueChangeImplementation(field);

        if (!shouldGenerateInterfaceImplementation)
        {
            return;
        }

        var fullyQualifiedFieldType = field.Type.GetFullyQualifiedType();
        var simpleFieldType = field.Type.GetSimpleTypeName();

        var interfaceName = $"INotifyType{simpleFieldType}ValueChanged";

        if (interfacesCreated.Contains(interfaceName))
        {
            return;
        }

        interfacesCreated.Add(interfaceName);

        classBuilder.AppendLine($"\tpublic interface {interfaceName}");
        classBuilder.AppendLine("\t{");
        classBuilder.AppendLine(
            $"\t\tevent {nameof(ValueChangedEventHandler<object>)}<{fullyQualifiedFieldType}> OnType{simpleFieldType}ValueChange;");
        classBuilder.AppendLine("\t}");
        classBuilder.AppendLine();
    }

    private string NormalizePropertyName(string fieldName) {
        return Regex.Replace(fieldName, "_[a-z]", m => m.ToString().TrimStart('_').ToUpper());
    }

    private string GenerateClassValueChangedImplementation(string propertyName, string fieldType)
    {
        
        return $@"
        public event ValueChangedEventHandler<{fieldType}> On{propertyName}ValueChange;
        
        private void Notify{propertyName}ValueChanged({fieldType} previousValue, {fieldType} newValue, DateTimeOffset? previousValueDateTimeSet, DateTimeOffset? newValueDateTimeSet, [CallerMemberName] string propertyName = """") 
        {{
            On{propertyName}ValueChange?.Invoke(this, new {nameof(ValueChangedEventArgs<object>)}<{fieldType}>(propertyName, previousValue, newValue, previousValueDateTimeSet, newValueDateTimeSet));
        }}
        ";
    }

    private static bool ShouldGenerateTypeValueChangeImplementation(IFieldSymbol field)
    {
        return SymbolHelper.GetAttributePropertyValue<NotifyValueChangeAttribute, bool>(field, x => x.GenerateGenericTypeValueChangeEvent);
    }
    
    private static bool ShouldGenerateAnyValueChangeImplementation(IFieldSymbol field)
    {
        return SymbolHelper.GetAttributePropertyValue<NotifyValueChangeAttribute, bool>(field, x => x.GenerateAnyValueChangeInClassEvent);
    }
}