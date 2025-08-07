using System;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal abstract class Property(IFieldSymbol field, string name)
    {
        public IFieldSymbol Field { get; } = field;

        public string Name { get; } = name;

        public static Property? TryCreate(Types types, IFieldSymbol field)
        {
            if (!field.IsStatic)
            {
                return null;
            }

            if (field.Type is INamedTypeSymbol type && type.IsGenericType)
            {
                type = type.ConstructUnboundGenericType();

                if (SymbolEqualityComparer.Default.Equals(type, types.StyledProperty))
                {
                    return StyledProperty.TryCreate(types, field);
                }

                if (SymbolEqualityComparer.Default.Equals(type, types.DirectProperty))
                {
                    return DirectProperty.TryCreate(types, field);
                }

                if (SymbolEqualityComparer.Default.Equals(type, types.AttachedProperty))
                {
                    return AttachedProperty.TryCreate(types, field);
                }
            }

            return null;
        }

        public static string? GetPropertyName(IFieldSymbol field)
        {
            var i = field.Name.LastIndexOf("Property", StringComparison.Ordinal);

            if (i == -1 || i == 0)
            {
                return null;
            }

            return field.Name.Substring(0, i);
        }
    }
}
