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

            var attributes =
@"using System;

namespace Avalonia.PropertyGenerator
{
    /// <summary>
    /// Copied from roslyn: https://github.com/dotnet/roslyn/blob/9007fbc80c19cef52defe1cfb981c838b995c74d/src/Compilers/Core/Portable/Symbols/Accessibility.cs
    /// </summary>
    internal enum BackingFieldAccessibility
    {
        //
        // Summary:
        //     No accessibility specified.
        NotApplicable = 0,
        Private = 1,
        ProtectedAndInternal = 2,
        ProtectedAndFriend = 2,
        Protected = 3,
        Internal = 4,
        Friend = 4,
        ProtectedOrInternal = 5,
        ProtectedOrFriend = 5,
        Public = 6
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class BackingFieldAttribute : Attribute
    {
        public string? Name { get; set; }

        public BackingFieldAccessibility Accessibility { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class ReadonlyAttribute : Attribute
    {
    }
}
";

            var attributesSource = SourceText.From(attributes, Encoding.UTF8);
            compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(attributesSource));

            var visitor = new AvaloniaPropertyRootVisitor(wellKnowntypes);
            var types = visitor.Visit(compilation.Assembly.GlobalNamespace);

            if (!types.HasValue || types.Value.Length == 0)
            {
                return;
            }

            context.AddSource("Attributes.cs", attributesSource);

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

                    var data = GetDirectPropertyData(property);
                    var isReadonly = data.IsReadonly;

                    if (isReadonly)
                    {
                        sourceBuilder.Append($@"
        {SyntaxFacts.GetText(data.BackingFieldAccessibility)} {typeFullName} {data.BackingFieldName};

        {accessibility} {typeFullName} {property.Name} => {data.BackingFieldName};
");
                    }
                    else
                    {

                        sourceBuilder.Append($@"
        {SyntaxFacts.GetText(data.BackingFieldAccessibility)} {typeFullName} {data.BackingFieldName};

        {accessibility} {typeFullName} {property.Name}
        {{
            get => {data.BackingFieldName};
            set => SetAndRaise({property.Field.Name}, ref {data.BackingFieldName}, value);
        }}
");
                    }
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

        private static (string BackingFieldName, Accessibility BackingFieldAccessibility, bool IsReadonly) GetDirectPropertyData(Property property)
        {
            var accessibility = default(Accessibility);
            var name = default(string);
            var isReadonly = false;

            foreach (var attribute in property.Field.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == "Avalonia.PropertyGenerator.ReadonlyAttribute")
                {
                    isReadonly = true;
                    continue;
                }

                if (attribute.AttributeClass?.ToDisplayString() != "Avalonia.PropertyGenerator.BackingFieldAttribute")
                {
                    continue;
                }

                foreach (var arg in attribute.NamedArguments)
                {
                    if (arg.Key == "Name" && arg.Value.Value is string value)
                    {
                        name = value;
                    }
                    else if (arg.Key == "Accessibility" && arg.Value.Value is not null)
                    {
                        accessibility = (Accessibility)arg.Value.Value;
                    }
                }
            }

            if (accessibility == Accessibility.NotApplicable)
            {
                accessibility = Accessibility.Private;
            }

            return (name ?? GetDefaultBackingFieldName(property.Name), accessibility, isReadonly);
        }

        private static string GetDefaultBackingFieldName(string name) =>
            String.IsNullOrWhiteSpace(name)
            ? throw new InvalidOperationException()
            : $"_{Char.ToLowerInvariant(name[0])}{name.Substring(1)}";
    }
}
