using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal sealed class AttachedProperty : Property
    {
        private AttachedProperty(
            IFieldSymbol field,
            string name,
            bool getterExists,
            Accessibility getterAccessibility,
            bool setterExists,
            Accessibility setterAccessibility)
            : base(field, name)
        {
            GetterExists = getterExists;
            GetterAccessibility = getterAccessibility;

            SetterExists = setterExists;
            SetterAccessibility = setterAccessibility;
        }

        public bool GetterExists { get; }

        public Accessibility GetterAccessibility { get; }

        public bool SetterExists { get; }

        public Accessibility SetterAccessibility { get; }

        public static new AttachedProperty? TryCreate(Types types, IFieldSymbol field)
        {
            var name = GetPropertyName(field);

            if (name is null)
            {
                return null;
            }

            var getterName = "Get" + name;
            var setterName = "Set" + name;

            var getterExists = false;
            var setterExists = false;

            if (field.ContainingType is INamedTypeSymbol declaringType)
            {
                getterExists = declaringType.GetMembers().Any(x => String.Equals(x.Name, getterName, StringComparison.Ordinal));
                setterExists = declaringType.GetMembers().Any(x => String.Equals(x.Name, setterName, StringComparison.Ordinal));
            }

            return new AttachedProperty(field, name, getterExists, field.DeclaredAccessibility, setterExists, field.DeclaredAccessibility);
        }
    }
}
