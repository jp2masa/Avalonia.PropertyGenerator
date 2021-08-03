using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal sealed class StyledProperty : Property
    {
        private StyledProperty(IFieldSymbol field, string name, bool clrPropertyExists, Accessibility clrPropertyAccessibility)
            : base(field, name)
        {
            ClrPropertyExists = clrPropertyExists;
            ClrPropertyAccessibility = clrPropertyAccessibility;
        }

        public bool ClrPropertyExists { get; }

        public Accessibility ClrPropertyAccessibility { get; }

        public static new StyledProperty? TryCreate(Types types, IFieldSymbol field)
        {
            var name = GetPropertyName(field);

            if (name is null)
            {
                return null;
            }

            var clrPropertyExists = false;

            if (field.ContainingType is INamedTypeSymbol declaringType)
            {
                clrPropertyExists = declaringType.GetMembers().Any(x => String.Equals(x.Name, name, StringComparison.Ordinal));
            }

            return new StyledProperty(field, name, clrPropertyExists, field.DeclaredAccessibility);
        }
    }
}
