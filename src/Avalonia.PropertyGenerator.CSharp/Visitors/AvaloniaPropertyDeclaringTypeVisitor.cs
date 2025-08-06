using System.Collections.Immutable;
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
            if (!symbol.IsStatic && !IsAvaloniaObject(symbol))
            {
                return null;
            }

            var styled = ImmutableArray.CreateBuilder<StyledProperty>();
            var direct = ImmutableArray.CreateBuilder<DirectProperty>();
            var attached = ImmutableArray.CreateBuilder<AttachedProperty>();

            var visitor = new AvaloniaPropertyFieldVisitor(_types);

            foreach (var member in symbol.GetMembers())
            {
                var property = member.Accept(visitor);

                switch (property)
                {
                    case StyledProperty x when !symbol.IsStatic:
                        styled.Add(x);
                        break;
                    case DirectProperty x when !symbol.IsStatic:
                        direct.Add(x);
                        break;
                    case AttachedProperty x:
                        attached.Add(x);
                        break;
                }
            }

            return (styled.Count > 0 || direct.Count > 0 || attached.Count > 0)
                ? new DeclaringType(symbol, styled.ToImmutable(), direct.ToImmutable(), attached.ToImmutable())
                : default;
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
