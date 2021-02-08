using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Avalonia.PropertyGenerator.CSharp.Visitors;

namespace Avalonia.PropertyGenerator.CSharp
{
    [Generator]
    internal sealed class AvaloniaToClrPropertyGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;

            if (Types.FromCompilation(compilation, context.ReportDiagnostic) is not Types wellKnowntypes)
            {
                return;
            }

            var visitor = new AvaloniaPropertyRootVisitor(wellKnowntypes);
            var types = visitor.Visit(compilation.Assembly.GlobalNamespace);

            if (types is null)
            {
                return;
            }

            foreach (var type in types)
            {
                var sourceBuilder = new StringBuilder(
$@"namespace {type.Type.ContainingNamespace.ToDisplayString()}
{{");

                sourceBuilder.Append($@"
    partial class {type.Type.Name}
    {{");

                foreach (var property in type.StyledProperties)
                {
                    var typeFullName = ((INamedTypeSymbol)property.Field.Type).TypeArguments[0].ToDisplayString();
                    var accessibility = SyntaxFacts.GetText(property.Field.DeclaredAccessibility);

                    sourceBuilder.Append($@"
        {accessibility} {typeFullName} {property.Name}
        {{
            get => GetValue({property.Field.Name});
            set => SetValue({property.Field.Name}, value);
        }}
");
                }

                foreach (var property in type.DirectProperties)
                {
                    var typeFullName = ((INamedTypeSymbol)property.Field.Type).TypeArguments[1].ToDisplayString();
                    var accessibility = SyntaxFacts.GetText(property.Field.DeclaredAccessibility);

                    var backingField = GetBackingFieldName(property.Name);

                    sourceBuilder.Append($@"
        private {typeFullName} {backingField};

        {accessibility} {typeFullName} {property.Name}
        {{
            get => {backingField};
            set => SetAndRaise({property.Field.Name}, ref {backingField}, value);
        }}
");
                }

                foreach (var property in type.AttachedProperties)
                {
                    var typeFullName = ((INamedTypeSymbol)property.Field.Type).TypeArguments[0].ToDisplayString();
                    var accessibility = SyntaxFacts.GetText(property.Field.DeclaredAccessibility);

                    sourceBuilder.Append($@"
        {accessibility} static {typeFullName} Get{property.Name}(Avalonia.IAvaloniaObject obj) => obj.GetValue({property.Field.Name});

        {accessibility} static void Set{property.Name}(Avalonia.IAvaloniaObject obj, {typeFullName} value) => obj.SetValue({property.Field.Name}, value);
");
                }

                sourceBuilder.Append(
@"    }
}
");

                context.AddSource(
                    Path.GetFileName(type.Type.DeclaringSyntaxReferences.First().SyntaxTree.FilePath),
                    SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        private static string GetBackingFieldName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException();
            }

            if (name.StartsWith("_", StringComparison.Ordinal))
            {
                if (name.Length == 1)
                {
                    return name;
                }

                return "_" + Char.ToLowerInvariant(name[1]) + name.Substring(2);
            }

            return $"_{Char.ToLowerInvariant(name[0])}{name.Substring(1)}";
        }
    }
}
