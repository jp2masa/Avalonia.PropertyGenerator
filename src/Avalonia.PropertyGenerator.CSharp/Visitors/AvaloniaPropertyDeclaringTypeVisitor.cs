using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp.Visitors
{
    internal sealed class AvaloniaPropertyDeclaringTypeVisitor(Types types)
        : SymbolVisitor<DeclaringType?>
    {
        private readonly Types _types = types;

        public override DeclaringType? VisitNamedType(INamedTypeSymbol symbol)
        {
            if (!symbol.IsStatic && !IsAvaloniaObject(symbol))
            {
                return null;
            }

            ImmutableArray<StyledProperty>.Builder? styledBuilder = null;
            ImmutableArray<DirectProperty>.Builder? directBuilder = null;
            ImmutableArray<AttachedProperty>.Builder? attachedBuilder = null;

            foreach (var member in symbol.GetMembers())
            {
                if (member.Kind != SymbolKind.Field)
                {
                    continue;
                }

                var property = Property.TryCreate(_types, (IFieldSymbol)member);

                switch (property)
                {
                    case StyledProperty x when !symbol.IsStatic:
                        (styledBuilder ??= ImmutableArray.CreateBuilder<StyledProperty>()).Add(x);
                        break;
                    case DirectProperty x when !symbol.IsStatic:
                        (directBuilder ??= ImmutableArray.CreateBuilder<DirectProperty>()).Add(x);
                        break;
                    case AttachedProperty x:
                        (attachedBuilder ??= ImmutableArray.CreateBuilder<AttachedProperty>()).Add(x);
                        break;
                }
            }

            var styled = styledBuilder?.ToImmutable() ?? ImmutableArray<StyledProperty>.Empty;
            var direct = directBuilder?.ToImmutable() ?? ImmutableArray<DirectProperty>.Empty;
            var attached = attachedBuilder?.ToImmutable() ?? ImmutableArray<AttachedProperty>.Empty;

            return (styled.Length > 0 || direct.Length > 0 || attached.Length > 0)
                ? new DeclaringType(symbol, styled, direct, attached)
                : null;
        }

        private bool IsAvaloniaObject(INamedTypeSymbol? type)
        {
            while (type is not null)
            {
                if (SymbolEqualityComparer.Default.Equals(type, _types.AvaloniaObject))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
