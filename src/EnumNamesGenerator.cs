using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EnumNameGenerator
{
    [Generator]
    internal class EnumNamesGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new AnnotatedEnumSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not AnnotatedEnumSyntaxReceiver receiver) return;

            foreach (var annotatedEnum in receiver.AnnotatedEnums)
            {
                context.AddEnumNameExtensionMethods(annotatedEnum);
            }
        }
    }
    
    internal class AnnotatedEnumSyntaxReceiver : ISyntaxReceiver
    {
        public HashSet<EnumDeclarationSyntax> AnnotatedEnums { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is EnumDeclarationSyntax enumDec &&
                enumDec.AttributeLists.Any(list =>
                    list.Attributes.Any(attr =>
                        $"{attr}Attribute".ToString().Equals(nameof(GenerateEnumNamesAttribute)))))
            {
                AnnotatedEnums.Add(enumDec);
            }
        }
    }

    internal static class EnumNameExtensionMethodBuilder
    {
        public static void AddEnumNameExtensionMethods(this GeneratorExecutionContext context, EnumDeclarationSyntax enumSyntax)
        {
            var className = $"{enumSyntax.Identifier.ValueText}Extensions_generated";

            context.AddSource($"{className}.generated.cs", SourceText.From(GetEnumNameImplementation(className, enumSyntax), Encoding.UTF8));
        }

        public static string GetEnumNameImplementation(string className, EnumDeclarationSyntax theEnum)
        {
            var theEnumName = theEnum.Identifier.ValueText;
            var theNamespace = Namespace(theEnum).Name.ToString();

            var switchBody = new StringBuilder();

            foreach (var enumValueSyntax in theEnum.Members)
            {
                var enumValue = enumValueSyntax.Identifier.Text;
                switchBody.AppendLine($@"{enumValue} => nameof({enumValue}),");
            }

            switchBody.Append("_ => null");

            var template = $@"
using System;
using {theNamespace};
using static {theNamespace}.{theEnumName};

namespace {theNamespace}
{{
    public static class {className}
    {{
        public static string GetName(this {theEnumName} e)
        {{
            return e switch
            {{
                {switchBody}
            }};
        }} 
    }}
}}";

            return template;
        }

        private static NamespaceDeclarationSyntax Namespace(SyntaxNode e)
        {
            SyntaxNode node = e.Parent;

            while (node is not (null or NamespaceDeclarationSyntax))
            {
                node = node.Parent;
            }

            return (NamespaceDeclarationSyntax)node;
        }
    }
}