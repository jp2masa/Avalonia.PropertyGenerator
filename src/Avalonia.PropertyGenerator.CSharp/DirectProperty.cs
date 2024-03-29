using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal sealed class DirectProperty : Property
    {
        private DirectProperty(
            IFieldSymbol field,
            string name,
            bool clrPropertyExists,
            Accessibility clrPropertyAccessibility,
            bool backingFieldExists,
            Accessibility backingFieldAccessibility,
            string backingFieldName,
            bool isReadonly)
            : base(field, name)
        {
            ClrPropertyExists = clrPropertyExists;
            ClrPropertyAccessibility = clrPropertyAccessibility;

            BackingFieldExists = backingFieldExists;
            BackingFieldAccessibility = backingFieldAccessibility;
            BackingFieldName = backingFieldName;

            IsReadonly = isReadonly;
        }

        public bool ClrPropertyExists { get; }

        public Accessibility ClrPropertyAccessibility { get; }

        public bool BackingFieldExists { get; }

        public Accessibility BackingFieldAccessibility { get; }

        public string BackingFieldName { get; }

        public bool IsReadonly { get; }

        public static new DirectProperty? TryCreate(Types types, IFieldSymbol field)
        {
            var name = GetPropertyName(field);

            if (name is null)
            {
                return null;
            }

            var backingFieldAccessibility = Accessibility.NotApplicable;
            var backingFieldName = default(string);
            var isReadonly = false;

            foreach (var attribute in field.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == "Avalonia.PropertyGenerator.ReadonlyAttribute")
                {
                    isReadonly = true;
                    continue;
                }

                if (attribute.AttributeClass?.ToDisplayString() == "Avalonia.PropertyGenerator.BackingFieldAttribute")
                {
                    foreach (var arg in attribute.NamedArguments)
                    {
                        if (arg.Key == "Name" && arg.Value.Value is string value)
                        {
                            backingFieldName = value;
                        }
                        else if (arg.Key == "Accessibility" && arg.Value.Value is not null)
                        {
                            backingFieldAccessibility = (Accessibility)arg.Value.Value;
                        }
                    }

                    continue;
                }
            }

            if (backingFieldAccessibility == Accessibility.NotApplicable)
            {
                backingFieldAccessibility = Accessibility.Private;
            }

            backingFieldName ??= GetDefaultBackingFieldName(name);

            var clrPropertyExists = false;
            var backingFieldExists = false;

            if (field.ContainingType is INamedTypeSymbol declaringType)
            {
                clrPropertyExists = declaringType.GetMembers().Any(x => String.Equals(x.Name, name, StringComparison.Ordinal));
                backingFieldExists = declaringType.GetMembers().Any(x => String.Equals(x.Name, backingFieldName, StringComparison.Ordinal));
            }

            return new DirectProperty(field, name, clrPropertyExists, field.DeclaredAccessibility, backingFieldExists, backingFieldAccessibility, backingFieldName, isReadonly);
        }

        private static string GetDefaultBackingFieldName(string name) =>
            String.IsNullOrWhiteSpace(name)
            ? throw new InvalidOperationException()
            : $"_{Char.ToLowerInvariant(name[0])}{name.Substring(1)}";
    }
}
