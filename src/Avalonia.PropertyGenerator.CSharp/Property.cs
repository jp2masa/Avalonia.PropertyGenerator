using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal sealed class Property
    {
        private Property(IFieldSymbol field, string name)
        {
            Field = field;
            Name = name;
        }

        public IFieldSymbol Field { get; }

        public string Name { get; }

        public static Property? TryCreate(IFieldSymbol field)
        {
            if (!field.IsStatic)
            {
                return null;
            }

            var i = field.Name.LastIndexOf("Property");

            if (i == -1 || i == 0)
            {
                return null;
            }

            return new Property(field, field.Name.Substring(0, i));
        }
    }
}
