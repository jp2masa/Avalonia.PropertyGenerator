using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp.Visitors
{
    internal sealed class AvaloniaPropertyFieldVisitor : SymbolVisitor<Property>
    {
        public override Property? VisitField(IFieldSymbol symbol)
        {
            if (Property.TryCreate(symbol) is Property property)
            {
                return property;
            }

            return null;
        }
    }
}
