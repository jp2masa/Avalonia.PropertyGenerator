using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp.Visitors
{
    internal sealed class AvaloniaPropertyFieldVisitor : SymbolVisitor<Property>
    {
        private readonly Types _types;

        public AvaloniaPropertyFieldVisitor(Types types)
        {
            _types = types;
        }

        public override Property? VisitField(IFieldSymbol symbol)
        {
            if (Property.TryCreate(_types, symbol) is { } property)
            {
                return property;
            }

            return null;
        }
    }
}
