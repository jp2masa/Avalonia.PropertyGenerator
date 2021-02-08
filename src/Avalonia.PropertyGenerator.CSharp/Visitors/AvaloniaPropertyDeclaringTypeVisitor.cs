using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp.Visitors
{
    internal sealed class AvaloniaPropertyDeclaringTypeVisitor : SymbolVisitor<DeclaringType?>
    {
        private readonly Types _types;

        public AvaloniaPropertyDeclaringTypeVisitor(Types types)
        {
            _types = types;
        }

        public override DeclaringType? VisitNamedType(INamedTypeSymbol symbol)
        {
            if (!symbol.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, _types.IAvaloniaObject)))
            {
                return default;
            }

            var styled = ImmutableArray.CreateBuilder<Property>();
            var direct = ImmutableArray.CreateBuilder<Property>();
            var attached = ImmutableArray.CreateBuilder<Property>();

            var visitor = new AvaloniaPropertyFieldVisitor();

            foreach (var member in symbol.GetMembers())
            {
                var property = member.Accept(visitor);

                if (property is not null
                    && property.Field.Type is INamedTypeSymbol boundType
                    && boundType.IsGenericType)
                {
                    var type = boundType.ConstructUnboundGenericType();

                    if (SymbolEqualityComparer.Default.Equals(type, _types.StyledProperty))
                    {
                        styled.Add(property);
                    }

                    if (SymbolEqualityComparer.Default.Equals(type, _types.DirectProperty))
                    {
                        direct.Add(property);
                    }

                    if (SymbolEqualityComparer.Default.Equals(type, _types.AttachedProperty))
                    {
                        attached.Add(property);
                    }
                }
            }

            return (styled.Count > 0 || direct.Count > 0 || attached.Count > 0)
                ? new DeclaringType(symbol, styled.ToImmutable(), direct.ToImmutable(), attached.ToImmutable())
                : default;
        }
    }
}
